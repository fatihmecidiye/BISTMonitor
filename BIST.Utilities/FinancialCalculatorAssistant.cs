using BIST.Utilities.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BIST.Utilities
{
    public class FinancialCalculatorAssistant
    {
        private List<Eldekiler> eldekiHisseler;
        private List<Altin> altin; 
        public FinancialCalculatorAssistant()
        {
            List<List<HisseAlim>> outerList = JsonConvert.DeserializeObject<List<List<HisseAlim>>>(File.ReadAllText("hisseler.json"))!;

            eldekiHisseler = new List<Eldekiler>();

            foreach (List<HisseAlim> innerList in outerList)
            {
                Eldekiler eldekiler = new Eldekiler
                {
                    HisseAlimlari = innerList
                };
                eldekiHisseler.Add(eldekiler);
            }

            altin = JsonConvert.DeserializeObject<List<Altin>>(File.ReadAllText("altin.json"))!;
        }

        public double TotalMaliyetUSD { get; set; }
        public Dictionary<Dictionary<double, double>,List<BISTResult>> CalculateBISTProfit()
        {
            var dict = new Dictionary<Dictionary<double,double>,List<BISTResult>>();
            var res = new List<BISTResult>();

            // Get the current price of asset from Yahoo Finance.
            var totalMaliyet = eldekiHisseler.SelectMany(h => h.HisseAlimlari).Where(h => h.isOld).Sum(h => h.amount * h.maliyet);
            var totalMaliyetUSD = eldekiHisseler.SelectMany(h => h.HisseAlimlari).Where(h => h.isOld).Sum(h => h.amount * h.maliyet / h.buyUsd);

            double currentTotal = 0;
            double totalKZ = 0;
            foreach (var hisse in eldekiHisseler)
            {
                try
                {
                    var quoteUrl = $"https://query1.finance.yahoo.com/v8/finance/chart/{hisse.HisseAlimlari.First().Name}";
                    var response = Task.Run(() =>
                    {
                        var client = new WebClient();
                        var content = client.DownloadString(quoteUrl);
                        return content;
                    }).Result;

                    // Parse the JSON response and get the price.
                    var json = JsonConvert.DeserializeObject<Root>(response);
                    var price = json.chart.result[0].meta.regularMarketPrice;
                    var previousClose = json.chart.result[0].meta.previousClose;

                    var dailyIncrease = (price - previousClose) / previousClose * 100;
                    double ortMaliyet = hisse.HisseAlimlari.Sum(h => h.maliyet * h.amount) / hisse.HisseAlimlari.Sum(h => h.amount);
                    var count = hisse.HisseAlimlari.Count;
                    var totalHisseAmount = hisse.HisseAlimlari.Sum(h => h.amount);
                    string name = "";
                    foreach (var alim in hisse.HisseAlimlari)
                    {
                        name = alim.Name;
                        if (alim.isActive)
                        {
                            totalKZ += (price - alim.maliyet) * alim.amount;
                            currentTotal += price * alim.amount;
                        }
                    }
                    if (hisse.HisseAlimlari.All(h => h.isActive))
                    {
                        var profit = new BISTResult()
                        {
                            Name = name,
                            Miktar = totalHisseAmount,
                            BirimMaliyet = ortMaliyet,
                            BirimFiyat = price,
                            BirimKZ = price - ortMaliyet,
                            GunlukDegisim = dailyIncrease,
                            BirimDegisimYuzde = (price - ortMaliyet) / ortMaliyet * 100,
                            TotalKZ = (price - ortMaliyet) * totalHisseAmount,
                            Tutar = ortMaliyet * totalHisseAmount
                        };
                        res.Add(profit);
                        //Console.WriteLine(name + " birim maliyet = " + ortMaliyet);
                        //Console.WriteLine(name + " birim fiyat = " + price);
                        //Console.WriteLine(name + " birim k/z = " + (price - ortMaliyet));
                        //Console.WriteLine(name + " günlük % = " + dailyIncrease + " %");
                        //Console.WriteLine(name + " birim k/z % = " + (price - ortMaliyet) / ortMaliyet * 100 + " %");
                        //Console.WriteLine(name + " total k/z = " + (price - ortMaliyet) * totalHisseAmount);
                        //Console.WriteLine();
                        //totalKZ += (price - ortMaliyet) * hisse.amount;
                        //currentTotal += price * hisse.amount;
                        //currentPrices[hisse.Name] = price;
                    }
                    else if (hisse.HisseAlimlari.All(h => !h.isActive))
                    {

                        var sellprice = hisse.HisseAlimlari.Sum(h => h.sellPrice * h.amount) / totalHisseAmount;

                        var profit = new BISTResult()
                        {
                            Name = name,
                            BirimMaliyet = ortMaliyet,
                            Miktar = totalHisseAmount,
                            BirimFiyat = price,
                            BirimKZ = sellprice - ortMaliyet,
                            GunlukDegisim = dailyIncrease,
                            BirimDegisimYuzde = (sellprice - ortMaliyet) / ortMaliyet * 100,
                            TotalKZ = (sellprice - ortMaliyet) * totalHisseAmount,
                            isTotallySelled = true,
                            SatilanBirimSatisFiyati = sellprice,
                            Tutar = ortMaliyet * totalHisseAmount
                        };
                        res.Add(profit);

                        //Console.WriteLine(name + " HİSSE SATILDI");
                        //Console.WriteLine(name + " birim maliyet = " + ortMaliyet);
                        //Console.WriteLine(name + " birim satış fiyat = " + price);
                        //Console.WriteLine(name + " birim k/z = " + (price - ortMaliyet));
                        //Console.WriteLine(name + " birim k/z % = " + (price - ortMaliyet) / ortMaliyet * 100 + " %");
                        //Console.WriteLine(name + " total k/z = " + (price - ortMaliyet) * totalHisseAmount);
                        //Console.WriteLine();
                        //totalKZ += (price - ortMaliyet) * hisse.amount;
                        //currentTotal += price * hisse.amount;
                        //currentPrices[hisse.Name] = price;
                    }
                    else
                    {
                        var soldAmount = hisse.HisseAlimlari.Where(h => !h.isActive).Sum(h => h.amount);
                        var soldMaliyet = hisse.HisseAlimlari.Where(h => !h.isActive).Sum(h => h.maliyet * h.amount) / soldAmount;

                        var activeAmount = hisse.HisseAlimlari.Where(h => h.isActive).Sum(h => h.amount);
                        var activeMaliyet = hisse.HisseAlimlari.Where(h => h.isActive).Sum(h => h.maliyet * h.amount) / activeAmount;
                        var sellPrice = hisse.HisseAlimlari.Where(h => !h.isActive).Sum(h => h.sellPrice * h.amount) / soldAmount;

                        var profit = new BISTResult()
                        {
                            Name = name,
                            BirimMaliyet = activeMaliyet,
                            BirimFiyat = price,
                            Miktar = activeAmount,
                            BirimKZ = price - activeMaliyet,
                            GunlukDegisim = dailyIncrease,
                            BirimDegisimYuzde = (price - activeMaliyet) / activeMaliyet * 100,
                            TotalKZ = (price - activeMaliyet) * activeAmount,
                            isPartiallySelled = true,
                            SatilanBirimMaliyet = soldMaliyet,
                            SatilanBirimSatisFiyati = sellPrice,
                            SatilanBirimKZ = sellPrice - soldMaliyet,
                            SatilanBirimKZYuzde = (sellPrice - soldMaliyet) / ortMaliyet * 100,
                            SatilanTotalKZ = (sellPrice - soldMaliyet) * soldAmount,
                            Tutar = activeAmount*activeMaliyet + soldAmount * soldMaliyet
                        };
                        res.Add(profit);

                        //Console.WriteLine(name + " birim maliyet = " + activeMaliyet);
                        //Console.WriteLine(name + " birim fiyat = " + price);
                        //Console.WriteLine(name + " birim k/z = " + (price - activeMaliyet));
                        //Console.WriteLine(name + " günlük % = " + dailyIncrease + " %");
                        //Console.WriteLine(name + " birim k/z % = " + (price - activeMaliyet) / activeMaliyet * 100 + " %");
                        //Console.WriteLine(name + " total k/z = " + (price - activeMaliyet) * activeAmount);
                        //Console.WriteLine(name + " KISMİ HİSSE SATILDI");
                        //Console.WriteLine(name + " birim maliyet = " + soldMaliyet);
                        //Console.WriteLine(name + " birim satış fiyat = " + sellPrice);
                        //Console.WriteLine(name + " birim k/z = " + (sellPrice - soldMaliyet));
                        //Console.WriteLine(name + " birim k/z % = " + (sellPrice - soldMaliyet) / ortMaliyet * 100 + " %");
                        //Console.WriteLine(name + " total k/z = " + (sellPrice - soldMaliyet) * soldAmount);
                        //Console.WriteLine();

                        //totalKZ += (price - hisse.maliyet) * hisse.amount;
                        //currentTotal += price * hisse.amount;
                    }
                }
                catch (Exception e)
                {
                    var total = hisse.HisseAlimlari.Where(a => a.isActive).Sum(a => a.maliyet * a.amount);
                    var amount = hisse.HisseAlimlari.Where(a => a.isActive).Sum(a => a.amount);
                    var name = hisse.HisseAlimlari.First().Name;

                    foreach (var alim in hisse.HisseAlimlari)
                    {
                        if (alim.isActive)
                            currentTotal += alim.maliyet * alim.amount;
                    }
                    //var price = hisse.maliyet;
                    Console.WriteLine(name + " birim maliyet = " + total / amount);
                    Console.WriteLine(name + " birim fiyat = HESAPLANAMADI");
                    Console.WriteLine(name + " birim k/z = HESAPLANAMADI");
                    Console.WriteLine(name + " birim k/z % = HESAPLANAMADI");
                    Console.WriteLine(name + " total k/z = HESAPLANAMADI");
                    Console.WriteLine();

                    //if(hisse.isActive)
                    //    currentTotal += price * hisse.amount;
                    //Console.WriteLine(hisse.Name + e.Message);
                }

            }

            var key = new Dictionary<double, double>();
            key.Add(totalMaliyet, currentTotal);
            dict[key] = res;

            var usdTry = Utilities.GetUsdTryPrice();
            var currentUsd = currentTotal / usdTry;
            var kzUsd = currentUsd - totalMaliyetUSD;
            Console.WriteLine("Total maliyet = " + totalMaliyet + " Şimdiki toplam değer = " + currentTotal);
            Console.WriteLine("Total maliyet (USD) = " + totalMaliyetUSD + " Şimdiki toplam değer = " + currentUsd);
            Console.WriteLine("Total KZ = " + totalKZ);
            Console.WriteLine("Total KZ (USD) = " + kzUsd);
            Console.WriteLine("Total KZ % = " + (totalKZ / totalMaliyet) * 100 + "%");
            Console.WriteLine("Total KZ (USD) % = " + (kzUsd / totalMaliyetUSD) * 100 + "%");

            Console.WriteLine("Ortalama dolar kuru: " + totalMaliyet / totalMaliyetUSD);
            Console.WriteLine("Dolar artışı % : " + (usdTry - (totalMaliyet / totalMaliyetUSD)) / usdTry * 100 + "%");
            TotalMaliyetUSD = totalMaliyetUSD;
            return dict;
        }


        public double GetUsdTryPrice()
        {
            try
            {
                var quoteUrl = "https://query1.finance.yahoo.com/v8/finance/chart/TRY=X";

                var response = Task.Run(() =>
                {
                    var client = new WebClient();
                    var content = client.DownloadString(quoteUrl);
                    return content;
                }).Result;

                // Parse the JSON response and get the price.
                var json = JsonConvert.DeserializeObject<Root>(response);
                var price = json.chart.result[0].meta.regularMarketPrice;
                return price;


            }
            catch (Exception e)
            {
                return 0;
            }

        }

    }

    public static class Utilities
    {
        public static double GetGoldTryPrice(double usdtry)
        {
            try
            {
                var quoteUrl = "https://query1.finance.yahoo.com/v8/finance/chart/GC=F";

                var response = Task.Run(() =>
                {
                    var client = new WebClient();
                    var content = client.DownloadString(quoteUrl);
                    return content;
                }).Result;

                // Parse the JSON response and get the price.
                var json = JsonConvert.DeserializeObject<Root>(response);
                var price = json.chart.result[0].meta.regularMarketPrice;
                var onsToGram = 0.03215;

                return price * onsToGram * usdtry;


            }
            catch (Exception e)
            {
                return 0;
            }

        }

        public static double GetUsdTryPrice()
        {
            try
            {
                var quoteUrl = "https://query1.finance.yahoo.com/v8/finance/chart/TRY=X";

                var response = Task.Run(() =>
                {
                    var client = new WebClient();
                    var content = client.DownloadString(quoteUrl);
                    return content;
                }).Result;

                // Parse the JSON response and get the price.
                var json = JsonConvert.DeserializeObject<Root>(response);
                var price = json.chart.result[0].meta.regularMarketPrice;
                var onsToGram = 28.3495231;

                return price;


            }
            catch (Exception e)
            {
                return 0;
            }

        }
        private static readonly CultureInfo Tr = new CultureInfo("tr-TR");
        public static decimal ParseDecimal(string s)
        {

            if (s.Contains(","))
                return decimal.Parse(s, Tr);

            return decimal.Parse(s, NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

        }

    }
}
