using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
   public class TempPaymentTypeReportItemViewModel
    {
        public DateTime Time { get; set; }
        public double Cash { get; set; }
        public double Bank { get; set; }
        public double DirectBill { get; set; }
    }
}
