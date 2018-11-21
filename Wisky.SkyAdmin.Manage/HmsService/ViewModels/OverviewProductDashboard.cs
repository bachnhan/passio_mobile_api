using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class OverviewProductDashboard
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public double Total { get; set; }
        public double FinalTotal { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
    }
}
