using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Sdk;
using HmsService.ViewModels;
using Remotion.FunctionalProgramming;
using OfficeOpenXml;
using System.IO;
using System.Globalization;
using OfficeOpenXml.Style;
using Excel = Microsoft.Office.Interop.Excel;
using System.Drawing;
using System.Net;
using HmsService.Models;
using HmsService.Models.Entities;
using System.Data.Entity;
using System.Data.OleDb;
using AutoMapper.Internal;
using Wisky.SkyAdmin.Manage.Controllers;
using System.Web.Configuration;
using Microsoft.Office.Interop.Excel;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan.Controllers
{
    public class CalendarAttendaceDay
    {
        public int IdEmp;
        public string NameEmp;
        //public bool isTwoDay;
        public List<AttendanceDayReport> AttendanceList;
    }
    public class AttendanceDayReport
    {
        public int Id;
        public double duration;
        public double startHour;
        public string status;
        public string color;
        public string checkMax;
        public string checkMin;
        public Boolean checkYet;
    }
    public class CalendarFingerCheck
    {
        public Nullable<int> IdEmp;
        public string NameEmp;
        public List<CheckTime> CheckfingerList;
    }
    public class CheckTime
    {
        public double startHour;
        public TimeSpan checkHour;
    }
    public class CalendarAttendanceWeek
    {
        public string date;
        public List<TimeSlotWeek> timeslot;
    }
    public class TimeSlotWeek
    {
        public double duration;
        public double startHour;
        public string status;
        public string color;
        public int idTimeFrame;
        public string dateSlot;
        public string totalEmp = "";
        public string startTime;
        public string endTime;
    }
    public class storeHour
    {
        public double start;
        public double end;
    }
    public class AttendanceController : DomainBasedController
    {
        // GET: FingerScan/Attendance
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AttendanceHistory()
        {

            return View();
        }

        public ActionResult AttendanceAtBrand()
        {
            return View();
        }

        public ActionResult GetAttendanceOnBrand()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetDetailAttendance(int storeId, int employeeId, string timeCheck)
        {
            var dateCheck = timeCheck.ToDateTime();
            var attendanceApi = new AttendanceApi();
            var employeeApi = new EmployeeApi();
            var employeeName = employeeApi.Get(employeeId).Name;
            IEnumerable<Attendance> listAttendances = attendanceApi.GetAttendanceByEmpIdAndStoreByTimeRange(employeeId, storeId, dateCheck.GetStartOfDate(), dateCheck.GetEndOfDate());
            var count = 1;
            var result = listAttendances.Select(q => new IConvertible[] {
                count++,
                q.ShiftMin.ToString("HH:mm"),
                q.ShiftMax.ToString("HH:mm"),
                q.CheckMin == null? "N/A" : q.CheckMin.Value.ToString("HH:mm"),
                q.CheckMax == null? "N/A" : q.CheckMin.Value.ToString("HH:mm"),
                q.Status == (int)StatusAttendanceEnum.Approved ? "Đã duyệt" :
                q.Status == (int)StatusAttendanceEnum.Processing ? "Chờ Duyệt" :
                q.Status == (int)StatusAttendanceEnum.Reject ? "Đã hủy" : "N/A"
            });
            return Json(new { data = result, employeeName = employeeName }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AttendanceDashboard(int storeId)
        {
            var api = new StoreApi();
            var model = api.Get(storeId);
            return View(model);
        }
        [HttpPost]
        public ActionResult LoadCalendar(int storeId, String startTime, String endTime, int brandId)
        {

            //Get open and close time---------------
            var storeApi = new StoreApi();
            var store = storeApi.GetStoreById(storeId);
            var storeModel = new storeHour();
            if (store.OpenTime != null && store.CloseTime != null)
            {
                var StartTime = store.OpenTime ?? DateTime.Now;
                var EndTime = store.CloseTime ?? DateTime.Now;
                var now = StartTime.GetStartOfDate();
                storeModel.start = (Math.Round(StartTime.Subtract(now).TotalHours, 1) - 1) <= 0 ? 0 : (Math.Round(StartTime.Subtract(now).TotalHours, 1) - 1);
                storeModel.end = Math.Round(EndTime.Subtract(now).TotalHours, 1) + 1 >= 23 ? 23 : Math.Round(EndTime.Subtract(now).TotalHours, 1) + 1;
                // Trường hợp cửa hàng cập nhật open time vs close time sai
                if (storeModel.start + 1 == storeModel.end - 1)
                {
                    storeModel.start = 0;
                    storeModel.end = 23;
                }
                int i = 0;
                while (i <= (storeModel.start - 1) && i <= 24)
                {
                    i = i + 1;
                }
                storeModel.start = i;
            }
            //----------
            var listAttendance = new List<CalendarAttendanceWeek>();

            DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).GetStartOfDate();
            DateTime endDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday).GetEndOfDate();
            if (startTime.Length > 0)
            {
                startDate = startTime.ToDateTime().GetStartOfDate();
                endDate = endTime.ToDateTime().GetEndOfDate();
            }
            var timeSlotTemp = new List<TimeSlotWeek>();
            var attendanceApi = new AttendanceApi();
            var timeFrameApi = new TimeFrameApi();
            var empApi = new EmployeeApi();
            var attenTime = attendanceApi.GetAttendanceByTimeRangeAndStore(storeId, startDate, endDate).OrderByDescending(q => q.ShiftMin).GroupBy(q => DbFunctions.TruncateTime(q.ShiftMin)).ToList();
            var attenTimeAsList = attendanceApi.GetAttendanceByTimeRangeAndStore2(storeId, startDate, endDate).ToList();
            var status = attenTimeAsList.FirstOrDefault() != null ? attenTimeAsList.FirstOrDefault().Status : 4;
            var tmpDate = startDate;
            var numEmp = new List<List<int[]>>();
            //while ((tmpDate.AddDays(-1).Date) != endDate.Date)
            //{
            //    var numEmpDay = new List<int[]>();
            //    for (var i = 4; i < 23; i++)
            //    {
            //        var startTimeParse = tmpDate.Date.Add(new TimeSpan(i, 0, 0));
            //        var endTimeParse = startTimeParse.AddHours(1);
            //        var empInHour = attenTimeAsList.Where(q => !(q.ShiftMin > endTimeParse && q.ShiftMin > startTimeParse) && !(startTimeParse > q.ShiftMin && startTimeParse > q.ShiftMax));
            //        var colorTime = 0;
            //        var checkRepeat = new bool[] { false, false };
            //        foreach (var emp in empInHour)
            //        {
            //            if ((emp.ShiftMax.Hour - 1 == i && emp.ShiftMax.Minute == 0) || (emp.ShiftMax.Hour == i && emp.ShiftMax.Minute != 0))
            //            {
            //                colorTime = 2;
            //                checkRepeat[0] = true;
            //            }
            //            else if (emp.ShiftMin.Hour == i)
            //            {
            //                colorTime = 1;
            //                checkRepeat[1] = true;
            //            }
            //        }
            //        colorTime = checkRepeat[0] && checkRepeat[1] ? 3 : colorTime;
            //        numEmpDay.Add(new int[] { empInHour.Count(), colorTime });
            //    }
            //    numEmp.Add(numEmpDay);
            //    tmpDate = tmpDate.AddDays(1);
            //}
            foreach(var att in attenTimeAsList)
            {
                var numEmpDay = new List<int[]>();
                for (var i = 4; i < 23; i++)
                {
                    var startTimeParse = att.ShiftMin.Date.Add(new TimeSpan(i, 0, 0));
                    var endTimeParse = startTimeParse.AddHours(1);
                    var empInHour = attenTimeAsList.Where(q => !(q.ShiftMin > endTimeParse && q.ShiftMin > startTimeParse) && !(startTimeParse > q.ShiftMin && startTimeParse > q.ShiftMax));
                    var colorTime = 0;
                    var checkRepeat = new bool[] { false, false };
                    foreach (var emp in empInHour)
                    {
                        if ((emp.ShiftMax.Hour - 1 == i && emp.ShiftMax.Minute == 0) || (emp.ShiftMax.Hour == i && emp.ShiftMax.Minute != 0))
                        {
                            colorTime = 2;
                            checkRepeat[0] = true;
                        }
                        else if (emp.ShiftMin.Hour == i)
                        {
                            colorTime = 1;
                            checkRepeat[1] = true;
                        }
                    }
                    colorTime = checkRepeat[0] && checkRepeat[1] ? 3 : colorTime;
                    numEmpDay.Add(new int[] { empInHour.Count(), colorTime });
                }
                numEmp.Add(numEmpDay);
                tmpDate = tmpDate.AddDays(1);
            }

            DateTime itemLastDay = new DateTime();
            foreach (var item in attenTime)
            {
                var attendance = new CalendarAttendanceWeek();
                var date = item.Key.Value;
                var startDateInDate = date.GetStartOfDate();
                var endDateIndate = date.GetEndOfDate();
                attendance.date = item.Key.Value.DayOfWeek.ToString();
                var list = attendanceApi.GetAttendanceByTimeRangeAndStore(storeId, startDateInDate, endDateIndate).Where(p => p.TimeFramId != 0).GroupBy(x => x.TimeFramId).ToList();
                var timeSlot = new List<TimeSlotWeek>();
                foreach (var itemTime in list)
                {
                    var time = new TimeSlotWeek();
                    var total = attendanceApi.GetAttendanceByTimeRangeAndStore(storeId, startDateInDate, endDateIndate).Where(p => p.TimeFramId == itemTime.Key).Count();
                    time.idTimeFrame = itemTime.Key;
                    time.totalEmp = "" + total;
                    //foreach(var x in itemTime)
                    //{
                    //    time.totalEmp += empApi.Get(x.EmployeeId).Name + ", ";
                    //}                    
                    time.dateSlot = item.Key.Value.ToString("dd'/'MM'/'yyyy");
                    var timeFrame = timeFrameApi.Get(itemTime.Key);
                    var strEndAttendance = itemTime.FirstOrDefault().ShiftMax.ToString("dd/MM/yyyy") + " " + timeFrame.EndTime;
                    var strStartAttendance = itemTime.FirstOrDefault().ShiftMin.ToString("dd/MM/yyyy") + " " + timeFrame.StartTime;
                    var endTimeInDate = Utils.ToDateTimeHourSeconds(strEndAttendance);
                    var startTimeInDate = Utils.ToDateTimeHourSeconds(strStartAttendance);
                    time.startTime = startTimeInDate.ToString("dd/MM/yyyy HH:mm");
                    time.endTime = endTimeInDate.ToString("dd/MM/yyyy HH:mm");
                    TimeSpan durationLast; // Khoảng thời gian nửa dưới của ca xuyên ngày VD: Ca 23h-4h => 23h-24h
                    TimeSpan durationFirst;// Khoảng thời gian nửa trên của ca xuyên ngày VD: Ca 23h-4h => 00h-04h
                    if (startTimeInDate.Date != endTimeInDate.Date)
                    {

                        durationLast = startTimeInDate.GetEndOfDate() - startTimeInDate;
                        durationFirst = endTimeInDate - endTimeInDate.GetStartOfDate();
                        double durationLastTofloat = Math.Round(durationLast.TotalHours, 1);
                        double durationFirstTofloat = Math.Round(durationFirst.TotalHours, 1);
                        time.duration = durationLastTofloat;
                        time.status = timeFrame.Name;
                        double starHour = Math.Round(timeFrame.StartTime.TotalHours - storeModel.start, 1);
                        time.startHour = starHour;
                        if (starHour >= 18)
                        {
                            time.color = "#5CB85C";
                        }
                        else if (starHour >= 12)
                        {
                            time.color = "#F0AD4E";
                        }
                        else
                        {
                            time.color = "#5BC0DE";
                        }
                        if (timeSlotTemp.Count > 0)
                        {
                            timeSlot.Add(timeSlotTemp[0]);
                            timeSlotTemp.RemoveAt(0);
                        }
                        timeSlot.Add(time);
                        TimeSlotWeek time2 = new TimeSlotWeek
                        {
                            duration = durationFirstTofloat,
                            status = time.status,
                            dateSlot = endTimeInDate.Date.ToString("dd/MM/yyyy"),
                            startHour = endTimeInDate.GetStartOfDate().Hour,
                            color = time.color,
                            idTimeFrame = time.idTimeFrame,
                            totalEmp = time.totalEmp,
                            startTime = time.startTime,
                            endTime = time.endTime
                        };
                        timeSlotTemp.Add(time2);

                    }
                    else
                    {
                        durationLast = endTimeInDate - startTimeInDate;
                        double durationtofloat = Math.Round(durationLast.TotalHours, 1);
                        time.duration = durationtofloat;
                        time.status = timeFrame.Name;
                        double starHour = Math.Round(timeFrame.StartTime.TotalHours - storeModel.start, 1);
                        time.startHour = starHour;
                        if (starHour + storeModel.start >= 18)
                        {
                            time.color = "#5CB85C";
                        }
                        else if (starHour + storeModel.start >= 12)
                        {
                            time.color = "#F0AD4E";
                        }
                        else
                        {
                            time.color = "#5BC0DE";
                        }
                        if (timeSlotTemp.Count > 0)
                        {
                            timeSlot.Add(timeSlotTemp[0]);
                            timeSlotTemp.RemoveAt(0);
                        }
                        timeSlot.Add(time);
                    }
                }
                attendance.timeslot = timeSlot;
                listAttendance.Add(attendance);
                itemLastDay = item.Key.Value;
            }

            if (timeSlotTemp.Count > 0)
            {
                var attendance2 = new CalendarAttendanceWeek();
                attendance2.date = itemLastDay.AddDays(1).DayOfWeek.ToString();
                attendance2.timeslot = timeSlotTemp;
                listAttendance.Add(attendance2);
                //timeSlotTemp.RemoveAt(0);
            }
            //return Json(new { rs = listAttendance, storeModel = storeModel }, JsonRequestBehavior.AllowGet);
            return Json(new { rs = listAttendance, listEmp = numEmp, storeModel = storeModel, status = status }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult detailsAttendanceWeek(int storeId, string Date, string startTime, string endTime)
        {
            var startTimeParse = Utils.ToDateTimeHour(startTime);
            var endTimeParse = Utils.ToDateTimeHour(endTime);

            int count = 1;
            var apiAttendance = new AttendanceApi();
            var employeeApi = new EmployeeApi();

            var timeFrameApi = new TimeFrameApi();
            var startDate = Date.ToDateTime().GetStartOfDate();
            var endDate = Date.ToDateTime().GetEndOfDate();
            var datatable = apiAttendance.GetAttendanceByTimeRangeAndStore2(storeId, startDate, endDate).Where(q => !(q.ShiftMin > endTimeParse && q.ShiftMin > startTimeParse) && !(startTimeParse > q.ShiftMin && startTimeParse > q.ShiftMax)).ToList();
            var storeApi = new StoreApi();
            var result = datatable.OrderByDescending(q => q.TotalWorkTime).OrderBy(q => q.ShiftMin).Select(q => new IConvertible[]
              {
                count++,
                employeeApi.Get(q.EmployeeId).Name,
                q.CheckMin == null ? "Chưa cập nhật" : q.CheckMin.Value.ToString("HH:mm"),
                q.CheckMax == null ? "Chưa cập nhật" : q.CheckMax.Value.ToString("HH:mm"),
                q.TotalWorkTime == null ? "N/A" :  DateTime.Today.Add(q.TotalWorkTime.Value).ToString("HH:mm"),
                q.Id,
                (q.CheckMax==null||q.CheckMin==null)? false:true,
                q.CheckMin == null ? false : ((q.ShiftMin-q.CheckMin <= (-q.ExpandTime))? false:true),
                q.CheckMax == null ? false : ((q.ShiftMax-q.CheckMax >= q.ExpandTime)? false:true),
                q.Status == null ? "N/A" : ((StatusAttendanceEnum)q.Status).DisplayName(),
                q.UpdatePerson == null ? "N/A" : q.UpdatePerson,
                q.ProcessingStatus == null ? "N/A" : ((ProcessingStatusAttendenceEnum)q.ProcessingStatus).DisplayName(),
                q.ShiftMin.ToString("HH:mm"),
                q.ShiftMax.ToString("HH:mm"),
                (employeeApi.Get(q.EmployeeId).MainStoreId != storeId)?storeApi.Get(employeeApi.Get(q.EmployeeId).MainStoreId).Name:"",
              }).ToList();

            return Json(new
            {
                result = result,
                Totalslot = count - 1,

            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListTimeFrame(int brandId)
        {
            int count = 1;
            var timeFrameApi = new TimeFrameApi();
            var datatable = timeFrameApi.GetActive().Where(p => p.BrandId == brandId).Select(q => new IConvertible[] {
                count++,
                q.Name,
                q.StartTime.ToString(),
                q.EndTime.ToString(),
                q.Duration.ToString(),
                q.Id
            }).ToList();
            return Json(new { datatable }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult detailsTimeFrame(int Id)
        {
            var timeFrameApi = new TimeFrameApi();
            var result = timeFrameApi.Get(Id);
            var tmpModel = new
            {
                Name = result.Name,
                StartTime = result.StartTime.ToString(),
                EndTime = result.EndTime.ToString(),
                //Duration = result.ExpandTime.ToString()
            };
            return Json(tmpModel);
        }

        [Authorize(Roles = "BrandManager, ShiftCreator")]
        public ActionResult CreateMoreAttendanceAction(List<int> empIdList, int storeId, string startTime, string endTime, int empGroupId, int timeFrameId, TimeSpan? timeFrameStart, TimeSpan? timeFrameEnd)
        {
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var empGroupApi = new EmployeeGroupApi();
            var timeFrameApi = new TimeFrameApi();
            var attendanceApi = new AttendanceApi();
            var timeFrame = timeFrameApi.Get(timeFrameId);
            var empGroup = empGroupApi.Get(empGroupId);
            bool result = false;

            for (DateTime i = startDate; DateTime.Compare(i, endDate) < 0; i = i.AddDays(1))
            {
                var current = i.GetStartOfDate();
                var timeStart = current.Add(timeFrame.StartTime);
                var timeEnd = current.Add(timeFrame.EndTime);
                if (timeStart != null && timeEnd != null)
                {
                    timeStart = current.Add(timeFrame.StartTime);
                    timeEnd = current.Add(timeFrame.EndTime);
                }

                try
                {
                    foreach (var item in empIdList)
                    {
                        var attendance = new Attendance();
                        attendance.EmployeeId = item;
                        attendance.StoreId = storeId;
                        attendance.Status = (int)StatusAttendanceEnum.Processing;
                        attendance.ShiftMin = timeStart;
                        attendance.ShiftMax = timeEnd;
                        attendance.ProcessingStatus = (int)ProcessingStatusAttendenceEnum.Assign;
                        attendance.TimeFramId = timeFrameId;
                        //attendance.ExpandTime = empGroup.ExpandTime;
                        attendance.BreakTime = timeFrame.BreakTime;
                        attendance.Active = true;
                        if (timeFrame.IsOverTime == true)
                        {
                            attendance.IsOverTime = true;
                        }
                        attendanceApi.BaseService.Create(attendance);
                    }
                    result = true;
                }
                catch (Exception e)
                {
                    result = false;
                }
            }
            return Json(result);
        }
        public ActionResult getTodate()
        {
            //DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            //DateTime endDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday);
            DateTime startDate = DateTime.Today;
            DateTime endDate = DateTime.Today;
            var tmpModel = new
            {
                StartTime = startDate.ToString("dd/MM/yyyy"),
                EndTime = endDate.ToString("dd/MM/yyyy"),
            };
            return Json(tmpModel);
        }
        public ActionResult GetAllEmployeeToAdd(int storeId, int brandId, TimeSpan shiftMin, TimeSpan shiftMax, string endTime, string startTime,/* int empGroupId,*/ List<int> dateList)
        {
            int count = 1;
            var listDate = new List<DateTime>();
            DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).GetStartOfDate();
            DateTime endDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Sunday).GetEndOfDate();
            if (startTime.Length > 0)
            {
                startDate = startTime.ToDateTime().GetStartOfDate();
                endDate = endTime.ToDateTime().GetEndOfDate();
            }
            var employeeApi = new EmployeeApi();
            var storeApi = new StoreApi();
            var Listemployees = employeeApi.GetAllEmployeeFreeByTimeSpanByDate(shiftMin, shiftMax, dateList, endDate, startDate, storeId/*, empGroupId*/, brandId);
            //var Listemployees = employeeApi.GetAllEmployeeFreeByTimeSpan1(startDate, endDate, shiftMin, shiftMax, storeId/*, empGroupId*/, brandId);
            var listCanApprove = new List<StoreAttendanceApply>();
            var listCanNotApprove = new List<StoreAttendanceApply>();
            foreach (var item in Listemployees)
            {
                if (item.ListCannotApprove.Any())
                {
                    var temp = new StoreAttendanceApply();
                    temp.ListCanApprove = item.ListCanApprove;
                    temp.ListCannotApprove = item.ListCannotApprove;
                    listCanNotApprove.Add(temp);
                }
                else
                {
                    var temp = new StoreAttendanceApply();
                    temp.ListCanApprove = item.ListCanApprove;
                    temp.ListCannotApprove = item.ListCannotApprove;
                    listCanApprove.Add(temp);
                }
            }
            if (listCanNotApprove.Any())
            {
                for (var i = 1; i < listCanNotApprove.Count; i++)
                {
                    foreach (var item in listCanNotApprove[i].ListCannotApprove)
                    {
                        var temp = listCanNotApprove.First().ListCanApprove;
                        if (temp.Any(q => q.Id == item.Id))
                        {
                            listCanNotApprove.First().ListCanApprove.Remove(item);
                            listCanNotApprove.First().ListCannotApprove.Add(item);
                        }
                        else
                        {
                            listCanNotApprove.First().ListCannotApprove.Add(item);
                        }
                    }
                }
            }
            //var result = listCanNotApprove.First();
            var finalResult = listCanNotApprove.Any() ? listCanNotApprove.First() : listCanApprove.First();
            var listResultCanApprove = finalResult.ListCanApprove
        .Select(q => new IConvertible[] {
                count++,
                q.Name,
                q.Phone,
                q.Id,
                (q.MainStoreId != storeId)?storeApi.Get(q.MainStoreId).Name:"",
        });
            var listResultCanNotApprove = finalResult.ListCannotApprove
            .Select(q => new IConvertible[] {
                count++,
                q.Name,
                q.Phone,
                q.Id,
                (q.MainStoreId != storeId)?storeApi.Get(q.MainStoreId).Name:"",
            });
            return Json(new { datatable = listResultCanApprove, datatableNotApprove = listResultCanNotApprove }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult GetAllEmployeeToAdd(int storeId, int brandId, string shiftMin, string shiftMax, int empGroupId, string startDate)
        //{
        //    int count = 1;
        //    var employeeApi = new EmployeeApi();
        //    var storeApi = new StoreApi();

        //    DateTime shiftMinDateTime = DateTime.Parse(startDate + " " + shiftMin);
        //    DateTime shiftMaxDateTime = DateTime.Parse(startDate + " " + shiftMax);

        //    var Listemployees = employeeApi.GetAllEmployeeFreeByTimeSpanByDate(shiftMinDateTime, shiftMaxDateTime, storeId, empGroupId, brandId);
        //    var listResultCanApprove = Listemployees.ListCanApprove
        //    .Select(q => new IConvertible[] {
        //        count++,
        //        q.Name,
        //        q.Phone,
        //        q.Id,
        //        (q.MainStoreId != storeId)?storeApi.Get(q.MainStoreId).Name:"",
        //    });
        //    var listResultCanNotApprove = Listemployees.ListCannotApprove
        //    .Select(q => new IConvertible[] {
        //        count++,
        //        q.Name,
        //        q.Phone,
        //        q.Id,
        //        (q.MainStoreId != storeId)?storeApi.Get(q.MainStoreId).Name:"",
        //    });
        //    return Json(new { datatable = listResultCanApprove, datatableNotApprove = listResultCanNotApprove }, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult GetAllEmpInStore(int storeid)
        {
            int count = 1;
            var empIntore = new EmployeeInStoreApi();
            var emp = new EmployeeApi();
            var listID = empIntore.BaseService.Get(q => q.Active && q.StoreId == storeid).Select(q => q.EmployeeId).ToList();

            var datatable = emp.GetActive().Where(p => listID.Contains(p.Id)).Select(q => new IConvertible[] {
                count++,
                q.Name,                
                //q.ExpandTime.ToString(),
                q.Id
            }).ToList();
            return Json(new { datatable }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAllEmpInStoreFreeByTimeRange(int storeid, string strChooseDate, string strShiftMin, string strShiftMax)
        {
            int count = 1;
            var start = strChooseDate.ToDateTime().Add(Utils.ToHourTime(strShiftMin).TimeOfDay);
            var end = strChooseDate.ToDateTime().Add(Utils.ToHourTime(strShiftMax).TimeOfDay);
            var emp = new EmployeeApi();
            var att = new AttendanceApi();
            var lastResult = new List<Employee>();
            var listEmp = emp.GetActive().Where(p => p.MainStoreId == storeid).ToList();

            foreach (var item in listEmp)
            {
                var listAttendance = att.GetAttendanceByTimeRange2(start, end, storeid, item.Id).ToList();
                if (!listAttendance.Any())
                {
                    lastResult.Add(item.ToEntity());
                }
            }
            //var datatable = emp.GetActive().Where(p => p.MainStoreId == storeid).Select(q => new IConvertible[] {
            //    count++,
            //    q.Name,                
            //    //q.ExpandTime.ToString(),
            //    q.Id
            //}).ToList();
            return Json(new { data = lastResult }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDetailAttendanceOfWeek(int storeId, int employeeId, string endTime, string startTime, List<int> dateList)
        {
            var listDate = new List<DateTime>();
            //DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).GetStartOfDate();
            //DateTime endDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday).GetEndOfDate();
            DateTime startDate = DateTime.Parse(startTime).GetStartOfDate();
            DateTime endDate = DateTime.Parse(endTime).GetEndOfDate();
            for (int i = 0; i < dateList.Count; i++)
            {
                dateList[i] = (dateList[i] + 1) % 7;
            }
            var sDate = startDate.ToShortDateString();
            var eDate = endDate.ToShortDateString();
            var attendanceApi = new AttendanceApi();
            var employeeApi = new EmployeeApi();
            var employeeName = employeeApi.Get(employeeId).Name;
            IEnumerable<Attendance> listAttendances = attendanceApi.BaseService.Get(q => q.Active == true && q.ShiftMin >= startDate && q.ShiftMax <= endDate && q.StoreId == storeId && q.EmployeeId == employeeId);
            listAttendances = listAttendances.Where(q => dateList.Any(d => d == (int)q.ShiftMin.DayOfWeek));
            var count = 1;
            var result = listAttendances.Select(q => new IConvertible[] {
                count++,
                q.ShiftMin.ToString(),
                q.ShiftMax.ToString(),
                q.CheckMin == null? "N/A" : q.CheckMin.Value.ToString("HH:mm"),
                q.CheckMax == null? "N/A" : q.CheckMin.Value.ToString("HH:mm"),
                q.Status == (int)StatusAttendanceEnum.Approved ? "Đã duyệt" :
                q.Status == (int)StatusAttendanceEnum.Processing ? "Chờ Duyệt" :
                q.Status == (int)StatusAttendanceEnum.Reject ? "Đã hủy" : "N/A"
            });
            return Json(new { data = result, employeeName = employeeName, sDate = sDate, eDate = eDate }, JsonRequestBehavior.AllowGet);
        }
        #region Old Code
        public ActionResult CreateMoreAttendanceActionByDateRange(List<int> empIdList, int storeId, string shiftMin, string shiftMax, string startTime, string endTime, List<int> dateList/*, int empGroupId*/, int timeFrameId, int inMode, int outMode/*, int isBreakCount*/, string checkinExpandtime, string checkoutExpandtime, string breaktime)
        {
            DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).GetStartOfDate();
            DateTime endDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday).GetEndOfDate();

            var empGroupApi = new EmployeeGroupApi();
            var timeFrameApi = new TimeFrameApi();
            var attendanceApi = new AttendanceApi();
            var timeFrame = timeFrameApi.Get(timeFrameId);
            //var empGroup = empGroupApi.Get(empGroupId);
            var shiftmin = Utils.ToHourTime(shiftMin);
            var shiftmax = Utils.ToHourTime(shiftMax);
            var breakTime = Utils.ToHourTime(breaktime);
            var checkinexpandtime = Utils.ToHourTime(checkinExpandtime);
            var checkoutexpandtime = Utils.ToHourTime(checkoutExpandtime);

            if (startTime.Length > 0)
            {
                startDate = startTime.ToDateTime().GetStartOfDate();
                endDate = endTime.ToDateTime().GetEndOfDate();
            }
            bool result = false;
            for (int i = 0; i < dateList.Count; i++)
            {
                var current = startDate.AddDays(dateList[i]);
                var timeStart = current.Add(shiftmin.TimeOfDay);
                var timeEnd = current.Add(shiftmax.TimeOfDay);
                try
                {
                    foreach (var item in empIdList)
                    {
                        var attendance = new Attendance();
                        attendance.EmployeeId = item;
                        attendance.StoreId = storeId;
                        attendance.Status = (int)StatusAttendanceEnum.Processing;
                        attendance.ShiftMin = timeStart;
                        attendance.ShiftMax = timeEnd;
                        attendance.ProcessingStatus = (int)ProcessingStatusAttendenceEnum.Assign;
                        attendance.TimeFramId = timeFrameId;
                        //attendance.ExpandTime = empGroup.ExpandTime;
                        attendance.BreakTime = breakTime.TimeOfDay;
                        attendance.Active = true;
                        attendance.InMode = inMode;
                        attendance.OutMode = outMode;
                        //attendance.BreakCount = isBreakCount;
                        attendance.CheckInExpandTime = checkinexpandtime.TimeOfDay;
                        attendance.CheckOutExpandTime = checkoutexpandtime.TimeOfDay;
                        attendanceApi.BaseService.Create(attendance);
                    }
                    result = true;
                }
                catch (Exception e)
                {
                    result = false;
                }
            }
            return Json(result);
        }
        public ActionResult CreateMoreAttendanceActionByDateRange1(int empId, int storeId, List<string> shiftMin, List<string> shiftMax, List<string> dateList/*, int empGroupId*/, int timeFrameId, int inMode, int outMode/*, int isBreakCount*/, string checkinExpandtime, string checkoutExpandtime, string breaktime)
        {
            //DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).GetStartOfDate();
            //DateTime endDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday).GetEndOfDate();

            //var empGroupApi = new EmployeeGroupApi();
            //var timeFrameApi = new TimeFrameApi();
            var attendanceApi = new AttendanceApi();
            //var timeFrame = timeFrameApi.Get(timeFrameId);
            ////var empGroup = empGroupApi.Get(empGroupId);
            //var shiftmin = Utils.ToHourTime(shiftMin);
            //var shiftmax = Utils.ToHourTime(shiftMax);
            //var breakTime = Utils.ToHourTime(breaktime);
            //var checkinexpandtime = Utils.ToHourTime(checkinExpandtime);
            //var checkoutexpandtime = Utils.ToHourTime(checkoutExpandtime);

            //if (startTime.Length > 0)
            //{
            //    startDate = startTime.ToDateTime().GetStartOfDate();
            //    endDate = endTime.ToDateTime().GetEndOfDate();
            //}
            bool result = false;
            for (int i = 0; i < dateList.Count; i++)
            {
                try
                {
                    var attendance = new Attendance();
                    attendance.EmployeeId = empId;
                    attendance.StoreId = storeId;
                    attendance.Status = (int)StatusAttendanceEnum.Processing;
                    attendance.ShiftMin = dateList[i].ToDateTime().GetStartOfDate().Add(Utils.ToHourTime(shiftMin[i]).TimeOfDay);
                    attendance.ShiftMax = dateList[i].ToDateTime().GetStartOfDate().Add(Utils.ToHourTime(shiftMax[i]).TimeOfDay);
                    attendance.ProcessingStatus = (int)ProcessingStatusAttendenceEnum.Assign;
                    attendance.TimeFramId = timeFrameId;
                    //attendance.ExpandTime = empGroup.ExpandTime;
                    attendance.BreakTime = Utils.ToHourTime(breaktime).TimeOfDay;
                    attendance.Active = true;
                    attendance.InMode = inMode;
                    attendance.OutMode = outMode;
                    //attendance.BreakCount = isBreakCount;
                    attendance.CheckInExpandTime = Utils.ToHourTime(checkinExpandtime).TimeOfDay;
                    attendance.CheckOutExpandTime = Utils.ToHourTime(checkoutExpandtime).TimeOfDay;
                    attendanceApi.BaseService.Create(attendance);
                    result = true;
                }
                catch (Exception e)
                {
                    result = false;
                }
            }
            return Json(result);
        }            
        public ActionResult CopyAttendance(List<int> empIdList, int storeId, List<string> shiftMin, List<string> shiftMax, List<string> date, List<int> timeFrameId, List<int> inMode, List<int> outMode/*, int isBreakCount*/, List<string> checkinExpandtime, List<string> checkoutExpandtime, List<string> breaktime)
        {
            var attendanceApi = new AttendanceApi();
            //var empGroup = empGroupApi.Get(empGroupId);            
            bool result = false;
            var existStartWeek = date.First().ToDateTime().GetStartOfDate();
            var existEndWeek = date.Last().ToDateTime().GetEndOfDate();
            var listExistAtt = attendanceApi.GetActive().Where(q => q.StoreId == storeId && q.ShiftMin >= existStartWeek && q.ShiftMax <= existEndWeek).ToList();
            if (!listExistAtt.Any())
            {
                for (int i = 0; i < empIdList.Count; i++)
                {
                    try
                    {
                        var attendance = new Attendance();
                        attendance.EmployeeId = empIdList[i];
                        attendance.StoreId = storeId;
                        attendance.Status = (int)StatusAttendanceEnum.Draft;
                        attendance.ShiftMin = date[i].ToDateTime().GetStartOfDate().Add(Utils.ToHourTime(shiftMin[i]).TimeOfDay);
                        attendance.ShiftMax = date[i].ToDateTime().GetStartOfDate().Add(Utils.ToHourTime(shiftMax[i]).TimeOfDay);
                        attendance.ProcessingStatus = (int)ProcessingStatusAttendenceEnum.Assign;
                        attendance.TimeFramId = timeFrameId[i];
                        //attendance.ExpandTime = empGroup.ExpandTime;
                        attendance.BreakTime = Utils.ToHourTime(breaktime[i]).TimeOfDay;
                        attendance.Active = true;
                        attendance.InMode = inMode[i];
                        attendance.OutMode = outMode[i];
                        //attendance.BreakCount = isBreakCount;
                        attendance.CheckInExpandTime = Utils.ToHourTime(checkinExpandtime[i]).TimeOfDay;
                        attendance.CheckOutExpandTime = Utils.ToHourTime(checkoutExpandtime[i]).TimeOfDay;
                        attendanceApi.BaseService.Create(attendance);
                        result = true;
                    }
                    catch (Exception e)
                    {
                        result = false;
                    }
                }
            }
            else
            {
                foreach (var item in listExistAtt)
                {
                    item.Active = false;
                    attendanceApi.BaseService.Update(item.ToEntity());
                }
                for (int i = 0; i < empIdList.Count; i++)
                {
                    try
                    {
                        var attendance = new Attendance();
                        attendance.EmployeeId = empIdList[i];
                        attendance.StoreId = storeId;
                        attendance.Status = (int)StatusAttendanceEnum.Draft;
                        attendance.ShiftMin = date[i].ToDateTime().GetStartOfDate().Add(Utils.ToHourTime(shiftMin[i]).TimeOfDay);
                        attendance.ShiftMax = date[i].ToDateTime().GetStartOfDate().Add(Utils.ToHourTime(shiftMax[i]).TimeOfDay);
                        attendance.ProcessingStatus = (int)ProcessingStatusAttendenceEnum.Assign;
                        attendance.TimeFramId = timeFrameId[i];
                        //attendance.ExpandTime = empGroup.ExpandTime;
                        attendance.BreakTime = Utils.ToHourTime(breaktime[i]).TimeOfDay;
                        attendance.Active = true;
                        attendance.InMode = inMode[i];
                        attendance.OutMode = outMode[i];
                        //attendance.BreakCount = isBreakCount;
                        attendance.CheckInExpandTime = Utils.ToHourTime(checkinExpandtime[i]).TimeOfDay;
                        attendance.CheckOutExpandTime = Utils.ToHourTime(checkoutExpandtime[i]).TimeOfDay;
                        attendanceApi.BaseService.Create(attendance);
                        result = true;
                    }
                    catch (Exception e)
                    {
                        result = false;
                    }
                }
            }
            return Json(result);
        }
        #endregion
        public ActionResult CreateMoreAttendanceActionByDate(List<int> empIdList, int storeId, string shiftMin, string shiftMax, string startDate/*, int empGroupId*/, int timeFrameId, int inMode, int outMode/*, int isBreakCount*/, string checkinExpandtime, string checkoutExpandtime, string breaktime)
        {
            var empGroupApi = new EmployeeGroupApi();
            var timeFrameApi = new TimeFrameApi();
            var attendanceApi = new AttendanceApi();
            var timeFrame = timeFrameApi.Get(timeFrameId);
            //var empGroup = empGroupApi.Get(empGroupId);

            //var checkinexpandTime = Utils.ToHourTime(checkinExpandtime);
            //var checkoutexpandTime = Utils.ToHourTime(checkoutExpandtime);
            var checkinexpandTime = timeFrame.CheckInExpandTime;
            var checkoutexpandTime = timeFrame.CheckOutExpandTime;
            var breakTime = Utils.ToHourTime(breaktime);

            var strShiftMinDateTime = startDate.Replace('/', '-') + " " + shiftMin;
            var strShiftMaxDateTime = startDate.Replace('/', '-') + " " + shiftMax;
            DateTime shiftMinDateTime = DateTime.ParseExact(strShiftMinDateTime, "dd-MM-yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            DateTime shiftMaxDateTime = DateTime.ParseExact(strShiftMaxDateTime, "dd-MM-yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

            bool result = false;
            foreach (var item in empIdList)
            {
                try
                {
                    var attendance = new Attendance();
                    attendance.EmployeeId = item;
                    attendance.StoreId = storeId;
                    attendance.Status = (int)StatusAttendanceEnum.Processing;
                    attendance.ShiftMin = shiftMinDateTime;
                    attendance.ShiftMax = shiftMaxDateTime;
                    attendance.ProcessingStatus = (int)ProcessingStatusAttendenceEnum.Assign;
                    attendance.TimeFramId = timeFrameId;
                    attendance.BreakTime = breakTime.TimeOfDay;
                    attendance.Active = true;
                    attendance.InMode = inMode;
                    attendance.OutMode = outMode;
                    //attendance.BreakCount = isBreakCount;
                    attendance.CheckInExpandTime = checkinexpandTime;
                    attendance.CheckOutExpandTime = checkoutexpandTime;
                    attendanceApi.BaseService.Create(attendance);
                    result = true;
                }
                catch (Exception e)
                {
                    result = false;
                }
            }

            return Json(result);
        }
        public ActionResult CreateMoreAttendanceActionByDate1(int empId, int storeId, string shiftMin, string shiftMax, string startDate/*, int empGroupId*/, int timeFrameId, int inMode, int outMode/*, int isBreakCount*/, string checkinExpandtime, string checkoutExpandtime, string breaktime)
        {
            var empGroupApi = new EmployeeGroupApi();
            var timeFrameApi = new TimeFrameApi();
            var attendanceApi = new AttendanceApi();
            var timeFrame = timeFrameApi.Get(timeFrameId);
            //var empGroup = empGroupApi.Get(empGroupId);

            //var checkinexpandTime = Utils.ToHourTime(checkinExpandtime);
            //var checkoutexpandTime = Utils.ToHourTime(checkoutExpandtime);
            var checkinexpandTime = timeFrame.CheckInExpandTime;
            var checkoutexpandTime = timeFrame.CheckOutExpandTime;
            var breakTime = Utils.ToHourTime(breaktime);

            var strShiftMinDateTime = startDate.Replace('/', '-') + " " + shiftMin;
            var strShiftMaxDateTime = startDate.Replace('/', '-') + " " + shiftMax;
            DateTime shiftMinDateTime = DateTime.ParseExact(strShiftMinDateTime, "dd-MM-yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            DateTime shiftMaxDateTime = DateTime.ParseExact(strShiftMaxDateTime, "dd-MM-yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

            bool result = false;
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
                attendance.InMode = inMode;
                attendance.OutMode = outMode;
                //attendance.BreakCount = isBreakCount;
                attendance.CheckInExpandTime = checkinexpandTime;
                attendance.CheckOutExpandTime = checkoutexpandTime;
                attendanceApi.BaseService.Create(attendance);
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }

            return Json(result);
        }
        public JsonResult GetAttendance1(/*string thisWeek, */int storeId, int status, int empType, int isLate, string strDateChooseStart, string strDateChooseEnd /*string strChooseDay*/, int? typeShift)
        {
            //var thisStartWeek = thisWeek.ToDateTime().GetStartOfDate();
            //var thisEndWeek = thisWeek.ToDateTime().GetEndOfDate();

            var dateChooseStart = strDateChooseStart.ToDateTime();
            var dateChooseEnd = strDateChooseEnd.ToDateTime().GetEndOfDate();
            int count = 1;
            var api = new AttendanceApi();
            var employeeApi = new EmployeeApi();

            //var listExistAtt = api.GetActive().Where(q => q.StoreId == storeId && q.ShiftMin == existDate).ToList();

            var datatable = api.BaseService.Get(q => q.Active == true && q.ShiftMin >= dateChooseStart &&
                                                        q.ShiftMax <= dateChooseEnd &&
                                                        //q.Status != (int)StatusAttendanceEnum.Absent &&
                                                        q.StoreId == storeId).ToList();

            if (status == 0)
            {
                datatable = datatable.Where(q => q.Status == 0).ToList();
            }
            else if (status == 1)
            {
                datatable = datatable.Where(q => q.Status == 1).ToList();
            }
            else if (status == 2)
            {
                datatable = datatable.Where(q => q.Status == 2).ToList();
            }

            if (empType == 0)
            {
                datatable = datatable.Where(q => q.ProcessingStatus == 0 ||
                                                 q.ProcessingStatus == 1 ||
                                                 q.ProcessingStatus == 2 ||
                                                 q.ProcessingStatus == 3 ||
                                                 q.ProcessingStatus == 5).ToList();
            }
            else if (empType == 1)
            {
                datatable = datatable.Where(q => q.ProcessingStatus == 6 ||
                                                 q.ProcessingStatus == 7).ToList();
            }

            if (isLate == 0)
            {
                //datatable = datatable.Where(q => q.CheckMin - shiftStartTime >= new TimeSpan(0, 0, expandTime, 0) && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
                datatable = datatable.Where(q => q.CheckMin - q.ShiftMin >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
            }
            else if (isLate == 1)
            {
                //datatable = datatable.Where(q => shiftEndTime - q.CheckMax >= new TimeSpan(0, 0, expandTime, 0) && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
                datatable = datatable.Where(q => q.ShiftMax - q.CheckMax >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
            }
            else if (isLate == 2)
            {
                //datatable = datatable.Where(q => shiftEndTime - q.CheckMax < new TimeSpan(0, 0, expandTime, 0) && q.CheckMin - shiftStartTime < new TimeSpan(0, 0, expandTime, 0)).ToList();
                datatable = datatable.Where(q => q.ShiftMax - q.CheckMax < q.ExpandTime && q.CheckMin - q.ShiftMin < q.ExpandTime).ToList();
            }

            if (typeShift != null)
            {
                if (typeShift == 0)
                {
                    datatable = datatable.Where(q => q.IsOverTime != true).ToList();
                }
                if (typeShift == 1)
                {
                    datatable = datatable.Where(q => q.IsOverTime == true).ToList();
                }
            }
            var result = datatable.OrderBy(q => q.ShiftMin).Select(q => new IConvertible[]
            {
                count++,
                employeeApi.Get(q.EmployeeId).Name,
                //q.CheckMin == null ? "Chưa cập nhật" : q.CheckMin.Value.ToString("HH:mm"),
                //q.CheckMax == null ? "Chưa cập nhật" : q.CheckMax.Value.ToString("HH:mm"),
                //q.TotalWorkTime == null ? "N/A" :  DateTime.Today.Add(q.TotalWorkTime.Value).ToString("HH:mm"),
                q.Id,
                //(q.CheckMax==null||q.CheckMin==null)? false:true,
                //q.CheckMin == null ? false : ((q.ShiftMin-q.CheckMin <= (-q.ExpandTime))? false:true),
                //q.CheckMax == null ? false : ((q.ShiftMax-q.CheckMax >= q.ExpandTime)? false:true),
                q.Status == null ? "N/A" : ((StatusAttendanceEnum)q.Status).DisplayName(),
                //q.UpdatePerson == null ? "N/A" : q.UpdatePerson,
                q.ProcessingStatus == null ? "N/A" : ((ProcessingStatusAttendenceEnum)q.ProcessingStatus).DisplayName(),
                q.ShiftMin.ToString("HH:mm"),
                q.ShiftMax.ToString("HH:mm"),
                q.ShiftMin.ToString("dd/MM/yyyy"),
                q.EmployeeId,
                q.TimeFramId,
                DateTime.Parse(q.BreakTime.Value.ToString()).ToString("HH:mm"),
                q.InMode,
                q.OutMode,
                DateTime.Parse(q.CheckInExpandTime.Value.ToString()).ToString("HH:mm"),
                DateTime.Parse(q.CheckInExpandTime.Value.ToString()).ToString("HH:mm"),
                //q.Note == null ? "" : q.Note,
                //q.IsOverTime
            }).ToList();
            //TimeSpan totalWorkTimeOfShift = new TimeSpan(datatable.Sum(q => q.TotalWorkTime.GetValueOrDefault().Ticks));
            //var timenull = TimeSpan.Zero;

            //var listemp = datatable.Where(q => q.TotalWorkTime != null && q.TotalWorkTime != timenull).Count();
            //var listempnotright = datatable.Count() - listemp;
            //var numberOfEmpCheckRight = datatable.Count(q => q.CheckMax.GetValueOrDefault() - q.CheckMin.GetValueOrDefault() > new TimeSpan(0, 0, 1, 0) && q.Status == 1);
            //var numberOfEmpNA = datatable.Count(q => q.Status == 0);
            //var numberOfEmpNotRight = datatable.Count(q => q.CheckMin - shiftStartTime >= new TimeSpan(0, 0, expandTime, 0) || shiftEndTime - q.CheckMax >= new TimeSpan(0, 0, expandTime, 0));
            //var numberOfEmp = datatable.Count(q => q.Status == 1);
            //var avgTotalWorkTimeOfShift = new TimeSpan(0, 0, 0);
            //if (numberOfEmpCheckRight != 0)
            //{
            //    avgTotalWorkTimeOfShift = TimeSpan.FromHours(totalWorkTimeOfShift.TotalHours / (datatable.Count() - numberOfEmpNotRight));
            //}

            //foreach (var item in datatable)
            //{
            //    totalWorkDayOfShift = totalWorkDayOfShift + item.TotalWorkTime.GetValueOrDefault();
            //}

            //bool shiftWorkDay;
            //if (shiftApi.Get(shiftId).StartTime.GetValueOrDefault().Date >= DateTime.Now.Date)
            //{
            //    shiftWorkDay = true;
            //}
            //else
            //{
            //    shiftWorkDay = false;
            //}
            //var test = totalWorkTimeOfShift.ToString().Contains(".") ? totalWorkTimeOfShift.ToString().Split('.')[0] + " ngày " + totalWorkTimeOfShift.ToString().Split('.')[1] : totalWorkTimeOfShift.ToString();
            return Json(new
            {
                result = result,
                /*rsShiftWorkDay = shiftWorkDay,*/
                //numberOfEmp = listemp,
                //totalWorkTimeOfShift = totalWorkTimeOfShift.ToString().Contains(".") ? totalWorkTimeOfShift.ToString().Split('.')[0] + " ngày " + totalWorkTimeOfShift.ToString().Split('.')[1] : totalWorkTimeOfShift.ToString(),
                //avgTotalWorkTimeOfShift = avgTotalWorkTimeOfShift.ToString("g"),
                //numberOfEmpCheckRight = listemp,
                //numberOfEmpNA = listempnotright,
                //numberOfEmpNotRight = numberOfEmpNotRight,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAttendance(int storeId, int status, int empType, int isLate, string strChooseDay, int? typeShift)
        {
            var chooseDay = strChooseDay.ToDateTime();
            int count = 1;
            var api = new AttendanceApi();
            var employeeApi = new EmployeeApi();

            var datatable = api.GetActive().Where(q => q.ShiftMin.Date == chooseDay.Date && q.StoreId == storeId).ToList();

            if (status == 0)
            {
                datatable = datatable.Where(q => q.Status == 0).ToList();
            }
            else if (status == 1)
            {
                datatable = datatable.Where(q => q.Status == 1).ToList();
            }
            else if (status == 2)
            {
                datatable = datatable.Where(q => q.Status == 2).ToList();
            }

            if (empType == 0)
            {
                datatable = datatable.Where(q => q.ProcessingStatus == 0 ||
                                                 q.ProcessingStatus == 1 ||
                                                 q.ProcessingStatus == 2 ||
                                                 q.ProcessingStatus == 3 ||
                                                 q.ProcessingStatus == 5).ToList();
            }
            else if (empType == 1)
            {
                datatable = datatable.Where(q => q.ProcessingStatus == 6 ||
                                                 q.ProcessingStatus == 7).ToList();
            }

            if (isLate == 0)
            {
                //datatable = datatable.Where(q => q.CheckMin - shiftStartTime >= new TimeSpan(0, 0, expandTime, 0) && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
                datatable = datatable.Where(q => q.CheckMin - q.ShiftMin >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
            }
            else if (isLate == 1)
            {
                //datatable = datatable.Where(q => shiftEndTime - q.CheckMax >= new TimeSpan(0, 0, expandTime, 0) && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
                datatable = datatable.Where(q => q.ShiftMax - q.CheckMax >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
            }
            else if (isLate == 2)
            {
                //datatable = datatable.Where(q => shiftEndTime - q.CheckMax < new TimeSpan(0, 0, expandTime, 0) && q.CheckMin - shiftStartTime < new TimeSpan(0, 0, expandTime, 0)).ToList();
                datatable = datatable.Where(q => q.ShiftMax - q.CheckMax < q.ExpandTime && q.CheckMin - q.ShiftMin < q.ExpandTime).ToList();
            }

            if (typeShift != null)
            {
                if (typeShift == 0)
                {
                    datatable = datatable.Where(q => q.IsOverTime != true).ToList();
                }
                if (typeShift == 1)
                {
                    datatable = datatable.Where(q => q.IsOverTime == true).ToList();
                }
            }
            var storeApi = new StoreApi();
            var result = datatable.OrderByDescending(q => q.TotalWorkTime).Select(q => new IConvertible[]
            {
                count++,
                employeeApi.Get(q.EmployeeId).Name,
                q.CheckMin == null ? "Chưa cập nhật" : q.CheckMin.Value.ToString("HH:mm"),
                q.CheckMax == null ? "Chưa cập nhật" : q.CheckMax.Value.ToString("HH:mm"),
                q.TotalWorkTime == null ? "N/A" :  DateTime.Today.Add(q.TotalWorkTime.Value).ToString("HH:mm"),
                q.Id,
                (q.CheckMax==null||q.CheckMin==null)? false:true,
                //q.CheckMin == null ? false : ((q.ShiftMin-q.CheckMin < (-q.ExpandTime))? false:true),
                q.CheckMin == null ? 1 : (q.CheckMin.Value.TimeOfDay>q.ShiftMin.TimeOfDay ? 2 : 3),
                //q.CheckMax == null ? false : ((q.ShiftMax-q.CheckMax > q.ExpandTime)? false:true),
                q.CheckMax == null ? 1 : (q.CheckMax.Value.TimeOfDay<q.ShiftMax.TimeOfDay ? 2 : 3),
                q.Status == null ? "N/A" : ((StatusAttendanceEnum)q.Status).DisplayName(),
                q.UpdatePerson == null ? "N/A" : q.UpdatePerson,
                q.ProcessingStatus == null ? "N/A" : ((ProcessingStatusAttendenceEnum)q.ProcessingStatus).DisplayName(),
                q.ShiftMin.ToString("HH:mm"),
                q.ShiftMax.ToString("HH:mm"),
                q.ShiftMin.ToString("dd/MM/yyyy"),
                q.Note == null ? "" : q.Note,
                q.IsOverTime,
                (employeeApi.Get(q.EmployeeId).MainStoreId != storeId)?storeApi.Get(employeeApi.Get(q.EmployeeId).MainStoreId).Name:"",
            }).ToList();
            //TimeSpan totalWorkTimeOfShift = new TimeSpan(datatable.Sum(q => q.TotalWorkTime.GetValueOrDefault().Ticks));
            //var timenull = TimeSpan.Zero;

            //var listemp = datatable.Where(q => q.TotalWorkTime != null && q.TotalWorkTime != timenull).Count();
            //var listempnotright = datatable.Count() - listemp;
            //var numberOfEmpCheckRight = datatable.Count(q => q.CheckMax.GetValueOrDefault() - q.CheckMin.GetValueOrDefault() > new TimeSpan(0, 0, 1, 0) && q.Status == 1);
            //var numberOfEmpNA = datatable.Count(q => q.Status == 0);
            //var numberOfEmpNotRight = datatable.Count(q => q.CheckMin - shiftStartTime >= new TimeSpan(0, 0, expandTime, 0) || shiftEndTime - q.CheckMax >= new TimeSpan(0, 0, expandTime, 0));
            //var numberOfEmp = datatable.Count(q => q.Status == 1);
            //var avgTotalWorkTimeOfShift = new TimeSpan(0, 0, 0);
            //if (numberOfEmpCheckRight != 0)
            //{
            //    avgTotalWorkTimeOfShift = TimeSpan.FromHours(totalWorkTimeOfShift.TotalHours / (datatable.Count() - numberOfEmpNotRight));
            //}

            //foreach (var item in datatable)
            //{
            //    totalWorkDayOfShift = totalWorkDayOfShift + item.TotalWorkTime.GetValueOrDefault();
            //}

            //bool shiftWorkDay;
            //if (shiftApi.Get(shiftId).StartTime.GetValueOrDefault().Date >= DateTime.Now.Date)
            //{
            //    shiftWorkDay = true;
            //}
            //else
            //{
            //    shiftWorkDay = false;
            //}
            //var test = totalWorkTimeOfShift.ToString().Contains(".") ? totalWorkTimeOfShift.ToString().Split('.')[0] + " ngày " + totalWorkTimeOfShift.ToString().Split('.')[1] : totalWorkTimeOfShift.ToString();
            return Json(new
            {
                result = result,
                /*rsShiftWorkDay = shiftWorkDay,*/
                //numberOfEmp = listemp,
                //totalWorkTimeOfShift = totalWorkTimeOfShift.ToString().Contains(".") ? totalWorkTimeOfShift.ToString().Split('.')[0] + " ngày " + totalWorkTimeOfShift.ToString().Split('.')[1] : totalWorkTimeOfShift.ToString(),
                //avgTotalWorkTimeOfShift = avgTotalWorkTimeOfShift.ToString("g"),
                //numberOfEmpCheckRight = listemp,
                //numberOfEmpNA = listempnotright,
                //numberOfEmpNotRight = numberOfEmpNotRight,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSpecificAttendance(int storeId, int status, int empType, int isLate, string strDateChooseStart, string strDateChooseEnd, int empId, int? typeShift)
        {
            int count = 1;
            var api = new AttendanceApi();
            var employeeApi = new EmployeeApi();
            var dateChooseStart = strDateChooseStart.ToDateTime();
            var dateChooseEnd = strDateChooseEnd.ToDateTime().GetEndOfDate();


            var datatable = api.GetActive().Where(q => q.ShiftMin.Date >= dateChooseStart.Date &&
                                                        q.ShiftMax.Date <= dateChooseEnd.Date &&
                                                        q.EmployeeId == empId &&
                                                        q.StoreId == storeId
                                                        /*q.Status != (int)StatusAttendanceEnum.Absent*/).ToList();
            if (status == 0)
            {
                datatable = datatable.Where(q => q.Status == 0).ToList();
            }
            else if (status == 1)
            {
                datatable = datatable.Where(q => q.Status == 1).ToList();
            }
            else if (status == 2)
            {
                datatable = datatable.Where(q => q.Status == 2).ToList();
            }

            if (empType == 0)
            {
                datatable = datatable.Where(q => q.ProcessingStatus == 0 ||
                                                 q.ProcessingStatus == 1 ||
                                                 q.ProcessingStatus == 2 ||
                                                 q.ProcessingStatus == 3 ||
                                                 q.ProcessingStatus == 5).ToList();
            }
            else if (empType == 1)
            {
                datatable = datatable.Where(q => q.ProcessingStatus == 6 ||
                                                 q.ProcessingStatus == 7).ToList();
            }

            if (isLate == 0)
            {
                //datatable = datatable.Where(q => q.CheckMin - shiftStartTime >= new TimeSpan(0, 0, expandTime, 0) && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
                datatable = datatable.Where(q => q.CheckMin - q.ShiftMin >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
            }
            else if (isLate == 1)
            {
                //datatable = datatable.Where(q => shiftEndTime - q.CheckMax >= new TimeSpan(0, 0, expandTime, 0) && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
                datatable = datatable.Where(q => q.ShiftMax - q.CheckMax >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
            }
            else if (isLate == 2)
            {
                //datatable = datatable.Where(q => shiftEndTime - q.CheckMax < new TimeSpan(0, 0, expandTime, 0) && q.CheckMin - shiftStartTime < new TimeSpan(0, 0, expandTime, 0)).ToList();
                datatable = datatable.Where(q => q.ShiftMax - q.CheckMax < q.ExpandTime && q.CheckMin - q.ShiftMin < q.ExpandTime).ToList();
            }

            if (typeShift != null)
            {
                if (typeShift == 0)
                {
                    datatable = datatable.Where(q => q.IsOverTime != true).ToList();
                }
                if (typeShift == 1)
                {
                    datatable = datatable.Where(q => q.IsOverTime == true).ToList();
                }
            }
            var storeApi = new StoreApi();
            var result = datatable.OrderByDescending(q => q.TotalWorkTime).Select(q => new IConvertible[]
            {
                count++,
                employeeApi.Get(q.EmployeeId).Name,
                //q.CheckMin == null ? "Chưa cập nhật" : q.CheckMin.Value.ToString("HH:mm"),
                //q.CheckMax == null ? "Chưa cập nhật" : q.CheckMax.Value.ToString("HH:mm"),
                //q.TotalWorkTime == null ? "N/A" :  DateTime.Today.Add(q.TotalWorkTime.Value).ToString("HH:mm"),
                q.Id,
                //(q.CheckMax==null||q.CheckMin==null)? false:true,
                //q.CheckMin == null ? false : ((q.ShiftMin-q.CheckMin <= (-q.ExpandTime))? false:true),
                //q.CheckMax == null ? false : ((q.ShiftMax-q.CheckMax >= q.ExpandTime)? false:true),
                q.Status == null ? "N/A" : ((StatusAttendanceEnum)q.Status).DisplayName(),
                //q.UpdatePerson == null ? "N/A" : q.UpdatePerson,
                q.ProcessingStatus == null ? "N/A" : ((ProcessingStatusAttendenceEnum)q.ProcessingStatus).DisplayName(),
                q.ShiftMin.ToString("HH:mm"),
                q.ShiftMax.ToString("HH:mm"),
                q.ShiftMin.ToString("dd/MM/yyyy"),
                //q.Note == null ? "" : q.Note,
                //q.IsOverTime         
            }).ToList();
            //TimeSpan totalWorkTimeOfShift = new TimeSpan(datatable.Sum(q => q.TotalWorkTime.GetValueOrDefault().Ticks));
            //var timenull = TimeSpan.Zero;

            //var listemp = datatable.Where(q => q.TotalWorkTime != null && q.TotalWorkTime != timenull).Count();
            //var listempnotright = datatable.Count() - listemp;
            //var numberOfEmpCheckRight = datatable.Count(q => q.CheckMax.GetValueOrDefault() - q.CheckMin.GetValueOrDefault() > new TimeSpan(0, 0, 1, 0) && q.Status == 1);
            //var numberOfEmpNA = datatable.Count(q => q.Status == 0);
            //var numberOfEmpNotRight = datatable.Count(q => q.CheckMin - shiftStartTime >= new TimeSpan(0, 0, expandTime, 0) || shiftEndTime - q.CheckMax >= new TimeSpan(0, 0, expandTime, 0));
            //var numberOfEmp = datatable.Count(q => q.Status == 1);
            //var avgTotalWorkTimeOfShift = new TimeSpan(0, 0, 0);
            //if (numberOfEmpCheckRight != 0)
            //{
            //    avgTotalWorkTimeOfShift = TimeSpan.FromHours(totalWorkTimeOfShift.TotalHours / (datatable.Count() - numberOfEmpNotRight));
            //}

            //foreach (var item in datatable)
            //{
            //    totalWorkDayOfShift = totalWorkDayOfShift + item.TotalWorkTime.GetValueOrDefault();
            //}

            //bool shiftWorkDay;
            //if (shiftApi.Get(shiftId).StartTime.GetValueOrDefault().Date >= DateTime.Now.Date)
            //{
            //    shiftWorkDay = true;
            //}
            //else
            //{
            //    shiftWorkDay = false;
            //}
            //var test = totalWorkTimeOfShift.ToString().Contains(".") ? totalWorkTimeOfShift.ToString().Split('.')[0] + " ngày " + totalWorkTimeOfShift.ToString().Split('.')[1] : totalWorkTimeOfShift.ToString();
            return Json(new
            {
                result = result,
                /*rsShiftWorkDay = shiftWorkDay,*/
                //numberOfEmp = listemp,
                //totalWorkTimeOfShift = totalWorkTimeOfShift.ToString().Contains(".") ? totalWorkTimeOfShift.ToString().Split('.')[0] + " ngày " + totalWorkTimeOfShift.ToString().Split('.')[1] : totalWorkTimeOfShift.ToString(),
                //avgTotalWorkTimeOfShift = avgTotalWorkTimeOfShift.ToString("g"),
                //numberOfEmpCheckRight = listemp,
                //numberOfEmpNA = listempnotright,
                //numberOfEmpNotRight = numberOfEmpNotRight,
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetAttendanceAtBrand(string strDateChooseStart, string strDateChooseEnd, int status, int empType, int isLate, int storeIdChoosed)
        {
            var dateChooseStart = Utils.ToDateTime(strDateChooseStart);
            var dateChooseEnd = Utils.ToDateTime(strDateChooseEnd);
            int count = 1;
            var api = new AttendanceApi();
            var employeeApi = new EmployeeApi();


            //var datatable = api.GetActive().Where(q => q.ShiftMin.Date >= dateChooseStart && q.ShiftMin.Date <= dateChooseEnd && q.StoreId == storeId).ToList();
            var datatable = api.GetActive().Where(q => q.ShiftMin.Date >= dateChooseStart && q.ShiftMin.Date <= dateChooseEnd).ToList();

            if (storeIdChoosed != 0)
            {
                datatable = datatable.Where(q => q.StoreId == storeIdChoosed).ToList();
            }

            if (status == 0)
            {
                datatable = datatable.Where(q => q.Status == 0).ToList();
            }
            else if (status == 1)
            {
                datatable = datatable.Where(q => q.Status == 1).ToList();
            }
            else if (status == 2)
            {
                datatable = datatable.Where(q => q.Status == 2).ToList();
            }

            if (empType == 0)
            {
                datatable = datatable.Where(q => q.ProcessingStatus == 0 ||
                                                 q.ProcessingStatus == 1 ||
                                                 q.ProcessingStatus == 2 ||
                                                 q.ProcessingStatus == 3 ||
                                                 q.ProcessingStatus == 5).ToList();
            }
            else if (empType == 1)
            {
                datatable = datatable.Where(q => q.ProcessingStatus == 6 ||
                                                 q.ProcessingStatus == 7).ToList();
            }

            if (isLate == 0)
            {
                //datatable = datatable.Where(q => q.CheckMin - shiftStartTime >= new TimeSpan(0, 0, expandTime, 0) && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
                datatable = datatable.Where(q => q.CheckMin - q.ShiftMin >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
            }
            else if (isLate == 1)
            {
                //datatable = datatable.Where(q => shiftEndTime - q.CheckMax >= new TimeSpan(0, 0, expandTime, 0) && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
                datatable = datatable.Where(q => q.ShiftMax - q.CheckMax >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
            }
            else if (isLate == 2)
            {
                //datatable = datatable.Where(q => shiftEndTime - q.CheckMax < new TimeSpan(0, 0, expandTime, 0) && q.CheckMin - shiftStartTime < new TimeSpan(0, 0, expandTime, 0)).ToList();
                datatable = datatable.Where(q => q.ShiftMax - q.CheckMax < q.ExpandTime && q.CheckMin - q.ShiftMin < q.ExpandTime).ToList();
            }


            var result = datatable.OrderBy(q => q.ProcessingStatus).ThenByDescending(q => q.TotalWorkTime).Select(q => new IConvertible[]
            {
                count++,
                employeeApi.Get(q.EmployeeId).Name,
                //shiftApi.Get(shiftId).StartTime.ToString(),
                //shiftApi.Get(shiftId).EndTime.ToString(),
                q.CheckMin == null ? "Chưa cập nhật" : q.CheckMin.Value.ToString("HH:mm"),
                q.CheckMax == null ? "Chưa cập nhật" : q.CheckMax.Value.ToString("HH:mm"),
                q.TotalWorkTime.ToString(),
                //q.TotalWorkTime.ToString(),
                q.Id,
                (q.CheckMax==null||q.CheckMin==null)?"chưa check đủ":"check đủ",
                (q.CheckMin-q.ShiftMin>= q.ExpandTime)? "Đi trễ":"Chuẩn vào",
                (q.ShiftMax-q.CheckMax>= q.ExpandTime)? "Về sớm":"Chuẩn ra",
                //((StatusAttendanceEnum)q.Status).DisplayName(),
                q.Status == null ? "N/a" : ((StatusAttendanceEnum)q.Status).DisplayName(),
                q.UpdatePerson == null ? "N/a" : q.UpdatePerson,
                q.ProcessingStatus == null ? "N/a" : ((ProcessingStatusAttendenceEnum)q.ProcessingStatus).DisplayName(),
                q.ShiftMin.ToString("HH:mm"),
                q.ShiftMax.ToString("HH:mm")
            }).ToList();
            TimeSpan totalWorkTimeOfShift = new TimeSpan(datatable.Sum(q => q.TotalWorkTime.GetValueOrDefault().Ticks));
            var timenull = TimeSpan.Zero;

            //var listemp = datatable.Where(q => q.TotalWorkTime != null && q.TotalWorkTime != timenull).Count();
            //var listempnotright = datatable.Count() - listemp;
            //var numberOfEmpCheckRight = datatable.Count(q => q.CheckMax.GetValueOrDefault() - q.CheckMin.GetValueOrDefault() > new TimeSpan(0, 0, 1, 0) && q.Status == 1);
            //var numberOfEmpNA = datatable.Count(q => q.Status == 0);
            //var numberOfEmpNotRight = datatable.Count(q => q.CheckMin - shiftStartTime >= new TimeSpan(0, 0, expandTime, 0) || shiftEndTime - q.CheckMax >= new TimeSpan(0, 0, expandTime, 0));
            //var numberOfEmp = datatable.Count(q => q.Status == 1);
            //var avgTotalWorkTimeOfShift = new TimeSpan(0, 0, 0);
            //if (numberOfEmpCheckRight != 0)
            //{
            //    avgTotalWorkTimeOfShift = TimeSpan.FromHours(totalWorkTimeOfShift.TotalHours / (datatable.Count() - numberOfEmpNotRight));
            //}

            //foreach (var item in datatable)
            //{
            //    totalWorkDayOfShift = totalWorkDayOfShift + item.TotalWorkTime.GetValueOrDefault();
            //}

            //bool shiftWorkDay;
            //if (shiftApi.Get(shiftId).StartTime.GetValueOrDefault().Date >= DateTime.Now.Date)
            //{
            //    shiftWorkDay = true;
            //}
            //else
            //{
            //    shiftWorkDay = false;
            //}
            //var test = totalWorkTimeOfShift.ToString().Contains(".") ? totalWorkTimeOfShift.ToString().Split('.')[0] + " ngày " + totalWorkTimeOfShift.ToString().Split('.')[1] : totalWorkTimeOfShift.ToString();
            return Json(new
            {
                result = result,
                /*rsShiftWorkDay = shiftWorkDay,*/
                //numberOfEmp = listemp,
                //totalWorkTimeOfShift = totalWorkTimeOfShift.ToString().Contains(".") ? totalWorkTimeOfShift.ToString().Split('.')[0] + " ngày " + totalWorkTimeOfShift.ToString().Split('.')[1] : totalWorkTimeOfShift.ToString(),
                //avgTotalWorkTimeOfShift = avgTotalWorkTimeOfShift.ToString("g"),
                //numberOfEmpCheckRight = listemp,
                //numberOfEmpNA = listempnotright,
                //numberOfEmpNotRight = numberOfEmpNotRight,
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "BrandManager, ShiftEditor")]
        public ActionResult DeleteAttendance(int attendanceId)
        {
            var user = HttpContext.User;
            var result = true;
            var api = new AttendanceApi();
            var entity = api.Get(attendanceId);

            if (entity == null)
            {
                result = false;

            }
            bool flag = true;
            var CanConfig = WebConfigurationManager.AppSettings["CanConfig"];
            if (user.IsInRole("BrandManager") || (user.IsInRole("StoreManager") && (CanConfig == "true")))
            {
                try
                {
                    entity.Active = false;
                    api.Edit(attendanceId, entity);
                }
                catch (Exception ex)
                {
                    result = false;
                }
            }
            else
            {
                flag = false;
            }

            return Json(new { result = result, flag = flag }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "BrandManager, ShiftEditor")]
        public ActionResult DeleteWeeklyAttendance(string start, string end, int storeId)
        {
            var startDate = start.ToDateTime().GetStartOfDate();
            var endDate = end.ToDateTime().GetEndOfDate();
            var user = HttpContext.User;
            var result = true;
            var api = new AttendanceApi();
            var listAtt = api.GetActive().Where(q => q.ShiftMin >= startDate && q.ShiftMax <= endDate && q.StoreId == storeId).ToList();
            if (!listAtt.Any())
            {
                result = false;
            }
            bool flag = true;
            var CanConfig = WebConfigurationManager.AppSettings["CanConfig"];
            if (user.IsInRole("BrandManager") || (user.IsInRole("StoreManager") && (CanConfig == "true")))
            {
                foreach (var item in listAtt)
                {
                    try
                    {
                        item.Active = false;
                        api.BaseService.Update(item.ToEntity());
                    }
                    catch (Exception ex)
                    {
                        result = false;
                    }
                }
            }
            else
            {
                flag = false;
            }

            return Json(new { result = result, flag = flag }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExportDateStoreReportToExcel(List<ReportEmployee> data, List<string> daysOfWeek)
        {
            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");



                #region header
                char startchar = 'A'; char endchar = 'Z';
                ws.Cells["A1:A3"].Merge = true;
                ws.Cells["A1:A3"].Value = "STT";
                ws.Cells["B1:B3"].Merge = true;
                ws.Cells["B1:B3"].Value = "Họ và Tên";
                ws.Cells["C1:AG1"].Merge = true;
                ws.Cells["C1:AG1"].Value = "Ngày trong tháng ";


                char ongoing = 'C';
                var o = 2;
                int date = 1;
                int cell = 31;
                int i;
                int checkon = 0;
                string on = "A";
                String[] array = { "A", "B", "C", "D", "E", "F", "G" };
                String[] thungay = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ Nhật" };
                foreach (var day in daysOfWeek)
                {
                    if (ongoing < endchar && ongoing >= startchar)
                    {
                        ws.Cells["" + (ongoing) + (o)].Value = date;
                        ongoing++;
                    }
                    else if (ongoing.Equals(endchar) == true)
                    {
                        ws.Cells["" + (ongoing) + (o)].Value = date;
                        ongoing++;
                    }
                    else
                    {
                        string next = on + array[checkon];
                        ws.Cells["" + (next) + (o)].Value = date;
                        checkon++;
                    }
                    date++;
                }
                int thu = 0;
                int checkon1 = 0;
                string on1 = "A";
                char ongoing1 = 'C';
                foreach (var day in daysOfWeek)
                {
                    if (ongoing1 <= endchar && ongoing1 >= startchar)
                    {
                        ws.Cells["" + (ongoing1) + (3)].Value = day;
                        ongoing1++;
                    }
                    else
                    {
                        string next = on1 + array[checkon1];
                        ws.Cells["" + (next) + (3)].Value = day;
                        checkon1++;
                    }
                    thu++;
                    if (!(thu < 7 && thu >= 0))
                    {
                        thu = 0;
                    }
                    if (checkon1 > 6)
                    {
                        checkon1 = 0;
                    }
                }
                ws.Cells["AH1:AH3"].Merge = true;
                ws.Cells["AH1:AH3"].Value = "Tổng cộng ngày công";


                #endregion
                //Set style for excel

                #region set value for cells
                int StartHeaderNumber = 4;
                foreach (var item in data)
                {
                    char StartHeaderChar = 'B';
                    string doub = "B";
                    string[] plus = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
                    int countOfPlus = 0;

                    ws.Cells["" + StartHeaderChar++ + StartHeaderNumber].Value = item.empName;
                    ws.Cells["" + "AH" + StartHeaderNumber].Value = item.totalTime;
                    foreach (var time in item.time)
                    {

                        if (StartHeaderChar <= 'Z')
                        {
                            ws.Cells["" + StartHeaderChar++ + StartHeaderNumber].Value = time;

                        }
                        else
                        {
                            string next = "A" + plus[countOfPlus];
                            ws.Cells["" + next + StartHeaderNumber].Value = time;
                            countOfPlus = countOfPlus + 1;
                        }
                    }
                    StartHeaderNumber = StartHeaderNumber + 1;
                }

                #endregion
                #region style
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ws.Cells[ws.Dimension.Address].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[ws.Dimension.Address].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["A1:A3,B1:B3,C1:AG1,AH1:AH3"].Style.Font.Color.SetColor(Color.DeepSkyBlue);
                ws.Cells["A1:A3,B1:B3,C1:AG1,AH1:AH3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A1:A3,B1:B3,C1:AG1,AH1:AH3"].Style.Fill.PatternColor.SetColor(Color.Gold);
                ws.Cells["A1:A3,B1:B3,C1:AG1,AH1:AH3"].Style.Fill.BackgroundColor.SetColor(Color.Gold);
                ws.Cells["A1:AH3"].Style.WrapText = true;
                ws.Cells["C1:AG1"].Style.Font.Size = 18;

                ws.Cells[ws.Dimension.Address].Style.WrapText = true;
                string fileName = "bao cao cham cong" + ".xlsx";
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }
        public ActionResult CreateMoreAttendance()
        {
            return View();
        }

        [Authorize(Roles = "BrandManager, ShiftBrowser")]
        public ActionResult UpdateAttendanceStt(int storeId, int stt, int attId)
        {
            var user = HttpContext.User;
            var name = HttpContext.User.Identity.Name;
            var attApi = new AttendanceApi();
            var att = attApi.Get(attId);
            bool rs = false;
            bool flag = true;
            var CanConfig = WebConfigurationManager.AppSettings["CanConfig"];
            if (user.IsInRole("BrandManager") || (user.IsInRole("StoreManager") && (CanConfig == "true")))
            {
                try
                {
                    att.Status = stt;
                    //att.ProcessingStatus = 3;
                    att.UpdatePerson = name;
                    attApi.Edit(attId, att);
                    rs = true;
                }
                catch (Exception ex)
                {
                    rs = false;
                }
            }
            else
            {
                flag = false;
            }

            return Json(new { rs = rs, flag = flag }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExportToExcelReport(string strChooseDay, /*string strDateExportStart, string strDateExportEnd,*/ int storeId)
        {
            //var dateExportStart = strDateExportStart.ToDateTime().GetStartOfDate();
            //var dateExportEnd = strDateExportEnd.ToDateTime().GetEndOfDate();
            var chooseDay = strChooseDay.ToDateTime();
            #region getdata
            int count = 1;
            var attendanceApi = new AttendanceApi();
            var employeeApi = new EmployeeApi();
            //var shiftApi = new ShiftApi();
            var storeapi = new StoreApi();
            //var shift = shiftApi.Get(shiftId);

            //var listAttendances = attendanceApi.GetActive().Where(q => q.ShiftMin.Date >= dateExportStart && q.ShiftMin <= dateExportEnd && q.StoreId == storeId).ToList();
            var listAttendances = attendanceApi.GetActive().Where(q => q.ShiftMin.Date == chooseDay.Date && q.StoreId == storeId).ToList();

            //var shiftStartTime = shift.StartTime;
            //var shiftEndTime = shift.EndTime;
            var result = listAttendances.Select(q => new IConvertible[]
            {
                count++,
                employeeApi.Get(q.EmployeeId).Name,
                 storeapi.GetStoreNameByID( employeeApi.Get(q.EmployeeId).MainStoreId),
                q.CheckMin == null ? "Chưa cập nhật" : q.CheckMin.Value.ToString("dd/MM/yyyy HH:mm"),
                q.CheckMax == null ? "Chưa cập nhật" : q.CheckMax.Value.ToString("dd/MM/yyyy HH:mm"),
                q.TotalWorkTime.ToString(),
                q.Id,
                (q.CheckMax==null||q.CheckMin==null)?"chưa check đủ":"check đủ",
                (q.CheckMin-q.ShiftMin>= q.ExpandTime)? "Đi trễ":"Chuẩn",
                (q.ShiftMax-q.CheckMax>=q.ExpandTime)? "Về sớm":"Chuẩn"
            }).ToList();
            #endregion
            return ExportReportToExcel(result, chooseDay);

        }
        public ActionResult ExportReportToExcel(List<IConvertible[]> data, DateTime dayexport)
        {
            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo");
                #region header
                ws.Cells["A1:A2"].Merge = true;
                ws.Cells["A1:A2"].Value = "STT";
                ws.Cells["A1:A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A1:A2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["A1:A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells["B1:B2"].Merge = true;
                ws.Cells["B1:B2"].Value = "Họ và Tên";
                ws.Cells["B1:B2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["B1:B2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["B1:B2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells["C1:C2"].Merge = true;
                ws.Cells["C1:C2"].Value = "Tên Cửa Hàng";
                ws.Cells["C1:C2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["C1:C2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["C1:C2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                ws.Cells["D1:D2"].Merge = true;
                ws.Cells["D1:D2"].Value = "Giờ vào";
                ws.Cells["D1:D2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["D1:D2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["D1:D2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells["E1:E2"].Merge = true;
                ws.Cells["E1:E2"].Value = "Giờ ra";
                ws.Cells["E1:E2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["E1:E2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["E1:E2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells["F1:F2"].Merge = true;
                ws.Cells["F1:F2"].Value = "Trạng Thái";
                ws.Cells["F1:F2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["F1:F2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["F1:F2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells["G1:G2"].Merge = true;
                ws.Cells["G1:G2"].Value = "Số Giờ Làm";
                ws.Cells["G1:G2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["G1:G2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["G1:G2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells["H1:H2"].Merge = true;
                ws.Cells["H1:H2"].Value = "Tình trạng làm việc";
                ws.Cells["H1:H2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["H1:H2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["H1:H2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                #endregion
                //Set style for excel

                #region set value for cells

                int indexRow = 3;
                foreach (var item in data)
                {
                    int indexCol = 1;
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[0];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[1];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[2];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[3];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[4];
                    //trang thai
                    var check = item[7];
                    var vao = item[8];
                    var ra = item[9];
                    var chuan = "Đúng giờ";
                    var tre = "Đi trễ";
                    var som = "Về sớm";
                    var strTrangThai = "";
                    if (check == "chưa check đủ")
                    {
                        strTrangThai = "--";
                    }
                    else
                    {
                        if (vao != "Chuẩn")
                            if (strTrangThai == "") strTrangThai = strTrangThai + tre;
                            else strTrangThai = strTrangThai + ", " + tre;
                        if (ra != "Chuẩn")
                            if (strTrangThai == "") strTrangThai = strTrangThai + som;
                            else strTrangThai = strTrangThai + ", " + som;
                    }
                    if (strTrangThai == "") strTrangThai = chuan;

                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = strTrangThai;
                    //
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[5];
                    var tinhtrang = "";
                    if (check == "chưa check đủ")
                    {
                        tinhtrang = "vắng";
                    }
                    else
                    {
                        tinhtrang = "có mặt";
                    }
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = tinhtrang;
                    indexRow = indexRow + 1;
                }
                #endregion
                #region style
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ws.Cells[ws.Dimension.Address].Style.WrapText = true;
                string fileName = "Báo cáo chấm công ngày " + dayexport.ToString("dd-MM-yyyy") + ".xlsx";
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }

        #region Export Excel Template
        public ActionResult ExportToExcelReportTemplate(string strChooseDay, /*string strDateExportStart, string strDateExportEnd,*/ int storeId)
        {
            //var dateExportStart = strDateExportStart.ToDateTime().GetStartOfDate();
            //var dateExportEnd = strDateExportEnd.ToDateTime().GetEndOfDate();
            var chooseDay = strChooseDay.ToDateTime();
            #region getdata
            int count = 1;
            var attendanceApi = new AttendanceApi();
            var groupApi = new EmployeeGroupApi();
            var employeeApi = new EmployeeApi();
            //var shiftApi = new ShiftApi();
            var storeapi = new StoreApi();
            //var shift = shiftApi.Get(shiftId);

            //var listAttendances = attendanceApi.GetActive().Where(q => q.ShiftMin.Date >= dateExportStart && q.ShiftMin <= dateExportEnd && q.StoreId == storeId).ToList();
            var listAttendances = attendanceApi.GetActive().Where(q => q.ShiftMin.Date == chooseDay.Date && q.StoreId == storeId).ToList();

            //var shiftStartTime = shift.StartTime;
            //var shiftEndTime = shift.EndTime;
            var listResult = new List<ReportDetailExcel>();
            foreach (var item in listAttendances)
            {
                count++;
                var emp = employeeApi.Get(item.EmployeeId);
                var groupEmp = groupApi.BaseService.Get(emp.EmployeeGroupId);
                var report = new ReportDetailExcel();
                report.Stt = count;
                report.Department = "";
                report.Name = emp.Name;
                report.Group = groupEmp.NameGroup;
                report.EmpId = emp.EmpEnrollNumber;
                report.Name = emp.Name;
                report.Date = item.ShiftMin.ToString("dd/MM/yyyy");
                report.WorkShift = item.ShiftMin.ToString("HH:mm") + " - " + item.ShiftMax.ToString("HH:mm");
                report.CheckIn = item.CheckMin != null ? item.CheckMin.Value.ToString("HH:mm") : "";
                report.CheckOut = item.CheckMax != null ? item.CheckMax.Value.ToString("HH:mm") : "";
                report.TotalTime = item.TotalWorkTime != null ? Math.Round(item.TotalWorkTime.Value.TotalHours, 2) : 0;
                report.OverTime = 0;
                listResult.Add(report);
            }

            #endregion
            return ExportReportToExcelTemplate(listResult, chooseDay);

        }

        public ActionResult ExportToExcelReportBrandTemplate(string strChooseDay, /*string strDateExportStart, string strDateExportEnd,*/ int listStoreId, int brandId)
        {
            //var dateExportStart = strDateExportStart.ToDateTime().GetStartOfDate();
            //var dateExportEnd = strDateExportEnd.ToDateTime().GetEndOfDate();
            var chooseDay = strChooseDay.ToDateTime();
            #region getdata
            int count = 0;
            var attendanceApi = new AttendanceApi();
            var groupApi = new EmployeeGroupApi();
            var employeeApi = new EmployeeApi();
            //var shiftApi = new ShiftApi();
            var storeapi = new StoreApi();
            //var shift = shiftApi.Get(shiftId);

            var listAttendances = new List<AttendanceViewModel>();
            var listStore = storeapi.GetActive().Where(q => q.BrandId == brandId).ToList();
            if (listStoreId != 0)
            {
                listAttendances = attendanceApi.GetActive().Where(q => q.ShiftMin.Date == chooseDay.Date && q.StoreId == listStoreId).ToList();
            }
            else
            {
                listAttendances = attendanceApi.GetActive().Where(q => q.ShiftMin.Date == chooseDay.Date && listStore.Any(a => a.ID == q.StoreId)).ToList();
            }

            //var shiftStartTime = shift.StartTime;
            //var shiftEndTime = shift.EndTime;
            var listResult = new List<ReportDetailExcel>();
            foreach (var item in listAttendances)
            {
                count++;
                var emp = employeeApi.Get(item.EmployeeId);
                var groupEmp = groupApi.BaseService.Get(emp.EmployeeGroupId);
                var report = new ReportDetailExcel();
                report.Stt = count;
                report.Department = "";
                report.Name = emp.Name;
                report.Group = groupEmp.NameGroup;
                report.EmpId = emp.EmpEnrollNumber;
                report.Name = emp.Name;
                report.Date = item.ShiftMin.ToString("dd/MM/yyyy");
                report.WorkShift = item.ShiftMin.ToString("HH:mm") + " - " + item.ShiftMax.ToString("HH:mm");
                report.CheckIn = item.CheckMin != null ? item.CheckMin.Value.ToString("HH:mm") : "";
                report.CheckOut = item.CheckMax != null ? item.CheckMax.Value.ToString("HH:mm") : "";
                if (item.IsOverTime == true)
                {
                    report.OverTime = item.TotalWorkTime != null ? Math.Round(item.TotalWorkTime.Value.TotalHours, 2) : 0;
                    report.TotalTime = 0;
                }
                else
                {
                    report.TotalTime = item.TotalWorkTime != null ? Math.Round(item.TotalWorkTime.Value.TotalHours, 2) : 0;
                    report.OverTime = 0;
                }
                listResult.Add(report);
            }

            #endregion
            return ExportReportToExcelTemplate(listResult, chooseDay);

        }
        public ActionResult ExportReportToExcelTemplate(List<ReportDetailExcel> data, DateTime dayexport)
        {

            string filepath = HttpContext.Server.MapPath(@"/Resource/DetailReport.xlsx");
            var filestream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);

            #region Export to Excel
            using (ExcelPackage package = new ExcelPackage(filestream))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets[1];
                ws.Cells["K2"].Value = HttpContext.User.Identity.Name;
                ws.Cells["K3"].Value = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");

                int indexRow = 6;
                foreach (var item in data)
                {
                    indexRow++;
                    ws.Cells[indexRow, 1].Value = item.Stt;
                    ws.Cells[indexRow, 2].Value = item.Department;
                    ws.Cells[indexRow, 3].Value = item.Group;
                    ws.Cells[indexRow, 4].Value = item.EmpId;
                    ws.Cells[indexRow, 5].Value = item.Name;
                    ws.Cells[indexRow, 6].Value = item.Date;
                    ws.Cells[indexRow, 7].Value = item.WorkShift;
                    ws.Cells[indexRow, 8].Value = item.CheckIn;
                    ws.Cells[indexRow, 9].Value = item.CheckOut;
                    ws.Cells[indexRow, 10].Value = item.TotalTime;
                    ws.Cells[indexRow, 11].Value = item.OverTime;
                }
                #region style
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ws.Cells[ws.Dimension.Address].Style.WrapText = true;
                MemoryStream ms = new MemoryStream();
                string fileName = "Báo cáo chấm công ngày " + dayexport.ToString("dd-MM-yyyy") + ".xlsx";
                #endregion
                package.SaveAs(ms);
                filestream.Close();
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }
        #endregion
        public string GetColNameFromIndex(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        public JsonResult GetAttendanceShift(string storeSelected, string DateSelected)
        {
            var Datechoose = Utils.ToDateTime(DateSelected);
            int storeid = 0;
            Int32.TryParse(storeSelected, out storeid);
            int count = 1;
            var api = new AttendanceApi();
            var employeeApi = new EmployeeApi();
            var datatable = api.GetActive();
            if (storeid == 0)
            {
                datatable = api.GetActive().Where(q => q.ShiftMin.Day == Datechoose.Day).ToList();
            }
            else
            {
                datatable = api.GetActive().Where(q => q.StoreId == storeid && q.ShiftMin.Day == Datechoose.Day).ToList();
            }

            var result = datatable.OrderBy(q => q.ProcessingStatus).ThenByDescending(q => q.TotalWorkTime).Select(q => new IConvertible[]
            {
                count++,
                employeeApi.Get(q.EmployeeId).Name,
                q.CheckMin == null ? "Chưa cập nhật" : q.CheckMin.Value.ToString("HH:mm"),
                q.CheckMax == null ? "Chưa cập nhật" : q.CheckMax.Value.ToString("HH:mm"),
                q.TotalWorkTime.ToString(),     
                //(q.CheckMax==null||q.CheckMin==null)?"chưa check đủ":"check đủ",
                (q.CheckMin-q.ShiftMin>= q.ExpandTime || q.ShiftMax-q.CheckMax>= q.ExpandTime )? "Không Đúng Giờ":"Ra Vào Đúng Giờ",
             q.Id,
            }).ToList();
            TimeSpan totalWorkTimeOfShift = new TimeSpan(datatable.Sum(q => q.TotalWorkTime.GetValueOrDefault().Ticks));
            var timenull = TimeSpan.Zero;


            return Json(new
            {
                result = result,

            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetApprovedStatus(int storeId, string strChooseDay)
        {
            var user = HttpContext.User;
            var name = HttpContext.User.Identity.Name;
            var chooseDay = strChooseDay.ToDateTime();
            int flag = 0;
            var CanConfig = WebConfigurationManager.AppSettings["CanConfig"];
            bool result = false;
            var attApi = new AttendanceApi();
            var attList = attApi.GetActive().Where(q => q.Status == (int)StatusAttendanceEnum.Processing && q.StoreId == storeId && q.ShiftMin.Date == chooseDay).ToList();
            var attDraftList = attApi.GetActive().Where(q => q.Status == (int)StatusAttendanceEnum.Draft && q.StoreId == storeId && q.ShiftMin.Date == chooseDay).ToList();
            if (user.IsInRole("BrandManager") || (user.IsInRole("StoreManager") && (CanConfig == "true")))
            {
                if (attDraftList.Any())
                {
                    flag = 1;
                }
                else
                {
                    try
                    {
                        foreach (var item in attList)
                        {
                            item.Status = (int)StatusAttendanceEnum.Approved;
                            item.UpdatePerson = name;
                            attApi.Edit(item.Id, item);
                        }

                        result = true;
                    }
                    catch (Exception ex)
                    {
                        result = false;
                    }
                }
            }
            else
            {
                flag = 2;
            }
            return Json(new { result = result, flag = flag }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetAbsentStatus(int attendanceId)
        {
            var user = HttpContext.User;
            var name = HttpContext.User.Identity.Name;
            bool flag = true;
            var CanConfig = WebConfigurationManager.AppSettings["CanConfig"];
            bool result = false;
            var attApi = new AttendanceApi();
            var attendance = attApi.Get(attendanceId);
            if (user.IsInRole("BrandManager") || (user.IsInRole("StoreManager") && (CanConfig == "true")))
            {
                try
                {
                    attendance.Status = (int)StatusAttendanceEnum.Absent;
                    attendance.UpdatePerson = name;
                    attApi.Edit(attendance.Id, attendance);
                    result = true;
                }
                catch (Exception ex)
                {
                    result = false;
                }
            }
            else
            {
                flag = false;
            }
            return Json(new { result = result, flag = flag }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetWeeklyProcessingStatus(int storeId, string strStartDay, string strEndDay, int Id)
        {
            var user = HttpContext.User;
            var name = HttpContext.User.Identity.Name;
            var startDay = strStartDay.ToDateTime().GetStartOfDate();
            var endDay = strEndDay.ToDateTime().GetEndOfDate();
            bool flag = true;
            var CanConfig = WebConfigurationManager.AppSettings["CanConfig"];
            bool result = false;
            var attApi = new AttendanceApi();
            var attList = attApi.GetActive().Where(q => q.Status == (int)StatusAttendanceEnum.Draft && q.StoreId == storeId && q.ShiftMin >= startDay && q.ShiftMax <= endDay).ToList();
            if (user.IsInRole("BrandManager") || (user.IsInRole("StoreManager") && (CanConfig == "true")))
            {
                if (attList.Any())
                {
                    if (Id == 0)
                    {
                        try
                        {
                            foreach (var item in attList)
                            {
                                item.Status = (int)StatusAttendanceEnum.Processing;
                                item.UpdatePerson = name;
                                attApi.Edit(item.Id, item);
                            }

                            result = true;
                        }
                        catch (Exception ex)
                        {
                            result = false;
                        }
                    }
                    else
                    {
                        var att = attApi.Get(Id);
                        try
                        {

                            att.Status = (int)StatusAttendanceEnum.Processing;
                            att.UpdatePerson = name;
                            attApi.Edit(Id, att);

                            result = true;
                        }
                        catch (Exception ex)
                        {
                            result = false;
                        }
                    }
                }
            }
            else
            {
                flag = false;
            }
            return Json(new { result = result, flag = flag }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GetAllCheckFinger(int storeId, string strChooseDay, int empId)
        {
            var chooseDay = strChooseDay.ToDateTime();
            var startDay = chooseDay.GetStartOfDate();
            var endDay = chooseDay.GetEndOfDate();
            var checkFingerApi = new CheckFingerApi();
            var fingerMachineApi = new FingerScanMachineApi();
            var employeeApi = new EmployeeApi();
            var storeApi = new StoreApi();
            var attendanceApi = new AttendanceApi();
            var listS = new List<int>();
            var listEmpInStore = employeeApi.GetActive().Where(q => q.MainStoreId == storeId).ToList();
            foreach (var item in listEmpInStore)
            {
                listS.Add(item.Id);
            }
            var count = 1;
            if (empId == -1)
            {
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => (a.StoreId == storeId || listS.Any(d => d == a.EmployeeId)) && a.DateTime >= startDay && a.DateTime <= endDay)
               .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,

                        (a.DateTime.TimeOfDay >= attendanceApi.GetActive()
                        .Where(s => s.EmployeeId == a.EmployeeId && s.ShiftMin.ToString("dd/MM/yyyy").Contains(a.DateTime.ToString("dd/MM/yyyy")))
                        .Select(q => q.ShiftMin.Add(-q.CheckInExpandTime.Value).TimeOfDay).FirstOrDefault() &&
                        a.DateTime.Hour <= attendanceApi.GetActive()
                        .Where(s => s.EmployeeId == a.EmployeeId && s.ShiftMin.ToString("dd/MM/yyyy").Contains(a.DateTime.ToString("dd/MM/yyyy")))
                        .Select(q => q.ShiftMax.Hour + q.CheckOutExpandTime.Value.Hours).FirstOrDefault()) ?
                        "Đã ghi nhận" :
                        attendanceApi.GetActive().Where(s => s.EmployeeId == a.EmployeeId && s.ShiftMin.ToString("dd/MM/yyyy").Contains(a.DateTime.ToString("dd/MM/yyyy"))).Any() ?
                        "Chưa ghi nhận" :
                        "Không có phân công",

                        storeApi.Get(a.StoreId).ShortName,

                        //normal : viên cửa chuẩn , sub : nhân viên chuyển tới của hàng khác , plus : nhân viên từ cửa hàng khác chuyển tới
                        storeId == a.StoreId ? "Normal" : employeeApi.Get(a.EmployeeId).MainStoreId == storeId ? "Sub" : "Plus",
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).Name,
                        storeApi.Get(a.StoreId).Name
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
               }).ToList();
                return Json(checkFinger);
            }
            else
            {
                var checkFinger = checkFingerApi.GetActive()
                .OrderByDescending(q => q.DateTime).Where(a => a.DateTime >= startDay && a.DateTime <= endDay && a.EmployeeId == empId)
                 .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //normal : viên cửa chuẩn , sub : nhân viên chuyển tới của hàng khác , plus : nhân viên từ cửa hàng khác chuyển tới
                        storeId == a.StoreId ? "Normal" : employeeApi.Get(a.EmployeeId).MainStoreId == storeId ? "Sub" : "Plus",
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).Name,
                        storeApi.Get(a.StoreId).Name
                     //fingerMachineApi.Get(a.FingerScanMachineId).Name,
                 }).ToList();
                return Json(checkFinger);
            }
        }
        public JsonResult GetSmallReport(int storeId, string strChooseDay)
        {
            var startDate = strChooseDay.ToDateTime().GetStartOfDate();
            var endDate = strChooseDay.ToDateTime().GetEndOfDate();
            var chooseDay = strChooseDay.ToDateTime();

            var attendanceApi = new AttendanceApi();
            var employeeApi = new EmployeeApi();
            var finalTotalWorkTime = new TimeSpan();
            long totalTime = 0;
            var datatable = attendanceApi.GetActive().Where(q => q.ShiftMin.Date == chooseDay.Date && q.StoreId == storeId).ToList();
            var countEmp = datatable.Where(q => q.CheckMin != null).GroupBy(q => q.EmployeeId);
       
            //TotalEmployeeOntime
            var EmpOnTime = datatable.Where(q => q.ShiftMin != null).Where(q => q.CheckMin <= q.ShiftMin).Count();
            //ToralEmployeeLateTime
            var EmpLateTime = datatable.Where(q => q.ShiftMin != null).Where(q => q.CheckMin > q.ShiftMin).Count();
            //TotalEmpAccpepted
            var EmpAccepted = datatable.Where(q => q.Status == 1).Count();
            //TotalEmpNotAccepted 
            var EmpNotAccepted = datatable.Where(q => q.Status == 0).Count();
            // TotalWorkTime
            var getAllEmpWorkTime = datatable.Where(q => startDate <= q.ShiftMin && endDate >= q.ShiftMax).GroupBy(p => p.EmployeeId);
            
            foreach (var time in getAllEmpWorkTime)
            {
                foreach (var tmpTime in time)
                {
                    if (tmpTime.TotalWorkTime != null)
                    {
                        totalTime += tmpTime.TotalWorkTime.Value.Ticks;
                    }
                }
                finalTotalWorkTime = new TimeSpan(totalTime);
            }
            string stringtime = "";
            var tmp = Int32.Parse(finalTotalWorkTime.TotalHours.ToString("N").Split('.')[0]);
            stringtime += finalTotalWorkTime.TotalHours.ToString("N").Split('.')[0];
            stringtime += ":";
            if (finalTotalWorkTime.Minutes < 10)
            {
                stringtime += "0" + finalTotalWorkTime.Minutes;
            }
            else
            {
                stringtime += finalTotalWorkTime.Minutes;
            }
            var tmpMinute = int.Parse(stringtime.Split(':')[1]);
            if (tmpMinute > 60)
            {
                stringtime = (int.Parse(stringtime.Split(':')[0]) + (int)(tmpMinute / 60)).ToString("N");
                stringtime += ":" + (tmpMinute % 60).ToString("N");
            }
            var report = new
            {
                EmpOnTime = EmpOnTime,
                EmpLateTime = EmpLateTime,
                EmpAccepted = EmpAccepted,
                EmpNotAccepted = EmpNotAccepted,
                stringtime = stringtime
            };
            return Json(new
            {
                report = report
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadCalendarAttendance(int storeId, string strChooseDay)
        {
            var listAttendance = new List<CalendarAttendaceDay>();
            var listCheckfinger = new List<CalendarFingerCheck>();
            var checkFingerApi = new CheckFingerApi();
            var employeeApi = new EmployeeApi();
            var chooseDay = strChooseDay.ToDateTime();
            var startDay = chooseDay.GetStartOfDate();
            var endDay = chooseDay.GetEndOfDate();
            var apiAttendance = new AttendanceApi();
            //var checkFingerTable = checkFingerApi.GetCheckFingerByStoreByTimeRange(storeId, startDay, endDay).ToList();
            var list = apiAttendance.GetAttendanceByTimeRangeAndStore2(storeId, startDay, endDay)
                .GroupBy(q => q.EmployeeId);
            var datetimeNow = Utils.GetCurrentDateTime();

            //Get open and close time---------------
            var storeApi = new StoreApi();
            var store = storeApi.GetStoreById(storeId);
            var storeModel = new storeHour();
            if (store.OpenTime != null && store.CloseTime != null)
            {
                var StartTime = store.OpenTime ?? DateTime.Now;
                var EndTime = store.CloseTime ?? DateTime.Now;
                var now = StartTime.GetStartOfDate();
                storeModel.start = (Math.Round(StartTime.Subtract(now).TotalHours, 1) - 1) <= 0 ? 0 : (Math.Round(StartTime.Subtract(now).TotalHours, 1) - 1);
                storeModel.end = Math.Round(EndTime.Subtract(now).TotalHours, 1) + 1 >= 23 ? 23 : Math.Round(EndTime.Subtract(now).TotalHours, 1) + 1;
                // Trường hợp cửa hàng cập nhật open time vs close time sai
                if (storeModel.start + 1 == storeModel.end - 1)
                {
                    storeModel.start = 0;
                    storeModel.end = 23;
                }
                int i = 0;
                while (i <= (storeModel.start - 1) && i <= 24)
                {
                    i = i + 1;
                }
                storeModel.start = i;
            }
            //----------
            foreach (var item in list)
            {
                var detailAttendance = new CalendarAttendaceDay();
                detailAttendance.IdEmp = item.Key;
                detailAttendance.NameEmp = employeeApi.GetEmployee(item.Key).Name;
                var attendanceList = new List<AttendanceDayReport>();

                TimeSpan duration;
                DateTime timeNow;
                double starHour;
                foreach (var attendance in item)
                {
                    var report = new AttendanceDayReport();
                    if (attendance.ShiftMin.Date != attendance.ShiftMax.Date)
                    {
                        if (chooseDay.Date < attendance.ShiftMax.Date)
                        {
                            duration = attendance.ShiftMin.GetEndOfDate() - attendance.ShiftMin;
                            timeNow = attendance.ShiftMin.GetStartOfDate();
                            starHour = Math.Round(attendance.ShiftMin.Subtract(timeNow).TotalHours, 1);
                        }
                        else
                        {
                            duration = attendance.ShiftMax - attendance.ShiftMax.GetStartOfDate();
                            timeNow = attendance.ShiftMax.GetStartOfDate();
                            starHour = timeNow.Hour;
                        }

                    }
                    else
                    {
                        duration = attendance.ShiftMax - attendance.ShiftMin;
                        timeNow = attendance.ShiftMin.GetStartOfDate();
                        starHour = Math.Round(attendance.ShiftMin.Subtract(timeNow).TotalHours, 1);
                    }
                    double durationtofloat = Double.Parse(duration.TotalHours.ToString("F1"));
                    report.duration = durationtofloat;
                    //var timeNow = item.ShiftMin.GetStartOfDate();
                    //double starHour = Math.Round(item.ShiftMin.Subtract(timeNow).TotalHours, 1);
                    report.startHour = starHour - storeModel.start;
                    report.checkYet = false;
                    if (attendance.CheckMin != null || attendance.CheckMax != null)
                    {
                        var checkMin = attendance.CheckMin ?? attendance.ShiftMin;
                        var checkMax = attendance.CheckMax ?? attendance.ShiftMax;
                        report.checkMin = checkMin.ToString("HH:mm");
                        report.checkMax = checkMax.ToString("HH:mm");
                        report.checkYet = true;
                    }
                    if (attendance.CheckMin == null && attendance.CheckMax == null && attendance.ShiftMin < datetimeNow)
                    {
                        //absent mau đỏ
                        report.color = "#c71c22";
                    }
                    else if (attendance.CheckMin == null && attendance.CheckMax == null && attendance.ShiftMin >= datetimeNow)
                    {
                        report.color = "#2fa4e7";
                        //tuong lai mua xanh duong
                    }
                    else if ((attendance.CheckMin > (attendance.ShiftMin.Add(attendance.ExpandTime))) &&
                             (attendance.CheckMax < (attendance.ShiftMax.Add(-attendance.ExpandTime)) || (attendance.CheckMax >= (attendance.ShiftMax.Add(-attendance.ExpandTime))) || (attendance.CheckMax >= (attendance.ShiftMax.Add(-attendance.ExpandTime)))))
                    {
                        //ca sai ve mau vang
                        report.color = "#dd5600";
                    }
                    else
                    {
                        //chuan mau xanh la cay
                        report.color = "#73a839";
                    }
                    report.Id = attendance.Id;
                    attendanceList.Add(report);
                }
                detailAttendance.AttendanceList = attendanceList;
                listAttendance.Add(detailAttendance);
            }
            //// lấy checkFinger data--------------------
            //foreach (var item in checkFingerTable)
            //{
            //    var checkDetail = new CalendarFingerCheck();
            //    if (item.EmployeeId != null)
            //    {
            //        checkDetail.IdEmp = item.EmployeeId;
            //        checkDetail.NameEmp = employeeApi.GetEmployee(item.EmployeeId ?? -1).Name;
            //        var timecheckList = new List<CheckTime>();
            //        var timeCheck = new CheckTime();
            //        timeCheck.checkHour = item.DateTime.TimeOfDay;
            //        var timeNow = item.DateTime.GetStartOfDate();
            //        double starHour = Math.Round(item.DateTime.Subtract(timeNow).TotalHours, 1);
            //        timeCheck.startHour = starHour;
            //        timecheckList.Add(timeCheck);
            //        checkDetail.CheckfingerList = timecheckList;
            //        listCheckfinger.Add(checkDetail);
            //    }
            //}
            //---------------------


            return Json(new { rs = listAttendance, listCheckFinger = listCheckfinger, storeModel = storeModel }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowAttendanceSet(int attendanceId)
        {
            var attendanceApi = new AttendanceApi();
            var attendance = attendanceApi.GetAttendanceById(attendanceId);
            var model = new AttendanceEditViewModel();
            model.AttendanceId = attendance.Id;
            model.stt = attendance.Status;
            model.note = attendance.Note;
            model.updatePerson = attendance.UpdatePerson;
            model.totalWorkTime = attendance.TotalWorkTime == null ? "N/a" : attendance.TotalWorkTime.Value.ToString();
            model.processStt = attendance.ProcessingStatus;
            model.checkMin = attendance.CheckMin.GetValueOrDefault().ToString("dd/MM/yyyy HH:mm");
            model.checkMax = attendance.CheckMax.GetValueOrDefault().ToString("dd/MM/yyyy HH:mm");
            model.shiftMin = attendance.ShiftMin.ToString("dd/MM/yyyy HH:mm");
            model.shiftMax = attendance.ShiftMax.ToString("dd/MM/yyyy HH:mm");
            var empApi = new EmployeeApi();
            var empName = empApi.Get(attendanceApi.Get(attendanceId).EmployeeId).Name;
            ViewBag.empName = empName;
            return PartialView(model);
        }
        [HttpPost]
        public ActionResult EditAttendance(int attId, string shiftMin, string shiftMax, string breakTime, string checkMin, string checkMax, int processingStatus, int stt, int storeId, string note)
        {
            var user = HttpContext.User.Identity.Name;
            bool result = false;
            string mess = "";
            var api = new AttendanceApi();
            var attModel = api.BaseService.Get(attId);
            try
            {
                try
                {
                    attModel.ShiftMin = Utils.ToDateTimeHour(shiftMin);
                }
                catch (Exception)
                {
                }
                try
                {
                    attModel.ShiftMax = Utils.ToDateTimeHour(shiftMax);
                }
                catch (Exception)
                {
                }

                try
                {
                    attModel.CheckMin = Utils.ToDateTimeHour(checkMin);
                }
                catch (Exception)
                {
                    if (String.IsNullOrEmpty(checkMin))
                    {
                        attModel.CheckMin = null;
                    }
                }
                try
                {
                    attModel.CheckMax = Utils.ToDateTimeHour(checkMax);
                }
                catch (Exception)
                {
                    if (String.IsNullOrEmpty(checkMax))
                    {
                        attModel.CheckMax = null;
                    }
                }
                try
                {
                    TimeSpan timefree = TimeSpan.Parse(breakTime);
                    attModel.BreakTime = timefree;
                }
                catch (Exception)
                {
                }
                if (attModel.CheckMin != null && attModel.CheckMax != null && attModel.CheckMax > attModel.CheckMin)
                {
                    attModel.TotalWorkTime = (attModel.CheckMax.Value - attModel.CheckMin.Value).Subtract(attModel.BreakTime.Value);
                }
                attModel.ProcessingStatus = processingStatus;
                attModel.Status = stt;
                attModel.UpdatePerson = user;
                attModel.Note = note;
                api.BaseService.Update(attModel);
                result = true;
                mess = "Cập nhật thành công";
            }
            catch (Exception e)
            {
                result = false;
                mess = "Chư cập nhật được. Vui lòng liên hệ admin";
            }
            return Json(new { rs = result, mess = mess }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditAttendanceShift(int attId, string shiftMin, string shiftMax, string breakTime, string checkMin, string checkMax, int processingStatus, int stt, int storeId, string note, String checkinexpandtime, String checkoutexpandtime, int inmode, int outmode)
        {
            var user = HttpContext.User;
            var name = HttpContext.User.Identity.Name;
            bool flag = true;
            var CanConfig = WebConfigurationManager.AppSettings["CanConfig"];
            bool result = false;
            string mess = "";
            if (user.IsInRole("BrandManager") || (user.IsInRole("StoreManager") && (CanConfig == "true")))
            {
                var api = new AttendanceApi();
                var attModel = api.BaseService.Get(attId);
                var shiftMinDate = Utils.ToDateTimeHour(attModel.ShiftMin.ToString("dd/MM/yyyy ") + shiftMin);
                var shiftMaxDate = Utils.ToDateTimeHour(attModel.ShiftMax.ToString("dd/MM/yyyy ") + shiftMax);
                if (shiftMinDate > shiftMaxDate)
                {
                    shiftMaxDate = shiftMaxDate.AddDays(1);
                }
                try
                {
                    try
                    {
                        attModel.ShiftMin = shiftMinDate;
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        attModel.ShiftMax = shiftMaxDate;
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        attModel.CheckMin = Utils.ToDateTimeHour(checkMin);
                    }
                    catch (Exception)
                    {
                        if (String.IsNullOrEmpty(checkMin))
                        {
                            attModel.CheckMin = null;
                        }
                    }
                    try
                    {
                        attModel.CheckMax = Utils.ToDateTimeHour(checkMax);
                    }
                    catch (Exception)
                    {
                        if (String.IsNullOrEmpty(checkMax))
                        {
                            attModel.CheckMax = null;
                        }
                    }
                    try
                    {
                        TimeSpan timefree = TimeSpan.Parse(breakTime);
                        attModel.BreakTime = timefree;
                        TimeSpan checkInExpandTime = TimeSpan.Parse(checkinexpandtime);
                        attModel.CheckInExpandTime = checkInExpandTime;
                        TimeSpan checkOutExpandTime = TimeSpan.Parse(checkoutexpandtime);
                        attModel.CheckOutExpandTime = checkOutExpandTime;
                        attModel.InMode = inmode;
                        attModel.OutMode = outmode;
                    }
                    catch (Exception)
                    {
                    }
                    if (attModel.CheckMin != null && attModel.CheckMax != null && attModel.CheckMax > attModel.CheckMin)
                    {
                        attModel.TotalWorkTime = (attModel.CheckMax.Value - attModel.CheckMin.Value).Subtract(attModel.BreakTime.Value);
                    }
                    attModel.ProcessingStatus = processingStatus;
                    attModel.Status = stt;
                    attModel.UpdatePerson = name;
                    attModel.Note = note;
                    api.BaseService.Update(attModel);
                    result = true;
                    mess = "Cập nhật thành công";
                }
                catch (Exception e)
                {
                    result = false;
                    mess = "Chư cập nhật được. Vui lòng liên hệ admin";
                }
            }
            else
            {
                result = false;
                flag = false;
                mess = "Chưa được cấp quyền . Vui lòng liên hệ admin";
            }

            return Json(new { rs = result, mess = mess, flag = flag }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetEmp(int brandId, int storeId)
        {
            var empApi = new EmployeeApi();
            var empStoreApi = new EmployeeInStoreApi();
            if (storeId == 0)
            {
                var list = empApi.GetActive().Where(q => q.BrandId == brandId).ToList();
                return Json(list);
            }
            else
            {
                var list = empApi.GetActive().Where(q => q.BrandId == brandId && q.MainStoreId == storeId).ToList();
                return Json(list);
            }
        }


        public ActionResult ExportToExcelReportCheckFinger(int storeId, string strChooseDay, int empId)
        {
            var chooseDay = strChooseDay.ToDateTime();
            var startDay = chooseDay.GetStartOfDate();
            var endDay = chooseDay.GetEndOfDate();
            var checkFingerApi = new CheckFingerApi();
            var fingerMachineApi = new FingerScanMachineApi();
            var employeeApi = new EmployeeApi();
            var count = 1;
            var storeApi = new StoreApi();
            var nameStore = storeApi.Get(storeId).ShortName;
            string title = "";
            var sdate = startDay.ToString("dd-MM-yyyy");
            title = "Thông tin điểm danh ngày " + sdate + " - Cửa hàng " + nameStore;
            var listS = new List<int>();
            var listEmpInStore = employeeApi.GetActive().Where(q => q.MainStoreId == storeId).ToList();
            foreach (var item in listEmpInStore)
            {
                listS.Add(item.Id);
            }
            if (empId == -1)
            {
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => (a.StoreId == storeId || listS.Any(d => d == a.EmployeeId)) && a.DateTime >= startDay && a.DateTime <= endDay)
               .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
               }).ToList();
                return ExportReportToExcelFinger(checkFinger, title);
            }
            else
            {
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => a.DateTime >= startDay && a.DateTime <= endDay && a.EmployeeId == empId)
                 .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
                 }).ToList();
                return ExportReportToExcelFinger(checkFinger, title);
            }

        }

        public ActionResult ExportReportToExcelFinger(List<IConvertible[]> data, string title)
        {
            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo");
                #region header
                ws.Cells["A1:A2"].Merge = true;
                ws.Cells["A1:A2"].Value = "STT";
                ws.Cells["A1:A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A1:A2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["A1:A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["A1:A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["B1:B2"].Merge = true;
                ws.Cells["B1:B2"].Value = "Tên nhân viên";
                ws.Cells["B1:B2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["B1:B2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["B1:B2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["C1:C2"].Merge = true;
                ws.Cells["C1:C2"].Value = "Mã điểm danh";
                ws.Cells["C1:C2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["C1:C2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["C1:C2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["C1:C2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["D1:D2"].Merge = true;
                ws.Cells["D1:D2"].Value = "Ngày giờ check";
                ws.Cells["D1:D2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["D1:D2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["D1:D2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["D1:D2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["E1:E2"].Merge = true;
                ws.Cells["E1:E2"].Value = "Nv Cừa hàng";
                ws.Cells["E1:E2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["E1:E2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["E1:E2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["E1:E2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["F1:F2"].Merge = true;
                ws.Cells["F1:F2"].Value = "Máy scan Cửa hàng";
                ws.Cells["F1:F2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["F1:F2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["F1:F2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["F1:F2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                #endregion
                //Set style for excel

                #region set value for cells

                int indexRow = 3;
                foreach (var item in data)
                {
                    int indexCol = 1;
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[0];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[1];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[2];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[3];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[4];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[5];



                    indexRow = indexRow + 1;
                }
                #endregion
                #region style
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ws.Cells[ws.Dimension.Address].Style.WrapText = true;
                string fileName = title + ".xlsx";
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }


        [Authorize(Roles = "BrandManager, ShiftEditor")]
        public ActionResult PrepareEdit(int attId)
        {
            var api = new AttendanceApi();
            var model = api.Get(attId);
            var rsModel = new
            {
                id = model.Id,
                stt = model.Status,
                timeStart = model.ShiftMin.ToString("HH:mm"),
                timeEnd = model.ShiftMax.ToString("HH:mm"),
                timebreak = DateTime.Today.Add(model.BreakTime.GetValueOrDefault()).ToString("HH:mm"),
                checkMin = model.CheckMin == null ? "" : model.CheckMin.GetValueOrDefault().ToString("dd/MM/yyyy HH:mm"),
                checkMax = model.CheckMax == null ? "" : model.CheckMax.GetValueOrDefault().ToString("dd/MM/yyyy HH:mm"),
                note = model.Note,
                processStt = model.ProcessingStatus,
                updatePerson = model.UpdatePerson,
                //expandTime = DateTime.Today.Add(model.ExpandTime).ToString("HH:mm"),
                inMode = model.InMode,
                outMode = model.OutMode,
                //breakCount = model.BreakCount,
                checkinExpandTime = model.CheckInExpandTime.Value.ToString(@"hh\:mm"),
                checkoutExpandTime = model.CheckOutExpandTime.Value.ToString(@"hh\:mm"),
                totalWorkTime = (model.TotalWorkTime != null) ? DateTime.Today.Add(model.TotalWorkTime.GetValueOrDefault()).ToString("HH:mm") : "",
            };
            var employeeApi = new EmployeeApi();
            var emp = employeeApi.Get(model.EmployeeId).Name;
            var headerTitle = emp + " - Ngày " + model.ShiftMin.ToString("dd/MM/yyyy");

            var timeFrameApi = new TimeFrameApi();
            return Json(new { rs = rsModel, workDayEdit = headerTitle }, JsonRequestBehavior.AllowGet);
        }
        // Quan ly ngay lam viec tren brand 
        public JsonResult GetAttendanceInBrand(int status, int empType, int isLate, string strChooseDay, int listStoreId, int brandId, int? typeShift)
        {
            var chooseDay = strChooseDay.ToDateTime();
            var startDate = chooseDay.GetStartOfDate();
            var endDate = chooseDay.GetEndOfDate();
            int count = 1;
            var attendanceApi = new AttendanceApi();
            var employeeApi = new EmployeeApi();
            var storeApi = new StoreApi();
            if (listStoreId == 0)
            {
                var listS = new List<int>();
                var listStore = storeApi.GetActive().Where(q => q.BrandId == brandId).ToList();
                foreach (var item in listStore)
                {
                    listS.Add(item.ID);
                }
                var listAttendance = attendanceApi.GetAttendanceByTimeRange(startDate, endDate, listS).ToList();
                var datatable = listAttendance;
                if (status == 0)
                {
                    datatable = datatable.Where(q => q.Status == 0).ToList();
                }
                else if (status == 1)
                {
                    datatable = datatable.Where(q => q.Status == 1).ToList();
                }
                else if (status == 2)
                {
                    datatable = datatable.Where(q => q.Status == 2).ToList();
                }
                if (empType == 0)
                {
                    datatable = datatable.Where(q => q.ProcessingStatus == 0 ||
                                                     q.ProcessingStatus == 1 ||
                                                     q.ProcessingStatus == 2 ||
                                                     q.ProcessingStatus == 3 ||
                                                     q.ProcessingStatus == 5).ToList();
                }
                else if (empType == 1)
                {
                    datatable = datatable.Where(q => q.ProcessingStatus == 6 ||
                                                     q.ProcessingStatus == 7).ToList();
                }
                if (isLate == 0)
                {
                    datatable = datatable.Where(q => q.CheckMin - q.ShiftMin >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
                }
                else if (isLate == 1)
                {
                    datatable = datatable.Where(q => q.ShiftMax - q.CheckMax >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
                }
                else if (isLate == 2)
                {
                    datatable = datatable.Where(q => q.ShiftMax - q.CheckMax < q.ExpandTime && q.CheckMin - q.ShiftMin < q.ExpandTime).ToList();
                }
                if (typeShift != null)
                {
                    if (typeShift == 0)
                    {
                        datatable = datatable.Where(q => q.IsOverTime != true).ToList();
                    }
                    if (typeShift == 1)
                    {
                        datatable = datatable.Where(q => q.IsOverTime == true).ToList();
                    }
                }

                var result = datatable.OrderByDescending(q => q.TotalWorkTime).Select(q => new IConvertible[]
                {
                count++,
                employeeApi.Get(q.EmployeeId).Name,
                q.CheckMin == null ? "Chưa cập nhật" : q.CheckMin.Value.ToString("HH:mm"),
                q.CheckMax == null ? "Chưa cập nhật" : q.CheckMax.Value.ToString("HH:mm"),
                q.TotalWorkTime == null ? "N/A" :  DateTime.Today.Add(q.TotalWorkTime.Value).ToString("HH:mm"),
                q.Id,
                (q.CheckMax==null||q.CheckMin==null)? false:true,
                q.CheckMin == null ? false : ((q.ShiftMin-q.CheckMin <= (-q.ExpandTime))? false:true),
                q.CheckMax == null ? false : ((q.ShiftMax-q.CheckMax >= q.ExpandTime)? false:true),
                q.Status == null ? "N/A" : ((StatusAttendanceEnum)q.Status).DisplayName(),
                q.UpdatePerson == null ? "N/A" : q.UpdatePerson,
                q.ProcessingStatus == null ? "N/A" : ((ProcessingStatusAttendenceEnum)q.ProcessingStatus).DisplayName(),
                q.ShiftMin.ToString("HH:mm"),
                q.ShiftMax.ToString("HH:mm"),
                q.Note == null ? "" : q.Note,
                q.IsOverTime
                }).ToList();
                return Json(new
                {
                    result = result,
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var datatable = attendanceApi.GetActive().Where(q => q.ShiftMin.Date == chooseDay.Date && q.StoreId == listStoreId).ToList();
                if (status == 0)
                {
                    datatable = datatable.Where(q => q.Status == 0).ToList();
                }
                else if (status == 1)
                {
                    datatable = datatable.Where(q => q.Status == 1).ToList();
                }
                else if (status == 2)
                {
                    datatable = datatable.Where(q => q.Status == 2).ToList();
                }
                if (empType == 0)
                {
                    datatable = datatable.Where(q => q.ProcessingStatus == 0 ||
                                                     q.ProcessingStatus == 1 ||
                                                     q.ProcessingStatus == 2 ||
                                                     q.ProcessingStatus == 3 ||
                                                     q.ProcessingStatus == 5).ToList();
                }
                else if (empType == 1)
                {
                    datatable = datatable.Where(q => q.ProcessingStatus == 6 ||
                                                     q.ProcessingStatus == 7).ToList();
                }
                if (isLate == 0)
                {
                    datatable = datatable.Where(q => q.CheckMin - q.ShiftMin >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
                }
                else if (isLate == 1)
                {
                    datatable = datatable.Where(q => q.ShiftMax - q.CheckMax >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
                }
                else if (isLate == 2)
                {
                    datatable = datatable.Where(q => q.ShiftMax - q.CheckMax < q.ExpandTime && q.CheckMin - q.ShiftMin < q.ExpandTime).ToList();
                }
                var result = datatable.OrderByDescending(q => q.TotalWorkTime).Select(q => new IConvertible[]
                {
                count++,
                employeeApi.Get(q.EmployeeId).Name,
                q.CheckMin == null ? "Chưa cập nhật" : q.CheckMin.Value.ToString("HH:mm"),
                q.CheckMax == null ? "Chưa cập nhật" : q.CheckMax.Value.ToString("HH:mm"),
                q.TotalWorkTime == null ? "N/A" :  DateTime.Today.Add(q.TotalWorkTime.Value).ToString("HH:mm"),
                q.Id,
                (q.CheckMax==null||q.CheckMin==null)? false:true,
                q.CheckMin == null ? false : ((q.ShiftMin-q.CheckMin <= (-q.ExpandTime))? false:true),
                q.CheckMax == null ? false : ((q.ShiftMax-q.CheckMax >= q.ExpandTime)? false:true),
                q.Status == null ? "N/A" : ((StatusAttendanceEnum)q.Status).DisplayName(),
                q.UpdatePerson == null ? "N/A" : q.UpdatePerson,
                q.ProcessingStatus == null ? "N/A" : ((ProcessingStatusAttendenceEnum)q.ProcessingStatus).DisplayName(),
                q.ShiftMin.ToString("HH:mm"),
                q.ShiftMax.ToString("HH:mm"),
                q.Note == null ? "" : q.Note,
                }).ToList();
                return Json(new
                {
                    result = result,
                }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult GetAllCheckFingerInBrand(int brandId, string strChooseDay, int empId, int listStoreId)
        {
            var chooseDay = strChooseDay.ToDateTime();
            var startDay = chooseDay.GetStartOfDate();
            var endDay = chooseDay.GetEndOfDate();
            var checkFingerApi = new CheckFingerApi();
            var fingerMachineApi = new FingerScanMachineApi();
            var employeeApi = new EmployeeApi();
            var storeApi = new StoreApi();

            var count = 1;
            if (empId == -1 && listStoreId == 0)
            {
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => a.BrandId == brandId && a.DateTime >= startDay && a.DateTime <= endDay)
               .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //normal : viên cửa chuẩn , sub : nhân viên chuyển tới của hàng khác , plus : nhân viên từ cửa hàng khác chuyển tới
                        listStoreId == a.StoreId ? "Normal" : employeeApi.Get(a.EmployeeId).MainStoreId == listStoreId ? "Sub" : "Plus",
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).Name,
                        storeApi.Get(a.StoreId).Name
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
               }).ToList();
                return Json(checkFinger);
            }
            else if (empId != -1 && listStoreId == 0)
            {
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => a.BrandId == brandId && a.DateTime >= startDay && a.DateTime <= endDay && a.EmployeeId == empId)
                 .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //normal : viên cửa chuẩn , sub : nhân viên chuyển tới của hàng khác , plus : nhân viên từ cửa hàng khác chuyển tới
                        listStoreId == a.StoreId ? "Normal" : employeeApi.Get(a.EmployeeId).MainStoreId == listStoreId ? "Sub" : "Plus",
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).Name,
                        storeApi.Get(a.StoreId).Name
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
                 }).ToList();
                return Json(checkFinger);
            }
            else if (empId == -1 && listStoreId != 0)
            {
                var listS = new List<int>();
                var listEmpInStore = employeeApi.GetActive().Where(q => q.MainStoreId == listStoreId).ToList();
                foreach (var item in listEmpInStore)
                {
                    listS.Add(item.Id);
                }
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => (a.StoreId == listStoreId || listS.Any(d => d == a.EmployeeId)) && a.DateTime >= startDay && a.DateTime <= endDay)
                 .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //normal : viên cửa chuẩn , sub : nhân viên chuyển tới của hàng khác , plus : nhân viên từ cửa hàng khác chuyển tới
                        listStoreId == a.StoreId ? "Normal" : employeeApi.Get(a.EmployeeId).MainStoreId == listStoreId ? "Sub" : "Plus",
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).Name,
                        storeApi.Get(a.StoreId).Name
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
                 }).ToList();
                return Json(checkFinger);
            }
            else
            {
                var listS = new List<int>();
                var listEmpInStore = employeeApi.GetActive().Where(q => q.MainStoreId == listStoreId).ToList();
                foreach (var item in listEmpInStore)
                {
                    listS.Add(item.Id);
                }
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => (a.StoreId == listStoreId || listS.Any(d => d == a.EmployeeId)) && a.DateTime >= startDay && a.DateTime <= endDay && a.EmployeeId == empId)
                 .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //normal : viên cửa chuẩn , sub : nhân viên chuyển tới của hàng khác , plus : nhân viên từ cửa hàng khác chuyển tới
                        listStoreId == a.StoreId ? "Normal" : employeeApi.Get(a.EmployeeId).MainStoreId == listStoreId ? "Sub" : "Plus",
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).Name,
                        storeApi.Get(a.StoreId).Name
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
                 }).ToList();
                return Json(checkFinger);
            }
        }
        public ActionResult LoadCalendarAttendanceInBrand(int brandId, string strChooseDay, int listStoreId)
        {
            var listAttendance = new List<CalendarAttendaceDay>();
            var listCheckfinger = new List<CalendarFingerCheck>();
            var checkFingerApi = new CheckFingerApi();
            var employeeApi = new EmployeeApi();
            var chooseDay = strChooseDay.ToDateTime();
            var startDay = chooseDay.GetStartOfDate();
            var endDay = chooseDay.GetEndOfDate();
            var apiAttendance = new AttendanceApi();
            if (listStoreId == 0)
            {
                var storeApi = new StoreApi();
                var listS = new List<int>();
                var listStore = storeApi.GetActive().Where(q => q.BrandId == brandId).ToList();
                foreach (var item in listStore)
                {
                    listS.Add(item.ID);
                }
                var list = apiAttendance.GetAttendanceByTimeRangeAndBrand(listS, startDay, endDay)
                .GroupBy(q => q.EmployeeId);
                var datetimeNow = Utils.GetCurrentDateTime();

                //Get open and close time---------------

                var store = storeApi.GetStoreById(listStoreId);
                var storeModel = new storeHour();
                storeModel.start = 0;
                storeModel.end = 23;
                foreach (var item in list)
                {
                    var detailAttendance = new CalendarAttendaceDay();
                    detailAttendance.IdEmp = item.Key;
                    detailAttendance.NameEmp = employeeApi.GetEmployee(item.Key).Name;
                    var attendanceList = new List<AttendanceDayReport>();

                    TimeSpan duration;
                    DateTime timeNow;
                    double starHour;
                    foreach (var attendance in item)
                    {
                        var report = new AttendanceDayReport();
                        if (attendance.ShiftMin.Date != attendance.ShiftMax.Date)
                        {
                            if (chooseDay.Date < attendance.ShiftMax.Date)
                            {
                                duration = attendance.ShiftMin.GetStartOfDate().AddHours(storeModel.end + 1) - attendance.ShiftMin;
                                timeNow = attendance.ShiftMin.GetStartOfDate();
                                starHour = Math.Round(attendance.ShiftMin.Subtract(timeNow).TotalHours, 1);
                            }
                            else
                            {
                                duration = attendance.ShiftMax - attendance.ShiftMax.GetStartOfDate();
                                timeNow = attendance.ShiftMax.GetStartOfDate();
                                starHour = timeNow.Hour;
                            }

                        }
                        else
                        {
                            duration = attendance.ShiftMax - attendance.ShiftMin;
                            timeNow = attendance.ShiftMin.GetStartOfDate();
                            starHour = Math.Round(attendance.ShiftMin.Subtract(timeNow).TotalHours, 1);
                        }
                        double durationtofloat = Double.Parse(duration.TotalHours.ToString("F1"));
                        report.duration = durationtofloat;
                        report.startHour = starHour - storeModel.start;
                        report.checkYet = false;
                        if (attendance.CheckMin != null || attendance.CheckMax != null)
                        {
                            var checkMin = attendance.CheckMin ?? attendance.ShiftMin;
                            var checkMax = attendance.CheckMax ?? attendance.ShiftMax;
                            report.checkMin = checkMin.ToString("HH:mm");
                            report.checkMax = checkMax.ToString("HH:mm");
                            report.checkYet = true;
                        }
                        if (attendance.CheckMin == null && attendance.CheckMax == null && attendance.ShiftMin < datetimeNow)
                        {
                            //absent mau đỏ
                            report.color = "#c71c22";
                        }
                        else if (attendance.CheckMin == null && attendance.CheckMax == null && attendance.ShiftMin >= datetimeNow)
                        {
                            report.color = "#2fa4e7";
                            //tuong lai mua xanh duong
                        }
                        else if ((attendance.CheckMin > (attendance.ShiftMin.Add(attendance.ExpandTime))) &&
                                 (attendance.CheckMax < (attendance.ShiftMax.Add(-attendance.ExpandTime)) || (attendance.CheckMax >= (attendance.ShiftMax.Add(-attendance.ExpandTime))) || (attendance.CheckMax >= (attendance.ShiftMax.Add(-attendance.ExpandTime)))))
                        {
                            //ca sai ve mau vang
                            report.color = "#dd5600";
                        }
                        else
                        {
                            //chuan mau xanh la cay
                            report.color = "#73a839";
                        }
                        report.Id = attendance.Id;
                        attendanceList.Add(report);
                    }
                    detailAttendance.AttendanceList = attendanceList;
                    listAttendance.Add(detailAttendance);
                }
                return Json(new { rs = listAttendance, listCheckFinger = listCheckfinger, storeModel = storeModel }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = apiAttendance.GetAttendanceByTimeRangeAndStore2(listStoreId, startDay, endDay)
                .GroupBy(q => q.EmployeeId);
                var datetimeNow = Utils.GetCurrentDateTime();
                //Get open and close time---------------
                var storeApi = new StoreApi();
                var store = storeApi.GetStoreById(listStoreId);
                var storeModel = new storeHour();
                if (store.OpenTime != null && store.CloseTime != null)
                {
                    var StartTime = store.OpenTime ?? DateTime.Now;
                    var EndTime = store.CloseTime ?? DateTime.Now;
                    var now = StartTime.GetStartOfDate();
                    storeModel.start = (Math.Round(StartTime.Subtract(now).TotalHours, 1) - 1) <= 0 ? 0 : (Math.Round(StartTime.Subtract(now).TotalHours, 1) - 1);
                    storeModel.end = Math.Round(EndTime.Subtract(now).TotalHours, 1) + 1 >= 23 ? 23 : Math.Round(EndTime.Subtract(now).TotalHours, 1) + 1;
                    // Trường hợp cửa hàng cập nhật open time vs close time sai
                    if (storeModel.start + 1 == storeModel.end - 1)
                    {
                        storeModel.start = 0;
                        storeModel.end = 23;
                    }
                    int i = 0;
                    while (i <= (storeModel.start - 1) && i <= 24)
                    {
                        i = i + 1;
                    }
                    storeModel.start = i;
                }
                //----------
                foreach (var item in list)
                {
                    var detailAttendance = new CalendarAttendaceDay();
                    detailAttendance.IdEmp = item.Key;
                    detailAttendance.NameEmp = employeeApi.GetEmployee(item.Key).Name;
                    var attendanceList = new List<AttendanceDayReport>();

                    TimeSpan duration;
                    DateTime timeNow;
                    double starHour;
                    foreach (var attendance in item)
                    {
                        var report = new AttendanceDayReport();
                        if (attendance.ShiftMin.Date != attendance.ShiftMax.Date)
                        {
                            if (chooseDay.Date < attendance.ShiftMax.Date)
                            {
                                duration = attendance.ShiftMin.GetStartOfDate().AddHours(storeModel.end + 1) - attendance.ShiftMin;
                                timeNow = attendance.ShiftMin.GetStartOfDate();
                                starHour = Math.Round(attendance.ShiftMin.Subtract(timeNow).TotalHours, 1);
                            }
                            else
                            {
                                duration = attendance.ShiftMax - attendance.ShiftMax.GetStartOfDate();
                                timeNow = attendance.ShiftMax.GetStartOfDate();
                                starHour = timeNow.Hour;
                            }
                        }
                        else
                        {
                            duration = attendance.ShiftMax - attendance.ShiftMin;
                            timeNow = attendance.ShiftMin.GetStartOfDate();
                            starHour = Math.Round(attendance.ShiftMin.Subtract(timeNow).TotalHours, 1);
                        }
                        double durationtofloat = Double.Parse(duration.TotalHours.ToString("F1"));
                        report.duration = durationtofloat;
                        report.startHour = starHour - storeModel.start;
                        report.checkYet = false;
                        if (attendance.CheckMin != null || attendance.CheckMax != null)
                        {
                            var checkMin = attendance.CheckMin ?? attendance.ShiftMin;
                            var checkMax = attendance.CheckMax ?? attendance.ShiftMax;
                            report.checkMin = checkMin.ToString("HH:mm");
                            report.checkMax = checkMax.ToString("HH:mm");
                            report.checkYet = true;
                        }
                        if (attendance.CheckMin == null && attendance.CheckMax == null && attendance.ShiftMin < datetimeNow)
                        {
                            //absent mau đỏ
                            report.color = "#c71c22";
                        }
                        else if (attendance.CheckMin == null && attendance.CheckMax == null && attendance.ShiftMin >= datetimeNow)
                        {
                            report.color = "#2fa4e7";
                            //tuong lai mua xanh duong
                        }
                        else if ((attendance.CheckMin > (attendance.ShiftMin.Add(attendance.ExpandTime))) &&
                                 (attendance.CheckMax < (attendance.ShiftMax.Add(-attendance.ExpandTime)) || (attendance.CheckMax >= (attendance.ShiftMax.Add(-attendance.ExpandTime))) || (attendance.CheckMax >= (attendance.ShiftMax.Add(-attendance.ExpandTime)))))
                        {
                            //ca sai ve mau vang
                            report.color = "#dd5600";
                        }
                        else
                        {
                            //chuan mau xanh la cay
                            report.color = "#73a839";
                        }
                        report.Id = attendance.Id;
                        attendanceList.Add(report);
                    }
                    detailAttendance.AttendanceList = attendanceList;
                    listAttendance.Add(detailAttendance);
                }
                return Json(new { rs = listAttendance, listCheckFinger = listCheckfinger, storeModel = storeModel }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult ExportToExcelReportInBrand(string strChooseDay, int listStoreId, int brandId)
        {
            var chooseDay = strChooseDay.ToDateTime();
            string nameStore = "";
            #region getdata
            int count = 1;
            var attendanceApi = new AttendanceApi();
            var employeeApi = new EmployeeApi();
            var storeapi = new StoreApi();
            var listS = new List<int>();
            var listStore = storeapi.GetActive().Where(q => q.BrandId == brandId).ToList();
            foreach (var item in listStore)
            {
                listS.Add(item.ID);
            }
            var listAttendances = attendanceApi.GetActive().Where(q => q.ShiftMin.Date == chooseDay.Date);
            if (listStoreId == 0)
            {
                listAttendances = listAttendances.Where(q => listS.Any(d => d == q.StoreId));
                nameStore = "Toàn hệ thống";
            }
            else
            {
                listAttendances = listAttendances.Where(q => q.StoreId == listStoreId);
                nameStore = storeapi.Get(listStoreId).ShortName;
            }

            var result = listAttendances.Select(q => new IConvertible[]
            {
                count++,
                employeeApi.Get(q.EmployeeId).Name,
                storeapi.GetStoreNameByID( employeeApi.Get(q.EmployeeId).MainStoreId),
                q.CheckMin == null ? "Chưa cập nhật" : q.CheckMin.Value.ToString("dd/MM/yyyy HH:mm"),
                q.CheckMax == null ? "Chưa cập nhật" : q.CheckMax.Value.ToString("dd/MM/yyyy HH:mm"),
                q.TotalWorkTime.ToString(),
                q.Id,
                (q.CheckMax==null||q.CheckMin==null)?"chưa check đủ":"check đủ",
                (q.CheckMin-q.ShiftMin>= q.ExpandTime)? "Đi trễ":"Chuẩn",
                (q.ShiftMax-q.CheckMax>=q.ExpandTime)? "Về sớm":"Chuẩn"
            }).ToList();
            #endregion
            return ExportReportToExcelInBrand(result, chooseDay, nameStore);

        }
        public ActionResult ExportReportToExcelInBrand(List<IConvertible[]> data, DateTime dayexport, string nameStore)
        {
            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo");
                #region header
                ws.Cells["A1:A2"].Merge = true;
                ws.Cells["A1:A2"].Value = "STT";
                ws.Cells["A1:A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A1:A2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["A1:A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells["B1:B2"].Merge = true;
                ws.Cells["B1:B2"].Value = "Họ và Tên";
                ws.Cells["B1:B2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["B1:B2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["B1:B2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells["C1:C2"].Merge = true;
                ws.Cells["C1:C2"].Value = "Tên Cửa Hàng";
                ws.Cells["C1:C2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["C1:C2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["C1:C2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                ws.Cells["D1:D2"].Merge = true;
                ws.Cells["D1:D2"].Value = "Giờ vào";
                ws.Cells["D1:D2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["D1:D2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["D1:D2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells["E1:E2"].Merge = true;
                ws.Cells["E1:E2"].Value = "Giờ ra";
                ws.Cells["E1:E2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["E1:E2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["E1:E2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells["F1:F2"].Merge = true;
                ws.Cells["F1:F2"].Value = "Trạng Thái";
                ws.Cells["F1:F2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["F1:F2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["F1:F2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells["G1:G2"].Merge = true;
                ws.Cells["G1:G2"].Value = "Số Giờ Làm";
                ws.Cells["G1:G2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["G1:G2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["G1:G2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells["H1:H2"].Merge = true;
                ws.Cells["H1:H2"].Value = "Tình trạng làm việc";
                ws.Cells["H1:H2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["H1:H2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["H1:H2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                #endregion
                //Set style for excel

                #region set value for cells

                int indexRow = 3;
                foreach (var item in data)
                {
                    int indexCol = 1;
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[0];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[1];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[2];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[3];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[4];
                    //trang thai
                    var check = item[7];
                    var vao = item[8];
                    var ra = item[9];
                    var chuan = "Đúng giờ";
                    var tre = "Đi trễ";
                    var som = "Về sớm";
                    var strTrangThai = "";
                    if (check == "chưa check đủ")
                    {
                        strTrangThai = "--";
                    }
                    else
                    {
                        if (vao != "Chuẩn")
                            if (strTrangThai == "") strTrangThai = strTrangThai + tre;
                            else strTrangThai = strTrangThai + ", " + tre;
                        if (ra != "Chuẩn")
                            if (strTrangThai == "") strTrangThai = strTrangThai + som;
                            else strTrangThai = strTrangThai + ", " + som;
                    }
                    if (strTrangThai == "") strTrangThai = chuan;

                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = strTrangThai;
                    //
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[5];
                    var tinhtrang = "";
                    if (check == "chưa check đủ")
                    {
                        tinhtrang = "vắng";
                    }
                    else
                    {
                        tinhtrang = "có mặt";
                    }
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = tinhtrang;
                    indexRow = indexRow + 1;
                }
                #endregion
                #region style
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ws.Cells[ws.Dimension.Address].Style.WrapText = true;
                string fileName = "Báo cáo chấm công " + nameStore + " ngày " + dayexport.ToString("dd-MM-yyyy") + ".xlsx";
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }
        public ActionResult ExportToExcelReportCheckFingerInBrand(string strChooseDay, int empId, int listStoreId, int brandId)
        {
            var chooseDay = strChooseDay.ToDateTime();
            var startDay = chooseDay.GetStartOfDate();
            var endDay = chooseDay.GetEndOfDate();
            var checkFingerApi = new CheckFingerApi();
            var fingerMachineApi = new FingerScanMachineApi();
            var employeeApi = new EmployeeApi();
            var count = 1;
            var storeApi = new StoreApi();
            var nameStore = "";
            if (listStoreId == 0)
            {
                nameStore = "Toàn hệ thống";
            }
            else
            {
                nameStore = "Cửa hàng " + storeApi.Get(listStoreId).ShortName;
            }

            string title = "";
            var sdate = startDay.ToString("dd-MM-yyyy");
            title = "Thông tin điểm danh ngày " + sdate + " - " + nameStore;
            if (empId == -1 && listStoreId == 0)
            {
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => a.BrandId == brandId && a.DateTime >= startDay && a.DateTime <= endDay)
               .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
               }).ToList();
                return ExportReportToExcelFinger(checkFinger, title);
            }
            else if (empId != -1 && listStoreId == 0)
            {
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => a.BrandId == brandId && a.DateTime >= startDay && a.DateTime <= endDay && a.EmployeeId == empId)
               .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
               }).ToList();
                return ExportReportToExcelFinger(checkFinger, title);
            }
            else if (empId == -1 && listStoreId != 0)
            {
                var listS = new List<int>();
                var listEmpInStore = employeeApi.GetActive().Where(q => q.MainStoreId == listStoreId).ToList();
                foreach (var item in listEmpInStore)
                {
                    listS.Add(item.Id);
                }
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => (a.StoreId == listStoreId || listS.Any(d => d == a.EmployeeId)) && a.DateTime >= startDay && a.DateTime <= endDay)
               .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
               }).ToList();
                return ExportReportToExcelFinger(checkFinger, title);
            }
            else
            {
                var listS = new List<int>();
                var listEmpInStore = employeeApi.GetActive().Where(q => q.MainStoreId == listStoreId).ToList();
                foreach (var item in listEmpInStore)
                {
                    listS.Add(item.Id);
                }
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => (a.StoreId == listStoreId || listS.Any(d => d == a.EmployeeId)) && a.DateTime >= startDay && a.DateTime <= endDay && a.EmployeeId == empId)
                 .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
                 }).ToList();
                return ExportReportToExcelFingerInBrand(checkFinger, title);
            }

        }

        public ActionResult ExportReportToExcelFingerInBrand(List<IConvertible[]> data, string title)
        {
            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo");
                #region header
                ws.Cells["A1:A2"].Merge = true;
                ws.Cells["A1:A2"].Value = "STT";
                ws.Cells["A1:A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A1:A2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["A1:A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["A1:A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["B1:B2"].Merge = true;
                ws.Cells["B1:B2"].Value = "Tên nhân viên";
                ws.Cells["B1:B2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["B1:B2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["B1:B2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["C1:C2"].Merge = true;
                ws.Cells["C1:C2"].Value = "Mã điểm danh";
                ws.Cells["C1:C2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["C1:C2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["C1:C2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["C1:C2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["D1:D2"].Merge = true;
                ws.Cells["D1:D2"].Value = "Ngày giờ check";
                ws.Cells["D1:D2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["D1:D2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["D1:D2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["D1:D2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["E1:E2"].Merge = true;
                ws.Cells["E1:E2"].Value = "Nv Cừa hàng";
                ws.Cells["E1:E2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["E1:E2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["E1:E2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["E1:E2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["F1:F2"].Merge = true;
                ws.Cells["F1:F2"].Value = "Máy scan Cửa hàng";
                ws.Cells["F1:F2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["F1:F2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["F1:F2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["F1:F2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;




                #endregion
                //Set style for excel

                #region set value for cells

                int indexRow = 3;
                foreach (var item in data)
                {
                    int indexCol = 1;
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[0];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[1];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[2];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[3];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[4];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[5];



                    indexRow = indexRow + 1;
                }
                #endregion
                #region style
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ws.Cells[ws.Dimension.Address].Style.WrapText = true;
                string fileName = title + ".xlsx";
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }


        public ActionResult ExportToExcelReportCheckFingerInBrandForRange(string sDate, string eDate, int empId, int listStoreId, int brandId)
        {
            var sDateTime = sDate.ToDateTime();
            var eDateTime = eDate.ToDateTime();
            var startDay = sDateTime.GetStartOfDate();
            var endDay = eDateTime.GetEndOfDate();
            var checkFingerApi = new CheckFingerApi();
            var fingerMachineApi = new FingerScanMachineApi();
            var employeeApi = new EmployeeApi();
            var count = 1;
            var storeApi = new StoreApi();
            var nameStore = "";
            if (listStoreId == 0)
            {
                nameStore = "Toàn hệ thống";
            }
            else
            {
                nameStore = "Cửa hàng " + storeApi.Get(listStoreId).ShortName;
            }

            string title = "";
            var sdate = startDay.ToString("dd-MM-yyyy");
            title = "Thông tin điểm danh ngày " + sdate + " - " + nameStore;
            if (empId == -1 && listStoreId == 0)
            {
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => a.BrandId == brandId && a.DateTime >= startDay && a.DateTime <= endDay)
               .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime,
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
               }).ToList();
                return ExportReportToExcelFingerInBrandForRange(checkFinger, startDay, endDay, title);
            }
            else if (empId != -1 && listStoreId == 0)
            {
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => a.BrandId == brandId && a.DateTime >= startDay && a.DateTime <= endDay && a.EmployeeId == empId)
               .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
               }).ToList();
                return ExportReportToExcelFingerInBrandForRange(checkFinger, startDay, endDay, title);
            }
            else if (empId == -1 && listStoreId != 0)
            {
                var listS = new List<int>();
                var listEmpInStore = employeeApi.GetActive().Where(q => q.MainStoreId == listStoreId).ToList();
                foreach (var item in listEmpInStore)
                {
                    listS.Add(item.Id);
                }
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => (a.StoreId == listStoreId || listS.Any(d => d == a.EmployeeId)) && a.DateTime >= startDay && a.DateTime <= endDay)
               .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
               }).ToList();
                return ExportReportToExcelFingerInBrandForRange(checkFinger, startDay, endDay, title);
            }
            else
            {
                var listS = new List<int>();
                var listEmpInStore = employeeApi.GetActive().Where(q => q.MainStoreId == listStoreId).ToList();
                foreach (var item in listEmpInStore)
                {
                    listS.Add(item.Id);
                }
                var checkFinger = checkFingerApi.GetActive()
               .OrderByDescending(q => q.DateTime).Where(a => (a.StoreId == listStoreId || listS.Any(d => d == a.EmployeeId)) && a.DateTime >= startDay && a.DateTime <= endDay && a.EmployeeId == empId)
                 .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(employeeApi.Get(a.EmployeeId).MainStoreId).ShortName,
                        storeApi.Get(a.StoreId).ShortName,
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
                 }).ToList();
                return ExportReportToExcelFingerInBrandForRange(checkFinger, startDay, endDay, title);
            }

        }


        public ActionResult ExportReportToExcelFingerInBrandForRange(List<IConvertible[]> data, DateTime sDateTime, DateTime eDateTime, string title)
        {

            var currentTime = DateTime.Now;
            // Normalize Data For Range
            var listRCFR = NormalizeDataForRange(data, sDateTime, eDateTime);
            var countDates = (eDateTime.Date - sDateTime.Date).Days;

            string filepath = HttpContext.Server.MapPath(@"/Resource/BANG_CHAM_CONG_TOAN_CUA_HANG.xlsx");
            var attApi = new AttendanceApi();
            var filestream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);
            //return null;
            using (ExcelPackage package = new ExcelPackage(filestream))
            {
                try
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets[1];
                    ws.Cells["A2"].Value = "Thời Gian: " + currentTime.ToString("dd/MM/yyyy HH:mm:ss");
                    ws.Cells["D5"].Value = "Giờ vào";
                    ws.Cells["D5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["E5"].Value = "Giờ ra";
                    ws.Cells["E5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["D4:E4"].Merge = true;
                    ws.Cells["D4:E4"].Value = currentTime.ToString("dd-mm-yyyy");
                    ws.Cells["D4:E4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells["D4:E4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    var row = 5;

                    int count = 0;
                    //var endCol = data.FirstOrDefault().time.Count();
                    ws.InsertColumn(4, countDates * 2);
                    var firstrow = listRCFR.FirstOrDefault();
                    var countCol = 4;
                    ExcelRange rgOriginal = ws.Cells["A4"];
                    foreach (var item in firstrow.ListDateCheck)
                    {
                        ws.Cells[4, countCol].Value = item.Date.ToString("dd-MM-yyyy");
                        ws.Cells[4, countCol].StyleID = rgOriginal.StyleID;

                        ws.Cells[4, countCol, 4, countCol + 1].Merge = true;

                        ws.Cells[5, countCol].Value = "Giờ Vào";
                        ws.Cells[5, countCol].StyleID = rgOriginal.StyleID;
                        ws.Cells[5, countCol + 1].Value = "Giờ Ra";
                        ws.Cells[5, countCol + 1].StyleID = rgOriginal.StyleID;

                        ++countCol;
                        ++countCol;
                    }
                    foreach (var item in listRCFR)
                    {

                        row = row + 2;
                        count++;
                        //ws.Cells["A8:A9"].Merge = true;                    
                        ws.Cells[row - 1, 1].Value = count;
                        ws.Cells[row - 1, 1, row, 1].Merge = true;
                        ws.Cells[row - 1, 2].Value = item.EmpRollId;
                        ws.Cells[row - 1, 2, row, 2].Merge = true;
                        ws.Cells[row - 1, 3].Value = item.Name;
                        ws.Cells[row - 1, 3, row, 3].Merge = true;
                        var countInCol = 4;
                        foreach (var dateCheck in item.ListDateCheck)
                        {
                            ws.Cells[row - 1, countInCol].Value = dateCheck.TimeIn != null ? dateCheck.TimeIn.Value.ToString("HH:mm:ss") : "chưa xác định";
                            ws.Cells[row - 1, countInCol, row, countInCol].Merge = true;
                            ws.Cells[row - 1, countInCol + 1].Value = dateCheck.TimeOut != null ? dateCheck.TimeOut.Value.ToString("HH:mm:ss") : "chưa xác định";
                            ws.Cells[row - 1, countInCol + 1, row, countInCol + 1].Merge = true;

                            ++countInCol;
                            ++countInCol;
                        }

                        ws.Cells[row - 1, countInCol].Value = item.StoreName;
                        ws.Cells[row - 1, countInCol, row, countInCol].Merge = true;

                    }

                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.WrapText = true;

                    MemoryStream ms = new MemoryStream();

                    package.SaveAs(ms);
                    filestream.Close();
                    ms.Seek(0, SeekOrigin.Begin);
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    string fileName = "BaoCaoChamCong" + "-tu " + sDateTime + "den " + eDateTime + ".xlsx";
                    return this.File(ms, contentType, fileName);

                }
                catch (Exception)
                {
                    MemoryStream ms = new MemoryStream();
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    string fileName = "BaoCaoChamCong" + "-tu " + sDateTime + "den " + eDateTime + ".xlsx";
                    package.SaveAs(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    filestream.Close();
                    return this.File(ms, contentType, fileName);
                }
            }
        }

        public class ReportCheckerForRange
        {
            public string Name { get; set; }
            public string EmpRollId { get; set; }
            public List<DateCheck> ListDateCheck { get; set; }
            public string StoreName { get; set; }
        }

        public class DateCheck
        {
            public DateTime Date { get; set; }
            public DateTime? TimeIn { get; set; }
            public DateTime? TimeOut { get; set; }
        }

        private List<ReportCheckerForRange> NormalizeDataForRange(List<IConvertible[]> data, DateTime sDateTime, DateTime eDateTime)
        {
            List<ReportCheckerForRange> listRCFR = new List<ReportCheckerForRange>();
            int countDate = (eDateTime.Date - sDateTime.Date).Days;

            try
            {
                foreach (var item in data)
                {
                    var tempRCFR = listRCFR.FirstOrDefault(p => p.EmpRollId == item[2].ToString());
                    if (tempRCFR == null)
                    {
                        var rCFR = new ReportCheckerForRange()
                        {
                            Name = item[1].ToString(),
                            EmpRollId = item[2].ToString(),
                            ListDateCheck = new List<DateCheck>(),
                            StoreName = item[5].ToString()
                        };
                        var listDateCheck = new List<DateCheck>();
                        for (int i = 0; i <= countDate; ++i)
                        {
                            var curDate = sDateTime.AddDays(i);
                            var tempData = data.Where(p => ((DateTime)p[3]).Date == curDate.Date && p[2].ToString() == item[2].ToString()).ToList();
                            if (tempData.Count > 0)
                            {
                                var timeIn = tempData?.Min(p => (DateTime)p[3]);
                                var timeOut = tempData?.Max(p => (DateTime)p[3]) != timeIn ? tempData?.Max(p => (DateTime)p[3]) : null;

                                rCFR.ListDateCheck.Add(new DateCheck() { Date = curDate.Date, TimeIn = timeIn, TimeOut = timeOut });
                            }
                            else
                            {
                                rCFR.ListDateCheck.Add(new DateCheck() { Date = curDate.Date, TimeIn = null, TimeOut = null });
                            }
                        }
                        listRCFR.Add(rCFR);
                    }
                }

                return listRCFR;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult GetAllCheckFingerInBrandServerSide(JQueryDataTableParamModel param, int brandId, string strChooseDay, int empId, int? listStoreId)
        {
            var chooseDay = strChooseDay.ToDateTime();
            var startDay = chooseDay.GetStartOfDate();
            var endDay = chooseDay.GetEndOfDate();
            var checkFingerApi = new CheckFingerApi();
            var fingerMachineApi = new FingerScanMachineApi();
            var employeeApi = new EmployeeApi();
            var storeApi = new StoreApi();

            //var count = 1;
            if (empId == -1 && listStoreId == 0)
            {
                IEnumerable<CheckFinger> checkFinger = checkFingerApi.GetCheckFingerByBrand(brandId, startDay, endDay);
                IEnumerable<Employee> employeesBrands = employeeApi.GetEmployeesByBrand(brandId);
                IEnumerable<Store> allStore = storeApi.GetAllStoreByBrandId(brandId);
                var listCheckFingerInfo = from currentCheck in checkFinger
                                          join empBrand in employeesBrands
                                          on currentCheck.EmployeeId equals empBrand.Id
                                          join aStore in allStore
                                          on currentCheck.StoreId equals aStore.ID
                                          select new
                                          {
                                              EmployeeId = currentCheck.EmployeeId,
                                              EmpEnrollNumber = currentCheck.EmpEnrollNumber,
                                              DateTime = currentCheck.DateTime,
                                              EmpStoreId = empBrand.MainStoreId,
                                              StoreId = currentCheck.StoreId,
                                              EmpName = empBrand.Name,
                                              ShortNameStore = aStore.ShortName,
                                              NameStore = aStore.Name
                                          };
                var listFinger = listCheckFingerInfo;
                //var listcheckFinger = checkFingerApi.GetActive().Where(a => a.BrandId == brandId && a.DateTime >= startDay && a.DateTime <= endDay);
                var totalResult = listFinger.Count();
                if (!String.IsNullOrEmpty(param.sSearch))
                {
                    listFinger = listFinger.Where(q => q.EmpName.Contains(param.sSearch) || q.EmpEnrollNumber.Contains(param.sSearch));
                }
                var totalDisplay = listFinger.Count();
                var count = param.iDisplayStart + 1;
                var result = listFinger
               .OrderByDescending(q => q.DateTime)
               .Skip(param.iDisplayStart)
               .Take(param.iDisplayLength)
               .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : a.EmpName,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(a.EmpStoreId).ShortName,
                        a.ShortNameStore,
                        //normal : viên cửa chuẩn , sub : nhân viên chuyển tới của hàng khác , plus : nhân viên từ cửa hàng khác chuyển tới
                        listStoreId == a.StoreId ? "Normal" : a.EmpStoreId == listStoreId ? "Sub" : "Plus",
                        storeApi.Get(a.EmpStoreId).Name,
                        a.NameStore
                        //fingerMachineApi.Get(a.FingerScanMachineId).Name,
               }).ToList();
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalResult,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }
            else if (empId != -1 && listStoreId == 0)
            {
                //var listcheckFinger = checkFingerApi.GetActive().Where(a => a.BrandId == brandId && a.DateTime >= startDay && a.DateTime <= endDay && a.EmployeeId == empId);
                IEnumerable<CheckFinger> checkFinger = checkFingerApi.GetCheckFingerByBrand(brandId, startDay, endDay);
                IEnumerable<Employee> employeesBrands = employeeApi.GetEmployeesByBrand(brandId);
                IEnumerable<Store> allStore = storeApi.GetAllStoreByBrandId(brandId);
                var listCheckFingerInfo = from currentCheck in checkFinger
                                          join empBrand in employeesBrands
                                          on currentCheck.EmployeeId equals empBrand.Id
                                          join aStore in allStore
                                          on currentCheck.StoreId equals aStore.ID
                                          select new
                                          {
                                              EmployeeId = currentCheck.EmployeeId,
                                              EmpEnrollNumber = currentCheck.EmpEnrollNumber,
                                              DateTime = currentCheck.DateTime,
                                              EmpStoreId = empBrand.MainStoreId,
                                              StoreId = currentCheck.StoreId,
                                              EmpName = empBrand.Name,
                                              ShortNameStore = aStore.ShortName,
                                              NameStore = aStore.Name
                                          };
                var listFinger = listCheckFingerInfo.Where(a => a.DateTime >= startDay && a.DateTime <= endDay && a.EmployeeId == empId);
                var totalResult = listFinger.Count();
                if (!String.IsNullOrEmpty(param.sSearch))
                {
                    listFinger = listFinger.Where(q => q.EmpName.Contains(param.sSearch) || q.EmpEnrollNumber.Contains(param.sSearch));
                }
                var totalDisplay = listFinger.Count();
                var count = param.iDisplayStart + 1;
                var result = listFinger
               .OrderByDescending(q => q.DateTime)
               .Skip(param.iDisplayStart)
               .Take(param.iDisplayLength)
               .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : a.EmpName,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(a.EmpStoreId).ShortName,
                        a.ShortNameStore,
                        //normal : viên cửa chuẩn , sub : nhân viên chuyển tới của hàng khác , plus : nhân viên từ cửa hàng khác chuyển tới
                        listStoreId == a.StoreId ? "Normal" : a.EmpStoreId == listStoreId ? "Sub" : "Plus",
                        storeApi.Get(a.EmpStoreId).Name,
                        a.NameStore
               }).ToList();
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalResult,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }
            else if (empId == -1 && listStoreId != 0)
            {
                var listS = new List<int>();
                var listEmpInStore = employeeApi.GetActive().Where(q => q.MainStoreId == listStoreId).ToList();
                foreach (var item in listEmpInStore)
                {
                    listS.Add(item.Id);
                }
                //var listcheckFinger = checkFingerApi.GetActive().Where(a => (a.StoreId == listStoreId || listS.Any(d => d == a.EmployeeId)) && a.DateTime >= startDay && a.DateTime <= endDay);
                IEnumerable<CheckFinger> checkFinger = checkFingerApi.GetCheckFingerByBrand(brandId, startDay, endDay);
                IEnumerable<Employee> employeesBrands = employeeApi.GetEmployeesByBrand(brandId);
                IEnumerable<Store> allStore = storeApi.GetAllStoreByBrandId(brandId);
                var listCheckFingerInfo = from currentCheck in checkFinger
                                          join empBrand in employeesBrands
                                          on currentCheck.EmployeeId equals empBrand.Id
                                          join aStore in allStore
                                          on currentCheck.StoreId equals aStore.ID
                                          select new
                                          {
                                              EmployeeId = currentCheck.EmployeeId,
                                              EmpEnrollNumber = currentCheck.EmpEnrollNumber,
                                              DateTime = currentCheck.DateTime,
                                              EmpStoreId = empBrand.MainStoreId,
                                              StoreId = currentCheck.StoreId,
                                              EmpName = empBrand.Name,
                                              ShortNameStore = aStore.ShortName,
                                              NameStore = aStore.Name
                                          };
                var listFinger = listCheckFingerInfo.Where(a => (a.StoreId == listStoreId || listS.Any(d => d == a.EmployeeId)));
                var totalResult = listFinger.Count();
                if (!String.IsNullOrEmpty(param.sSearch))
                {
                    listFinger = listFinger.Where(q => q.EmpName.Contains(param.sSearch) || q.EmpEnrollNumber.Contains(param.sSearch));
                }
                var totalDisplay = listFinger.Count();
                var count = param.iDisplayStart + 1;
                var result = listFinger
               .OrderByDescending(q => q.DateTime)
               .Skip(param.iDisplayStart)
               .Take(param.iDisplayLength)
               .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : a.EmpName,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(a.EmpStoreId).ShortName,
                        a.ShortNameStore,
                        //normal : viên cửa chuẩn , sub : nhân viên chuyển tới của hàng khác , plus : nhân viên từ cửa hàng khác chuyển tới
                        listStoreId == a.StoreId ? "Normal" : a.EmpStoreId == listStoreId ? "Sub" : "Plus",
                        storeApi.Get(a.EmpStoreId).Name,
                        a.NameStore
               }).ToList();
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalResult,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var listS = new List<int>();
                var listEmpInStore = employeeApi.GetActive().Where(q => q.MainStoreId == listStoreId).ToList();
                foreach (var item in listEmpInStore)
                {
                    listS.Add(item.Id);
                }
                //var listcheckFinger = checkFingerApi.GetActive().Where(a => (a.StoreId == listStoreId || listS.Any(d => d == a.EmployeeId)) && a.DateTime >= startDay && a.DateTime <= endDay && a.EmployeeId == empId);
                IEnumerable<CheckFinger> checkFinger = checkFingerApi.GetCheckFingerByBrand(brandId, startDay, endDay);
                IEnumerable<Employee> employeesBrands = employeeApi.GetEmployeesByBrand(brandId);
                IEnumerable<Store> allStore = storeApi.GetAllStoreByBrandId(brandId);
                var listCheckFingerInfo = from currentCheck in checkFinger
                                          join empBrand in employeesBrands
                                          on currentCheck.EmployeeId equals empBrand.Id
                                          join aStore in allStore
                                          on currentCheck.StoreId equals aStore.ID
                                          select new
                                          {
                                              EmployeeId = currentCheck.EmployeeId,
                                              EmpEnrollNumber = currentCheck.EmpEnrollNumber,
                                              DateTime = currentCheck.DateTime,
                                              EmpStoreId = empBrand.MainStoreId,
                                              StoreId = currentCheck.StoreId,
                                              EmpName = empBrand.Name,
                                              ShortNameStore = aStore.ShortName,
                                              NameStore = aStore.Name
                                          };
                var listFinger = listCheckFingerInfo.Where(a => (a.StoreId == listStoreId || listS.Any(d => d == a.EmployeeId)) && a.EmployeeId == empId);
                var totalResult = listFinger.Count();
                if (!String.IsNullOrEmpty(param.sSearch))
                {
                    listFinger = listFinger.Where(q => q.EmpName.Contains(param.sSearch) || q.EmpEnrollNumber.Contains(param.sSearch));
                }
                var totalDisplay = listFinger.Count();
                var count = param.iDisplayStart + 1;
                var result = listFinger
               .OrderByDescending(q => q.DateTime)
               .Skip(param.iDisplayStart)
               .Take(param.iDisplayLength)
               .Select(a => new IConvertible[]{
                        count++,
                        a.EmployeeId == null ? "Chưa xác định" : a.EmpName,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        storeApi.Get(a.EmpStoreId).ShortName,
                        a.ShortNameStore,
                        //normal : viên cửa chuẩn , sub : nhân viên chuyển tới của hàng khác , plus : nhân viên từ cửa hàng khác chuyển tới
                        listStoreId == a.StoreId ? "Normal" : a.EmpStoreId == listStoreId ? "Sub" : "Plus",
                        storeApi.Get(a.EmpStoreId).Name,
                        a.NameStore
               }).ToList();
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalResult,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetAttendanceByTimeRange(string strChooseDay, string strShiftMin, string strShiftMax, int storeId, int brandId, int empId)
        {
            var chooseDay = strChooseDay.ToDateTime();
            var shiftMin = strShiftMin.ToHourTime();
            var shiftMax = strShiftMax.ToHourTime();
            var startDate = chooseDay.Add(shiftMin.TimeOfDay);
            var endDate = chooseDay.Add(shiftMax.TimeOfDay);
            var attendanceApi = new AttendanceApi();
            var employeeApi = new EmployeeApi();
            var storeApi = new StoreApi();
            bool result = false;
            var listAttendance = attendanceApi.GetAttendanceByTimeRange2(startDate, endDate, storeId, empId).ToList();
            result = listAttendance.Any() ? true : false;
            return Json(new
            {
                result = result,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
