using GameOfStocksHT16.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.ActivityViewModels
{
    public class ActivityModel
    {
        public List<StockTransaction> Activities { get; set; }
        public List<Stock> Stocks { get; set; }

        public ActivityModel(List<StockTransaction> incomingTransactions, List<Stock> AllStocks)
        {
            Activities = incomingTransactions;
            Stocks = AllStocks;
        }
    }
}
    