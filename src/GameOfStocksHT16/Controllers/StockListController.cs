using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GameOfStocksHT16.Controllers
{
    public class StockListController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}