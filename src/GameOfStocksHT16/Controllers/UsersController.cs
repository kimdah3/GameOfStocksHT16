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

        public UsersController(UserManager<ApplicationUser> userManager, IStockService stockService,  IGameOfStocksRepository gameOfStocksRepository)
        {
            _userManager = userManager;
            _stockService = stockService;
            _gameOfStocksRepository = gameOfStocksRepository;
        }

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
                    TotalWorth = user.Money + user.ReservedMoney,
                    FullName = user.FullName,
                    PictureUrl = user.PictureUrl
                };

                usersWithTotalWorth.TotalWorth += GetSellingStocksWorth(user);

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

        private decimal GetSellingStocksWorth(ApplicationUser user)
        {
            var total = 0M;
            var sellingStocks = _gameOfStocksRepository.GetSellingStockTransactionsByUser(user);
            if (sellingStocks.Any())
                foreach (var sellingStock in sellingStocks)
                {
                    total +=
                        _stockService.GetStockByLabel(sellingStock.Label).LastTradePriceOnly * sellingStock.Quantity;
                }

            return total;
        }

        [HttpGet]
        public ActionResult VisitProfile(string email)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var userToCheck = _gameOfStocksRepository.GetUserById(userId);
            var userToDisplay = _gameOfStocksRepository.GetUserByEmail(email);

            if (userToDisplay == null)
            {
                ModelState.AddModelError("email", "No user with that email");
                return View();
            }
            else if (userToCheck.Email == email)
            {
                return RedirectToAction("DisplayProfile");
            }
            else
            {
                var model = new ProfileViewModel
                {
                    Email = userToDisplay.Email,
                    Money = userToDisplay.Money,
                    StockOwnerships = GetOwnershipsWithLastTradePriceByUser(userToDisplay),
                    StockTransactions = GetStockTransWithTimeLeft(userToDisplay),
                    TotalWorth = userToDisplay.Money + userToDisplay.ReservedMoney,
                    FullName = userToDisplay.FullName,
                    PictureUrl = userToDisplay.PictureUrl,
                    ProgressAllDays = _stockService.GetUserTotalWorthProgressNew(userToDisplay)
                };

                foreach (var s in model.StockOwnerships)
                {
                    model.TotalWorth += (s.Quantity * s.LastTradePrice);
                }

                model.TotalWorth += GetSellingStocksWorth(userToDisplay);

                return View(model);
            }
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
                    TotalWorth = user.Money + user.ReservedMoney,
                    StockTransactions = GetStockTransWithTimeLeft(user),
                    StockOwnerships = GetOwnershipsWithLastTradePriceByUser(user),
                    ProgressAllDays = _stockService.GetUserTotalWorthProgressNew(user),
                    FullName = user.FullName,
                    PictureUrl = user.PictureUrl
                };

                foreach (var s in model.StockOwnerships)
                {
                    model.TotalWorth = model.TotalWorth+ (s.Quantity * s.LastTradePrice);
                }

                model.TotalWorth += GetSellingStocksWorth(user);

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
                    Growth = (lastTradePrice/s.Gav -1)*100,
                    User = s.User
                });
            }
            return ownerships;
        }
    }
}