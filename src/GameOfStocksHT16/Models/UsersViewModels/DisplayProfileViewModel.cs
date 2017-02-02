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
        public List<StockOwnershipWithLastTradePrice> StockOwnerships { get; set; }
        public List<StockSold> StockSolds { get; set; }
    }

    public class StockOwnershipWithLastTradePrice : StockOwnership
    {
        public decimal LastTradePrice { get; set; }

    }

    public class StockTransWithTimeLeftViewModel : StockTransaction
    {
        public TimeSpan TimeLeftToCompletion { get; set; }
    }
}
