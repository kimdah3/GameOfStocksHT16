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
        public ActionResult Leaderboard()
        {
            if (!_gameOfStocksRepository.UsersExists()) return View();

            var model = new LeaderBoardViewModel()
            {
                AllUsers = _stockService.GetAllUsersWithTotalWorth().OrderByDescending(x => x.TotalWorth).ToList()
            };

            return View(model);
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

            var stockTrans = GetStockTransWithTimeLeftByUser(userToDisplay);
            var ownerships = GetOwnershipsWithLastTradePriceByUser(userToDisplay);
            var usersWithTotal = _stockService.GetAllUsersWithTotalWorth().OrderByDescending(x => x.TotalWorth).ToList();
            var usersTotalWorthProgress = _stockService.GetUserTotalWorthProgress(userToDisplay);

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

            var stockTrans = GetStockTransWithTimeLeftByUser(user);
            var ownerships = GetOwnershipsWithLastTradePriceByUser(user);
            var usersWithTotal = _stockService.GetAllUsersWithTotalWorth().OrderByDescending(x => x.TotalWorth).ToList();
            var usersTotalWorthProgress = _stockService.GetUserTotalWorthProgress(user);

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
                    TotalWorth = s.Quantity * lastTradePrice
                });
            }
            return ownerships;
        }



    }
}