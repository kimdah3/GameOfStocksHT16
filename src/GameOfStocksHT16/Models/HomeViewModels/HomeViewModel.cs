using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameOfStocksHT16.Models.UsersViewModels;

namespace GameOfStocksHT16.Models.HomeViewModels
{
    public class HomeViewModel
    {
        public List<UserModel> CurrentLeaderBoard { get; set; }
        public List<UserPercentModel> TodaysLoosers { get; set; }
        public List<UserPercentModel> TodaysWinners { get; set; }
    }
}
