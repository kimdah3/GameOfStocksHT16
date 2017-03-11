using GameOfStocksHT16.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.ActivityViewModels
{
    public class ActivityViewModel
    {
        public List<StockTransaction> MostRecentCompletedTransactions { get; set; }
        public List<Stock> AllStocks { get; set; }

    }
}
    