using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models
{
    public class StockSold
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public DateTime DateSold { get; set; }
        public decimal LastTradePrice { get; set; }
        public int Quantity { get; set; }

        [Required]
        public ApplicationUser User { get; set; }

    }
}
