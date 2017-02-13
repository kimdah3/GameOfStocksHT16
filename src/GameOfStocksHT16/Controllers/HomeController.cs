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
            //var model = new AllUsersViewModel() { AllUsers = new List<User>() };
            var allUsers = new List<UserInfoModel>();

            var model = new HomeViewModel()
            {
                CurrentLeaderBoard = new List<UserInfoModel>(),
                TodaysLoosers =  new List<UserInfoModel>(),
                TodaysWinners = new List<UserInfoModel>()
            };

            foreach (var user in users)
            {
                var userWithTotalWorth = new UserInfoModel()
                {
                    Email = user.Email,
                    Money = user.Money,
                    TotalWorth = user.Money
                };

                foreach (var stockOwnership in user.StockOwnerships)
                {
                    userWithTotalWorth.TotalWorth += _stockService.GetStockByLabel(stockOwnership.Label).LastTradePriceOnly *stockOwnership.Quantity;
                }

                allUsers.Add(userWithTotalWorth);
            }

            model.CurrentLeaderBoard = allUsers.OrderByDescending(u => u.TotalWorth).ToList().GetRange(0,5);

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
