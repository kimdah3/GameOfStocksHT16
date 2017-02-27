using GameOfStocksHT16.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.StocksViewModels
{
    public class StockViewModel
    {
        public Stock Stock { get; set; }
        [Display(Name = "Antal")]
        public decimal UsersMoney { get; set; }
        public bool UserHasStock { get; set; }
        [Display(Name = "Antal")]
        public int UsersQuantity { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal UsersGav { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal UsersStockTotalSum { get; set; }

    }
}
