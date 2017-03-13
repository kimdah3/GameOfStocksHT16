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
        private readonly IStockService _stockService;
        private readonly IGameOfStocksRepository _gameOfStocksRepository;

        public StockListController(IStockService stockService, IGameOfStocksRepository gameOfStocksRepository)
        {
            _stockService = stockService;
            _gameOfStocksRepository = gameOfStocksRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new StockListViewModel()
            {
                Stocks = _stockService.GetStocks()
            });
        }

        [HttpGet]
        [Authorize]
        public IActionResult Stock(string label)
        {
            var userId = User.Identity.Name;
            var user = _gameOfStocksRepository.GetUserByEmail(userId);

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
            }
            else
            {
                model.UserHasStock = false;
            }


            return View(model);
        }

        [HttpGet]
        public IActionResult StockListNew()
        {
            return View();
        }
    }
}