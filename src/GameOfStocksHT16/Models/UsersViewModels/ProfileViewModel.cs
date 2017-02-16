using GameOfStocksHT16.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.UsersViewModels
{
    public class ProfileViewModel
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Money { get; set; }     
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal PendingMoney { get; set;}
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal TotalWorth { get; set; }


        public List<StockTransWithTimeLeftViewModel> StockTransactions { get; set; }
        public List<StockOwnerShipViewModel> StockOwnerships { get; set; }

    }

    public class StockOwnerShipViewModel : StockOwnership
    {
        public decimal LastTradePrice { get; set; }
        public decimal Growth { get; set; }
    }

    public class StockTransWithTimeLeftViewModel : StockTransaction
    {
        public TimeSpan TimeLeftToCompletion { get; set; }
    }
}
