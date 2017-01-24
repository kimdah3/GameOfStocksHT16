using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.StockTransactionsViewModels
{
    public class StockTransactionsViewModel
    {
        public List<StockTransactionViewModel> StockTransactions { get; set; }
    }

    public class StockTransactionViewModel : StockTransaction
    {
        public string UserEmail { get; set; }
    }
}
