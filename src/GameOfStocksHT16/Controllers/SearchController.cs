using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GameOfStocksHT16.Services;
using Microsoft.AspNetCore.Identity;
using GameOfStocksHT16.Entities;
using GameOfStocksHT16.Models.UsersViewModels;

namespace GameOfStocksHT16.Controllers
{
    public class SearchController : Controller
    {
        private readonly IStockService _stockService;
        private readonly IGameOfStocksRepository _gameOfStocksRepository;

        public SearchController(UserManager<ApplicationUser> userManager, IStockService stockService, IGameOfStocksRepository gameOfStocksRepository)
        {
            _stockService = stockService;
            _gameOfStocksRepository = gameOfStocksRepository;
        }

        public IActionResult Result(string searchString)
        {
            var result = _gameOfStocksRepository.GetSearchResult(searchString);
            var model = new SearchViewModel();
            if(result != null && result.Count() > 0)
            {
                foreach(var user in result)
                {
                    model.Result.Add(user);
                }
            }
            return View(model);
        }
    }
}