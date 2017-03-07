using GameOfStocksHT16.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GameOfStocksHT16.Entities;

namespace GameOfStocksHT16.Services
{
    public interface IStockService
    {
        void CompleteStockTransactions(object o);
        
        void SaveStocksOnStartup(object o);

        Stock GetStockByLabel(string label);

        List<Stock> GetStocks();

        //Old
        void SaveUsersTotalWorthPerDay(object state);
        //New
        void SaveUsersTotalEveryDay(object state);

        //Old
        List<UserTotalWorth> GetUsersTotalWorthPerDay();
        //New
        List<UserMoneyHistory> GetUserMoneyHistory(ApplicationUser user);

        JsonResult GetUserTotalWorthProgress(string email);
        //TEST
        JsonResult GetUserTotalWorthProgressNew(ApplicationUser user);

        bool IsTradingTime();

        bool IsMarketOpenForStockTransactions();

        bool DailyUsersTotalWorthExists();
    }
}
