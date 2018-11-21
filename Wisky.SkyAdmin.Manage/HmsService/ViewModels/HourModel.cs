using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HmsService.ViewModels
{
    public class HourReportModel
    {
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public double TakeAway { get; set; }
        public double PriceTakeAway { get; set; }
        public double AtStore { get; set; }
        public double PriceAtStore { get; set; }
        public double Delivery { get; set; }
        public double PriceDelivery { get; set; }
        public double Discount { get; set; }
        public double TotalQuantity { get; set; }
        public double TotalPrice { get; set; }
        public double FinalPrice { get; set; }

    }
}