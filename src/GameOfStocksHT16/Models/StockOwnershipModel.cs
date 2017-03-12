using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models
{
    public class StockOwnershipModel
    {
        [Display(Name = "Namn")]
        public string Name { get; set; }
        [Display(Name = "Label")]
        public string Label { get; set; }
        [Display(Name = "Antal")]
        public int Quantity { get; set; }
        [Display(Name = "Totalt anskaffningsvärde")]
        public decimal TotalSum { get; set; }
        [Display(Name = "GAV")]
        public decimal Gav { get; set; }

        [Display(Name = "Senaste handlingsvärde")]
        public decimal LastTradePrice { get; set; }
        [Display(Name = "Utveckling")]
        public decimal Growth { get; set; }
        [Display(Name = "Totalt värde")]
        public decimal TotalWorth { get; set; }
    }
}
