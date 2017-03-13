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
        private readonly IStockService _stockService;
        private readonly IGameOfStocksRepository _gameOfStocksRepository;

        public UsersController(IStockService stockService, IGameOfStocksRepository gameOfStocksRepository)
        {
            _stockService = stockService;
            _gameOfStocksRepository = gameOfStocksRepository;
        }

        // GET: Users
        //public ActionResult Leaderboard()
        //{
        //    if (!_gameOfStocksRepository.UsersExists()) return View();

        //    var model = new LeaderBoardViewModel()
        //    {
        //        AllUsers = _stockService.GetAllUsersWithTotalWorth().OrderByDescending(x => x.TotalWorth).ToList()
        //    };

        //    return View(model);
        //}


        //public ActionResult Leaderboard()
        //{

        //    if (!_gameOfStocksRepository.UsersExists()) return View();

        //    var model = new LeaderBoardViewModel()
        //    {
        //        AllUsers = new List<UserModel>()
        //    };

        //    var users = _gameOfStocksRepository.GetAllUsers();

        //    foreach (var user in users)
        //    {
        //        var usersWithTotalWorth = new UserModel()
        //        {
        //            Email = user.Email,
        //            Money = user.Money,
        //            TotalWorth = _stockService.GetUserTotalWorth(user),
        //            FullName = user.FullName,
        //            PictureUrl = user.PictureUrl,
        //            Id = user.Id
        //        };

        //        usersWithTotalWorth.GrowthPercent = Math.Round(((usersWithTotalWorth.TotalWorth / 100000 - 1) * 100), 2);

        //        model.AllUsers.Add(usersWithTotalWorth);

        //    }
        //    model.AllUsers = model.AllUsers.OrderByDescending(x => x.TotalWorth).ToList();
        //    return View(model);
        //}



        public ActionResult Leaderboard()
        {
            var users = _gameOfStocksRepository.GetAllUsers();

            if (users == null)
                return View();

            var model = new LeaderBoardViewModel()
            {
                AllUsers = new List<UserModel>()
            };

            var stocks = _stockService.GetStocks();

            foreach (var user in users)
            {

                var usersWithTotalWorth = new UserModel()
                {
                    Id = user.Id,
                    Email = user.Email,
                    Money = user.Money,
                    TotalWorth = user.Money + user.ReservedMoney,
                    FullName = user.FullName,
                    PictureUrl = user.PictureUrl
                };

                usersWithTotalWorth.TotalWorth += GetSellingStocksWorth(user, stocks);

                foreach (var ownership in user.StockOwnerships)
                {
                    usersWithTotalWorth.TotalWorth += stocks.FirstOrDefault(s => s.Label == ownership.Label).LastTradePriceOnly * ownership.Quantity;
                }

                usersWithTotalWorth.GrowthPercent = Math.Round(((usersWithTotalWorth.TotalWorth / 100000 - 1) * 100), 2);

                model.AllUsers.Add(usersWithTotalWorth);

            }
            model.AllUsers = model.AllUsers.OrderByDescending(x => x.TotalWorth).ToList();
            return View(model);
        }

        private decimal GetSellingStocksWorth(ApplicationUser user, List<Stock> stocks)
        {
            var total = 0M;
            var sellingStocks = _gameOfStocksRepository.GetSellingStockTransactionsByUser(user);
            if (sellingStocks.Any())
                foreach (var sellingStock in sellingStocks)
                {
                    if (sellingStock.Label == null) continue;
                    total +=
                        stocks.FirstOrDefault(s => s.Label == sellingStock.Label).LastTradePriceOnly * sellingStock.Quantity;
                }

            return total;
        }


        [HttpGet]
        public ActionResult VisitProfile(string id)
        {
            var userId = User.Identity.Name;/* _userManager.GetUserId(HttpContext.User);*/
            var userToCheck = _gameOfStocksRepository.GetUserByEmail(userId);
            var userToDisplay = _gameOfStocksRepository.GetUserById(id);

            if (userToDisplay == null)
            {
                ModelState.AddModelError("email", "No user with that email");
                return View();
            }

            if (userToCheck.Id == id) return RedirectToAction("Profile");

            var users = _gameOfStocksRepository.GetAllUsers() as List<ApplicationUser>;
            var userMoneyHistory = _gameOfStocksRepository.GetUserMoneyHistory(userToDisplay).ToList();
            var stockTrans = GetStockTransWithTimeLeftByUser(userToDisplay);
            var ownerships = GetOwnershipsWithLastTradePriceByUser(userToDisplay);
            var usersWithTotal = _stockService.GetAllUsersWithTotalWorth(users).OrderByDescending(x => x.TotalWorth).ToList();
            var usersTotalWorthProgress = _stockService.GetUserTotalWorthProgress(userToDisplay, userMoneyHistory);

            var model = new ProfileViewModel
            {
                User = new UserModel
                {
                    Email = userToDisplay.Email,
                    Money = userToDisplay.Money,
                    TotalWorth = _stockService.GetUserTotalWorth(userToDisplay),
                    FullName = userToDisplay.FullName,
                    PictureUrl = userToDisplay.PictureUrl,
                },
                Position = usersWithTotal.IndexOf(usersWithTotal.First(u => u.Id == userToDisplay.Id)) + 1,
                PendingTransactions = stockTrans.Where(s => !s.IsCompleted && !s.IsFailed).ToList(),
                CompletedTransactions = stockTrans.Where(s => s.IsCompleted && !s.IsFailed).ToList(),
                FailedTransactions = stockTrans.Where(s => s.IsFailed).ToList(),
                StockOwnerships = ownerships,
                ProgressAllDays = usersTotalWorthProgress,
            };

            return View(model);
        }



        [HttpGet]
        public ActionResult Profile()
        {
            var userId = User.Identity.Name; /*_userManager.GetUserId(HttpContext.User);*/
            var user = _gameOfStocksRepository.GetUserByEmail(userId);

            if (user == null) return RedirectToAction("Login", "Account");

            var users = _gameOfStocksRepository.GetAllUsers().ToList();

            var userMoneyHistory = _gameOfStocksRepository.GetUserMoneyHistory(user).ToList();
            var stockTrans = GetStockTransWithTimeLeftByUser(user);
            var ownerships = GetOwnershipsWithLastTradePriceByUser(user);
            var usersWithTotal = _stockService.GetAllUsersWithTotalWorth(users).OrderByDescending(x => x.TotalWorth).ToList();
            var usersTotalWorthProgress = _stockService.GetUserTotalWorthProgress(user, userMoneyHistory);

            var model = new ProfileViewModel()
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
            var userId = User.Identity.Name; /*_userManager.GetUserId(HttpContext.User);*/
            var user = _gameOfStocksRepository.GetUserByEmail(userId);
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
            stockTrans = stockTrans.OrderByDescending(s => s.Date).ToList();
            return stockTrans;
        }

        private List<StockOwnershipModel> GetOwnershipsWithLastTradePriceByUser(ApplicationUser user)
        {
            var stocks = _stockService.GetStocks();
            var ownerships = new List<StockOwnershipModel>();
            foreach (var stockOwnership in _gameOfStocksRepository.GetStockOwnershipsByUser(user))
            {
                var lastTradePrice = stocks.FirstOrDefault(s => s.Label == stockOwnership.Label).LastTradePriceOnly;
                ownerships.Add(new StockOwnershipModel()
                {
                    Label = stockOwnership.Label,
                    Name = stockOwnership.Name,
                    Quantity = stockOwnership.Quantity,
                    Gav = stockOwnership.Gav,
                    TotalSum = stockOwnership.TotalSum,
                    LastTradePrice = lastTradePrice,
                    Growth = (lastTradePrice / stockOwnership.Gav - 1) * 100,
                    TotalWorth = stockOwnership.Quantity * lastTradePrice
                });
            }
            return ownerships;
        }



    }
}