using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        private ApplicationDbContext _dbContext { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStockService _stockService;

        public UsersController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IStockService stockService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _stockService = stockService;
        }

        // GET: Users
        public ActionResult Index()
        {
            var users = _dbContext.Users.ToList();
            var model = new AllUsersViewModel() { AllUsers = new List<User>() };
            users.ForEach(x => model.AllUsers.Add(new User() { Email = x.Email, Money = x.Money }));
            return View(model);
        }

        [HttpGet]
        public ActionResult VisitProfile(string email)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("email", "No user with that email");
                return View();
            }

            var model = new DisplayProfileViewModel
            {
                Email = user.Email,
                Money = user.Money,
                StockOwnerships = GetOwnershipsWithLastTradePriceByUser(user),
                FullstockOwnerships = new List<FullStockOnwerShipViewModel>(),
                StockTransactions = GetStockTransWithTimeLeft(user),
                TotalWorth = user.Money
            };

            foreach (var s in model.StockOwnerships)
            {
                model.TotalWorth += (s.Quantity * s.LastTradePrice);
            }

            foreach (var s in model.StockOwnerships)
            {
                if (!model.FullstockOwnerships.Exists(x => x.Label == s.Label))
                {
                    model.FullstockOwnerships.Add(new FullStockOnwerShipViewModel()
                    {
                        Label = s.Label,
                        Name = s.Name,
                        LastTradePrice = _stockService.GetStockByLabel(s.Label).LastTradePriceOnly,
                        Quantity = s.Quantity
                    });
                }
                else
                    model.FullstockOwnerships.Find(x => x.Label == s.Label).Quantity += s.Quantity;
            }

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
                    StockTransactions = GetStockTransWithTimeLeft(user),
                    StockOwnerships = GetOwnershipsWithLastTradePriceByUser(user),
                    FullstockOwnerships = new List<FullStockOnwerShipViewModel>(),
                    StockSolds = _dbContext.StockSold.Where(x => x.User.Id == user.Id).ToList()
                };

                foreach (var s in model.StockOwnerships)
                {
                    model.TotalWorth += (s.Quantity * s.LastTradePrice);
                }

                foreach (var s in model.StockOwnerships)
                {
                    if (!model.FullstockOwnerships.Exists(x => x.Label == s.Label))
                    {

                        model.FullstockOwnerships.Add(new FullStockOnwerShipViewModel()
                        {
                            Label = s.Label,
                            Name = s.Name,
                            LastTradePrice = _stockService.GetStockByLabel(s.Label).LastTradePriceOnly,
                            Quantity = s.Quantity
                        });
                    }
                    else
                        model.FullstockOwnerships.Find(x => x.Label == s.Label).Quantity += s.Quantity;
                }


                return View(model);
            }
            return RedirectToAction("Login", "Account");
        }



        [HttpGet]
        [Authorize]
        [Route("api/Users/[action]")]
        public async Task<string> GetMoney()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return user.Money.ToString();
        }

        private List<StockTransWithTimeLeftViewModel> GetStockTransWithTimeLeft(ApplicationUser user)
        {
            var stockTrans = new List<StockTransWithTimeLeftViewModel>();
            foreach (var tran in _dbContext.StockTransaction.Where(x => x.User.Id == user.Id).ToList())
            {
                var timeLeft = tran.Date.AddMinutes(15) - DateTime.Now;
                if (timeLeft.CompareTo(TimeSpan.Zero) < 0)
                    timeLeft = TimeSpan.Zero;

                stockTrans.Add(new StockTransWithTimeLeftViewModel()
                {
                    Id = tran.Id,
                    Bid = tran.Bid,
                    Date = tran.Date,
                    IsBuying = tran.IsBuying,
                    IsCompleted = tran.IsCompleted,
                    IsSelling = tran.IsSelling,
                    Label = tran.Label,
                    Name = tran.Name,
                    Quantity = tran.Quantity,
                    User = tran.User,
                    TotalMoney = tran.TotalMoney,
                    TimeLeftToCompletion = timeLeft
                });

            }
            return stockTrans;
        }

        private List<StockOwnerShipViewModel> GetOwnershipsWithLastTradePriceByUser(ApplicationUser user)
        {
            var ownerships = new List<StockOwnerShipViewModel>();
            foreach (var s in _dbContext.StockOwnership.Where(x => x.User.Id == user.Id).ToList())
            {
                ownerships.Add(new StockOwnerShipViewModel()
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
    }
}