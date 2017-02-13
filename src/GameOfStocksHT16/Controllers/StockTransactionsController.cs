using System;
using System.Collections.Generic;
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

namespace GameOfStocksHT16.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class StockTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStockService _stockService;

        public StockTransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHostingEnvironment hostingEnvironment, IStockService stockService)
        {
            _context = context;
            _userManager = userManager;
            _stockService = stockService;
        }

        // GET: api/StockTransactions
        [HttpGet]
        public IEnumerable<StockTransaction> GetStockTransaction()
        {
            return _context.StockTransaction;
        }

        // GET: api/StockTransactions/5
        [HttpGet("{id}", Name = "GetStockTransaction")]
        public async Task<IActionResult> GetStockTransaction([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StockTransaction stockTransaction = await _context.StockTransaction.SingleOrDefaultAsync(m => m.Id == id);

            if (stockTransaction == null)
            {
                return NotFound();
            }

            return Ok(stockTransaction);
        }

        // PUT: api/StockTransactions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStockTransaction([FromRoute] int id, [FromBody] StockTransaction stockTransaction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != stockTransaction.Id)
            {
                return BadRequest();
            }

            _context.Entry(stockTransaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockTransactionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/StockTransactions
        [HttpPost]
        public IActionResult PostBuyingStockTransaction(string label, int quantity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = _userManager.GetUserId(HttpContext.User);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return BadRequest();

            var stock = _stockService.GetStockByLabel(label);

            if (user.Money <= stock.LastTradePriceOnly * quantity)
            {
                return BadRequest();
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

            user.Money -= stockTransaction.TotalMoney;
            user.PendingMoney += stockTransaction.TotalMoney;
            _context.StockTransaction.Add(stockTransaction);

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (StockTransactionExists(stockTransaction.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetStockTransaction", new { id = stockTransaction.Id }, stockTransaction);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostSellingStockTransaction(string label, int quantity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var stockOwnership = await _context.StockOwnership.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == 1 && s.User == user);

            if (user == null || stockOwnership.User != user)
            {
                return BadRequest();
            }

            var stock = _stockService.GetStockByLabel(label);

            var stockTransaction = new StockTransaction()
            {
                Bid = stock.LastTradePriceOnly,
                Date = DateTime.Now,
                IsBuying = false,
                IsSelling = true,
                Label = stock.Label,
                Name = stock.Name,
                Quantity = stockOwnership.Quantity,
                TotalMoney = stockOwnership.Quantity * stock.LastTradePriceOnly,
                IsCompleted = false,
                User = user
            };

            _context.StockTransaction.Add(stockTransaction);
            _context.StockOwnership.Remove(stockOwnership);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (StockTransactionExists(stockTransaction.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // DELETE: api/StockTransactions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockTransaction([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StockTransaction stockTransaction = await _context.StockTransaction.SingleOrDefaultAsync(m => m.Id == id);
            if (stockTransaction == null)
            {
                return NotFound();
            }

            _context.StockTransaction.Remove(stockTransaction);
            await _context.SaveChangesAsync();

            return Ok(stockTransaction);
        }

        private bool StockTransactionExists(int id)
        {
            return _context.StockTransaction.Any(e => e.Id == id);
        }
    }
}