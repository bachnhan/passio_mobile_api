using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using System.Web.Script.Serialization;
using HmsService.Models.Entities;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan.Controllers
{
    public class OverviewController : DomainBasedController
    {
        public class ReturnObject
        {
            public List<OverviewViewModel> listAttendance;
            public List<EmployeeViewModel> listEmployee;
            public List<CheckFingerViewModel> listCheckFinger;
        }
        public class EmployeeResult
        {
            public int Id;
            public string Name;
            public string ListAttendance;
            public string ListCheckFinger;

        }

        // GET: FingerScan/Overview
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult IndexList(string dateSearch, int storeId)
        {
            var date = dateSearch.ToDateTime();
            var startDate = date.GetStartOfDate();
            var endDate = date.GetEndOfDate();
            var employeeApi = new EmployeeApi();
            var employeeInstore = employeeApi.GetActive().Where(q => q.MainStoreId == storeId).ToList();
            return null;
        }
        public ActionResult GetAllEmployee(JQueryDataTableParamModel param, int storeId, string dateSearch)
        {
            var EmployeeApi = new EmployeeApi();
            var attendanceApi = new AttendanceApi();
            var checkfingerApi = new CheckFingerApi();
            var day = DateTime.Today.ToString("dd/MM/yyyy");
            var date = day.ToDateTime();
            if (dateSearch != "")
            {
                date = dateSearch.ToDateTime();

            }
            var startDate = date.GetStartOfDate();
            var endDate = date.GetEndOfDate();
            var count = param.iDisplayStart;
            var employeeTableSelect = EmployeeApi.GetEmployeeByStoreId(storeId).ToList();
            var attendanceTableSelect = attendanceApi.GetAttendanceByStoreByTimeRange(storeId, startDate, endDate);
            var checkFingerTableSelect = checkfingerApi.GetCheckFingerByStoreByTimeRange(storeId, startDate, endDate);
            // Get Employee
            var employees = EmployeeApi.GetEmployeeByStoreId(storeId).Skip(param.iDisplayStart)
               .Take(param.iDisplayLength).ToList();

            var totalRecords = employeeTableSelect.Count();
            var displayRecord = employeeTableSelect.Count();
            // Get Attendance Data
            var AttendanceResultList = from employeeTable in employees
                             join attendanceTable in attendanceApi.Get() on employeeTable.Id equals attendanceTable.EmployeeId
                             where (employeeTable.MainStoreId == storeId && attendanceTable.ShiftMin >= startDate && attendanceTable.ShiftMax <= endDate)
                             select new { empId = employeeTable.Id, shiftMin = attendanceTable.ShiftMin.TimeOfDay, shiftMax = attendanceTable.ShiftMax.TimeOfDay, checkMin = attendanceTable.CheckMin, checkMax = attendanceTable.CheckMax };
            // Get CheckFinger Data
            var datatableFingerCheck = from employeeTable in employees
                                       join checkFingerTable in checkFingerTableSelect on employeeTable.Id equals checkFingerTable.EmployeeId
                                       where (employeeTable.MainStoreId == storeId && checkFingerTable.DateTime >= startDate && checkFingerTable.DateTime <= endDate)
                                       select new { EmployeeId = employeeTable.Id, checkFinger = checkFingerTable.DateTime.TimeOfDay, Id = checkFingerTable.Id };
          
            //return list Employ
            List<EmployeeResult> EmployeeList = new List<EmployeeResult>();
            foreach (var item in employees)
            {
                var employee = new EmployeeResult();
                employee.Id = item.Id;
                employee.Name = item.Name;
                List<CheckFingerViewModel> fingercheckList = new List<CheckFingerViewModel>();// List of CheckFInger
                List<OverviewViewModel> attendanceList = new List<OverviewViewModel>();// Get List Of Attendance

                foreach (var i in datatableFingerCheck)
                {
                    if (i.EmployeeId == item.Id)
                    {
                        var resultItem = new CheckFingerViewModel();
                        resultItem.Id = i.Id;
                       // resultItem.TimeCheck = i.checkFinger;
                        resultItem.EmployeeId = i.EmployeeId;
                        fingercheckList.Add(resultItem);
                    }
                }
                foreach (var i in AttendanceResultList)
                {
                    var AttendanceResultItem = new OverviewViewModel();
                    if (i.empId== item.Id)
                    {
                        if (i.checkMin != null && i.checkMax != null)
                        {
                            var CheckMin = i.checkMin ?? DateTime.Now;
                            var CheckMax = i.checkMax ?? DateTime.Now;
                            AttendanceResultItem.checkMin = CheckMin.TimeOfDay;
                            AttendanceResultItem.checkMax = CheckMax.TimeOfDay;
                            AttendanceResultItem.shiftMin = i.shiftMin;
                            AttendanceResultItem.shiftMax = i.shiftMax;
                            AttendanceResultItem.EmployeeId = item.Id;
                            attendanceList.Add(AttendanceResultItem);
                        }
                        else
                        {
                            AttendanceResultItem.shiftMin = i.shiftMin;
                            AttendanceResultItem.shiftMax = i.shiftMax;
                            AttendanceResultItem.EmployeeId = item.Id;
                            attendanceList.Add(AttendanceResultItem);
                        }
                    }
                }
                var jsonSerialiser = new JavaScriptSerializer();
                employee.ListCheckFinger = jsonSerialiser.Serialize(fingercheckList);
                employee.ListAttendance = jsonSerialiser.Serialize(attendanceList);
                EmployeeList.Add(employee);
            }
            var result = EmployeeList
                    .Select(a => new IConvertible[]{
                        a.Name,
                        a.Id,
                        a.ListCheckFinger,
                        a.ListAttendance
                    }).ToList();
                return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = displayRecord,
                aaData = result,
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ShowAttendanceSet(int EmpId, int CheckFingerId, string date, int brandId, string dateSearch, int Choice)
        {
            var empApi = new EmployeeApi();
            var checkfingerApi = new CheckFingerApi();
            var emp = empApi.GetActive().Where(q => q.Id == EmpId).FirstOrDefault();
            var model = new OverviewViewModel();
            var timeFrameapi = new TimeFrameApi();
            model.EmployeeId = emp.Id;
            if (Choice == 1)
            {
                var day = DateTime.Today.ToString("dd/MM/yyyy");
                var time = day.ToDateTime();
                if (dateSearch != "")
                {

                }
                {
                    time = dateSearch.ToDateTime();

                }
                var startDate = time.GetStartOfDate();
                var endDate = time.GetEndOfDate();
                if (date != null)
                {

                    string[] arrHour = date.Split(':');
                    if (arrHour[0].Count() == 1)
                    {
                        arrHour[0] = "0" + arrHour[0];
                    }
                    if (arrHour[1].Count() == 1)
                    {
                        arrHour[1] = "0" + arrHour[1];
                    }
                    date = arrHour[0] + ":" + arrHour[1];

                    var timeFrameSelected = timeFrameapi.GetActive().Where(q => q.BrandId == brandId && q.StartTime <= Utils.ToHourTime(date).TimeOfDay && q.EndTime >= Utils.ToHourTime(date).TimeOfDay).FirstOrDefault();

                    if (timeFrameSelected != null)
                    {
                        model.datePicked = timeFrameSelected.Id;
                    }
                    model.checkMin = Utils.ToHourTime(date).TimeOfDay;
                }
            }

            return PartialView(model);
        }
        public ActionResult CreateShift(int storeId, String day, int Id, TimeSpan start, TimeSpan end, TimeSpan checkMin, TimeSpan checkMax)
        {


            var attendanceApi = new AttendanceApi();
            var toDay = DateTime.Today.ToString("dd/MM/yyyy");
            var date = Utils.ToDateTime(toDay);
            if (day != "")
            {
                date = day.ToDateTime();
            }
            var currentUser = HttpContext.User.Identity.Name;
            try
            {
                var model = new AttendanceViewModel()
                {
                    EmployeeId = Id,
                    Status = 0,
                    CheckMin = date.GetStartOfDate().Add(checkMin),
                    CheckMax = date.GetStartOfDate().Add(checkMax),
                    TotalWorkTime = date.GetStartOfDate().Add(end) - date.GetStartOfDate().Add(start),
                    ProcessingStatus = (int)ProcessingStatusAttendenceEnum.UpdatedByManualCanNotOveride,
                    UpdatePerson = currentUser,
                    ShiftMax = date.GetStartOfDate().Add(end),
                    ShiftMin = date.GetStartOfDate().Add(start),
                    Active = true,
                };
                attendanceApi.Create(model);
            }
            catch (Exception e)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllTimeFrame(int brandId)
        {
            var timeFrameapi = new TimeFrameApi();
            var timeframe = timeFrameapi.GetActive().Where(q => q.BrandId == brandId).ToList();
            var timeFrameSelect = from timeframeTable in timeframe
                                  select new { Id = timeframeTable.Id, Name = timeframeTable.Name };



            return Json(new { timeFrameSelect }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTimeStartAndEnd(int Id)
        {
            var timeFrameapi = new TimeFrameApi();
            var timeframe = timeFrameapi.GetActive().Where(q => q.Id == Id).FirstOrDefault();
            var timeStart = "";
            var timeEnd = "";
            if (timeframe.StartTime.Minutes < 10)
            {
                timeStart = timeframe.StartTime.Hours + ":0" + timeframe.StartTime.Minutes;
            }
            else
            {
                timeStart = timeframe.StartTime.Hours + ":" + timeframe.StartTime.Minutes;
            }
            if (timeframe.EndTime.Minutes < 10)
            {
                timeEnd = timeframe.EndTime.Hours + ":0" + timeframe.EndTime.Minutes;
            }
            else
            {
                timeEnd = timeframe.EndTime.Hours + ":" + timeframe.EndTime.Minutes;
            }
            return Json(new { startTime = timeStart, endTime = timeEnd }, JsonRequestBehavior.AllowGet);
        }

        public class Point
        {
            public double indexPosition { get; set; }
            public string time { get; set; }
            public string store { get; set; }
        }
    }
}