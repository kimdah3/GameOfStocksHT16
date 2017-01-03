using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using GameOfStocksHT16.Services;

namespace GameOfStocksHT16.Controllers
{
    [Produces("application/json")]
    [Route("api/Stocks")]
    public class StocksController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IStockService _stockService;


        public StocksController(IHostingEnvironment hostingEnvironment, IStockService stockService)
        {
            _hostingEnvironment = hostingEnvironment;
            _stockService = stockService;
        }


        // GET: api/Stocks
        [HttpGet]
        public IEnumerable<Stock> Get()
        {
            var stocks = new List<Stock>();
            var webRootPath = _hostingEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "stocks.json");
            using (var r = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                var json = r.ReadToEnd();
                stocks = JsonConvert.DeserializeObject<List<Stock>>(json);
            }
            return stocks;
        }

        [HttpGet("{id}")]
        public Stock GetById(string label)
        {
            return _stockService.GetStockByLabel(label);
        }

    }
}
