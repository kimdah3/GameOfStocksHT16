using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.HomeViewModels
{
    public class UserInfoModel
    {
        public string Email { get; set; }
        public decimal Money { get; set; }
        public decimal TotalWorth { get; set; }
        public decimal Percent { get; set; }
    }
}
