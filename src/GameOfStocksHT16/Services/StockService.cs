﻿using System;
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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
            var newSoldStocks = new List<StockSold>();

            foreach (var transaction in pendingStockTransactions)
            {

                if (transaction.IsBuying)
                {
                    newOwnerships.Add(new StockOwnership()
                    {
                        Name = transaction.Name,
                        Label = transaction.Label,
                        DateBought = DateTime.Now,
                        Quantity = transaction.Quantity,
                        Ask = GetStockByLabel(transaction.Label).LastTradePriceOnly,
                        User = transaction.User
                    });
                }
                else if (transaction.IsSelling)
                {
                    var lastTradePrice = GetStockByLabel(transaction.Label).LastTradePriceOnly;
                    newSoldStocks.Add(new StockSold()
                    {
                        Name = transaction.Name,
                        Label = transaction.Label,
                        DateSold = DateTime.Now,
                        Quantity = transaction.Quantity,
                        LastTradePrice = lastTradePrice,
                        User = transaction.User
                    });
                    var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == transaction.User.Id);
                    user.Money += lastTradePrice * transaction.Quantity;
                }
                _dbContext.StockTransaction.FirstOrDefault(x => x.Id == transaction.Id).IsCompleted = true;
            }
            _dbContext.StockOwnership.AddRange(newOwnerships);
            _dbContext.StockSold.AddRange(newSoldStocks);

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
            var debugPath = Path.Combine(_hostingEnvironment.WebRootPath, "debug.txt");

            try
            {
                //Tid för börsstängning, ska vara 1800
                var currentTime = DateTime.Now.TimeOfDay;
                var morningOpen = new TimeSpan(09, 10, 0);
                var eveningClose = new TimeSpan(18, 00, 0);

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
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var parser = new CsvReader(reader);
                        while (true)
                        {
                            var row = parser.Read();
                            var rowNumber = parser.Row;
                            var cap = "LargeCap";

                            if (row == false)
                            {
                                break;
                            }

                            // Kollar om aktien är large - mid - small cap
                            if (rowNumber > 105 && rowNumber < 226)
                            {
                                cap = "MidCap";
                            }
                            else if (rowNumber > 225)
                            {
                                cap = "SmallCap";
                            }

                            var fields = parser.CurrentRecord;

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

                            if (lastTradePriceOnly == null || daysLow.Equals("N/A") || daysHigh.Equals("N/A"))
                            {
                                continue;
                            }

                            var stock = new Stock()
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
                            stockList.Add(stock);
                        }
                    }
                }

                SerializeToJson(stockPath, stockList);
                WriteToDebug(debugPath, DateTime.Now, "sucess, stockList.Count = " + stockList.Count);
            }
            catch (Exception exception)
            {
                WriteToDebug(debugPath, DateTime.Now, exception.Message);

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
            var webRootPath = _hostingEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "stocks.json");
            using (var r = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                var json = r.ReadToEnd();
                stocks = JsonConvert.DeserializeObject<List<Stock>>(json);
            }

            return stocks.Find(x => x.Label.ToLower() == label.ToLower());
        }

        private static void SerializeToJson(string path, List<Stock> stockList)
        {
            using (var file = File.CreateText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, stockList);
            }
        }

        private static void WriteToDebug(string path, DateTime date, string message)
        {
            using (var sw = File.AppendText(path))
            {
                sw.Write(date.ToString("U") + " " + message + Environment.NewLine);
            }
        }
    }
}
