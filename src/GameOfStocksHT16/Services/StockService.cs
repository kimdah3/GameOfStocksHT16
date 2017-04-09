using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CsvHelper;
using GameOfStocksHT16.Data;
using GameOfStocksHT16.Models;
using GameOfStocksHT16.Models.HomeViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using GameOfStocksHT16.Entities;
using GameOfStocksHT16.Models.UsersViewModels;
using Microsoft.Extensions.Logging;

namespace GameOfStocksHT16.Services
{
    public class StockService : IStockService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IGameOfStocksRepository _gameOfStocksRepository;
        private readonly ILogger _logger;

        public StockService(IHostingEnvironment hostingEnvironment, IGameOfStocksRepository gameOfStocksRepository, ILogger<StockService> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _gameOfStocksRepository = gameOfStocksRepository;
            _logger = logger;
        }

        public void CompleteStockTransactions(object state)
        {
            if (!IsTradingTime()) return;

            var users = _gameOfStocksRepository.GetUsersWithPendingStockTransactions();
            var newOwnerships = new List<StockOwnership>();

            foreach (var user in users)
            {
                foreach (var transaction in user.StockTransactions.Where(x => !x.IsCompleted && !x.IsFailed))
                {
                    ////Wait 15 min before completing transaction.
                    var newTime = transaction.Date + TimeSpan.FromMinutes(15);
                    if (DateTime.Now < newTime) continue;

                    if (transaction.IsBuying)
                    {
                        BuyStock(transaction, ref newOwnerships);
                    }
                    else if (transaction.IsSelling)
                    {
                        SellStock(transaction);
                    }
                }
                _gameOfStocksRepository.AddStockOwnerships(newOwnerships);
            }

            _gameOfStocksRepository.Save();

        }

        private void SellStock(StockTransaction transaction)
        {
            var stockRecentValue = GetStockByLabel(transaction.Label);

            //Lasttradeprice = 0 means somethings wrong with yahoo finance api
            if (stockRecentValue.LastTradePriceOnly == 0.0M) return;

            transaction.User.Money += stockRecentValue.LastTradePriceOnly * transaction.Quantity;
            transaction.Bid = stockRecentValue.LastTradePriceOnly;
            transaction.IsCompleted = true;
        }

        private void BuyStock(StockTransaction transaction, ref List<StockOwnership> newOwnerships)
        {
            //Most recent values
            var stockRecentValue = GetStockByLabel(transaction.Label);

            //If no recent buyer of stock, skip transaction
            if (!StockHasBuyer(stockRecentValue, transaction.Date)) return;

            //Lasttradeprice = 0 means somethings wrong with yahoo finance api
            if (stockRecentValue.LastTradePriceOnly == 0.0M) return;

            //Restore reserved money
            transaction.User.Money += transaction.TotalMoney;
            transaction.User.ReservedMoney -= transaction.TotalMoney;

            //Update transaction with in time values
            transaction.Bid = stockRecentValue.LastTradePriceOnly;
            transaction.TotalMoney = stockRecentValue.LastTradePriceOnly * transaction.Quantity;

            //If not enough money, skip transaction for now
            if (transaction.TotalMoney > transaction.User.Money)
            {
                transaction.IsFailed = true;
                WriteToDebug(DateTime.Now, $"Stocktransaction failed, ask: {transaction.TotalMoney}, users money: {transaction.User.Money}, user: {transaction.User.FullName}");
                return;
            }

            var existingOwnership = transaction.User.StockOwnerships.FirstOrDefault(s => s.Label == transaction.Label);
            var stockToModify = newOwnerships.FirstOrDefault(s => s.User == transaction.User && s.Label == transaction.Label);

            if (existingOwnership != null)
            {
                existingOwnership.Quantity += transaction.Quantity;
                existingOwnership.TotalSum += transaction.TotalMoney;
                existingOwnership.Gav = existingOwnership.TotalSum / existingOwnership.Quantity;
            }
            else if (stockToModify != null)
            {
                stockToModify.Quantity += transaction.Quantity;
                stockToModify.TotalSum += transaction.TotalMoney;
                stockToModify.Gav = stockToModify.TotalSum / stockToModify.Quantity;
            }
            else
            {
                newOwnerships.Add(new StockOwnership()
                {
                    Name = transaction.Name,
                    Label = transaction.Label,
                    Quantity = transaction.Quantity,
                    Gav = transaction.TotalMoney / transaction.Quantity,
                    TotalSum = transaction.TotalMoney,
                    User = transaction.User
                });
            }

            transaction.User.Money -= transaction.TotalMoney;
            transaction.IsCompleted = true;
        }


