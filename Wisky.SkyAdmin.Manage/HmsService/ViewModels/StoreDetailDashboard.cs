using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class StoreDetailDashboard
    {
        public string StoreName { get; set; }
        public int TotalOrder { get; set; }
        public int TotalOrderCard { get; set; }
        public int TotalRevenue { get; set; }
        public int TotalRevenueCard { get; set; }
        public int TotalRevenueAll { get; set; }
        public int AvgRevenue { get; set; }
        public int TypeBest { get; set; }
        public int TotalProduct { get; set; }
    }
}
