using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Services
{
    public interface IStockService
    {
        void CompleteStockTransactions(object o);

        void SaveStocksOnStartup(object o);

        Stock GetStockByLabel(string label);

        List<Stock> GetStocks();
    }
}
