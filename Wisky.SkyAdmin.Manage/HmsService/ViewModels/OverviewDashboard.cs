using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class OverviewDashboard
    {
        public double TotalRevenue { get; set; }
        public double TotalRevenueWithoutCard { get; set; }
        public double TotalRevenueWithDiscount { get; set; }
        public double TotalDiscount { get; set; }
        public double TotalRevenueWithoutDiscountAndCard { get; set; }
        public double TotalRevenueCard { get; set; }
        public double TotalRevenuePrecancel { get; set; }
        public double TotalRevenueAftercancel { get; set; }
        public int TotalOrder { get; set; }
        public int TotalOrderAtStore { get; set; }
        public long TotalRevenueAtStore { get; set; }
        public long TotalOrderTakeAway { get; set; }
        public long TotalRevenueTakeAway { get; set; }

        public long TotalOrderDelivery { get; set; }
        public long TotalRevenueDelivery { get; set; }

        public long TotalOrderCard { get; set; }
        public long TotalRevenueOrderCard { get; set; }

        public long TotalOrderPreCancel { get; set; }
        public long TotalOrderAfterCancel { get; set; }
        public double AvgRevenueOrder { get; set; }
        public double AvgRevenueOrderAtStore { get; set; }
        public double AvgRevenueOrderTakeAway { get; set; }
        public double AvgRevenueOrderDelivery { get; set; }
        public double AvgProductOrder { get; set; }
        public double AvgProductOrderTakeAway { get; set; }
        public double AvgProductOrderAtStore { get; set; }
        public double AvgProductOrderDelivery { get; set; }
    }
}
