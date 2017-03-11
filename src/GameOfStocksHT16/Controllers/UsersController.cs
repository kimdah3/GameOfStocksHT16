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
using GameOfStocksHT16.Models;

namespace GameOfStocksHT16.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStockService _stockService;
        private readonly IGameOfStocksRepository _gameOfStocksRepository;

        public UsersController(UserManager<ApplicationUser> userManager, IStockService stockService, IGameOfStocksRepository gameOfStocksRepository)
        {
            _userManager = userManager;
            _stockService = stockService;
            _gameOfStocksRepository = gameOfStocksRepository;
        }

        // GET: Users
        public ActionResult Leaderboard()
        {

            if (!_gameOfStocksRepository.UsersExists()) return View();

            var model = new LeaderBoardViewModel()
            {
                AllUsers = _stockService.GetAllUsersWithTotalWorth()
            };

            //var users = _gameOfStocksRepository.GetAllUsers();
            //foreach (var user in users)
            //{
            //    var usersWithTotalWorth = new UserModel()
            //    {
            //        Email = user.Email,
            //        Money = user.Money,
            //        TotalWorth = _stockService.GetUserTotalWorth(user),
            //        FullName = user.FullName,
            //        PictureUrl = user.PictureUrl,
            //        Id = user.Id
            //    };

            //    usersWithTotalWorth.GrowthPercent = Math.Round(((usersWithTotalWorth.TotalWorth / 100000 - 1) * 100), 2);

            //    model.AllUsers.Add(usersWithTotalWorth);

            //}
            model.AllUsers.OrderByDescending(x => x.TotalWorth);
            return View(model);
        }

        [HttpGet]
        public ActionResult VisitProfile(string id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var userToCheck = _gameOfStocksRepository.GetUserById(userId);
            var userToDisplay = _gameOfStocksRepository.GetUserById(id);

            if (userToDisplay == null)
            {
                ModelState.AddModelError("email", "No user with that email");
                return View();
            }

            if (userToCheck.Id == id) return RedirectToAction("DisplayProfile");

            var model = new ProfileViewModel
            {
                Email = userToDisplay.Email,
                Money = userToDisplay.Money,
                StockOwnerships = GetOwnershipsWithLastTradePriceByUser(userToDisplay),
                StockTransactions = GetStockTransWithTimeLeft(userToDisplay),
                TotalWorth = _stockService.GetUserTotalWorth(userToDisplay),
                FullName = userToDisplay.FullName,
                PictureUrl = userToDisplay.PictureUrl,
                ProgressAllDays = _stockService.GetUserTotalWorthProgress(userToDisplay)
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult DisplayProfile()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = _gameOfStocksRepository.GetUserById(userId);
            if (user != null)
            {

                var model = new ProfileViewModel()
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Money = user.Money,
                    ReservedMoney = user.ReservedMoney,
                    TotalWorth = _stockService.GetUserTotalWorth(user),
                    StockTransactions = GetStockTransWithTimeLeft(user),
                    StockOwnerships = GetOwnershipsWithLastTradePriceByUser(user),
                    ProgressAllDays = _stockService.GetUserTotalWorthProgress(user),
                    FullName = user.FullName,
                    PictureUrl = user.PictureUrl
                };

                return View(model);
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult Profile()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = _gameOfStocksRepository.GetUserById(userId);

            if (user == null) return RedirectToAction("Login", "Account");

            var stockTrans = GetStockTransWithTimeLeftByUser(user);
            var ownerships = GetOwnershipsWithLastTradePriceByUserSimplified(user);
            var usersWithTotal = _stockService.GetAllUsersWithTotalWorth().OrderByDescending(x => x.TotalWorth).ToList();
            var usersTotalWorthProgress = _stockService.GetUserTotalWorthProgress(user);

            var model = new ProfileViewModelSimplified()
            {
                User = new UserModel
                {
                    FullName = user.FullName,
                    PictureUrl = user.PictureUrl,
                    Email = user.Email,
                    Money = user.Money,
                    ReservedMoney = user.ReservedMoney,
                    TotalWorth = _stockService.GetUserTotalWorth(user),
                },
                Position = usersWithTotal.IndexOf(usersWithTotal.First(u => u.Id == user.Id)) + 1,
                PendingTransactions = stockTrans.Where(s => !s.IsCompleted && !s.IsFailed).ToList(),
                CompletedTransactions = stockTrans.Where(s => s.IsCompleted && !s.IsFailed).ToList(),
                FailedTransactions = stockTrans.Where(s => s.IsFailed).ToList(),
                StockOwnerships = ownerships,
                ProgressAllDays = usersTotalWorthProgress,
            };

            return View(model);
        }

        [HttpGet]
        [Route("api/Users/[action]")]
        public string GetMoney()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = _gameOfStocksRepository.GetUserById(userId);
            return $"{user.Money:c}";
        }

        private List<StockTransactionModel> GetStockTransWithTimeLeftByUser(ApplicationUser user)
        {
            var stockTrans = new List<StockTransactionModel>();
            foreach (var tran in _gameOfStocksRepository.GetStockTransactionsByUser(user))
            {
                var timeLeft = tran.Date.AddMinutes(15) - DateTime.Now;
                if (timeLeft.CompareTo(TimeSpan.Zero) < 0) timeLeft = TimeSpan.Zero;

                stockTrans.Add(new StockTransactionModel()
                {
                    Bid = tran.Bid,
                    Date = tran.Date,
                    IsBuying = tran.IsBuying,
                    IsCompleted = tran.IsCompleted,
                    IsSelling = tran.IsSelling,
                    IsFailed = tran.IsFailed,
                    Label = tran.Label,
                    Name = tran.Name,
                    Quantity = tran.Quantity,
                    TotalMoney = tran.TotalMoney,
                    TimeLeftToCompletion = timeLeft
                });
            }
            return stockTrans;
        }

        private List<StockOwnershipModel> GetOwnershipsWithLastTradePriceByUserSimplified(ApplicationUser user)
        {
            var ownerships = new List<StockOwnershipModel>();
            foreach (var s in _gameOfStocksRepository.GetStockOwnershipsByUser(user))
            {
                var lastTradePrice = _stockService.GetStockByLabel(s.Label).LastTradePriceOnly;
                ownerships.Add(new StockOwnershipModel()
                {
                    Label = s.Label,
                    Name = s.Name,
                    Quantity = s.Quantity,
                    Gav = s.Gav,
                    TotalSum = s.TotalSum,
                    LastTradePrice = lastTradePrice,
                    Growth = (lastTradePrice / s.Gav - 1) * 100,
                });
            }
            return ownerships;
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
                    IsFailed = tran.IsFailed,
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
                    Growth = (lastTradePrice / s.Gav - 1) * 100,
                    User = s.User
                });
            }
            return ownerships;
        }


    }
}