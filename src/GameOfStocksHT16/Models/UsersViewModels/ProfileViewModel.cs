using GameOfStocksHT16.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.UsersViewModels
{
    public class ProfileViewModel
    {
        public UserModel User { get; set; }
        public JsonResult ProgressAllDays { get; set; }

        public int Position { get; set; }

        public List<StockTransactionModel> PendingTransactions { get; set; }
        public List<StockTransactionModel> CompletedTransactions { get; set; }
        public List<StockTransactionModel> FailedTransactions { get; set; }

        public List<StockOwnershipModel> StockOwnerships { get; set; }

    }
}
