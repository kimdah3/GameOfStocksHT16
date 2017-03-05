using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameOfStocksHT16.Data;
using GameOfStocksHT16.Models;
using GameOfStocksHT16.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using GameOfStocksHT16.Entities;
using AutoMapper;

namespace GameOfStocksHT16.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class StockTransactionsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStockService _stockService;
        private readonly IGameOfStocksRepository _gameOfStocksRepository;

        public StockTransactionsController(
            UserManager<ApplicationUser> userManager,
            IStockService stockService,
            IGameOfStocksRepository gameOfStocksRepository)
        {
            _userManager = userManager;
            _stockService = stockService;
            _gameOfStocksRepository = gameOfStocksRepository;
        }

        // GET: api/StockTransactions
        //[HttpGet]
        //public IEnumerable<StockTransaction> GetStockTransaction()
        //{
        //    return _context.StockTransaction;
        //}

        // GET: api/StockTransactions/5
        [HttpGet("{id}", Name = "GetStockTransaction")]
        public IActionResult GetStockTransaction([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var stockTransaction = _gameOfStocksRepository.GetStockTransactionById(id);

            if (stockTransaction == null)
            {
                return NotFound();
            }

            var stockTransactionResult = Mapper.Map<StockTransationDto>(stockTransaction);

            return Ok(stockTransactionResult);
        }

        // POST: api/StockTransactions
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateBuyingStockTransaction(string label, int quantity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            //if (!_stockService.IsMarketOpenForStockTransactions()) return BadRequest("Börsen är stängd.");

            var userId = _userManager.GetUserId(HttpContext.User);
            var user = _gameOfStocksRepository.GetUserById(userId);

            if (user == null) return BadRequest("Logga in först.");

            var stock = _stockService.GetStockByLabel(label);
            if (quantity <= 0 || stock.Volume < quantity) return BadRequest("Du kan inte handla mindre än 0 eller mer än aktiens volym.");

            if (user.Money <= stock.LastTradePriceOnly * quantity) return BadRequest("Inte tillräckligt med pengar.");

            //If quantity is over 20%
            var maxStockVolume = (double)stock.Volume * 1 / 5;
            if (maxStockVolume <= quantity) return BadRequest("Du kan inte handla mer än 20% av en akties totala volym.");

            //If quantity in possesion and pending is over 20% in stock volume
            var quantityInPossesion = GetUsersStockQuantityInPossesion(user, label) + GetUsersStockQuantityInPending(user, label);
            if ((quantityInPossesion + quantity) > maxStockVolume)
            {
                var currentOwnerShipPercent = Math.Round((double)(quantityInPossesion) / stock.Volume * 100, 2);
                var requestedQuantityPercent = Math.Round((double)(quantityInPossesion + quantity) / stock.Volume * 100, 2);
                return BadRequest("Antalet du äger kan inte överstiga mer än 20% av dagsvolymen, du äger just nu "
                        + currentOwnerShipPercent + "% och försöker köpa totalt " + requestedQuantityPercent + "%.");
            }

            var stockTransaction = new StockTransaction()
            {
                Bid = stock.LastTradePriceOnly,
                Date = DateTime.Now,
                IsBuying = true,
                IsSelling = false,
                Label = stock.Label,
                Name = stock.Name,
                Quantity = quantity,
                TotalMoney = quantity * stock.LastTradePriceOnly,
                IsCompleted = false,
                User = user
            };

            //Reserve money
            user.Money -= stockTransaction.TotalMoney;
            user.ReservedMoney += stockTransaction.TotalMoney;

            _gameOfStocksRepository.AddStockTransactions(stockTransaction);

            if (!_gameOfStocksRepository.Save())
                return StatusCode(500, "A problem happend while handeling your request.");

            var createdStockTransaction = Mapper.Map<Models.StockTransationDto>(stockTransaction);

            return CreatedAtRoute("GetStockTransaction", new { id = stockTransaction.Id }, createdStockTransaction);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSellingStockTransaction(string label, int quantity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            //if (!_stockService.IsMarketOpenForStockTransactions()) return BadRequest("Börsen är stängd.");

            var userId = _userManager.GetUserId(HttpContext.User);
            var user = _gameOfStocksRepository.GetUserById(userId);

            var stockOwnership = _gameOfStocksRepository.GetStockOwnershipByUserAndLabel(user, label);

            if (user == null || stockOwnership == null || quantity <= 0) return BadRequest("Något gick åt skogen.");

            if (quantity > stockOwnership.Quantity) return BadRequest("Du försöker sälja mer än vad du har.");

            var stock = _stockService.GetStockByLabel(label);

            var stockTransaction = new StockTransaction()
            {
                Bid = stock.LastTradePriceOnly,
                Date = DateTime.Now,
                IsBuying = false,
                IsSelling = true,
                Label = stock.Label,
                Name = stock.Name,
                Quantity = quantity,
                TotalMoney = quantity * stock.LastTradePriceOnly,
                IsCompleted = false,
                User = user
            };

            _gameOfStocksRepository.AddStockTransactions(stockTransaction);

            if (stockOwnership.Quantity == quantity)
            {
                _gameOfStocksRepository.RemoveStockOwnership(stockOwnership);
            }
            else
            {
                stockOwnership.Quantity -= quantity;
            }

            // Saves db
            if (!_gameOfStocksRepository.Save())
            {
                if (_gameOfStocksRepository.StockTransactionExists(stockTransaction))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                return StatusCode(500, "A problem happend while handeling your request.");
            }

            var createdStockTransaction = Mapper.Map<Models.StockTransationDto>(stockTransaction);

            return CreatedAtRoute("GetStockTransaction", new { id = stockTransaction.Id }, createdStockTransaction);
        }

        private int GetUsersStockQuantityInPossesion(ApplicationUser user, string label)
        {
            var quantity = 0;

            var ownership = user.StockOwnerships.FirstOrDefault(s => s.Label == label);
            if (ownership != null) quantity = ownership.Quantity;

            return quantity;
        }

        private int GetUsersStockQuantityInPending(ApplicationUser user, string label)
        {
            var quantity = 0;
            var pendingTransactions = user.StockTransactions.Where(s => s.Label == label && s.IsBuying && !s.IsCompleted);
            if (pendingTransactions.Any())
            {
                foreach (var transation in pendingTransactions)
                    quantity += transation.Quantity;
            }

            return quantity;
        }

        //// DELETE: api/StockTransactions/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteStockTransaction([FromRoute] int id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    StockTransaction stockTransaction = await _context.StockTransaction.SingleOrDefaultAsync(m => m.Id == id);
        //    if (stockTransaction == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.StockTransaction.Remove(stockTransaction);
        //    await _context.SaveChangesAsync();

        //    return Ok(stockTransaction);
        //}


    }
}