using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameOfStocksHT16.Data;
using GameOfStocksHT16.Models.HomeViewModels;
using GameOfStocksHT16.Models.UsersViewModels;
using GameOfStocksHT16.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameOfStocksHT16.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStockService _stockService;
        private IGameOfStocksRepository _gameOfStocksRepository;

        public HomeController(ApplicationDbContext dbContext, IStockService stockService, IGameOfStocksRepository gameOfStocksRepository)
        {
            _stockService = stockService;
            _gameOfStocksRepository = gameOfStocksRepository;
        }

        public IActionResult Index()
        {
            var users = _gameOfStocksRepository.GetAllUsers().OrderByDescending(u => u.Money).ToList();
                //_dbContext.Users.Include(u => u.StockOwnerships).OrderByDescending(u => u.Money).ToList();
            var allUsers = new List<UserModel>();

            var model = new HomeViewModel()
            {
                CurrentLeaderBoard = new List<UserModel>(),
                TodaysLoosers = new List<UserModel>(),
                TodaysWinners = new List<UserModel>()
            };

            if (!users.Any()) return View(model);

            foreach (var user in users)
            {
                var userWithTotalWorth = new UserModel()
                {
                    Email = user.Email,
                    Money = user.Money,
                    TotalWorth = user.Money + user.ReservedMoney
                };

                foreach (var stockOwnership in user.StockOwnerships)
                {
                    userWithTotalWorth.TotalWorth += _stockService.GetStockByLabel(stockOwnership.Label).LastTradePriceOnly * stockOwnership.Quantity;
                    userWithTotalWorth.GrowthPercent = Math.Round(((userWithTotalWorth.TotalWorth / 100000 - 1) * 100), 2);
                }

                allUsers.Add(userWithTotalWorth);
            }

            model.CurrentLeaderBoard = allUsers.OrderByDescending(u => u.TotalWorth).Take(5).ToList();

            var usersTotalWorthPerDay = _stockService.GetUsersTotalWorthPerDay();
            var usersWithPercentPerDay = new List<UserModel>();

            foreach (var userWithTotal in usersTotalWorthPerDay)
            {
                if (allUsers.All(u => u.Email != userWithTotal.Email)) continue;

                usersWithPercentPerDay.Add(new UserModel()
                {
                    Email = userWithTotal.Email,
                    PercentPerDay = Math.Round(((allUsers.First(u => u.Email == userWithTotal.Email).TotalWorth / userWithTotal.TotalWorth - 1) * 100), 2)
                });
            }

            model.TodaysWinners = usersWithPercentPerDay.OrderByDescending(u => u.PercentPerDay).Take(5).ToList();
            model.TodaysLoosers = usersWithPercentPerDay.OrderBy(u => u.PercentPerDay).Take(5).ToList();

            return View(model);
        }

        [Authorize]
        public IActionResult News()
        {
            var users = _gameOfStocksRepository.GetAllUsers().OrderByDescending(u => u.Money);
                //_dbContext.Users.Include(u => u.StockOwnerships).OrderByDescending(u => u.Money).ToList();
            var allUsers = new List<UserModel>();

            var model = new HomeViewModel()
            {
                CurrentLeaderBoard = new List<UserModel>(),
                TodaysLoosers = new List<UserModel>(),
                TodaysWinners = new List<UserModel>()
            };

            if (!users.Any()) return View(model);

            foreach (var user in users)
            {
                var userWithTotalWorth = new UserModel()
                {
                    Email = user.Email,
                    Money = user.Money,
                    TotalWorth = user.Money + user.ReservedMoney
                };

                foreach (var stockOwnership in user.StockOwnerships)
                {
                    userWithTotalWorth.TotalWorth += _stockService.GetStockByLabel(stockOwnership.Label).LastTradePriceOnly * stockOwnership.Quantity;
                    userWithTotalWorth.GrowthPercent = Math.Round(((userWithTotalWorth.TotalWorth / 100000 - 1) * 100), 2);
                }

                allUsers.Add(userWithTotalWorth);
            }

            model.CurrentLeaderBoard = allUsers.OrderByDescending(u => u.TotalWorth).Take(5).ToList();

            var usersTotalWorthPerDay = _stockService.GetUsersTotalWorthPerDay();
            var usersWithPercentPerDay = new List<UserModel>();

            foreach (var userWithTotal in usersTotalWorthPerDay)
            {
                if (allUsers.All(u => u.Email != userWithTotal.Email)) continue;

                usersWithPercentPerDay.Add(new UserModel()
                {
                    Email = userWithTotal.Email,
                    PercentPerDay = Math.Round(((allUsers.First(u => u.Email == userWithTotal.Email).TotalWorth / userWithTotal.TotalWorth - 1) * 100), 2)
                });
            }

            model.TodaysWinners = usersWithPercentPerDay.OrderByDescending(u => u.PercentPerDay).Take(5).ToList();
            model.TodaysLoosers = usersWithPercentPerDay.OrderBy(u => u.PercentPerDay).Take(5).ToList();

            return View(model);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Rules()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
