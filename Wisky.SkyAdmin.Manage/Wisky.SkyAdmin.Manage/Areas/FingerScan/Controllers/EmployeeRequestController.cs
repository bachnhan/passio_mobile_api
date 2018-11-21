using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using Microsoft.Office.Interop.Excel;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan.Controllers
{
    public class EmployeeRequestController : DomainBasedController
    {
        // GET: FingerScan/EmployeeRequest
        public ActionResult Index(int employeeId)
        {
            ViewBag.empId = employeeId;
            var employeeApi = new EmployeeApi();
            //string empName = employeeApi.GetEmployee(employeeId).Name;
            var emp = employeeApi.GetEmployee(employeeId);
            ViewBag.empName = emp.Name;
            ViewBag.empPhone = emp.Phone;
            ViewBag.Age = DateTime.Now.Year - emp.DateOfBirth.Value.Year;
            return View();
        }
        public ActionResult CheckFingerEmpReq(int employeeId)
        {
            ViewBag.empId = employeeId;
            var employeeApi = new EmployeeApi();
            string empName = employeeApi.GetEmployee(employeeId).Name;
            ViewBag.empName = empName;
            return View();
        }
        public ActionResult CalendarEmpReq(int employeeId)
        {
            ViewBag.empId = employeeId;
            var employeeApi = new EmployeeApi();
            string empName = employeeApi.GetEmployee(employeeId).Name;
            ViewBag.empName = empName;
            return View();
        }
        public ActionResult GetEmployeeInfo(int employeeId)
        {
            var empApi = new EmployeeApi();
            var emp = empApi.Get(employeeId);
            var groupId = emp.EmployeeGroupId;            
            return Json(new { groupid = groupId }, JsonRequestBehavior.AllowGet);
        }        
        public ActionResult LoadSessionAttendance(JQueryDataTableParamModel param, int employeeId, string startDate, string endDate, int status, int storeId)
        {
            var attendanceApi = new AttendanceApi();
            var timeFrameApi = new TimeFrameApi();

            var startTime = startDate.ToDateTime().GetStartOfDate();            
            var endTime = endDate.ToDateTime().GetEndOfDate();

            IEnumerable<Attendance> listResult = attendanceApi.GetAttendanceByEmpIdAndStoreByTimeRange(employeeId, storeId, startTime, endTime);            
            var totalResult = listResult.Count();
            if (totalResult > 0)
            {
                switch (status)
                {
                    case (int)StatusAttendance.OnTime:
                        listResult = listResult.Where(q => q.CheckMin <= (q.ShiftMin.Add(q.ExpandTime)) && q.CheckMax >= (q.ShiftMax - q.ExpandTime));
                        break;
                    case (int)StatusAttendance.ComeLate:
                        listResult = listResult.Where(q => q.CheckMin > (q.ShiftMin.Add(q.ExpandTime)));
                        break;
                    case (int)StatusAttendance.ReturnEarly:
                        listResult = listResult.Where(q => q.CheckMax < (q.ShiftMax.Add(-q.ExpandTime)));
                        break;
                    case (int)StatusAttendance.Miss:
                        listResult = listResult.Where(q => q.CheckMin == null && q.CheckMax == null);
                        break;
                    case (int)StatusAttendance.Bothviolate:
                        listResult = listResult.Where(q => q.CheckMax < (q.ShiftMax.Add(-q.ExpandTime)) && q.CheckMin > (q.ShiftMin.Add(q.ExpandTime)));
                        break;
                }
            }
            var totalDisplay = listResult.Count();

            var count = param.iDisplayStart + 1;

            var currentDate = Utils.GetCurrentDateTime();
            var startInDate = currentDate.GetEndOfDate();
            var endInDate = currentDate.GetEndOfDate();

            var result = listResult.OrderBy(q => q.ShiftMax)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList()
                .Select(a => new IConvertible[]
                {
                    count++,
                    a.ShiftMin.ToString("dd/MM/yyyy",CultureInfo.InvariantCulture) + " - Từ " + a.ShiftMin.ToString(@"HH\:mm",CultureInfo.InvariantCulture) + " đến " + a.ShiftMax.ToString(@"HH\:mm",CultureInfo.InvariantCulture),
                    a.CheckMin != null ? a.CheckMin.Value.ToString(@"HH\:mm",CultureInfo.InvariantCulture) : "Chưa cập nhật",
                    a.CheckMax != null ? a.CheckMax.Value.ToString(@"HH\:mm", CultureInfo.InvariantCulture) : "Chưa cập nhật",
                    a.TotalWorkTime == null  ? "N/A" : DateTime.Today.Add(a.TotalWorkTime.Value).ToString(@"HH\:mm", CultureInfo.InvariantCulture),
                    (a.CheckMin == null && a.CheckMax == null) ? (int)StatusAttendance.Miss :
                        (a.CheckMax < (a.ShiftMax.Add(-a.ExpandTime)) && a.CheckMin > (a.ShiftMin.Add(a.ExpandTime))) ? (int)StatusAttendance.Bothviolate :
                            (a.CheckMax < (a.ShiftMax.Add(-a.ExpandTime))) ? (int)StatusAttendance.ReturnEarly :
                                (a.CheckMin > (a.ShiftMin.Add(a.ExpandTime))) ? (int)StatusAttendance.ComeLate: (int)StatusAttendance.OnTime,
                    (a.ShiftMin >= startInDate && a.ShiftMin <= endInDate) ? (int)CurrentStatusEnum.Current : (a.ShiftMin < startInDate && a.ShiftMin <endInDate) ? (int)CurrentStatusEnum.Past : (int)CurrentStatusEnum.Future,
                    //a.RequestedCheckIn != null ? a.RequestedCheckIn.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    //a.RequestedCheckOut != null ? a.RequestedCheckOut.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    a.Id,                                    
                    a.IsRequested == null ?  0 : a.IsRequested,                    
                });
            return Json(new
            {                
                sEcho = param.sEcho,
                iTotalRecords = totalResult,
                iTotalDisplayRecords = totalDisplay,
                data = result,
                date = listResult.Select(a => a.ShiftMin.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                firstHour = listResult.Select(a => a.ShiftMin.ToString(@"HH\:mm", CultureInfo.InvariantCulture)),
                lastHour = listResult.Select(a => a.ShiftMax.ToString(@"HH\:mm", CultureInfo.InvariantCulture)),
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllCheckFinger(int storeId, String startTime, String endTime, int empId)
        {
            var startDate = startTime.ToDateTime();
            var endDate = endTime.ToDateTime();
            startDate = startDate.GetStartOfDate();
            endDate = endDate.GetEndOfDate();
            var checkFingerApi = new CheckFingerApi();
            var fingerMachineApi = new FingerScanMachineApi();
            var count = 1;


            var checkFinger2 = checkFingerApi.GetActive().ToList().OrderByDescending(q => q.DateTime)
                .Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate && a.EmployeeId == empId)
                .Select(a => new IConvertible[]{
                    count++,
                    a.DateTime.ToString("dd/MM/yyyy HH:mm"),
                    fingerMachineApi.Get(a.FingerScanMachineId).Name,
                }).ToList();

            return Json(checkFinger2);
        }

        public ActionResult LoadCalendarAttendance(int empId, int storeId, String startTime, String endTime)
        {
            var listAttendance = new List<CalendarAttendance>();
            DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).GetStartOfDate();
            DateTime endDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday).GetEndOfDate();
            if (startTime.Length > 0)
            {
                startDate = startTime.ToDateTime().GetStartOfDate();
                endDate = endTime.ToDateTime().GetEndOfDate();
            }

            var apiAttendance = new AttendanceApi();
            var list = apiAttendance.GetActive().Where(p => p.EmployeeId == empId && p.StoreId == storeId && p.ShiftMin >= startDate && p.ShiftMin <= endDate).ToList();
            var datetimeNow = Utils.GetCurrentDateTime();
            foreach (var item in list)
            {
                var date = item.ShiftMin.Date.DayOfWeek.ToString();
                bool flag = true;
                if (listAttendance.Count() > 0)
                {
                    foreach (var item2 in listAttendance)
                    {

                        if (item2.date.Equals(date))
                        {
                            flag = false;
                        }
                    }
                }
                bool flagEx = false; //kiem tra mot ca co thuoc nhieu ngay hay khong
                double durationtodouble = 0;
                if (flag == true)
                {
                    var detail = new CalendarAttendance();
                    detail.date = date;
                    var listTimeslot = new List<TimeSlot>();
                    var timeSlot = new TimeSlot();

                    var duration = item.ShiftMax - item.ShiftMin;
                    var timesub = item.ShiftMin.GetStartOfDate();
                    double starHour = Math.Round(item.ShiftMin.Subtract(timesub).TotalHours, 1);
                    timeSlot.startHour = starHour;
                    double durationtofloat = Double.Parse(duration.TotalHours.ToString("F1"));
                    durationtodouble = (starHour + durationtofloat) - 24;
                    if (durationtodouble > 0)
                    {
                        timeSlot.duration = durationtofloat - durationtodouble;
                        flagEx = true;
                    }
                    else
                    {
                        timeSlot.duration = durationtofloat;
                    }
                    timeSlot.idAttendance = item.Id;
                    var totalWork = TimeSpan.FromHours(timeSlot.duration);
                    if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin < datetimeNow)
                    {
                        //absent
                        timeSlot.color = "#D9534F";
                    }
                    else if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin >= datetimeNow)
                    {
                        timeSlot.color = "#5BC0DE";
                        //N/A 
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                             (item.CheckMax < (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //di tre vs ve mau hong
                        timeSlot.Late = "#26a69a";
                        timeSlot.LeaveEarly = "#7e57c2";
                        timeSlot.color = "#F0AD4E";
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                             (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //di tre ve dung gio mau xanh duong
                        timeSlot.Late = "#26a69a";
                        timeSlot.color = "#F0AD4E";
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                             (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //ve som di dung gio mau tim
                        timeSlot.LeaveEarly = "#7e57c2";
                        timeSlot.color = "#F0AD4E";
                    }
                    else
                    {
                        //chuan mau xanh la cay
                        timeSlot.color = "#5CB85C";
                    }
                    timeSlot.status = date;
                    listTimeslot.Add(timeSlot);
                    var timeSlotAllDay = new TimeSlot();
                    timeSlotAllDay.color = "#7cb342";
                    timeSlotAllDay.startHour = 24;
                    timeSlotAllDay.duration = 1;
                    timeSlotAllDay.status = "Tổng giờ :" + totalWork.ToString(@"hh\:mm");
                    listTimeslot.Add(timeSlotAllDay);
                    detail.timeslot = listTimeslot;
                    detail.totalWork = totalWork;
                    listAttendance.Add(detail);
                }
                else
                {
                    var detail = listAttendance.Where(p => p.date == date).FirstOrDefault();
                    int index = listAttendance.FindLastIndex(p => p.date == date);
                    var timeSlot = new TimeSlot();
                    var duration = item.ShiftMax - item.ShiftMin;
                    var timesub = item.ShiftMin.GetStartOfDate();
                    double starHour = Math.Round(item.ShiftMin.Subtract(timesub).TotalHours, 1);
                    timeSlot.startHour = starHour;
                    double durationtofloat = Double.Parse(duration.TotalHours.ToString("F1"));
                    durationtodouble = (starHour + durationtofloat) - 24;
                    if (durationtodouble > 0)
                    {
                        timeSlot.duration = durationtofloat - durationtodouble;
                        flagEx = true;
                    }
                    else
                    {
                        timeSlot.duration = durationtofloat;
                    }
                    timeSlot.idAttendance = item.Id;
                    if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin < datetimeNow)
                    {
                        //absent
                        timeSlot.color = "#D9534F";
                    }
                    else if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin >= datetimeNow)
                    {
                        timeSlot.color = "#5BC0DE";
                        //N/A
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                             (item.CheckMax < (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //di tre vs ve mau hong
                        //timeSlot.color = "#ec407a";
                        timeSlot.Late = "#26a69a";
                        timeSlot.LeaveEarly = "#7e57c2";
                        timeSlot.color = "#F0AD4E";
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                             (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //di tre ve dung gio mau xanh duong
                        timeSlot.Late = "#26a69a";
                        timeSlot.color = "#F0AD4E";
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                             (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //ve som di dung gio mau tim
                        timeSlot.LeaveEarly = "#7e57c2";
                        timeSlot.color = "#F0AD4E";
                    }
                    else
                    {
                        //chuan mau xanh la cay
                        timeSlot.color = "#5CB85C";
                    }
                    timeSlot.status = date;
                    var totalwork = TimeSpan.FromHours(timeSlot.duration) + detail.totalWork;
                    detail.totalWork = totalwork;
                    var totalallday = detail.timeslot.FindLastIndex(p => p.idAttendance == 0);
                    detail.timeslot.RemoveAt(totalallday);//xóa total work cũ
                    detail.timeslot.Add(timeSlot);
                    //thêm total work mới
                    var timeSlotAllDay = new TimeSlot();
                    timeSlotAllDay.color = "#7cb342";
                    timeSlotAllDay.startHour = 24;
                    timeSlotAllDay.duration = 1;
                    timeSlotAllDay.status = "Tổng giờ :" + totalwork.ToString(@"hh\:mm");
                    detail.timeslot.Add(timeSlotAllDay);
                    listAttendance.Add(detail);
                    listAttendance.RemoveAt(index);
                }
                if (flagEx == true)
                {
                    var dateEx = item.ShiftMin.Date.AddDays(1).DayOfWeek.ToString();
                    bool flag3 = true;
                    foreach (var item2 in listAttendance)
                    {
                        if (item2.date.Equals(dateEx))
                        {
                            flag3 = false;
                        }
                    }
                    if (flag3 == true)
                    {
                        var detail = new CalendarAttendance();
                        detail.date = dateEx;
                        var listTimeslot = new List<TimeSlot>();
                        var timeSlot = new TimeSlot();
                        timeSlot.startHour = 0;
                        timeSlot.duration = durationtodouble;
                        timeSlot.idAttendance = item.Id;
                        var totalWork = TimeSpan.FromHours(durationtodouble);
                        if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin < datetimeNow)
                        {
                            //absent
                            timeSlot.color = "#D9534F";
                        }
                        else if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin >= datetimeNow)
                        {
                            timeSlot.color = "#5BC0DE";
                            //N/A 
                        }
                        else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                                 (item.CheckMax < (item.ShiftMax.Add(-item.ExpandTime))))
                        {
                            //di tre vs ve mau hong
                            timeSlot.Late = "#26a69a";
                            timeSlot.LeaveEarly = "#7e57c2";
                            timeSlot.color = "#F0AD4E";
                        }
                        else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                                 (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                        {
                            //di tre ve dung gio mau xanh duong
                            timeSlot.Late = "#26a69a";
                            timeSlot.color = "#F0AD4E";
                        }
                        else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                                 (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                        {
                            //ve som di dung gio mau tim
                            timeSlot.LeaveEarly = "#7e57c2";
                            timeSlot.color = "#F0AD4E";
                        }
                        else
                        {
                            //chuan mau xanh la cay
                            timeSlot.color = "#5CB85C";
                        }
                        timeSlot.status = dateEx;
                        listTimeslot.Add(timeSlot);
                        var timeSlotAllDay = new TimeSlot();
                        timeSlotAllDay.color = "#7cb342";
                        timeSlotAllDay.startHour = 24;
                        timeSlotAllDay.duration = 1;
                        timeSlotAllDay.status = "Tổng giờ :" + totalWork.ToString(@"hh\:mm");
                        timeSlotAllDay.idAttendance = 0;
                        listTimeslot.Add(timeSlotAllDay);
                        detail.timeslot = listTimeslot;
                        detail.totalWork = totalWork;
                        listAttendance.Add(detail);
                    }
                    else
                    {
                        var detail = listAttendance.Where(p => p.date == dateEx).FirstOrDefault();
                        int index = listAttendance.FindLastIndex(p => p.date == dateEx);
                        var timeSlot = new TimeSlot();
                        var duration = TimeSpan.FromHours(durationtodouble);
                        timeSlot.startHour = 0;
                        timeSlot.duration = durationtodouble;
                        timeSlot.idAttendance = item.Id;
                        if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin < datetimeNow)
                        {
                            //absent
                            timeSlot.color = "#D9534F";
                        }
                        else if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin >= datetimeNow)
                        {
                            timeSlot.color = "#5BC0DE";
                            //N/A
                        }
                        else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                                 (item.CheckMax < (item.ShiftMax.Add(-item.ExpandTime))))
                        {
                            //di tre vs ve mau hong
                            //timeSlot.color = "#ec407a";
                            timeSlot.Late = "#26a69a";
                            timeSlot.LeaveEarly = "#7e57c2";
                            timeSlot.color = "#F0AD4E";
                        }
                        else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                                 (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                        {
                            //di tre ve dung gio mau xanh duong
                            timeSlot.Late = "#26a69a";
                            timeSlot.color = "#F0AD4E";
                        }
                        else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                                 (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                        {
                            //ve som di dung gio mau tim
                            timeSlot.LeaveEarly = "#7e57c2";
                            timeSlot.color = "#F0AD4E";
                        }
                        else
                        {
                            //chuan mau xanh la cay
                            timeSlot.color = "#5CB85C";
                        }
                        timeSlot.status = dateEx;
                        var totalwork = TimeSpan.FromHours(timeSlot.duration) + detail.totalWork;
                        var totalallday = detail.timeslot.FindLastIndex(p => p.idAttendance == 0);
                        detail.totalWork = totalwork;
                        detail.timeslot.RemoveAt(totalallday);//xóa total work cũ
                        detail.timeslot.Add(timeSlot);
                        //thêm total work mới
                        var timeSlotAllDay = new TimeSlot();
                        timeSlotAllDay.color = "#7cb342";
                        timeSlotAllDay.startHour = 24;
                        timeSlotAllDay.duration = 1;
                        timeSlotAllDay.status = "Tổng giờ :" + totalwork.ToString(@"hh\:mm");
                        timeSlotAllDay.idAttendance = 0;
                        detail.timeslot.Add(timeSlotAllDay);
                        listAttendance.Add(detail);
                        listAttendance.RemoveAt(index);
                    }
                }
            }

            return Json(new { rs = listAttendance }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DetailsAttendance(int id)
        {
            var attendanceApi = new AttendanceApi();
            var result = attendanceApi.Get(id);
            var empApi = new EmployeeApi();
            var datetimeNow = Utils.GetCurrentDateTime();
            string status = "";
            if (result.CheckMin == null && result.CheckMax == null && result.ShiftMin < datetimeNow)
            {
                //absent mau den
                status = "Vắng";
            }
            else if (result.CheckMin == null && result.CheckMax == null && result.ShiftMin >= datetimeNow)
            {
                status = "Tương lai";
                //N/A mau do
            }
            else if ((result.CheckMin > (result.ShiftMin.Add(result.ExpandTime))) &&
                     (result.CheckMax < (result.ShiftMax.Add(-result.ExpandTime))))
            {
                //di tre vs ve mau hong
                status = "Đi trễ về sớm";
            }
            else if ((result.CheckMin > (result.ShiftMin.Add(result.ExpandTime))) &&
                     (result.CheckMax >= (result.ShiftMax.Add(-result.ExpandTime))))
            {
                //di tre ve dung gio mau xanh duong
                status = "Đi trễ";
            }
            else if ((result.CheckMin > (result.ShiftMin.Add(result.ExpandTime))) &&
                     (result.CheckMax >= (result.ShiftMax.Add(-result.ExpandTime))))
            {
                //ve som di dung gio mau tim
                status = "Về sớm";
            }
            else
            {
                //chuan mau xanh la cay
                status = "Đúng giờ";
            }


            var tmpModel = new
            {
                Name = empApi.Get(result.EmployeeId).Name,
                StartHour = result.ShiftMin.ToString(),
                EndHour = result.ShiftMax.ToString(),
                Status = status,
                StartTime = result.CheckMin == null ? "N/A" : result.CheckMin.ToString(),
                EndTime = result.CheckMax == null ? "N/A" : result.CheckMax.ToString(),
            };
            return Json(tmpModel);
        }

        public ActionResult PrepareEditRequest(int attId)
        {
            var attApi = new AttendanceApi();
            var model = attApi.Get(attId);
            var tmpModel = new
            {
                id = model.Id,
                shiftMin = model.ShiftMin.ToString("dd/MM/yyyy HH:mm"),
                shiftMax = model.ShiftMax.ToString("dd/MM/yyyy HH:mm"),
                chekMin = model.CheckMin != null ? model.CheckMin.Value.ToString(@"HH\:mm", CultureInfo.InvariantCulture) : "Chưa cập nhật",
                chekMax = model.CheckMax != null ? model.CheckMax.Value.ToString(@"HH\:mm", CultureInfo.InvariantCulture) : "Chưa cập nhật",
                requsetCheckIn = model.RequestedCheckIn != null ? model.RequestedCheckIn.Value.ToString("dd/MM/yyyy HH:mm") : "",
                requsetCheckOut = model.RequestedCheckOut != null ? model.RequestedCheckOut.Value.ToString("dd/MM/yyyy HH:mm") : ""
            };
            return Json(new { rs = tmpModel }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditRequest(int attId, string strDateRequestCheckIn, string strDateRequestCheckOut, string strCurrentDate)
        {
            bool result = false;
            int flagCheckIn = 0;
            int flagCheckOut = 0;
            var attApi = new AttendanceApi();
            var model = attApi.Get(attId);

            var dateRequestCheckIn = new DateTime();
            var dateRequestCheckOut = new DateTime();
            if (strDateRequestCheckIn.Trim().Length > 0)
            {
                dateRequestCheckIn = Utils.ToDateTimeHour(strCurrentDate + " " + strDateRequestCheckIn);
                flagCheckIn = 1;
            }

            if (strDateRequestCheckOut.Trim().Length > 0)
            {
                if (strDateRequestCheckOut.ToHourTime() < model.ShiftMin.ToString("HH:mm").ToHourTime())
                {
                    dateRequestCheckOut = Utils.ToDateTimeHour(strCurrentDate.ToDateTime().AddDays(1).ToString("dd/MM/yyyy") + " " + strDateRequestCheckOut);
                }
                else
                {
                    dateRequestCheckOut = Utils.ToDateTimeHour(strCurrentDate + " " + strDateRequestCheckOut);
                }
                flagCheckOut = 1;
            }

            try
            {
                if (flagCheckIn == 1)
                {
                    model.RequestedCheckIn = dateRequestCheckIn;
                    model.IsRequested = 0;
                }
                if (flagCheckOut == 1)
                {
                    model.RequestedCheckOut = dateRequestCheckOut;
                    model.IsRequested = 0;
                }
                attApi.Edit(attId, model);
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }
            return Json(new
            {
                rs = result
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateMoreAttendanceAction(int empId, int storeId, string startDate, /*List<int> dateList*/ string shiftMin, string shiftMax, int empGroupId, int timeFrameId/*, string checkinExpandtime, string checkoutExpandtime*/, string breaktime)
        {
            //var startDate = startTime.ToDateTime().GetStartOfDate();
            //var endDate = endTime.ToDateTime().GetEndOfDate();
            //for (DateTime i = startDate; DateTime.Compare(i, endDate) < 0; i = i.AddDays(1))
            //{
            //    var current = i.GetStartOfDate();
            //    var timeStart = current.Add(timeFrame.StartTime);
            //    var timeEnd = current.Add(timeFrame.EndTime);
            //    try
            //    {
            //        var attendance = new AttendanceViewModel();
            //        attendance.EmployeeId = empId;
            //        attendance.StoreId = storeId;
            //        attendance.Status = (int)StatusAttendanceEnum.Processing;
            //        attendance.ShiftMin = timeStart;
            //        attendance.ShiftMax = timeEnd;
            //        attendance.ProcessingStatus = (int)ProcessingStatusAttendenceEnum.Assign;
            //        attendance.TimeFramId = timeFrameId;
            //        //attendance.ExpandTime = empGroup.ExpandTime;
            //        attendance.BreakTime = timeFrame.BreakTime;
            //        attendance.IsRequested = (int)IsRequest.RequestNewAttendance;
            //        attendance.Active = true;
            //        attendanceApi.Create(attendance);

            //        result = true;
            //    }
            //    catch (Exception e)
            //    {
            //        result = false;
            //    }
            //}
            //DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).GetStartOfDate();
            //DateTime endDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday).GetEndOfDate();
            var empGroupApi = new EmployeeGroupApi();
            var timeFrameApi = new TimeFrameApi();
            var attendanceApi = new AttendanceApi();
            var timeFrame = timeFrameApi.Get(timeFrameId);
            var empGroup = empGroupApi.Get(empGroupId);

            //var checkinexpandTime = Utils.ToHourTime(checkinExpandtime);
            //var checkoutexpandTime = Utils.ToHourTime(checkoutExpandtime);
            var breakTime = Utils.ToHourTime(breaktime);

            var strShiftMinDateTime = startDate.Replace('/', '-') + " " + shiftMin;
            var strShiftMaxDateTime = startDate.Replace('/', '-') + " " + shiftMax;
            DateTime shiftMinDateTime = DateTime.ParseExact(strShiftMinDateTime, "dd-MM-yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            DateTime shiftMaxDateTime = DateTime.ParseExact(strShiftMaxDateTime, "dd-MM-yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            //if (time.Length > 0)
            //{
            //    startDate = time.ToDateTime().GetStartOfDate();
            //    endDate = time.ToDateTime().GetEndOfDate();
            //}
            bool result = false;
            //for (int i = 0; i < dateList.Count; i++)
            //{
            //    var current = startDate.AddDays(dateList[i]);
            //    var timeStart = current.Add(timeFrame.StartTime);
            //    var timeEnd = timeStart.Add(timeFrame.Duration);                
            //}
            try
            {
                var attendance = new Attendance();
                attendance.EmployeeId = empId;
                attendance.StoreId = storeId;
                attendance.Status = (int)StatusAttendanceEnum.Processing;
                attendance.ShiftMin = shiftMinDateTime;
                attendance.ShiftMax = shiftMaxDateTime;
                attendance.ProcessingStatus = (int)ProcessingStatusAttendenceEnum.Assign;
                attendance.TimeFramId = timeFrameId;
                attendance.BreakTime = breakTime.TimeOfDay;
                attendance.Active = true;
                //attendance.InMode = inMode;
                //attendance.OutMode = outMode;
                //attendance.BreakCount = isBreakCount;
                //attendance.CheckInExpandTime = checkinexpandTime.TimeOfDay;
                //attendance.CheckOutExpandTime = checkoutexpandTime.TimeOfDay;
                attendanceApi.BaseService.Create(attendance);
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }
            return Json(new {rs = result}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowNoteRequest(int attId)
        {
            var attApi = new AttendanceApi();
            var note = attApi.Get(attId).NoteRequest;
            return Json(new { rs = "Lý do: " + note }, JsonRequestBehavior.AllowGet);
        }
    }
}