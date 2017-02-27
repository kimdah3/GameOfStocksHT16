using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GameOfStocksHT16.Services;
using GameOfStocksHT16.Models.StocksViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GameOfStocksHT16.Entities;

namespace GameOfStocksHT16.Controllers
{
    public class StockListController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStockService _stockService;
        private readonly IGameOfStocksRepository _gameOfStocksRepository;

        public StockListController(UserManager<ApplicationUser> userManager, IStockService stockService, IGameOfStocksRepository gameOfStocksRepository)
        {
            _userManager = userManager;
            _stockService = stockService;
            _gameOfStocksRepository = gameOfStocksRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult Stock(string label)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = _gameOfStocksRepository.GetUserById(userId);

            var model = new StockViewModel
            {
                Stock = _stockService.GetStockByLabel(label),
                UsersMoney = user.Money,
            };

            var ownership = _gameOfStocksRepository.GetStockOwnershipByUserAndLabel(user, label);

            if (ownership != null)
            {
                model.UserHasStock = true;
                model.UsersQuantity = ownership.Quantity;
                model.UsersStockTotalSum = ownership.TotalSum;
                model.UsersGav = ownership.Gav;
            }else
            {
                model.UserHasStock = false;
            }


            return View(model);
        }

        [HttpGet]
        public IActionResult StockListNew()
        {
            return View(new StockListViewModel()
            {
                Stocks = _stockService.GetStocks()
            });
        }
    }
}