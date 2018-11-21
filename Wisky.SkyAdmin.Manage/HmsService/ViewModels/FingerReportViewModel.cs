using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class FingerReportViewModel
    {
        public int No { get; set; }
        public string EmployeeName { get; set; }
        public string StoreName { get; set; }
        public int EmpID { get; set; }
        public string EmpEnroll { get; set; }
        public List<ItemMonth> Month { get; set; } 

        public FingerReportViewModel(DateTime startDate , DateTime endDate)
        {
            Month = new List<ItemMonth>();
            for (DateTime i = startDate; i < endDate; i=i.AddDays(1))
            {
                var itemMonth = new ItemMonth();
                itemMonth.DateString = i.ToString("dd/MM");
                itemMonth.timework = new TimeSpan(0);
                itemMonth.timeOverTime = new TimeSpan(0);
                itemMonth.Month = i.Month;
                itemMonth.numberdate = i.Day;
                Month.Add(itemMonth);
            }
        }
    }

    public class ItemMonth
    {
        public string DateString { get; set; }
        public int numberdate { get; set; }
        public int Month { get; set; }
        public TimeSpan timework { get; set; }
        public TimeSpan timeOverTime { get; set; }
        public TimeSpan CheckMin { get; set; }
        public TimeSpan CheckMax { get; set; }
        public TimeSpan timeworkApproved { get; set; }
    }
}