        private bool StockHasBuyer(Stock stockRecentValue, DateTime transactionTime)
        {
            try
            {
                var date = DateTime.ParseExact(stockRecentValue.LastTradeDate, "M/d/yyyy", CultureInfo.InvariantCulture);
                var time = DateTime.Parse(stockRecentValue.LastTradeTime);
                return date.Add(time.TimeOfDay) > transactionTime;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async void SaveStocksOnStartup(object state)
        {
            var stockPath = Path.Combine(_hostingEnvironment.WebRootPath, "stocks.json");

            try
            {
                var url = Startup.Configuration["StockQueries:HT16LargeMidSmall"];
                //LARGE 0 - 104 
                //MID 105 - 223 (120 st)
                //SMALL 224 - 332 (109 st)

                //if (!IsMarketOpenForStockUpdates()) return;

                var stockList = new List<Stock>();

                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";

                using (var response = (HttpWebResponse)(await Task<WebResponse>.Factory.FromAsync(req.BeginGetResponse, req.EndGetResponse, null)))
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        var csvReader = new CsvReader(streamReader);
                        csvReader.Configuration.HasHeaderRecord = false;    //Reads first row, skips it if this is false!

                        while (csvReader.Read())
                        {
                            var rowNumber = csvReader.Row;
                            var cap = "LargeCap";

                            // Kollar om aktien är large - mid - small cap
                            if (rowNumber > 104 && rowNumber < 224)
                                cap = "MidCap";
                            else if (rowNumber >= 224 && rowNumber < 332)
                                cap = "SmallCap";
                            else if (rowNumber >= 333)
                                cap = "FirstNorth";


                            var fields = csvReader.CurrentRecord;
                            Stock stock;

                            var lastTradeDate = fields[0];
                            var daysLow = fields[1];
                            var daysHigh = fields[2];
                            var lastTradePriceOnly = fields[3];
                            var name = fields[4];
                            var open = fields[5];
                            var change = fields[6];
                            var symbol = fields[7];
                            var lastTradeTime = fields[8];
                            var volume = fields[9];

                            if (lastTradePriceOnly == null)
                                lastTradePriceOnly = open;

                            if (open.Equals("N/A"))
                                open = "0";

                            if (daysLow.Equals("N/A"))
                                daysLow = open;

                            if (daysHigh.Equals("N/A"))
                                daysHigh = open;

                            if (name.Equals("N/A"))
                            {
                                stock = new Stock()
                                {
                                    Label = symbol,
                                    Name = "",
                                    Volume = 0,
                                    Change = change,
                                    Open = 0,
                                    DaysLow = 0,
                                    DaysHigh = 0,
                                    LastTradeTime = lastTradeTime,
                                    LastTradePriceOnly = 0,
                                    LastTradeDate = lastTradeDate,
                                    Cap = cap
                                };
                            }
                            else
                            {
                                stock = new Stock()
                                {
                                    Label = symbol,
                                    Name = name,
                                    Volume = int.Parse(volume),
                                    Change = change,
                                    Open = decimal.Parse(open, CultureInfo.InvariantCulture),
                                    DaysLow = decimal.Parse(daysLow, CultureInfo.InvariantCulture),
                                    DaysHigh = decimal.Parse(daysHigh, CultureInfo.InvariantCulture),
                                    LastTradeTime = lastTradeTime,
                                    LastTradePriceOnly = decimal.Parse(lastTradePriceOnly, CultureInfo.InvariantCulture),
                                    LastTradeDate = lastTradeDate,
                                    Cap = cap
                                };
                            }
                            stockList.Add(stock);
                        }
                    }
                }

                SerializeToJson(stockPath, stockList);
                WriteToDebug(DateTime.Now, "sucess, stockList.Count = " + stockList.Count);
            }
            catch (Exception exception)
            {
                WriteToDebug(DateTime.Now, "something went wrong while downloading stocks, " + exception.Message);

            }
        }

