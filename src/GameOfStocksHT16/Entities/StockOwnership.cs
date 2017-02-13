using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Entities
{
    public class StockOwnership
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public int Quantity { get; set; }
        public decimal TotalSum { get; set; }
        public decimal Gav { get; set; }

        [Required]
        public ApplicationUser User { get; set; }
    }
}
