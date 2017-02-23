using GameOfStocksHT16.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GameOfStocksHT16.Services
{
    public interface IStockService
    {
        void CompleteStockTransactions(object o);

        void SaveStocksOnStartup(object o);

        Stock GetStockByLabel(string label);

        List<Stock> GetStocks();

        void SaveUsersTotalWorthPerDay(object state);

        List<UserTotalWorth> GetUsersTotalWorthPerDay();

        JsonResult GetUserTotalWorthProgress(string email);

        bool IsTradingTime();

        bool IsTradingOpenForStockTransactions();

        bool DailyUsersTotalWorthExists();
    }
}
