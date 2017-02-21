using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GameOfStocksHT16.Data;
using GameOfStocksHT16.Models.UsersViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GameOfStocksHT16.Services;
using GameOfStocksHT16.Entities;

namespace GameOfStocksHT16.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStockService _stockService;
        private readonly IGameOfStocksRepository _gameOfStocksRepository;

        public UsersController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IStockService stockService,  IGameOfStocksRepository gameOfStocksRepository)
        {
            _userManager = userManager;
            _stockService = stockService;
            _gameOfStocksRepository = gameOfStocksRepository;
        }

        //public ActionResult Index()
        //{
        //    var users = _dbContext.Users.Include(u => u.StockOwnerships).OrderByDescending(u => u.Money).ToList();
        //    var allUsers = new List<UserInfoModel>();

        //    var model = new HomeViewModel()
        //    {
        //        CurrentLeaderBoard = new List<UserInfoModel>(),
        //        TodaysLoosers = new List<UserInfoModel>(),
        //        TodaysWinners = new List<UserInfoModel>()
        //    };

        //    if (users.Any()) return View(model);

        //    foreach (var user in users)
        //    {
        //        var userWithTotalWorth = new UserInfoModel()
        //        {
        //            Email = user.Email,
        //            Money = user.Money,
        //            TotalWorth = user.Money + user.ReservedMoney
        //        };

        //        foreach (var stockOwnership in user.StockOwnerships)
        //        {
        //            userWithTotalWorth.TotalWorth += _stockService.GetStockByLabel(stockOwnership.Label).LastTradePriceOnly * stockOwnership.Quantity;
        //            userWithTotalWorth.GrowthPercent = Math.Round(((userWithTotalWorth.TotalWorth / 100000 - 1) * 100), 2);
        //        }

        //        allUsers.Add(userWithTotalWorth);
        //    }

        //    model.CurrentLeaderBoard = allUsers.OrderByDescending(u => u.TotalWorth).Take(5).ToList();

        //    var usersTotalWorthPerDay = _stockService.GetUsersTotalWorthPerDay();
        //    var usersWithPercentPerDay = new List<UserInfoModel>();

        //    foreach (var userWithTotal in usersTotalWorthPerDay)
        //    {
        //        if (allUsers.All(u => u.Email != userWithTotal.Email)) continue;

        //        usersWithPercentPerDay.Add(new UserInfoModel()
        //        {
        //            Email = userWithTotal.Email,
        //            PercentPerDay = Math.Round(((allUsers.First(u => u.Email == userWithTotal.Email).TotalWorth / userWithTotal.TotalWorth - 1) * 100), 2)
        //        });
        //    }

        //    model.TodaysWinners = usersWithPercentPerDay.OrderByDescending(u => u.PercentPerDay).Take(5).ToList();
        //    model.TodaysLoosers = usersWithPercentPerDay.OrderBy(u => u.PercentPerDay).Take(5).ToList();

        //    return View(model);
        //}

        // GET: Users
        public ActionResult Leaderboard()
        {
            var users = _gameOfStocksRepository.GetAllUsers(); 
                //_dbContext.Users.Include(u => u.StockOwnerships).OrderByDescending(u => u.Money).ToList();

            if (users == null)
                return View();

            var model = new LeaderBoardViewModel()
            {
                AllUsers = new List<UserModel>()
            };

            foreach (var user in users)
            {

                var usersWithTotalWorth = new UserModel()
                {
                    Email = user.Email,
                    Money = user.Money,
                    TotalWorth = user.Money + user.ReservedMoney
                };

                var sellingStocks = _gameOfStocksRepository.GetSellingStockTransactionsByUser(user);

                if(sellingStocks.Any())
                    foreach (var sellingStock in sellingStocks)
                    {
                        usersWithTotalWorth.TotalWorth +=
                            _stockService.GetStockByLabel(sellingStock.Label).LastTradePriceOnly*sellingStock.Quantity;
                    }

                foreach (var ownership in user.StockOwnerships)
                {
                    usersWithTotalWorth.TotalWorth += _stockService.GetStockByLabel(ownership.Label).LastTradePriceOnly * ownership.Quantity;
                }

                usersWithTotalWorth.GrowthPercent = Math.Round(((usersWithTotalWorth.TotalWorth / 100000 - 1) * 100), 2);

                model.AllUsers.Add(usersWithTotalWorth);

            }
            model.AllUsers = model.AllUsers.OrderByDescending(x => x.TotalWorth).ToList();
            return View(model);
        }

        [HttpGet]
        public ActionResult VisitProfile(string email)
        {
            var user = _gameOfStocksRepository.GetUserByEmail(email);
                //_dbContext.Users.FirstOrDefault(u => u.Email == email);
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
                    ReservedMoney = user.ReservedMoney,
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
        [Route("api/Users/[action]")]
        public string GetMoney()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = _gameOfStocksRepository.GetUserById(userId);
            return $"{user.Money:c}";
        }

        private List<StockTransWithTimeLeftViewModel> GetStockTransWithTimeLeft(ApplicationUser user)
        {
            var stockTrans = new List<StockTransWithTimeLeftViewModel>();
            foreach (var tran in _gameOfStocksRepository.GetStockTransactionsByUser(user))
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
            foreach (var s in _gameOfStocksRepository.GetStockOwnershipsByUser(user))
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