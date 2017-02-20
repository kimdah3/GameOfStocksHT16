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
        [Display(Name = "Saldo")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Money { get; set; }
        [Display(Name = "Reserverat")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal ReservedMoney { get; set;}
        [Display(Name = "Totalt")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalWorth { get; set; }
        [Display(Name = "Utveckling")]
        public decimal GrowthPercent { get; set; }
        public JsonResult ProgressAllDays { get; set; }

        public List<StockTransWithTimeLeftViewModel> StockTransactions { get; set; }
        public List<StockOwnerShipViewModel> StockOwnerships { get; set; }


}

    public class StockOwnerShipViewModel : StockOwnership
    {
        [Display(Name = "Senaste handlingsvärde")]
        public decimal LastTradePrice { get; set; }
        [Display(Name = "Utveckling")]
        public decimal Growth { get; set; }
    }

    public class StockTransWithTimeLeftViewModel : StockTransaction
    {
        [Display(Name = "Tid kvar")]
        public TimeSpan TimeLeftToCompletion { get; set; }
    }
}
