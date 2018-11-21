using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IEmployeeFingerService
    {
        EmployeeFinger GetEmployeeFinger(int empId, string enrollNumber, int fingerIndex);
    }

    public partial class EmployeeFingerService
    {
        public EmployeeFinger GetEmployeeFinger(int empId, string enrollNumber, int fingerIndex)
        {
            return this.FirstOrDefault(q => q.EmpId == empId
                                           && q.EmpEnrollNumber == enrollNumber
                                           && q.FingerIndex == fingerIndex);
        }
    }
}
