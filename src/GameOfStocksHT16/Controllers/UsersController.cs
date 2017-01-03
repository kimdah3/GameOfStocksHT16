using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameOfStocksHT16.Data;
using GameOfStocksHT16.Models;
using GameOfStocksHT16.Models.UsersViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GameOfStocksHT16.Services;

namespace GameOfStocksHT16.Controllers
{
    public class UsersController : Controller
    {
        private ApplicationDbContext DbContext { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStockService _stockService;

        public UsersController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IStockService stockService)
        {
            DbContext = dbContext;
            _userManager = userManager;
            _stockService = stockService;
        }

        // GET: Users
        public ActionResult Index()
        {
            var users = DbContext.Users.ToList();
            var model = new AllUsersViewModel() { AllUsers = new List<User>() };
            users.ForEach(x => model.AllUsers.Add(new User() { Email = x.Email, Money = x.Money }));
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> DisplayProfile()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                var model = new DisplayProfileViewModel()
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Money = user.Money,
                    TotalWorth = user.Money,
                    StockTransactions = DbContext.StockTransaction.Where(x => x.User.Id == user.Id).ToList(),
                    StockOwnerships = GetOwnershipsWithLastTradePriceByUser(user)
                };
                
                foreach (var s in model.StockOwnerships)
                {
                    model.TotalWorth += (s.Quantity * s.LastTradePrice);
                }

                return View(model);
            }
            return RedirectToAction("Login","Account");
        }

        private List<StockOwnershipWithLastTradePrice> GetOwnershipsWithLastTradePriceByUser(ApplicationUser user)
        {
            var ownerships = new List<StockOwnershipWithLastTradePrice>();
            foreach(var s in DbContext.StockOwnership.Where(x => x.User.Id == user.Id).ToList())
            {
                ownerships.Add(new StockOwnershipWithLastTradePrice()
                {
                    Id = s.Id,
                    Label = s.Label,
                    Name = s.Name,
                    DateBought = s.DateBought,
                    Quantity = s.Quantity,
                    Ask = s.Ask,
                    User = s.User,
                    LastTradePrice = _stockService.GetStockByLabel(s.Label).LastTradePriceOnly
                });
            }
            return ownerships;
        }

        [HttpGet]
        [Authorize]
        [Route("api/Users/[action]")]
        public async Task<string> GetMoney()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return user.Money.ToString();
        }
    }
}