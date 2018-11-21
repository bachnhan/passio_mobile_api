using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IAttendanceService
    {
        IQueryable<Attendance> GetListAttendanceByStatusAndTimeRange(DateTime startTime, DateTime endtime, int status, int employeeId);
        IQueryable<Attendance> GetAttendanceInProcessByEmpId(DateTime startTime, DateTime endTime, int empId);
    }
    public partial class AttendanceService
    {

        public IQueryable<Attendance> GetListAttendanceByStatusAndTimeRange(DateTime startTime, DateTime endtime, int status, int employeeId)
        {
            if (status == (int)StatusAttendanceEnum.Processing)
            return this.Get(q => q.EmployeeId == employeeId && q.Active == true
                        && q.ShiftMin >= startTime && q.ShiftMax <= endtime && q.Status == (int)StatusAttendanceEnum.Processing);
            else if (status == (int)StatusAttendanceEnum.Approved)
            {
                return this.Get(q => q.EmployeeId == employeeId && q.Active == true
                                     && q.ShiftMin >= startTime && q.ShiftMax <= endtime && q.Status == (int)StatusAttendanceEnum.Approved);
            }
            else
            {
                return this.Get(q => q.EmployeeId == employeeId && q.Active == true
                                     && q.ShiftMin >= startTime && q.ShiftMax <= endtime && q.Status == (int)StatusAttendanceEnum.Reject);
            }
        }

        public IQueryable<Attendance> GetAttendanceInProcessByEmpId(DateTime startTime, DateTime endTime, int empId)
        {
            return this.Get(q => q.EmployeeId == empId && q.ShiftMin >= startTime && q.ShiftMax <= endTime
           && (q.Status == (int)StatusAttendanceEnum.Approved || q.Status == (int)StatusAttendanceEnum.Processing));
        }

    }
}
