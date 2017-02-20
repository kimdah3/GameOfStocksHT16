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
        [Display(Name = "Namn")]
        public string Name { get; set; }
        [Display(Name = "Label")]
        public string Label { get; set; }
        [Display(Name = "Antal")]
        public int Quantity { get; set; }
        [Display(Name = "Totalt värde")]
        public decimal TotalSum { get; set; }
        [Display(Name = "GAV")]
        public decimal Gav { get; set; }

        [Required]
        public ApplicationUser User { get; set; }
    }
}
