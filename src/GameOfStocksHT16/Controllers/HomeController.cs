using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameOfStocksHT16.Logic;
using Microsoft.AspNetCore.Mvc;

namespace GameOfStocksHT16.Controllers
{
    public class HomeController : Controller
    {
        public ViewResult Index()
        {

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
