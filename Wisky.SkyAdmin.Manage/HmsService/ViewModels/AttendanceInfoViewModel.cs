using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class AttendanceInfoViewModel
    {
        public int employeeId { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public int status { get; set; }
        public string employeeName { get; set; }
    }
}
