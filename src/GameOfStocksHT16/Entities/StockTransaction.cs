using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Entities
{
    public class StockTransaction
    {
        public int Id { get; set; }
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
        public bool IsCompleted { get; set; }

        [Required]
        public ApplicationUser User { get; set; }
    }
}
