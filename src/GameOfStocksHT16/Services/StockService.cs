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
using GameOfStocksHT16.StocksLogic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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
                    newSoldStocks.Add(new StockSold()
                    {
                        Name = transaction.Name,
                        Label = transaction.Label,
                        DateSold = DateTime.Now,
                        Quantity = transaction.Quantity,
                        LastTradePrice = GetStockByLabel(transaction.Label).LastTradePriceOnly,
                        User = transaction.User
                    });
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

                //Sökväg till yahoo finance api där all aktieinformation hämtas.
                //const string url =
                //    "http://download.finance.yahoo.com/d/quotes.csv?s=ORTI-B.ST,POOL-B.ST,PREC.ST,PREV-B.ST,PRIC-B.ST,PACT.ST,PROB.ST,PROF-B.ST,ORTI-A.ST,OPCO.ST,ODD.ST,NTEK-B.ST,NOVE.ST,NOTE.ST,NSP-B.ST,RAY-B.ST,VSSAB-B.ST,REJL-B.ST,RNBS.ST,RROS.ST,SEAM.ST,SEMC.ST,SENS.ST,SINT.ST,SOF-B.ST,STWK.ST,SVIK.ST,SVED-B.ST,SVOL-A.ST,SVOL-B.ST,TRAC-B.ST,TRAD.ST,TAGR.ST,UFLX-B.ST,VRG-B.ST,VIT-B.ST,XANO-B.ST,ACAN-B.ST,ANOD-B.ST,ATRLJ-B.ST,ALNX.ST,ATEL-A.ST,ANOT.ST,ARP.ST,ARISE.ST,DOM.ST,AVEG-B.ST,BEGR.ST,BELE.ST,BRG-B.ST,BESQ.ST,BINV.ST,BIOT.ST,BORG.ST,BEF-SDB.ST,BONG.ST,BOUL.ST,BTS-B.ST,BULTEN.ST,CRAD-B.ST,CEVI.ST,CCOR-B.ST,CONS-B.ST,CTT.ST,DEDI.ST,DGC.ST,DORO.ST,DURC-B.ST,ELAN-B.ST,ELEC.ST,ELOS-B.ST,ENDO.ST,ENEA.ST,EOLU-B.ST,EPIS-B.ST,ETX.ST,EWRK.ST,FEEL.ST,FPIP.ST,G5EN.ST,HUFV-C.ST,GHP.ST,HAV-B.ST,PNDX-B.ST,IAR-B.ST,IS.ST,ICTA-B.ST,KABE-B.ST,KARO.ST,KDEV.ST,KNOW.ST,LAMM-B.ST,MEAB-B.ST,MSAB-B.ST,MSON-A.ST,MSON-B.ST,MIDW-A.ST,MIDW-B.ST,MOB.ST,MQ.ST,MSC-B.ST,MULQ.ST,NAXS.ST,NETI-B.ST,NVP.ST,NOMI.ST,AAK.ST,ABB.ST,AOI.ST,ALFA.ST,ASSA-B.ST,AZN.ST,ATCO-A.ST,ATCO-B.ST,WALL-B.ST,ALIV-SDB.ST,AXFO.ST,AXIS.ST,BETS-B.ST,BILL.ST,BOL.ST,CAST.ST,COMH.ST,ELUX-A.ST,ELUX-B.ST,EKTA-B.ST,ENQ.ST,ERIC-A.ST,ERIC-B.ST,FABG.ST,BALD-B.ST,BALD-PREF.ST,GETI-B.ST,SHB-A.ST,SHB-B.ST,HM-B.ST,HEXA-B.ST,HPOL-B.ST,HOLM-A.ST,HOLM-B.ST,HUFV-A.ST,HUSQ-A.ST,HUSQ-B.ST,ICA.ST,INDU-A.ST,INDU-C.ST,INDT.ST,IJ.ST,INVE-A.ST,INVE-B.ST,JM.ST,KINV-A.ST,KINV-B.ST,LATO-B.ST,LIFCO-B.ST,LOOM-B.ST,LUND-B.ST,LUMI-SDB.ST,LUPE.ST,MEDA-A.ST,MELK.ST,MIC-SDB.ST,MTG-A.ST,MTG-B.ST,NCC-A.ST,NCC-B.ST,NIBE-B.ST,NOBI.ST,NDA-SEK.ST,WISE.ST,PEAB-B.ST,RATO-A.ST,RATO-B.ST,RATO-PREF.ST,SAAB-B.ST,SAND.ST,SCA-A.ST,SCA-B.ST,SEB-A.ST,SEB-C.ST,SECU-B.ST,SKA-B.ST,SKF-A.ST,SKF-B.ST,SSAB-A.ST,SSAB-B.ST,STE-A.ST,STE-R.ST,SWED-A.ST,SWMA.ST,SOBI.ST,TEL2-A.ST,TEL2-B.ST,TLSN.ST,TIEN.ST,TREL-B.ST,VOLV-A.ST,VOLV-B.ST,ACTI.ST,ADDT-B.ST,ARCM.ST,AZA.ST,BBTO-B.ST,BACTI-B.ST,BEIA-B.ST,BEIJ-B.ST,BILI-A.ST,BIOG-B.ST,PXXS-SDB.ST,BUFAB.ST,BURE.ST,BMAX.ST,CATE.ST,CCC.ST,CLA-B.ST,CLAS-B.ST,COIC.ST,CORE.ST,CORE-PREF.ST,CRED-A.ST,DIOS.ST,DUNI.ST,DUST.ST,ECEX.ST,ELTEL.ST,ENRO.ST,ENRO-PREF.ST,FAG.ST,FPAR.ST,FPAR-PREF.ST,FOI-B.ST,FING-B.ST,GRNG.ST,GUNN.ST,HLDX.ST,HEBA-B.ST,HEMF.ST,HEMF-PREF.ST,HIQ.ST,HMS.ST,IFS-A.ST,IFS-B.ST,INWI.ST,ITAB-B.ST,KAHL.ST,KLOV-A.ST,KLOV-B.ST,KLOV-PREF.ST,KLED.ST,LAGR-B.ST,LIAB.ST,LUC.ST,LUG.ST,MVIR-B.ST,MEKO.ST,MUNK1S.ST,NDX.ST,ORI.ST,SHOT.ST,THULE.ST,TOBII.ST,TROAX.ST,VNV-SDB.ST,NMAN.ST,NET-B.ST,NEWA-B.ST,NGQ.ST,NOLA-B.ST,NN-B.ST,NP3.ST,OASM.ST,OEM-B.ST,OPUS.ST,ORX.ST,PLAZ-B.ST,PROE-B.ST,QLRO.ST,RECI-B.ST,REZT.ST,SAGA-A.ST,SAGA-B.ST,SAGA-PREF.ST,SCST.ST,SPOR.ST,SAS.ST,SAS-PREF.ST,SECT-B.ST,SMF.ST,SKIS-B.ST,COOR.ST,DCAR-B.ST,HOFI.ST,IVSO.ST,MYCR.ST,NOBINA.ST,SWEC-A.ST,SWEC-B.ST,SWOL-B.ST,SYSR.ST,TETY.ST,TWW.ST,OP.ST,ALIG.ST,ATT.ST,BRAV.ST,CAMX.ST,HTRO.ST,HMED.ST,OP-PREF.ST,TRI.ST,UNIB-SDB.ST,VBG-B.ST,VICP-A.ST,VICP-B.ST,VICP-PREF.ST,VITR.ST,CLX.ST,WIHL.ST,AF-B.ST,ORES.ST,COLL.ST&f=d1ghl1nop2st1v&e=.csv";

                const string urlHt16LargeMidSmall =
                    "http://download.finance.yahoo.com/d/quotes.csv?s=AAK.ST,ABB.ST,AHSL.ST,ALFA.ST,ASSA-B.ST,AZN.ST,ATCO-A.ST,ATCO-B.ST,ATRLJ-B.ST,ALIV-SDB.ST,AXFO.ST,AXIS.ST,BETS-B.ST,BILL.ST,BOL.ST,BONAV-A.ST,BONAV-B.ST,CAST.ST,COMH.ST,DOM.ST,ELUX-A.ST,ELUX-B.ST,EKTA-B.ST,ERIC-A.ST,ERIC-B.ST,FABG.ST,BALD-B.ST,BALD-PREF.ST,FING-B.ST,GETI-B.ST,SHB-A.ST,SHB-B.ST,HM-B.ST,HEXA-B.ST,HPOL-B.ST,HOLM-A.ST,HOLM-B.ST,HUFV-A.ST,HUFV-C.ST,HUSQ-A.ST,HUSQ-B.ST,ICA.ST,INDU-A.ST,INDU-C.ST,INDT.ST,IJ.ST,INVE-A.ST,INVE-B.ST,JM.ST,KINV-A.ST,KINV-B.ST,KLOV-A.ST,KLOV-B.ST,KLOV-PREF.ST,LATO-B.ST,LIFCO-B.ST,LOOM-B.ST,LUND-B.ST,LUMI-SDB.ST,LUPE.ST,MELK.ST,MIC-SDB.ST,MTG-A.ST,MTG-B.ST,NCC-A.ST,NCC-B.ST,NET-B.ST,NIBE-B.ST,NOBI.ST,NDA-SEK.ST,PNDX-B.ST,PEAB-B.ST,RATO-A.ST,RATO-B.ST,RATO-PREF.ST,RESURS.ST,SAAB-B.ST,SAND.ST,SCA-A.ST,SCA-B.ST,SEB-A.ST,SEB-C.ST,SECU-B.ST,SKA-B.ST,SKF-A.ST,SKF-B.ST,SSAB-A.ST,SSAB-B.ST,STE-A.ST,STE-R.ST,SWEC-A.ST,SWEC-B.ST,SWED-A.ST,SWMA.ST,SOBI.ST,TEL2-A.ST,TEL2-B.ST,TELIA.ST,TIEN.ST,TREL-B.ST,UNIB-SDB.ST,VOLV-A.ST,VOLV-B.ST,WALL-B.ST,WIHL.ST,ACAD.ST,ACTI.ST,ALIF-B.ST,ADDT-B.ST,AOI.ST,ALIG.ST,ATORX.ST,ARCM.ST,ATT.ST,AZA.ST,BBTO-B.ST,BEIA-B.ST,BEIJ-B.ST,BESQ.ST,BILI-A.ST,BIOG-B.ST,PXXS-SDB.ST,BRAV.ST,BUFAB.ST,BULTEN.ST,BURE.ST,BMAX.ST,CAMX.ST,CAPIO.ST,CATE.ST,CCC.ST,CLAS-B.ST,CLA-B.ST,CLX.ST,COLL.ST,COIC.ST,COOR.ST,CORE.ST,CORE-PREF.ST,CRED-A.ST,DCAR-B.ST,DIOS.ST,DUNI.ST,DUST.ST,ECEX.ST,ELTEL.ST,ENQ.ST,FAG.ST,FPAR.ST,FPAR-PREF.ST,FOI-B.ST,GRNG.ST,GUNN.ST,HLDX.ST,HEBA-B.ST,HEMF.ST,HEMF-PREF.ST,HIQ.ST,HMS.ST,HOFI.ST,HUM.ST,ENG.ST,IVSO.ST,INWI.ST,ITAB-B.ST,KAHL.ST,KLED.ST,LAGR-B.ST,LIAB.ST,LUC.ST,LUG.ST,MVIR-B.ST,MEKO.ST,MUNK1S.ST,MYCR.ST,NMAN.ST,NEWA-B.ST,NGQ.ST,NOBINA.ST,NOLA-B.ST,NDX.ST,NWG.ST,NN-B.ST,NP3.ST,OASM.ST,OEM-B.ST,OPUS.ST,ORX.ST,ORI.ST,PLAZ-B.ST,QLRO.ST,RAY-B.ST,RECI-B.ST,REZT.ST,SAGA-A.ST,SAGA-B.ST,SAGA-D.ST,SAGA-PREF.ST,SAS.ST,SAS-PREF.ST,SCST.ST,SHOT.ST,SECT-B.ST,SMF.ST,SENS.ST,SRNKE-B.ST,SKIS-B.ST,SYSR.ST,TETY.ST,TFBANK.ST,THULE.ST,TOBII.ST,TRAC-B.ST,TWW.ST,TROAX.ST,VBG-B.ST,VICP-A.ST,VICP-B.ST,VICP-PREF.ST,VITR.ST,VNV-SDB.ST,WTX.ST,XVIVO.ST,AF-B.ST,ORES.ST,ACAN-B.ST,ANOD-B.ST,ATEL-A.ST,ANOT.ST,ARP.ST,ARISE.ST,AVEG-B.ST,BACTI-B.ST,BEGR.ST,BELE.ST,BRG-B.ST,BINV.ST,BIOT.ST,BORG.ST,BEF-SDB.ST,BONG.ST,BOUL.ST,BTS-B.ST,CRAD-B.ST,CEVI.ST,CCOR-B.ST,CONS-B.ST,CTT.ST,DEDI.ST,DGC.ST,DORO.ST,DURC-B.ST,ELAN-B.ST,ELEC.ST,ELOS-B.ST,ENDO.ST,ENEA.ST,ENRO.ST,ENRO-PREF.ST,EOLU-B.ST,EPIS-B.ST,ETX.ST,EWRK.ST,FEEL.ST,FPIP.ST,G5EN.ST,GARO.ST,GHP.ST,HMED.ST,HAV-B.ST,HTRO.ST,IAR-B.ST,IS.ST,ICTA-B.ST,KABE-B.ST,KARO.ST,KDEV.ST,KNOW.ST,LAMM-B.ST,MEAB-B.ST,MCAP.ST,MSAB-B.ST,MSON-A.ST,MSON-B.ST,MIDW-A.ST,MIDW-B.ST,MOB.ST,MQ.ST,MSC-B.ST,MULQ.ST,NAXS.ST,NETI-B.ST,NVP.ST,NGS.ST,NOMI.ST,NOTE.ST,NTEK-B.ST,ODD.ST,ORTI-A.ST,ORTI-B.ST,OP.ST,OP-PREF.ST,OP-PREFB.ST,POOL-B.ST,PREC.ST,PREV-B.ST,PRIC-B.ST,PACT.ST,PROB.ST,PROF-B.ST,REJL-B.ST,RNBS.ST,RROS.ST,SEAM.ST,SEMC.ST,SINT.ST,SOF-B.ST,SPOR.ST,STWK.ST,STRAX.ST,SVIK.ST,SVED-B.ST,SVOL-A.ST,SVOL-B.ST,SWOL-B.ST,TRAD.ST,TRENT.ST,TAGR.ST,UFLX-B.ST,VRG-B.ST,VSSAB-B.ST,VIT-B.ST,WISE.ST,XANO-B.ST&f=d1ghl1nop2st1v&e=.csv";
                //LARGE 0 - 105 
                //MID 106 - 225 (120 st)
                //SMALL 226 - 334 (109 st)


                //if (currentTime <= morningOpen || currentTime >= eveningClose) return;

                var stockList = new List<Stock>();

                var req = (HttpWebRequest)WebRequest.Create(urlHt16LargeMidSmall);
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

        private static void SerializeToJson(string path, List<Stock> stockList)
        {
            using (var file = File.CreateText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, stockList);
            }
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

        private static void WriteToDebug(string path, DateTime date, string message)
        {
            using (var sw = File.AppendText(path))
            {
                sw.Write(date.ToString("U") + " " + message + Environment.NewLine);
            }
        }
    }
}
