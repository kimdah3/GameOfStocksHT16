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
using Microsoft.AspNetCore.Identity;
using GameOfStocksHT16.Entities;
using GameOfStocksHT16.Models;

namespace GameOfStocksHT16.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStockService _stockService;
        private readonly IGameOfStocksRepository _gameOfStocksRepository;

        public HomeController(ApplicationDbContext dbContext, IStockService stockService, IGameOfStocksRepository gameOfStocksRepository)
        {
            _stockService = stockService;
            _gameOfStocksRepository = gameOfStocksRepository;
        }

        public IActionResult Index()
        {
            var model = new HomeViewModel()
            {
                CurrentLeaderBoard = new List<UserModel>(),
                TodaysLoosers = new List<UserPercentModel>(),
                TodaysWinners = new List<UserPercentModel>()
            };

            if (!_gameOfStocksRepository.UsersExists()) return View(model);

            var allUsersWithTotalWorth = _stockService.GetAllUsersWithTotalWorth();
            model.CurrentLeaderBoard = allUsersWithTotalWorth.OrderByDescending(u => u.TotalWorth).Take(5).ToList();

            var usersWithPercentPerDay = _stockService.GetUsersPercentToday(allUsersWithTotalWorth);
            model.TodaysWinners = usersWithPercentPerDay.OrderByDescending(u => u.PercentPerDay).Take(5).ToList();
            model.TodaysLoosers = usersWithPercentPerDay.OrderBy(u => u.PercentPerDay).Take(5).ToList();

            return View(model);
        }

        [Authorize]
        public IActionResult News()
        {
            var model = new HomeViewModel()
            {
                CurrentLeaderBoard = new List<UserModel>(),
                TodaysLoosers = new List<UserPercentModel>(),
                TodaysWinners = new List<UserPercentModel>()
            };

            if(!_gameOfStocksRepository.UsersExists()) return View(model);

            var allUsersWithTotalWorth = _stockService.GetAllUsersWithTotalWorth();
            model.CurrentLeaderBoard = allUsersWithTotalWorth.OrderByDescending(u => u.TotalWorth).Take(5).ToList();

            var usersWithPercentToday = _stockService.GetUsersPercentToday(allUsersWithTotalWorth);
            model.TodaysWinners = usersWithPercentToday.OrderByDescending(u => u.PercentPerDay).Take(5).ToList();
            model.TodaysLoosers = usersWithPercentToday.OrderBy(u => u.PercentPerDay).Take(5).ToList();

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
