using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Internal;
using Newtonsoft.Json;

namespace GameOfStocksHT16.Logic
{
    public class StockHandler
    {
        public static async Task SaveStocksOnStartup(string wwwRootPath)
        {
            try
            {
                //Tid för börsstängning, ska vara 1800
                var currentTime = DateTime.Now.TimeOfDay;
                var morningOpen = new TimeSpan(09, 10, 0);
                var eveningClose = new TimeSpan(18, 00, 0);

                //Sökväg till yahoo finance api där all aktieinformation hämtas.
                const string url =
                    "http://download.finance.yahoo.com/d/quotes.csv?s=ORTI-B.ST,POOL-B.ST,PREC.ST,PREV-B.ST,PRIC-B.ST,PACT.ST,PROB.ST,PROF-B.ST,ORTI-A.ST,OPCO.ST,ODD.ST,NTEK-B.ST,NOVE.ST,NOTE.ST,NSP-B.ST,RAY-B.ST,VSSAB-B.ST,REJL-B.ST,RNBS.ST,RROS.ST,SEAM.ST,SEMC.ST,SENS.ST,SINT.ST,SOF-B.ST,STWK.ST,SVIK.ST,SVED-B.ST,SVOL-A.ST,SVOL-B.ST,TRAC-B.ST,TRAD.ST,TAGR.ST,UFLX-B.ST,VRG-B.ST,VIT-B.ST,XANO-B.ST,ACAN-B.ST,ANOD-B.ST,ATRLJ-B.ST,ALNX.ST,ATEL-A.ST,ANOT.ST,ARP.ST,ARISE.ST,DOM.ST,AVEG-B.ST,BEGR.ST,BELE.ST,BRG-B.ST,BESQ.ST,BINV.ST,BIOT.ST,BORG.ST,BEF-SDB.ST,BONG.ST,BOUL.ST,BTS-B.ST,BULTEN.ST,CRAD-B.ST,CEVI.ST,CCOR-B.ST,CONS-B.ST,CTT.ST,DEDI.ST,DGC.ST,DORO.ST,DURC-B.ST,ELAN-B.ST,ELEC.ST,ELOS-B.ST,ENDO.ST,ENEA.ST,EOLU-B.ST,EPIS-B.ST,ETX.ST,EWRK.ST,FEEL.ST,FPIP.ST,G5EN.ST,HUFV-C.ST,GHP.ST,HAV-B.ST,PNDX-B.ST,IAR-B.ST,IS.ST,ICTA-B.ST,KABE-B.ST,KARO.ST,KDEV.ST,KNOW.ST,LAMM-B.ST,MEAB-B.ST,MSAB-B.ST,MSON-A.ST,MSON-B.ST,MIDW-A.ST,MIDW-B.ST,MOB.ST,MQ.ST,MSC-B.ST,MULQ.ST,NAXS.ST,NETI-B.ST,NVP.ST,NOMI.ST,AAK.ST,ABB.ST,AOI.ST,ALFA.ST,ASSA-B.ST,AZN.ST,ATCO-A.ST,ATCO-B.ST,WALL-B.ST,ALIV-SDB.ST,AXFO.ST,AXIS.ST,BETS-B.ST,BILL.ST,BOL.ST,CAST.ST,COMH.ST,ELUX-A.ST,ELUX-B.ST,EKTA-B.ST,ENQ.ST,ERIC-A.ST,ERIC-B.ST,FABG.ST,BALD-B.ST,BALD-PREF.ST,GETI-B.ST,SHB-A.ST,SHB-B.ST,HM-B.ST,HEXA-B.ST,HPOL-B.ST,HOLM-A.ST,HOLM-B.ST,HUFV-A.ST,HUSQ-A.ST,HUSQ-B.ST,ICA.ST,INDU-A.ST,INDU-C.ST,INDT.ST,IJ.ST,INVE-A.ST,INVE-B.ST,JM.ST,KINV-A.ST,KINV-B.ST,LATO-B.ST,LIFCO-B.ST,LOOM-B.ST,LUND-B.ST,LUMI-SDB.ST,LUPE.ST,MEDA-A.ST,MELK.ST,MIC-SDB.ST,MTG-A.ST,MTG-B.ST,NCC-A.ST,NCC-B.ST,NIBE-B.ST,NOBI.ST,NDA-SEK.ST,WISE.ST,PEAB-B.ST,RATO-A.ST,RATO-B.ST,RATO-PREF.ST,SAAB-B.ST,SAND.ST,SCA-A.ST,SCA-B.ST,SEB-A.ST,SEB-C.ST,SECU-B.ST,SKA-B.ST,SKF-A.ST,SKF-B.ST,SSAB-A.ST,SSAB-B.ST,STE-A.ST,STE-R.ST,SWED-A.ST,SWMA.ST,SOBI.ST,TEL2-A.ST,TEL2-B.ST,TLSN.ST,TIEN.ST,TREL-B.ST,VOLV-A.ST,VOLV-B.ST,ACTI.ST,ADDT-B.ST,ARCM.ST,AZA.ST,BBTO-B.ST,BACTI-B.ST,BEIA-B.ST,BEIJ-B.ST,BILI-A.ST,BIOG-B.ST,PXXS-SDB.ST,BUFAB.ST,BURE.ST,BMAX.ST,CATE.ST,CCC.ST,CLA-B.ST,CLAS-B.ST,COIC.ST,CORE.ST,CORE-PREF.ST,CRED-A.ST,DIOS.ST,DUNI.ST,DUST.ST,ECEX.ST,ELTEL.ST,ENRO.ST,ENRO-PREF.ST,FAG.ST,FPAR.ST,FPAR-PREF.ST,FOI-B.ST,FING-B.ST,GRNG.ST,GUNN.ST,HLDX.ST,HEBA-B.ST,HEMF.ST,HEMF-PREF.ST,HIQ.ST,HMS.ST,IFS-A.ST,IFS-B.ST,INWI.ST,ITAB-B.ST,KAHL.ST,KLOV-A.ST,KLOV-B.ST,KLOV-PREF.ST,KLED.ST,LAGR-B.ST,LIAB.ST,LUC.ST,LUG.ST,MVIR-B.ST,MEKO.ST,MUNK1S.ST,NDX.ST,ORI.ST,SHOT.ST,THULE.ST,TOBII.ST,TROAX.ST,VNV-SDB.ST,NMAN.ST,NET-B.ST,NEWA-B.ST,NGQ.ST,NOLA-B.ST,NN-B.ST,NP3.ST,OASM.ST,OEM-B.ST,OPUS.ST,ORX.ST,PLAZ-B.ST,PROE-B.ST,QLRO.ST,RECI-B.ST,REZT.ST,SAGA-A.ST,SAGA-B.ST,SAGA-PREF.ST,SCST.ST,SPOR.ST,SAS.ST,SAS-PREF.ST,SECT-B.ST,SMF.ST,SKIS-B.ST,COOR.ST,DCAR-B.ST,HOFI.ST,IVSO.ST,MYCR.ST,NOBINA.ST,SWEC-A.ST,SWEC-B.ST,SWOL-B.ST,SYSR.ST,TETY.ST,TWW.ST,OP.ST,ALIG.ST,ATT.ST,BRAV.ST,CAMX.ST,HTRO.ST,HMED.ST,OP-PREF.ST,TRI.ST,UNIB-SDB.ST,VBG-B.ST,VICP-A.ST,VICP-B.ST,VICP-PREF.ST,VITR.ST,CLX.ST,WIHL.ST,AF-B.ST,ORES.ST,COLL.ST&f=d1ghl1nop2st1v&e=.csv";

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
                            if (row == false)
                            {
                                break;
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
                                LastTradeDate = lastTradeDate

                            };
                            stockList.Add(stock);
                        }
                    }
                }

                var path = Path.Combine(wwwRootPath, "stocks.json");

                SerializeToJson(path, stockList);

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
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
    }

}
