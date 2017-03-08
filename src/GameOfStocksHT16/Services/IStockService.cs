using GameOfStocksHT16.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GameOfStocksHT16.Entities;
using GameOfStocksHT16.Models.UsersViewModels;

namespace GameOfStocksHT16.Services
{
    public interface IStockService
    {
        void CompleteStockTransactions(object o);
        
        void SaveStocksOnStartup(object o);

        Stock GetStockByLabel(string label);
        List<Stock> GetStocks();

        void SaveUsersTotalEveryDay(object state);

        List<UserMoneyHistory> GetUserMoneyHistory(ApplicationUser user);
        List<UserModel> GetAllUsersWithTotalWorth();

        JsonResult GetUserTotalWorthProgress(ApplicationUser user);
        decimal GetUserTotalWorth(ApplicationUser user);

        List<UserPercentModel> GetUsersPercentToday(List<UserModel> allUsers);

        bool IsTradingTime();

        bool IsMarketOpenForStockTransactions();
    }
}
