using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
   public class TempStoreRevenueReportItemViewModel
    {
        public DateTime StartTime { get; set; }
        public DateTime TimeLine { get; set; }
        public double TotalAmount { get; set; }
        public double TotalFinal { get; set; }
        public double TotalDiscountFee { get; set; }
        public int TotalBill { get; set; }
        public int BillAtStore { get; set; }
        public int BillDelivery { get; set; }
        public int BillTakeAway { get; set; }

    }
}
