using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.UsersViewModels
{
    public class UserModel
    {
        public string Id { get; set; }
        public string Email { get; set; }

        [Display(Name = "Saldo")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Money { get; set; }

        [Display(Name = "Reserverat")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal ReservedMoney { get; set; }

        [Display(Name = "Totalt")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal TotalWorth { get; set; }

        [Display(Name = "Namn")]
        public string FullName { get; set; }

        public string PictureUrl { get; set; }

        [Display(Name = "Utveckling")]
        public decimal GrowthPercent { get; set; }
        [Display(Name = "Utveckling/dag")]
        public decimal PercentPerDay { get; set; }

        public decimal HighestProgress { get; set; }
        public decimal HighestNegativProgress { get; set; }
        
    }
}

