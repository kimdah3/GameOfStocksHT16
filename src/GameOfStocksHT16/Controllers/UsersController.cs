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
using GameOfStocksHT16.Entities;
using Microsoft.EntityFrameworkCore;

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
            var users = _dbContext.Users.Include(u => u.StockOwnerships).OrderByDescending(u => u.Money).ToList();

            var stockUsers = new List<ProfileViewModel>();

            var model = new AllUsersViewModel()
            {
                AllUsers = new List<User>(),
                LeaderBoard = new List<ProfileViewModel>()
            };


            if (users == null) return View(model);

            foreach (var user in users)
            {

                var usersWithTotalWorth = new ProfileViewModel()
                {
                    Email = user.Email,
                    Money = user.Money,
                    TotalWorth = user.Money + user.ReservedMoney
                };

                foreach (var ownership in user.StockOwnerships)
                {
                    usersWithTotalWorth.TotalWorth += _stockService.GetStockByLabel(ownership.Label).LastTradePriceOnly * ownership.Quantity;
                }
                usersWithTotalWorth.GrowthPercent = Math.Round(((usersWithTotalWorth.TotalWorth / 100000 - 1) * 100), 2);

                stockUsers.Add(usersWithTotalWorth);

            }
            model.LeaderBoard = stockUsers.OrderByDescending(x => x.TotalWorth).ToList();

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

            var model = new ProfileViewModel
            {
                Email = user.Email,
                Money = user.Money,
                StockOwnerships = GetOwnershipsWithLastTradePriceByUser(user),
                StockTransactions = GetStockTransWithTimeLeft(user),
                TotalWorth = user.Money + user.ReservedMoney
            };

            foreach (var s in model.StockOwnerships)
            {
                model.TotalWorth += (s.Quantity * s.LastTradePrice);
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

                var model = new ProfileViewModel()
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Money = user.Money,
                    PendingMoney = user.ReservedMoney,
                    TotalWorth = user.Money + user.ReservedMoney,
                    StockTransactions = GetStockTransWithTimeLeft(user),
                    StockOwnerships = GetOwnershipsWithLastTradePriceByUser(user),
                    ProgressAllDays = _stockService.GetUserTotalWorthProgress(user.Email)
                };

                foreach (var s in model.StockOwnerships)
                {
                    model.TotalWorth = model.TotalWorth+ (s.Quantity * s.LastTradePrice);
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
                var lastTradePrice = _stockService.GetStockByLabel(s.Label).LastTradePriceOnly;
                ownerships.Add(new StockOwnerShipViewModel()
                {
                    Id = s.Id,
                    Label = s.Label,
                    Name = s.Name,
                    Quantity = s.Quantity,
                    Gav = s.Gav,
                    TotalSum = s.TotalSum,
                    LastTradePrice = lastTradePrice,
                    Growth = (lastTradePrice/s.Gav -1)*100,
                    User = s.User
                });
            }
            return ownerships;
        }
    }
}