using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.StocksLogic
{
    public class Stock
    {
        public string Label { get; set; }
        public string Name { get; set; }
        public string Change { get; set; }
        public int Volume { get; set; }
        public decimal Open { get; set; }
        public decimal DaysLow { get; set; }
        public decimal DaysHigh { get; set; }
        public decimal LastTradePriceOnly { get; set; }
        public string LastTradeTime { get; set; }
        public string LastTradeDate { get; set; }
        public string Cap { get; set; }
    }
}
