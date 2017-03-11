using GameOfStocksHT16.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models
{
    public class StockTransactionModel
    {
        [Display(Name = "Namn")]
        public string Name { get; set; }

        [Display(Name = "Label")]
        public string Label { get; set; }

        [Display(Name = "Antal")]
        public int Quantity { get; set; }

        [Display(Name = "Datum")]
        public DateTime Date { get; set; }

        [Display(Name = "Totalt värde")]
        public decimal TotalMoney { get; set; }

        public decimal Bid { get; set; }

        [Display(Name = "Köp")]
        public bool IsBuying { get; set; }

        [Display(Name = "Sälj")]
        public bool IsSelling { get; set; }

        [Display(Name = "Klar")]
        public bool IsCompleted { get; set; }

        [Display(Name = "Misslyckad")]
        public bool IsFailed { get; set; }

        [Display(Name = "Tid kvar")]
        public TimeSpan TimeLeftToCompletion { get; set; }
    }
}
