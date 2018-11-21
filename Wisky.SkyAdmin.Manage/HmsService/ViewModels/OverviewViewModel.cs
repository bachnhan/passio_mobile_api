using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
   public class OverviewViewModel
    {
        public virtual System.TimeSpan shiftMin { get; set; }
        public virtual System.TimeSpan shiftMax { get; set; }
        public virtual int EmployeeId { get; set; }
        public virtual string date { get; set; }
        public virtual System.TimeSpan checkMin { get; set; }
        public virtual System.TimeSpan checkMax { get; set; }
        public virtual Nullable<int>  datePicked { get; set; }
    }
}
