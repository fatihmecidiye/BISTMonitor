using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIST.Utilities.Models
{
    public class Eldekiler
    {
        public List<HisseAlim> HisseAlimlari { get; set; }
    }

    public class HisseAlim
    {
        public string Name { get; set; }
        public double maliyet { get; set; }
        public double amount { get; set; }
        public double buyUsd { get; set; }
        public bool isActive { get; set; }
        public bool isOld { get; set; }
        public double sellPrice { get; set; }
    }

    public class Altin
    {
        public string Name { get; set; }
        public double maliyet { get; set; }
        public double amount { get; set; }
        public double buyUsd { get; set; }
        public bool isActive { get; set; }
        public bool isOld { get; set; }
        public double sellPrice { get; set; }
    }
}
