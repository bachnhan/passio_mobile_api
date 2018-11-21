using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
   public class ShiftReportViewModel
    {
        public string StartTime { get; set; }
        public double TotalOrderShift1 { get; set; }
        public double TotalPriceShift1 { get; set; }
        public double AverageShift1 { get; set; }
        public double TotalOrderShift2 { get; set; }
        public double TotalPriceShift2 { get; set; }
        public double AverageShift2 { get; set; }
    }
}