        public List<Stock> GetStocks()
        {
            var stocks = new List<Stock>();
            var webRootPath = _hostingEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "stocks.json");
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var r = new StreamReader(fileStream))
                    {
                        var json = r.ReadToEnd();
                        stocks = JsonConvert.DeserializeObject<List<Stock>>(json);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Något gick fel vid hämtning av aktier: " + e.Message);
                return stocks;
            }

            return stocks;
        }

        public Stock GetStockByLabel(string label)
        {
            Stock stock;
            var stocks = new List<Stock>();
            var webRootPath = _hostingEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "stocks.json");
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var r = new StreamReader(fileStream))
                    {
                        var json = r.ReadToEnd();
                        stocks = JsonConvert.DeserializeObject<List<Stock>>(json);
                    }
                }
                stock = stocks.Find(x => x.Label.ToLower() == label.ToLower());
                if (stock == null) throw new NullReferenceException("Stock not found in stocks.json");

                return stock;
            }
            catch (Exception e)
            {
                _logger.LogError("Something went wrong finding a certain stock (" + label + "): " + e.Message);
                return new Stock();
            }

        }

        //New
        public JsonResult GetUserTotalWorthProgress(ApplicationUser user, List<UserMoneyHistory> userMoneyHistory)
        {
            var userTotalWorthProgress = new List<decimal>();
            try
            {
                foreach (var entity in userMoneyHistory)
                {
                    userTotalWorthProgress.Add(Math.Round(((entity.Money / 100000 - 1) * 100), 2));
                }
                return new JsonResult(userTotalWorthProgress);
            }
            catch (Exception)
            {
                return new JsonResult(userTotalWorthProgress);
            }
        }

        public decimal GetHighestDailyProgress(ApplicationUser user, List<UserMoneyHistory> userMoneyHistory)
        {
            var userTotalWorthProgress = new List<decimal>();
            var highestProgress = 0M;

            try
            {
                foreach (var entity in userMoneyHistory)
                {
                    userTotalWorthProgress.Add(Math.Round(((entity.Money / 100000 - 1) * 100), 2));
                }

                for (int i = 0; i < userTotalWorthProgress.Count; i++)
                {
                    var dayBefore = 0M;

                    if (i != 0)
                    {
                        dayBefore = userTotalWorthProgress[i - 1];
                    }

                    var daily = userTotalWorthProgress[i] - dayBefore;

                    if (daily > highestProgress)
                    {
                        highestProgress = daily;
                    }
                }


                return highestProgress;
            }
            catch (Exception)
            {
                return 0M;
            }
        }
        public decimal GetHighestDailyNegativeProgress(ApplicationUser user, List<UserMoneyHistory> userMoneyHistory)
        {
            var userTotalWorthProgress = new List<decimal>();
            var lowestProgress = 0M;

            try
            {
                foreach (var entity in userMoneyHistory)
                {
                    userTotalWorthProgress.Add(Math.Round(((entity.Money / 100000 - 1) * 100), 2));
                }

                for (int i = 0; i < userTotalWorthProgress.Count; i++)
                {
                    var dayBefore = 0M;

                    if (i != 0)
                    {
                        dayBefore = userTotalWorthProgress[i - 1];
                    }

                    var daily = userTotalWorthProgress[i] - dayBefore;

                    if (daily < lowestProgress)
                    {
                        lowestProgress = daily;
                    }
                }


                return lowestProgress;
            }
            catch (Exception)
            {
                return 0M;
            }
        }

        public decimal GetUserTotalWorth(ApplicationUser user)
        {
            var totalWorth = user.Money + user.ReservedMoney;

            foreach (var s in user.StockOwnerships)
            {
                totalWorth += s.Quantity * GetStockByLabel(s.Label).LastTradePriceOnly;
            }

            foreach (var s in user.StockTransactions.Where(s => s.IsSelling && !s.IsCompleted))
            {
                totalWorth += s.Quantity * GetStockByLabel(s.Label).LastTradePriceOnly;
            }

            return totalWorth;
        }

        public List<UserModel> GetAllUsersWithTotalWorth(List<ApplicationUser> users)
        {
            var allUsersWithTotalWorth = new List<UserModel>();

            foreach (var user in users)
            {
                var userWithTotalWorth = new UserModel()
                {
                    Email = user.Email,
                    Money = user.Money,
                    TotalWorth = user.Money + user.ReservedMoney,
                    FullName = user.FullName,
                    GrowthPercent = 0,
                    Id = user.Id,
                    PictureUrl = user.PictureUrl
                };

                userWithTotalWorth.TotalWorth += GetTotalWorthFromStockOwnershipsByUser(user);
                userWithTotalWorth.TotalWorth += GetTotalWorthFromSellingStockTransactionsByUser(user);
                userWithTotalWorth.GrowthPercent = Math.Round(((userWithTotalWorth.TotalWorth / 100000 - 1) * 100), 2);
                //userWithTotalWorth.PercentPerDay = GetUserPercentToday(user);

                allUsersWithTotalWorth.Add(userWithTotalWorth);
            }

            return allUsersWithTotalWorth;
        }



        private decimal GetTotalWorthFromStockOwnershipsByUser(ApplicationUser user)
        {
            var total = 0M;
            var ownerships = user.StockOwnerships;

            if (ownerships.Any())
            {
                foreach (var ownership in ownerships)
                {
                    total += GetStockByLabel(ownership.Label).LastTradePriceOnly * ownership.Quantity;
                }
            }

            return total;
        }

        private decimal GetTotalWorthFromSellingStockTransactionsByUser(ApplicationUser user)
        {
            var total = 0M;
            var transactions = user.StockTransactions.Where(x => x.IsSelling && !x.IsCompleted && !x.IsFailed);

            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    total += GetStockByLabel(transaction.Label).LastTradePriceOnly * transaction.Quantity;
                }
            }

            return total;
        }

        public void SaveUsersTotalEveryDay(object state)
        {
            var moneyList = new List<UserMoneyHistory>();
            var usersList = _gameOfStocksRepository.GetAllUsers();

            foreach (var user in usersList)
            {
                var userDailyWorth = new UserMoneyHistory()
                {
                    Money = GetUserTotalWorth(user),
                    User = user,
                    Time = DateTime.Today
                };

                moneyList.Add(userDailyWorth);
            }
            _gameOfStocksRepository.SaveUsersHistory(moneyList);
        }



        public List<UserPercentModel> GetUsersPercentToday(List<UserModel> allUsers, List<UserMoneyHistory> usersHistory)
        {
            var usersWithPercent = new List<UserPercentModel>();
            foreach (var userMoneyHistory in usersHistory)
            {
                usersWithPercent.Add(new UserPercentModel()
                {
                    FullName = userMoneyHistory.User.FullName,
                    PercentPerDay = Math.Round(((allUsers.First(u => u.Email == userMoneyHistory.User.Email).TotalWorth / userMoneyHistory.Money - 1) * 100), 2),
                    Id = userMoneyHistory.User.Id
                });
            }

            return usersWithPercent;
        }



        //Tid för börsstängning, ska vara 1747.
        //Och inte helgdag, därav !IsWeekDay()
        public bool IsTradingTime()
        {
            var currentTime = DateTime.Now.TimeOfDay;
            var morningOpen = new TimeSpan(09, 15, 0);
            var eveningClose = new TimeSpan(17, 47, 0);
            return currentTime >= morningOpen && currentTime <= eveningClose && IsWeekDay();
        }

        public bool IsMarketOpenForStockTransactions()
        {
            var currentTime = DateTime.Now.TimeOfDay;
            var morningOpen = new TimeSpan(09, 00, 0);
            var eveningClose = new TimeSpan(17, 30, 0);
            return currentTime >= morningOpen && currentTime <= eveningClose && IsWeekDay();
        }

        public bool IsMarketOpenForStockUpdates()
        {
            var currentTime = DateTime.Now.TimeOfDay;
            var morningOpen = new TimeSpan(09, 00, 0);
            var eveningClose = new TimeSpan(17, 45, 0);
            return currentTime >= morningOpen && currentTime <= eveningClose && IsWeekDay();
        }

        private bool IsWeekDay()
        {
            var today = DateTime.Now;
            var day = (int)today.DayOfWeek;
            return day > 0 && day < 6;
        }

        private void SerializeToJson(string path, List<Stock> stockList)
        {
            using (var file = File.CreateText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, stockList);
            }
        }

        private void WriteToDebug(DateTime date, string message)
        {
            var debugPath = Path.Combine(_hostingEnvironment.WebRootPath, "debug.txt");
            using (var sw = File.AppendText(debugPath))
            {
                sw.Write(date.ToString("U") + " " + message + Environment.NewLine);
            }
        }
    }
}
