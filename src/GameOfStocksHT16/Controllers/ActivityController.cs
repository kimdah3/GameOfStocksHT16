using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GameOfStocksHT16.Services;
using GameOfStocksHT16.Entities;
using GameOfStocksHT16.Models;
using GameOfStocksHT16.Models.ActivityViewModels;

namespace GameOfStocksHT16.Controllers
{
    public class ActivityController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStockService _stockService;
        private readonly IGameOfStocksRepository _gameOfStocksRepository;

        public ActivityController(UserManager<ApplicationUser> userManager, IStockService stockService, IGameOfStocksRepository gameOfStocksRepository)
        {
            _userManager = userManager;
            _stockService = stockService;
            _gameOfStocksRepository = gameOfStocksRepository;
        }

        public IActionResult Index()
        {
            var transactions = _gameOfStocksRepository.GetAllStockTransactionsSortedByDate();
            var model = new ActivityModel(transactions);
            return View(model);
        }
    }
}