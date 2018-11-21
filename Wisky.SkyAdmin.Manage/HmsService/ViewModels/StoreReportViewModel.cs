using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class StoreReportViewModel
    {
        public int StoreId { get; set; }
        public double FinalAmount { get; set; }
        public double FinalCard { get; set; }
        public int TotalOrder { get; set; }
        public int TotalOrderCard { get; set; }
        public int TotalOrderTakeAway { get; set; }
        public int TotalOrderDelivery { get; set; }
    }
}
