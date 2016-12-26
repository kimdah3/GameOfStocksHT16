using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models
{
    public class StockOwnership
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public DateTime DateBought { get; set; }
        public int Quantity { get; set; }
        public decimal Ask { get; set; }

        [Required]
        public ApplicationUser User { get; set; }
    }
}
