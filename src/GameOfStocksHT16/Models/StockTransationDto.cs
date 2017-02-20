using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models
{
    public class StockTransationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalMoney { get; set; }
        public decimal Bid { get; set; }
        public bool IsBuying { get; set; }
        public bool IsSelling { get; set; }
        public bool IsCompleted { get; set; }
    }
}
