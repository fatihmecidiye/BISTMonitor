namespace BIST.Utilities.Models
{
    public class BISTResult
    {
        public string Name { get; set; }
        public double BirimMaliyet { get; set; }
        public double BirimFiyat { get; set; }
        public double BirimKZ { get; set; }
        public double GunlukDegisim { get; set; }
        public double BirimDegisimYuzde { get; set; }
        public double TotalKZ { get; set; }
        public bool isPartiallySelled { get; set; } = false;
        public bool isTotallySelled { get; set; } = false;
        public double SatilanBirimMaliyet { get; set; }
        public double SatilanBirimSatisFiyati { get; set; }
        public double SatilanBirimKZ { get; set; }
        public double SatilanBirimKZYuzde { get; set; }
        public double SatilanTotalKZ { get; set; }
        public double Miktar { get; set; }
        public double Tutar { get; set; }
    }

    public class BISTReport
    {
        public DateTime Date { get; set; }
        public double USDTRY { get; set; }
        public double TotalMaliyet { get; set; }
        public double TotalMaliyetUSD { get; set; }
        public double TotalKz { get; set; }
        public double TotalKzUSD { get; set; }
        public double PortfolioTotal { get; set; }
        public double PortfolioTotalUSD { get; set; }
    }
}


//Console.WriteLine(name + " birim maliyet = " + activeMaliyet);
//Console.WriteLine(name + " birim fiyat = " + price);
//Console.WriteLine(name + " birim k/z = " + (price - activeMaliyet));
//Console.WriteLine(name + " günlük % = " + dailyIncrease + " %");
//Console.WriteLine(name + " birim k/z % = " + (price - activeMaliyet) / activeMaliyet * 100 + " %");
//Console.WriteLine(name + " total k/z = " + (price - activeMaliyet) * activeAmount);
//var sellPrice = hisse.HisseAlimlari.Where(h => !h.isActive).Sum(h => h.sellPrice * h.amount) / soldAmount;
//Console.WriteLine(name + " KISMİ HİSSE SATILDI");
//Console.WriteLine(name + " birim maliyet = " + soldMaliyet);
//Console.WriteLine(name + " birim satış fiyat = " + sellPrice);
//Console.WriteLine(name + " birim k/z = " + (sellPrice - soldMaliyet));
//Console.WriteLine(name + " birim k/z % = " + (sellPrice - soldMaliyet) / ortMaliyet * 100 + " %");
//Console.WriteLine(name + " total k/z = " + (sellPrice - soldMaliyet) * soldAmount);