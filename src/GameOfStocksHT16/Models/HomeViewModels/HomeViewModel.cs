using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.HomeViewModels
{
    public class HomeViewModel
    {
        public List<UserInfoModel> CurrentLeaderBoard { get; set; }
        public List<UserInfoModel> TodaysLoosers { get; set; }
        public List<UserInfoModel> TodaysWinners { get; set; }
    }
}
