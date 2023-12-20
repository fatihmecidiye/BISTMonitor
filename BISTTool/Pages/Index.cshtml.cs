using BIST.Utilities;
using BIST.Utilities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace BISTTool.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly FinancialCalculatorAssistant service;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            service = new FinancialCalculatorAssistant();
            this.Start();
            this.ThreadStarter();
            this.ReportThreadStarter();
        }


        public void ThreadStarter()
        {
            var borsaKZThread = new System.Threading.Thread(new System.Threading.ThreadStart(async () =>
            {
                BISTReports = System.IO.File.Exists(reportPath) ? JsonConvert.DeserializeObject<List<BISTReport>>(System.IO.File.ReadAllText(reportPath)) : new List<BISTReport>();
                a = service.CalculateBISTProfit();
                UsdPrice = service.GetUsdTryPrice();
                var list = a.Values.First();
                satilan = list.Where(br => br.isPartiallySelled || br.isTotallySelled).OrderByDescending(br => br.BirimDegisimYuzde).ToList();
                elde = list.Where(br => !(br.isPartiallySelled || br.isTotallySelled)).OrderByDescending(br => br.BirimDegisimYuzde).ToList();
                BISTResults = elde.Union(satilan).ToList();
                Task.Delay(100 * 60 * 30).Wait();
            }));
            borsaKZThread.Name = "CalculateKZ";
            borsaKZThread.Start();
        }

        private string reportPath = "report.json";

        public void ReportThreadStarter()
        {
            var reportThread = new System.Threading.Thread(new System.Threading.ThreadStart(async () =>
            {
                if (DateTime.Today.DayOfWeek != DayOfWeek.Sunday && DateTime.Today.DayOfWeek != DayOfWeek.Saturday)
                {
                    if (!System.IO.File.Exists(reportPath) && DateTime.Now.Hour > 18)
                    {
                        double totalTutar = a.Keys.First().Keys.First();
                        double currentTotal = a.Keys.First().Values.First();

                        var dailyReport = new BISTReport()
                        {
                            Date = DateTime.Today,
                            TotalMaliyet = totalTutar,
                            TotalKz = currentTotal - totalTutar,
                            USDTRY = this.UsdPrice,
                            PortfolioTotal = currentTotal,
                            TotalMaliyetUSD = service.TotalMaliyetUSD,
                            TotalKzUSD = currentTotal / UsdPrice - service.TotalMaliyetUSD,
                            PortfolioTotalUSD = currentTotal / UsdPrice
                        };

                        var reports = new List<BISTReport>() { dailyReport };

                        System.IO.File.WriteAllText(reportPath, JsonConvert.SerializeObject(reports));
                    }
                    else if (System.IO.File.Exists(reportPath) && DateTime.Now.Hour > 18)
                    {
                        var reports = JsonConvert.DeserializeObject<List<BISTReport>>(System.IO.File.ReadAllText(reportPath));
                        if (!reports.Any(r => r.Date == DateTime.Today))
                        {
                            double totalTutar = a.Keys.First().Keys.First();
                            double currentTotal = a.Keys.First().Values.First();
                            var dailyReport = new BISTReport()
                            {
                                Date = DateTime.Today,
                                TotalMaliyet = totalTutar,
                                TotalKz = currentTotal - totalTutar,
                                USDTRY = this.UsdPrice,
                                PortfolioTotal = currentTotal,
                                TotalMaliyetUSD = service.TotalMaliyetUSD,
                                TotalKzUSD = currentTotal / UsdPrice - service.TotalMaliyetUSD,
                                PortfolioTotalUSD = currentTotal / UsdPrice
                            };

                            reports.Add(dailyReport);

                            System.IO.File.WriteAllText(reportPath, JsonConvert.SerializeObject(reports));
                        }
                    }
                }

                Task.Delay(100 * 60 * 30).Wait();
            }));
            reportThread.Name = "reportThread";
            reportThread.Start();
        }


        public void Start()
        {
            BISTReports = System.IO.File.Exists(reportPath) ? JsonConvert.DeserializeObject<List<BISTReport>>(System.IO.File.ReadAllText(reportPath)) : new List<BISTReport>();
            a = service.CalculateBISTProfit();
            UsdPrice = service.GetUsdTryPrice();
            var list = a.Values.First();
            satilan = list.Where(br => br.isPartiallySelled || br.isTotallySelled).OrderByDescending(br => br.BirimDegisimYuzde).ToList();
            elde = list.Where(br => !(br.isPartiallySelled || br.isTotallySelled)).OrderByDescending(br => br.BirimDegisimYuzde).ToList();
            BISTResults = elde.Union(satilan).ToList();
        }

        public IActionResult OnPostMyButtonClick()
        {
            BISTReports = System.IO.File.Exists(reportPath) ? JsonConvert.DeserializeObject<List<BISTReport>>(System.IO.File.ReadAllText(reportPath)) : new List<BISTReport>();
            a = service.CalculateBISTProfit();
            UsdPrice = service.GetUsdTryPrice();
            var list = a.Values.First();
            satilan = list.Where(br => br.isPartiallySelled || br.isTotallySelled).OrderByDescending(br => br.BirimDegisimYuzde).ToList();
            elde = list.Where(br => !(br.isPartiallySelled || br.isTotallySelled)).OrderByDescending(br => br.BirimDegisimYuzde).ToList();
            BISTResults = elde.Union(satilan).ToList();

            double totalTutar = a.Keys.First().Keys.First();
            double currentTotal = a.Keys.First().Values.First();

            ViewData["TotalTutar"] = totalTutar;
            ViewData["TotalTotalKZ"] = currentTotal - totalTutar;
            ViewData["UsdPrice"] = UsdPrice;
            ViewData["TotalTutarUsd"] = service.TotalMaliyetUSD;
            ViewData["SimdikiToplam"] = currentTotal / UsdPrice;

            return RedirectToPage("Index");
        }

        public List<BISTResult> BISTResults { get; set; }
        public List<BISTReport> BISTReports { get; set; }
        public List<BISTResult> satilan { get; set; }
        public List<BISTResult> elde { get; set; }
        public Dictionary<Dictionary<double, double>, List<BISTResult>> a { get; set; }
        public double UsdPrice { get; private set; }
        public void OnGet()
        {
            double totalTutar = a.Keys.First().Keys.First();
            double currentTotal = a.Keys.First().Values.First();

            ViewData["TotalTutar"] = totalTutar;
            ViewData["TotalTotalKZ"] = currentTotal - totalTutar;
            ViewData["UsdPrice"] = UsdPrice;
            ViewData["TotalTutarUsd"] = service.TotalMaliyetUSD;
            ViewData["SimdikiToplam"] = currentTotal/UsdPrice;
        }
    }
}