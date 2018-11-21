using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace HmsService.Sdk
{
    public partial class EmployeeApi
    {
        public EmployeeViewModel GetEmployee(int id)
        {
            var entity = this.BaseService.GetEmployeeById(id);
            if (entity == null) return null;
            return new EmployeeViewModel(entity);
        }

        public StoreAttendanceApply GetAllEmployeeFreeByTimeSpan1(DateTime startDate, DateTime endDate, TimeSpan shiftMin, TimeSpan shiftMax, int storeId/*, int empGroupId*/, int brandId)
        {
            var resultCanApprove = new List<Employee>();
            var resultCanNotApprove = new List<Employee>();
            var attendanceApi = new AttendanceApi();
            var employeeInstoreApi = new EmployeeInStoreApi();
            IEnumerable<EmployeeInStore> currentEmployeeInStore = employeeInstoreApi.GetCurrentEmployeeInStore(storeId);
            IEnumerable<Employee> employeesBrands = this.GetEmployeesByBrand(brandId);
            var allEmployee = from currentEmp in currentEmployeeInStore
                                   join empBrand in employeesBrands
                                   on currentEmp.EmployeeId equals empBrand.Id
                                   //where empBrand.EmployeeGroupId == empGroupId
                                   select empBrand;
            IEnumerable<Attendance> listAttendances = attendanceApi.BaseService.Get(q => q.Active == true && q.ShiftMin >= startDate && q.ShiftMax <= endDate && q.StoreId == storeId);
            foreach (var item in allEmployee)
            {
                if (!listAttendances.Any(q => q.EmployeeId == item.Id && (
                (q.ShiftMin >= q.ShiftMin.GetStartOfDate().Add(shiftMin) && q.ShiftMin < q.ShiftMin.GetStartOfDate().Add(shiftMax))
                || (q.ShiftMax > q.ShiftMax.GetStartOfDate().Add(shiftMax) && q.ShiftMax <= q.ShiftMax.GetStartOfDate().Add(shiftMax))
                )))
                {
                    resultCanApprove.Add(item);
                }
                else
                {
                    resultCanNotApprove.Add(item);
                }
            }
            var result = new StoreAttendanceApply();
            result.ListCanApprove = resultCanApprove;
            result.ListCannotApprove = resultCanNotApprove;
            return result;
        }
        public StoreAttendanceApply GetAllEmployeeFreeByTimeSpan(DateTime startDate, DateTime endDate, TimeSpan shiftMin, TimeSpan shiftMax, int storeId, int empGroupId, int brandId)
        {
            var resultCanApprove = new List<Employee>();
            var resultCanNotApprove = new List<Employee>();
            var attendanceApi = new AttendanceApi();
            var employeeInstoreApi = new EmployeeInStoreApi();
            IEnumerable<EmployeeInStore> currentEmployeeInStore = employeeInstoreApi.GetCurrentEmployeeInStore(storeId);
            IEnumerable<Employee> employeesBrands = this.GetEmployeesByBrand(brandId);
            var allEmployee = from currentEmp in currentEmployeeInStore
                              join empBrand in employeesBrands
                              on currentEmp.EmployeeId equals empBrand.Id
                              where empBrand.EmployeeGroupId == empGroupId
                              select empBrand;
            IEnumerable<Attendance> listAttendances = attendanceApi.BaseService.Get(q => q.Active == true && q.ShiftMin >= startDate && q.ShiftMax <= endDate && q.StoreId == storeId);
            foreach (var item in allEmployee)
            {
                if (!listAttendances.Any(q => q.EmployeeId == item.Id && (
                (q.ShiftMin >= q.ShiftMin.GetStartOfDate().Add(shiftMin) && q.ShiftMin < q.ShiftMin.GetStartOfDate().Add(shiftMax))
                || (q.ShiftMax > q.ShiftMax.GetStartOfDate().Add(shiftMax) && q.ShiftMax <= q.ShiftMax.GetStartOfDate().Add(shiftMax))
                )))
                {
                    resultCanApprove.Add(item);
                }
                else
                {
                    resultCanNotApprove.Add(item);
                }
            }
            var result = new StoreAttendanceApply();
            result.ListCanApprove = resultCanApprove;
            result.ListCannotApprove = resultCanNotApprove;
            return result;
        }
        public List<StoreAttendanceApply> GetAllEmployeeFreeByTimeSpanByDate(TimeSpan shiftMin, TimeSpan shiftMax, List<int> dateList, DateTime endDate, DateTime startDate, int storeId/*, int empGroupId*/, int brandId)
        {
            //var resultCanApprove = new List<Employee>();            //danh sách nhân viên rảnh ở khung giờ đc chọn
            //var resultCanNotApprove = new List<Employee>();         //danh sách nhân viên bận ở khung giờ đc chọn
            var attendanceApi = new AttendanceApi();
            var employeeInstoreApi = new EmployeeInStoreApi();
            //lấy tất cả nhân viên thuộc store
            IEnumerable<EmployeeInStore> currentEmployeeInStore = employeeInstoreApi.GetCurrentEmployeeInStore(storeId);
            //lấy tất cả nhân viên thuộc brand
            IEnumerable<Employee> employeesBrands = this.GetEmployeesByBrand(brandId);

            //lấy tất cả nhân viên thuộc cùng group, cùng store và cùng brand
            var allEmployee = from currentEmp in currentEmployeeInStore
                              join empBrand in employeesBrands
                              on currentEmp.EmployeeId equals empBrand.Id
                              //where empBrand.EmployeeGroupId == empGroupId
                              select empBrand;

            var listDate = new List<DateTime>();

            var listResult = new List<StoreAttendanceApply>();
            for (var i = 0; i < dateList.Count; i++)
            {
                var current = startDate.AddDays(dateList[i]);
                var currentEnd = Utils.GetEndOfDate(current);
                var timeStart = current.Add(shiftMin);
                var timeEnd = current.Add(shiftMax);
                try
                {
                    var resultCanApprove = new List<Employee>();
                    var resultCanNotApprove = new List<Employee>();

                    IEnumerable<Attendance> listAttendances = attendanceApi.BaseService.Get(q => q.Active == true && q.ShiftMin >= timeStart &&
                                                                                                                    q.ShiftMax <= timeEnd &&
                                                                                                                    q.StoreId == storeId);
                    
                    //listAttendances = listAttendances.Where(q => dateOfWeek.Any(d => d == (int)q.ShiftMin.DayOfWeek));                
                    foreach (var emp in allEmployee)
                    {
                        //if (!listAttendances.Any(q => q.EmployeeId == emp.Id))
                        if (!listAttendances.Any(q => q.EmployeeId == emp.Id && (
                        (q.ShiftMin >= q.ShiftMin.GetStartOfDate().Add(shiftMin) && q.ShiftMin < q.ShiftMin.GetStartOfDate().Add(shiftMax))
                        || (q.ShiftMax > q.ShiftMax.GetStartOfDate().Add(shiftMax) && q.ShiftMax <= q.ShiftMax.GetStartOfDate().Add(shiftMax))
                        )))
                        {
                            resultCanApprove.Add(emp);
                        }
                        else
                        {
                            resultCanNotApprove.Add(emp);
                        }
                    }
                    var result = new StoreAttendanceApply();
                    result.ListCanApprove = resultCanApprove;
                    result.ListCannotApprove = resultCanNotApprove;
                    listResult.Add(result);
                }
                catch (Exception e)
                {

                }
            }
            var temp = new StoreAttendanceApply();
            foreach (var res in listResult)
            {
                if (res.ListCannotApprove.Any())
                {
                    temp.ListCanApprove = res.ListCanApprove;
                    temp.ListCannotApprove = res.ListCannotApprove;
                }
            }
            //return temp.ListCannotApprove != null ? temp : listResult;
            return listResult;
        }
        //public StoreAttendanceApply GetAllEmployeeFreeByTimeSpanByDate(DateTime startDate, DateTime endDate, TimeSpan shiftMin, TimeSpan shiftMax, int storeId, int empGroupId, int brandId, List<int> dateOfWeek)
        //{
        //    var resultCanApprove = new List<Employee>();
        //    var resultCanNotApprove = new List<Employee>();
        //    var attendanceApi = new AttendanceApi();
        //    var employeeInstoreApi = new EmployeeInStoreApi();
        //    IEnumerable<EmployeeInStore> currentEmployeeInStore = employeeInstoreApi.GetCurrentEmployeeInStore(storeId);
        //    IEnumerable<Employee> employeesBrands = this.GetEmployeesByBrand(brandId);
        //    var allEmployee = from currentEmp in currentEmployeeInStore
        //                      join empBrand in employeesBrands
        //                      on currentEmp.EmployeeId equals empBrand.Id
        //                      where empBrand.EmployeeGroupId == empGroupId
        //                      select empBrand;
        //    IEnumerable<Attendance> listAttendances = attendanceApi.BaseService.Get(q => q.Active == true && q.ShiftMin >= startDate && q.ShiftMax <= endDate && q.StoreId == storeId);

        //    listAttendances = listAttendances.Where(q => dateOfWeek.Any(d => d == (int)q.ShiftMin.DayOfWeek));
        //    foreach (var item in allEmployee)
        //    {
        //        if (!listAttendances.Any(q => q.EmployeeId == item.Id && (
        //        (q.ShiftMin >= q.ShiftMin.GetStartOfDate().Add(shiftMin) && q.ShiftMin < q.ShiftMin.GetStartOfDate().Add(shiftMax))
        //        || (q.ShiftMax > q.ShiftMax.GetStartOfDate().Add(shiftMax) && q.ShiftMax <= q.ShiftMax.GetStartOfDate().Add(shiftMax))
        //        )))
        //        {
        //            resultCanApprove.Add(item);
        //        }
        //        else
        //        {
        //            resultCanNotApprove.Add(item);
        //        }
        //    }

        //    var result = new StoreAttendanceApply();
        //    result.ListCanApprove = resultCanApprove;
        //    result.ListCannotApprove = resultCanNotApprove;
        //    return result;
        //}       
        //public StoreAttendanceApply GetAllEmployeeFreeByTimeSpanByDate(DateTime shiftMin, DateTime shiftMax, int storeId, int empGroupId, int brandId)
        //{
        //    var resultCanApprove = new List<Employee>();            //danh sách nhân viên rảnh ở khung giờ đc chọn
        //    var resultCanNotApprove = new List<Employee>();         //danh sách nhân viên bận ở khung giờ đc chọn
        //    var attendanceApi = new AttendanceApi();
        //    var employeeInstoreApi = new EmployeeInStoreApi();

        //    //lấy tất cả nhân viên thuộc store
        //    IEnumerable<EmployeeInStore> currentEmployeeInStore = employeeInstoreApi.GetCurrentEmployeeInStore(storeId);
        //    //lấy tất cả nhân viên thuộc brand
        //    IEnumerable<Employee> employeesBrands = this.GetEmployeesByBrand(brandId);

        //    //lấy tất cả nhân viên thuộc cùng group, cùng store và cùng brand
        //    var allEmployee = from currentEmp in currentEmployeeInStore
        //                      join empBrand in employeesBrands
        //                      on currentEmp.EmployeeId equals empBrand.Id
        //                      where empBrand.EmployeeGroupId == empGroupId
        //                      select empBrand;

        //    IEnumerable<Attendance> listAttendances = attendanceApi.BaseService.Get(q => q.Active == true && q.ShiftMin == shiftMin && q.ShiftMax == shiftMax && q.StoreId == storeId);

        //    //listAttendances = listAttendances.Where(q => dateOfWeek.Any(d => d == (int)q.ShiftMin.DayOfWeek));
        //    foreach (var item in allEmployee)
        //    {
        //        if (!listAttendances.Any(q => q.EmployeeId == item.Id))
        //        {
        //            resultCanApprove.Add(item);
        //        }
        //        else
        //        {
        //            resultCanNotApprove.Add(item);
        //        }
        //    }

        //    var result = new StoreAttendanceApply();
        //    result.ListCanApprove = resultCanApprove;
        //    result.ListCannotApprove = resultCanNotApprove;
        //    return result;
        //}      

        public Employee GetEmployeeByEmp(string numbercode)
        {
            return this.BaseService.Get(q => q.EmpEnrollNumber == numbercode).FirstOrDefault();
        }
        public EmployeeViewModel GetEmployee(string enrollNumber)
        {
            var entity = this.BaseService.GetEmployeeByEnroll(enrollNumber);
            if (entity == null) return null;
            return new EmployeeViewModel(entity);
        }

        public IEnumerable<Employee> GetEmployeeByStoreId(int storeId, int brandId)
        {
            if (storeId == 0) // For Brand
            {
                return this.BaseService.Get(q => q.BrandId == brandId && q.Active == true);
            }
            else
            {
                return this.BaseService.Get(q => q.MainStoreId == storeId && q.Active == true);
            }
        }

        public IQueryable<Employee> GetEmployeesByBrand(int brandId)
        {
            return this.BaseService.Get(q => q.BrandId == brandId && q.Active == true);
        }
        public Employee GetEmployeesByEnroll(string enroll)
        {
            return this.BaseService.Get(q => q.EmpEnrollNumber == enroll).FirstOrDefault();
        }
        public IEnumerable<Employee> GetEmployeeNonWorkingByStoreId(int storeId, int brandId)
        {
            if (storeId == 0) // For Brand
            {
                return this.BaseService.Get(q => q.BrandId == brandId && q.Active == false);
            }
            else
            {
                return this.BaseService.Get(q => q.MainStoreId == storeId && q.Active == false);
            }

        }

        public int Add(EmployeeViewModel model)
        {
            var entity = model.ToEntity();
            this.BaseService.Create(entity);
            return entity.Id;
        }
        public IEnumerable<Employee> GetEmployeeByBrandId(int brandID)
        {
            return this.BaseService.GetEmployeeByBrandId(brandID);
        }
        public IEnumerable<Employee> GetEmployeeByStoreId(int storeID)
        {
            return this.BaseService.GetEmployeeByStoreId(storeID);
        }
        public IQueryable<Employee> GetEmployeeStoredIdIQ(int storeID)
        {
            return this.BaseService.GetEmployeeByStoreId(storeID);
        }
        public IQueryable<Employee> GetEmployeeBrandIdIQ(int brandId)
        {
            return this.BaseService.GetEmployeeByBrandId(brandId);
        }

        public bool CheckTimeSpanOfEmployee(DateTime startDate, DateTime endDate, TimeSpan shiftMin, TimeSpan shiftMax, int empId)
        {
            var attendanceApi = new AttendanceApi();
            IEnumerable<Attendance> listAttendances = attendanceApi.BaseService.Get(q => q.Active == true && q.ShiftMin >= startDate && q.ShiftMax <= endDate && q.EmployeeId <= empId);
            if (!listAttendances.Any(q => q.EmployeeId == empId && (
                (q.ShiftMin >= q.ShiftMin.GetStartOfDate().Add(shiftMin) && q.ShiftMin < q.ShiftMin.GetStartOfDate().Add(shiftMax))
                || (q.ShiftMax > q.ShiftMax.GetStartOfDate().Add(shiftMax) && q.ShiftMax <= q.ShiftMax.GetStartOfDate().Add(shiftMax))
                )))
            {
                return false;
            }
            return true;
        }
    }


    public class StoreAttendanceApply
    {
        public List<Employee> ListCanApprove { set; get; }
        public List<Employee> ListCannotApprove { set; get; }

    }
}
