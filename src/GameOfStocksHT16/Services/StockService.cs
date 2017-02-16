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

namespace GameOfStocksHT16.Services
{
    public class StockService : IStockService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHostingEnvironment _hostingEnvironment;

        public StockService(ApplicationDbContext dbContext, IHostingEnvironment hostingEnvironment)
        {
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
        }

        public async void CompleteStockTransactions(object state)
        {
            var pendingStockTransactions = _dbContext.StockTransaction.Include(x => x.User).Where(x => !x.IsCompleted);
            if (!await pendingStockTransactions.AnyAsync()) { return; }
            var newOwnerships = new List<StockOwnership>();

            foreach (var transaction in pendingStockTransactions)
            {
                var newTime = transaction.Date + TimeSpan.FromMinutes(15);
                if (DateTime.Now < newTime) continue;
                var stockRecentValue = GetStockByLabel(transaction.Label); //Most recent values

                if (transaction.IsBuying)
                {
                    transaction.User.Money += transaction.TotalMoney;
                    transaction.User.ReservedMoney -= transaction.TotalMoney;

                    //Get recent value
                    transaction.Bid = stockRecentValue.LastTradePriceOnly;
                    transaction.TotalMoney = transaction.Bid * transaction.Quantity;

                    var existingStock = _dbContext.StockOwnership.FirstOrDefault(s => s.User == transaction.User && s.Label == transaction.Label);
                    var stockToModify = newOwnerships.FirstOrDefault(s => s.User == transaction.User && s.Label == transaction.Label);

                    if (existingStock != null)
                    {
                        existingStock.Quantity += transaction.Quantity;
                        existingStock.TotalSum += transaction.TotalMoney;
                        existingStock.Gav = existingStock.TotalSum / existingStock.Quantity;
                    }
                    else if (stockToModify != null)
                    {
                        newOwnerships.Remove(stockToModify);
                        stockToModify.Quantity += transaction.Quantity;
                        stockToModify.TotalSum += transaction.TotalMoney;
                        stockToModify.Gav = stockToModify.TotalSum / stockToModify.Quantity;
                        newOwnerships.Add(stockToModify);
                    }
                    else
                        newOwnerships.Add(new StockOwnership()
                        {
                            Name = transaction.Name,
                            Label = transaction.Label,
                            Quantity = transaction.Quantity,
                            Gav = transaction.TotalMoney / transaction.Quantity,
                            TotalSum = transaction.TotalMoney,
                            User = transaction.User
                        });
                    if (transaction.TotalMoney <= transaction.User.Money)
                        transaction.User.Money -= transaction.TotalMoney;
                    else
                    {
                        throw new Exception("User no money");
                    }
                }
                else if (transaction.IsSelling)
                {
                    transaction.User.Money += stockRecentValue.LastTradePriceOnly * transaction.Quantity;
                    transaction.Bid = stockRecentValue.LastTradePriceOnly;
                }
                transaction.IsCompleted = true;
            }

            _dbContext.StockOwnership.AddRange(newOwnerships);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine(ex.Entries.ToString());
            }
        }

        public async void SaveStocksOnStartup(object state)
        {
            var stockPath = Path.Combine(_hostingEnvironment.WebRootPath, "stocks.json");

            try
            {
                //Tid för börsstängning, ska vara 1800
                var currentTime = DateTime.Now.TimeOfDay;
                var morningOpen = new TimeSpan(09, 10, 0);
                var eveningClose = new TimeSpan(18, 00, 0);
                var isTradingTime = currentTime <= morningOpen || currentTime >= eveningClose;
                var url = Startup.Configuration["StockQueries:HT16LargeMidSmall"];
                //LARGE 0 - 105 
                //MID 106 - 225 (120 st)
                //SMALL 226 - 334 (109 st)

                //if (currentTime <= morningOpen || currentTime >= eveningClose) return;

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
                            if (rowNumber > 105 && rowNumber < 226)
                                cap = "MidCap";
                            else if (rowNumber > 225)
                                cap = "SmallCap";


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
            using (var r = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                var json = r.ReadToEnd();
                stocks = JsonConvert.DeserializeObject<List<Stock>>(json);
            }
            return stocks;
        }


        public Stock GetStockByLabel(string label)
        {
            var stocks = new List<Stock>();
            Stock stock;
            var webRootPath = _hostingEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "stocks.json");
            using (var r = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                var json = r.ReadToEnd();
                stocks = JsonConvert.DeserializeObject<List<Stock>>(json);
            }
            stock = stocks.Find(x => x.Label.ToLower() == label.ToLower());

            if (stock == null) throw new NullReferenceException("Stock not found in stocks.json");
            return stock;
        }

        public void SaveUsersTotalWorthPerDay(object state)
        {
            var today = DateTime.Today;
            var path = _hostingEnvironment.WebRootPath + @"\userdata\UsersTotalWorth " + today.ToString("M", CultureInfo.InvariantCulture) + ".json";
            var users = _dbContext.Users.Include(u => u.StockOwnerships).ToList();
            var allUsersTotalWorth = new List<UserTotalWorth>();

            foreach (var user in users)
            {
                var usersTotalWorth = new UserTotalWorth()
                {
                    Email = user.Email,
                    TotalWorth = user.Money + user.ReservedMoney
                };

                foreach (var stockOwnership in user.StockOwnerships)
                {
                    usersTotalWorth.TotalWorth += GetStockByLabel(stockOwnership.Label).LastTradePriceOnly * stockOwnership.Quantity;
                }

                allUsersTotalWorth.Add(usersTotalWorth);
            }

            using (var file = File.CreateText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, allUsersTotalWorth);
            }
        }

        public List<UserTotalWorth> GetUsersTotalWorthPerDay()
        {
            var usersTotalWorth = new List<UserTotalWorth>();

            var path = _hostingEnvironment.WebRootPath + @"\userdata\UsersTotalWorth " + DateTime.Today.ToString("M", CultureInfo.InvariantCulture) + ".json";
            try
            {
                using (var r = new StreamReader(new FileStream(path, FileMode.Open)))
                {
                    var json = r.ReadToEnd();
                    usersTotalWorth = JsonConvert.DeserializeObject<List<UserTotalWorth>>(json);
                }
            }
            catch (Exception ex)
            {
                WriteToDebug(DateTime.Now, "something went wrong reading daily UsersTotalWorth, " + ex.Message);
            }

            return usersTotalWorth;
        }

        public bool DailyUsersTotalWorthExists()
        {
            try
            {
                using (var r = new StreamReader(new FileStream(_hostingEnvironment.WebRootPath + @"\userdata\UsersTotalWorth " + DateTime.Today.ToString("M", CultureInfo.InvariantCulture) + ".json", FileMode.Open)))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
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
