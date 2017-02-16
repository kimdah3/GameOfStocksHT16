using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameOfStocksHT16.Data;
using GameOfStocksHT16.Models.HomeViewModels;
using GameOfStocksHT16.Models.UsersViewModels;
using GameOfStocksHT16.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameOfStocksHT16.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _dbContext { get; set; }
        private readonly IStockService _stockService;

        public HomeController(ApplicationDbContext dbContext, IStockService stockService)
        {
            _dbContext = dbContext;
            _stockService = stockService;
        }

        public IActionResult Index()
        {
            var users = _dbContext.Users.Include(u => u.StockOwnerships).OrderByDescending(u => u.Money).ToList();
            var allUsers = new List<UserInfoModel>();

            var model = new HomeViewModel()
            {
                CurrentLeaderBoard = new List<UserInfoModel>(),
                TodaysLoosers = new List<UserInfoModel>(),
                TodaysWinners = new List<UserInfoModel>()
            };

            if (users == null) return View(model);

            foreach (var user in users)
            {
                var userWithTotalWorth = new UserInfoModel()
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
            var usersWithPercentPerDay = new List<UserInfoModel>();

            foreach (var userWithTotal in usersTotalWorthPerDay)
            {
                if (allUsers.All(u => u.Email != userWithTotal.Email)) continue;

                usersWithPercentPerDay.Add(new UserInfoModel()
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

        public IActionResult Error()
        {
            return View();
        }
    }
}
