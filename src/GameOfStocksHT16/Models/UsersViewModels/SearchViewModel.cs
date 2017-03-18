using GameOfStocksHT16.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.UsersViewModels
{
    public class SearchViewModel
    {
        public List<ApplicationUser> Result { get; set; }
        
        public SearchViewModel()
        {
            Result = new List<ApplicationUser>();
        }
    }
}
