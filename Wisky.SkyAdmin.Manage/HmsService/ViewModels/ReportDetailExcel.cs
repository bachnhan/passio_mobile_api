using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class ReportDetailExcel
    {
        public int Stt { get; set; }
        public string Department { get; set; }
        public string Group { get; set; }
        public string EmpId { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string WorkShift { get; set;}
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public double TotalTime { get; set; }
        public double OverTime { get; set; }
    }
}
