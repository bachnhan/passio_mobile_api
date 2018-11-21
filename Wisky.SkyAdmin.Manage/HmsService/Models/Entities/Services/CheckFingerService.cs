using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ICheckFingerService
    {
        CheckFinger GetEmployeeCheckByEmpIdAndTime(int employeeId, DateTime time);
    }
    public partial class CheckFingerService
    {
        public CheckFinger GetEmployeeCheckByEmpIdAndTime(int employeeId, DateTime time)
        {
            return this.FirstOrDefault(q => q.EmployeeId == employeeId && q.DateTime == time);
        }
    }
}
