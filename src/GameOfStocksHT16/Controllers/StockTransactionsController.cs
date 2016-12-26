using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameOfStocksHT16.Data;
using GameOfStocksHT16.Models;
using GameOfStocksHT16.StocksLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace GameOfStocksHT16.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/StockTransactions")]
    public class StockTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StockHandler StockHandler;

        public StockTransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _userManager = userManager;
            StockHandler = new StockHandler(hostingEnvironment);
        }

        // GET: api/StockTransactions
        [HttpGet]
        public IEnumerable<StockTransaction> GetStockTransaction()
        {
            return _context.StockTransaction;
        }

        // GET: api/StockTransactions/5
        [HttpGet("{id}")]
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
        public async Task<IActionResult> PostBuyingStockTransaction(string label, int quantity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return BadRequest();
            }

            var stock = StockHandler.GetStockByLabel(label);
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
                IsPending = true,
                User = user
            };

            _context.StockTransaction.Add(stockTransaction);
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

            return NoContent();
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