using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.UsersViewModels
{
    public class DisplayProfileViewModel
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public decimal Money { get; set; }
        public decimal TotalWorth { get; set; }

        public List<StockTransWithTimeLeftViewModel> StockTransactions { get; set; }
        public List<StockOwnerShipViewModel> StockOwnerships { get; set; }
        public List<FullStockOnwerShipViewModel> FullstockOwnerships { get; set; }
        public List<StockSold> StockSolds { get; set; }
    }

    public class StockOwnerShipViewModel : StockOwnership
    {
        public decimal LastTradePrice { get; set; }

    }

    public class FullStockOnwerShipViewModel
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public int Quantity { get; set; }
        public decimal LastTradePrice { get; set; }
    }

    public class StockTransWithTimeLeftViewModel : StockTransaction
    {
        public TimeSpan TimeLeftToCompletion { get; set; }
    }
}
