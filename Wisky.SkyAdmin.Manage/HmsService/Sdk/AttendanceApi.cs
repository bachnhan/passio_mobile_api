using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class AttendanceApi
    {
        public IQueryable<Attendance> GetListAttendanceByStatusAndTimeRange(DateTime startTime, DateTime endtime, int status, int employeeId)
        {
            //return this.BaseService.GetListAttendanceByStatusAndTimeRange(startTime, endtime, (int)StatusAttendanceEnum.Processing, employeeId);
            return this.BaseService.GetListAttendanceByStatusAndTimeRange(startTime, endtime, status, employeeId);
        }

        public IQueryable<Attendance> GetAttendanceInProcessByEmpId(DateTime startTime, DateTime endTime, int empId)
        {
            return this.BaseService.GetAttendanceInProcessByEmpId(startTime, endTime, empId);
        }


        public IQueryable<Attendance> GetAttendanceByEmpIdAndStoreByTimeRange(int empId, int storeId, DateTime startTime, DateTime endTime)
        {
            return this.BaseService.Get(q => q.Active == true && q.StoreId == storeId && q.EmployeeId == empId
                                        && q.ShiftMin >=  startTime && q.ShiftMin <= endTime); 
        }


        public IQueryable<Attendance> GetAttendanceByRequestByTimeRange(int storeId, DateTime startTime, DateTime endTime)
        {
            //return this.BaseService.Get(q => q.Active == true && q.StoreId == storeId && q.IsRequested == (int)IsRequest.Request
            //                                 && q.ShiftMin >= startTime && q.ShiftMin <= endTime);
            return this.BaseService.Get(q => q.Active == true && q.StoreId == storeId && q.IsRequested != null
                                             && q.ShiftMin >= startTime && q.ShiftMin <= endTime);
        }

        public Attendance GetAttendanceByEmpIdAndShift(int empId, DateTime startShift, DateTime endShift)
        {
            return this.BaseService.Get(q => q.EmployeeId == empId && (startShift < q.ShiftMax && startShift >= q.ShiftMin)
                                        || (endShift <= q.ShiftMax && endShift >= q.ShiftMin)).FirstOrDefault();
        }

        public bool Update(AttendanceViewModel model)
        {
            try
            {
                var entity = model.ToEntity();
                this.BaseService.Update(entity);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public bool UpdateEntity(Attendance model)
        {
            try
            {
                this.BaseService.Update(model);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public IQueryable<Attendance> GetAttendanceByTimeRange( DateTime startDate, DateTime endDate, List<int> listStore)
        {
            return this.BaseService.GetActive(q =>q.ShiftMin >= startDate && q.ShiftMin <= endDate && listStore.Any(d => d == q.StoreId));
        }        
        public IQueryable<Attendance> GetAttendanceByTimeRange2(DateTime startDate, DateTime endDate, int storeId, int empId)
        {
            return this.BaseService.GetActive(q => q.Active == true && ((q.ShiftMin > startDate && q.ShiftMin < endDate) ||
                                                                        (q.ShiftMax > startDate && q.ShiftMax < endDate) ||
                                                                        (q.ShiftMin <= startDate && q.ShiftMax >= endDate)) &&                                                                        
                                                                        q.StoreId == storeId && q.EmployeeId == empId);
        }
        public IQueryable<Attendance> GetAttendanceByTimeRangeAndStore(int storeId, DateTime startDate, DateTime endDate)
        {
           return this.BaseService.GetActive(q => q.Active && q.StoreId == storeId && q.ShiftMin >= startDate && q.ShiftMin <= endDate);
        }

        public IEnumerable<Attendance> GetAttendanceByTimeRangeAndStore2(int storeId, DateTime startDate, DateTime endDate)
        {
            IEnumerable<Attendance> result = this.BaseService.Get(q => q.Active == true && q.StoreId == storeId && ((q.ShiftMin >= startDate && q.ShiftMin <= endDate) || (q.ShiftMax >= startDate && q.ShiftMax <= endDate)));
            return result;
        }
        public IEnumerable<Attendance> GetAttendanceByTimeRangeAndBrand(List<int> listStore, DateTime startDate, DateTime endDate)
        {
            IEnumerable<Attendance> result = this.BaseService.Get(q =>listStore.Any(d => d == q.StoreId) && ((q.ShiftMin >= startDate && q.ShiftMin <= endDate) || (q.ShiftMax >= startDate && q.ShiftMax <= endDate)) && q.Active == true);
            return result;
        }
        public IEnumerable<Attendance> GetAttendanceByStoreByTimeRange(int storeId, DateTime startTime, DateTime endTime)
        {
            return this.BaseService.Get(q => q.Active == true && q.StoreId == storeId
                                        && q.ShiftMin >= startTime && q.ShiftMin <= endTime);
        }
    
        public Attendance GetAttendanceById(int Id)
        {
            return this.BaseService.Get(q => q.Id==Id).FirstOrDefault();
        }
    }
}
