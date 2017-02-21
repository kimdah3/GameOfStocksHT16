using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.UsersViewModels
{
    public class UserModel
    {
        public string Email { get; set; }
        [DisplayFormat(DataFormatString = "{0:n0}")]
        public decimal Money { get; set; }
        [DisplayFormat(DataFormatString = "{0:n0}")]
        public decimal TotalWorth { get; set; }

        public decimal GrowthPercent { get; set; }
        public decimal PercentPerDay { get; set; }
    }
}
