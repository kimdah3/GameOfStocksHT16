using GameOfStocksHT16.Entities;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace GameOfStocksHT16.Models.UsersViewModels
{
    public class ProfileViewModel
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Money { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal PendingMoney { get; set;}
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalWorth { get; set; }
        public decimal GrowthPercent { get; set; }
        public JsonResult ProgressAllDays { get; set; }

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
