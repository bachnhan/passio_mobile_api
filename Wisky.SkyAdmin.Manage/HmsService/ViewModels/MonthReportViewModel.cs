using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class MonthReportViewModel
    {
        public int Month { get; set; }

        public string MonthName { get; set; }

        public int TotalOrder { get; set; }
        public double TotalPrice { get; set; }
        public double TotalFinalAmount { get; set; }
        public double TotalDiscount { get; set; }
        public double TakeAway { get; set; }
        public double PriceTakeAway { get; set; }
        public double AtStore { get; set; }
        public double PriceAtStore { get; set; }
        public double Delivery { get; set; }
        public double PriceDelivery { get; set; }
        public double Card { get; set; }
        public double PriceCard { get; set; }
    }
}
