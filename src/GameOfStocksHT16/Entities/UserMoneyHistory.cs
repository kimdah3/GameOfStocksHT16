using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Entities
{
    public class UserMoneyHistory
    {
        public int Id { get; set; } 
        public decimal Money { get; set; }
        public DateTime Time { get; set; }
        [Required]
        public virtual ApplicationUser User { get; set; }
    }
}
