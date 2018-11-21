using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan.Controllers
{
    
    public class Details
    {
        public int stt;
        public string date;//ngay
        public int numberOfAttendance;//so ca nhan vien
        public int numberOfNA;// ca chua xac dinh
        public int numberOfAbsent;//ca vang
        public int numberOfPresent;//ca co mat
        public int numberOfLate;//di tre
        public int numberOfLeaveEarly;// ve som
        public int numberOfComeTrueTime;// den dung gio
        public int numberOfLeaveTrueTime;//ve dung gio       
        public TimeSpan totalWorkTime;
        public string strTotalWorkTime;
    }


    public class Infringe
    {
        public int stt;
        public int id;
        public string name;//ten
        public int numberOfAttendance;//so ca nhan vien
        public int numberOfNA;// ca chua xac dinh
        public int numberOfAbsent;//ca vang
        public int numberOfPresent;//ca co mat
        public int numberOfLate;//di tre
        public int numberOfLeaveEarly;// ve som
        public int numberOfComeTrueTime;// den dung gio
        public int numberOfLeaveTrueTime;//ve dung gio
        public int numberOfTotalInfringe;//tong luot vi pham 
        public int numberworkdays;
        public string totalWorkTime;
    }
    public class DashBoardController : Controller
    {
        // GET: FingerScan/DashBoard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DashBoardInBrand()
        {
            return View();
        }

        public ActionResult ViewCallendar()
        {
            return View("CallenderView");
        }



        //sap xep 
        public class EmpInfringeComparer : IComparer<Infringe>
        {
            public int Compare(Infringe x, Infringe y)
            {
                return y.numberOfTotalInfringe.CompareTo(x.numberOfTotalInfringe);
            }
        }

        public ActionResult GetOverDataReportInBrand(int? listStoreId, string startTime, string endTime, int brandId)
        {
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var Date = endDate - startDate;
            var totalDate = Math.Round(Date.TotalDays * 10) / 10;
            var attendanceApi = new AttendanceApi();
            var storeApi = new StoreApi();

            var listAttendances = attendanceApi.GetActive().Join(storeApi.GetActive(), atten => atten.StoreId, store => store.ID, (atten, store) => new { atten, store })
                .Where(q => q.store.BrandId == brandId && q.atten.ShiftMin >= startDate && q.atten.ShiftMin <= endDate);
            if (listStoreId != 0)
            {
                listAttendances = listAttendances.Where(q => q.atten.StoreId == listStoreId);
            }
            try
            {
                var totalAttendance = listAttendances.Count();

                var totalEmployee = listAttendances.GroupBy(q => q.atten.EmployeeId).Count();

                var totalProcessing = listAttendances.Where(q => q.atten.Status == (int)StatusAttendanceEnum.Processing).Count();
                var totalReject = listAttendances.Where(q => q.atten.Status == (int)StatusAttendanceEnum.Reject).Count();
                var totalApproved = listAttendances.Where(q => q.atten.Status == (int)StatusAttendanceEnum.Approved).Count();
                

                var totalHourWork = listAttendances.Sum(p => p.atten.TotalWorkTime.GetValueOrDefault().TotalHours);
                totalHourWork = Math.Round(totalHourWork * 10) / 10;

                var totalLate = listAttendances.Where(q => q.atten.CheckMin > (q.atten.ShiftMin.Add(q.atten.ExpandTime))).Count();
                var totalReturnEarly = listAttendances.Where(q => q.atten.CheckMax < (q.atten.ShiftMax.Add(-q.atten.ExpandTime))).Count();

                var totalViolate = totalLate + totalReturnEarly;

                var result = new
                {
                    TotalAttendance = totalAttendance,
                    TotalEmployee = totalEmployee,
                    TotalViolate = totalViolate,
                    TotalLate = totalLate,
                    TotalReturnEarly = totalReturnEarly,
                    TotalHourWork = totalHourWork,
                    TotalDate = totalDate,
                    TotalProcessing = totalProcessing,
                    TotalReject = totalReject,
                    TotalApproved = totalApproved
                };

                return Json(new { susscess = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { susscess = false, message = "Có lỗi trong quá trình xử lý, vui lòng liên hệ Admin" }, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult GetOverDataReport(int storeId, string startTime, string endTime)
        {
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var Date = endDate - startDate;
            var totalDate = Math.Round(Date.TotalDays * 10) / 10;
            var attendanceApi = new AttendanceApi();

            IEnumerable<Attendance> listAttendances = attendanceApi.GetAttendanceByTimeRangeAndStore(storeId, startDate, endDate);

            try
            {
                var totalAttendance = listAttendances.Count();

                var totalEmployee = listAttendances.GroupBy(q => q.EmployeeId).Count();

                var totalHourWork = listAttendances.Sum(p => p.TotalWorkTime.GetValueOrDefault().TotalHours);
                totalHourWork = Math.Round(totalHourWork * 10) / 10;

                var totalLate = listAttendances.Where(q => q.CheckMin > (q.ShiftMin.Add(q.ExpandTime))).Count();
                var totalReturnEarly = listAttendances.Where(q => q.CheckMax < (q.ShiftMax.Add(-q.ExpandTime))).Count();

                var totalViolate = totalLate + totalReturnEarly;

                var result = new
                {
                    TotalAttendance = totalAttendance,
                    TotalEmployee = totalEmployee,
                    TotalViolate = totalViolate,
                    TotalLate = totalLate,
                    TotalReturnEarly = totalReturnEarly,
                    TotalHourWork = totalHourWork,
                    TotalDate = totalDate
                };

                return Json(new { susscess = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { susscess = false, message = "Có lỗi trong quá trình xử lý, vui lòng liên hệ Admin" }, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult GetDetailsDashBoardInBrand(JQueryDataTableParamModel param, int listStoreId, String startTime, String endTime, int brandId)
        {
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();

            TimeSpan difference = endDate - startDate;

            int totalRecord = (int)difference.TotalDays + 1;


            // Get First date
            if (param.iDisplayStart > 0)
            {
                endDate = endDate.AddDays(-param.iDisplayStart);
            }


            // Get end date
            var tmpTimestart = endDate.AddDays(-param.iDisplayLength).GetStartOfDate();
            if (tmpTimestart.CompareTo(startDate) > 0)
            {
                startDate = tmpTimestart;
            }
            difference = endDate - startDate;
            var totalResult = (int)difference.TotalDays + 1;

            var totalDisplay = totalResult;

            var attendanceApi = new AttendanceApi();
            var storeApi = new StoreApi();
            if (listStoreId == 0)
            {
                var listS = new List<int>();
                var listStore = storeApi.GetActive().Where(q => q.BrandId == brandId).ToList();
                foreach (var item in listStore)
                {
                    listS.Add(item.ID);
                }
                var listAttendance = attendanceApi.GetAttendanceByTimeRange(startDate, endDate, listS).OrderByDescending(q => q.ShiftMin).GroupBy(q => DbFunctions.TruncateTime(q.ShiftMin)).ToList();
                var datetimeNow = Utils.GetCurrentDateTime();
                var listResult = new List<Details>();
                var count = param.iDisplayStart + 1;

                for (DateTime i = endDate; i > startDate; i = i.AddDays(-1))
                {
                    var dateTmp = new Details();
                    dateTmp.stt = count++;
                    dateTmp.date = i.ToString("dd/MM/yyyy");
                    dateTmp.strTotalWorkTime = "00:00";
                    listResult.Add(dateTmp);
                }
                foreach (var item in listAttendance)
                {
                    var datestring = item.Key.Value.ToString("dd/MM/yyyy");
                    var dateTmp = listResult.FirstOrDefault(q => q.date.Equals(datestring));
                    var listdata = item;
                    dateTmp.numberOfAttendance = listdata.GroupBy(q => q.EmployeeId).Count();
                    dateTmp.numberOfNA = listdata.Where(q => q.CheckMax == null || q.CheckMin == null || q.TotalWorkTime == null || q.TotalWorkTime.Value.Ticks <= 0).Count();
                    dateTmp.numberOfPresent = listdata.Where(q => q.CheckMin != null || q.CheckMax != null).Count();
                    dateTmp.numberOfLate = listdata.Where(q => q.CheckMin > (q.ShiftMin.Add(q.ExpandTime))).Count();
                    dateTmp.numberOfLeaveEarly = listdata.Where(q => q.CheckMax < (q.ShiftMax.Add(-q.ExpandTime))).Count();
                    dateTmp.numberOfAbsent = listdata.Where(q => q.CheckMax == null && q.CheckMin == null && q.ShiftMax < datetimeNow).Count();
                    // dateTmp.numberOfComeTrueTime = listdata.Where(q => q.CheckMin < (q.ShiftMin.Add(q.ExpandTime)) && q.CheckMin >= q.ShiftMin).Count();
                    //  dateTmp.numberOfLeaveTrueTime = listdata.Where(q => q.CheckMax > (q.ShiftMax.Add(-q.ExpandTime))).Count();
                    dateTmp.totalWorkTime = new TimeSpan(listdata.Where(q => q.TotalWorkTime != null).Sum(q => q.TotalWorkTime.Value.Ticks));
                    var stringtime = "";
                    if (dateTmp.totalWorkTime.Hours < 10)
                    {
                        stringtime += "0" + dateTmp.totalWorkTime.Hours;
                    }
                    else
                    {
                        stringtime += dateTmp.totalWorkTime.Hours;
                    }

                    stringtime += ":";
                    if (dateTmp.totalWorkTime.Minutes < 10)
                    {
                        stringtime += "0" + dateTmp.totalWorkTime.Minutes;
                    }
                    else
                    {
                        stringtime += dateTmp.totalWorkTime.Minutes;
                    }

                    dateTmp.strTotalWorkTime = stringtime;

                }

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecord,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = listResult
                }, JsonRequestBehavior.AllowGet);



            }
            else
            {


                var listAttendance = attendanceApi.GetAttendanceByTimeRangeAndStore(listStoreId, startDate, endDate).OrderByDescending(q => q.ShiftMin).GroupBy(q => DbFunctions.TruncateTime(q.ShiftMin)).ToList();

                var datetimeNow = Utils.GetCurrentDateTime();
                var listResult = new List<Details>();
                var count = param.iDisplayStart + 1;

                for (DateTime i = endDate; i > startDate; i = i.AddDays(-1))
                {
                    var dateTmp = new Details();
                    dateTmp.stt = count++;
                    dateTmp.date = i.ToString("dd/MM/yyyy");
                    dateTmp.strTotalWorkTime = "00:00";
                    listResult.Add(dateTmp);
                }
                foreach (var item in listAttendance)
                {
                    var datestring = item.Key.Value.ToString("dd/MM/yyyy");
                    var dateTmp = listResult.FirstOrDefault(q => q.date.Equals(datestring));
                    var listdata = item;
                    dateTmp.numberOfAttendance = listdata.GroupBy(q => q.EmployeeId).Count();
                    dateTmp.numberOfNA = listdata.Where(q => q.CheckMax == null || q.CheckMin == null || q.TotalWorkTime == null || q.TotalWorkTime.Value.Ticks <= 0).Count();
                    dateTmp.numberOfPresent = listdata.Where(q => q.CheckMin != null || q.CheckMax != null).Count();
                    dateTmp.numberOfLate = listdata.Where(q => q.CheckMin > (q.ShiftMin.Add(q.ExpandTime))).Count();
                    dateTmp.numberOfLeaveEarly = listdata.Where(q => q.CheckMax < (q.ShiftMax.Add(-q.ExpandTime))).Count();
                    dateTmp.numberOfAbsent = listdata.Where(q => q.CheckMax == null && q.CheckMin == null && q.ShiftMax < datetimeNow).Count();
                    // dateTmp.numberOfComeTrueTime = listdata.Where(q => q.CheckMin < (q.ShiftMin.Add(q.ExpandTime)) && q.CheckMin >= q.ShiftMin).Count();
                    //  dateTmp.numberOfLeaveTrueTime = listdata.Where(q => q.CheckMax > (q.ShiftMax.Add(-q.ExpandTime))).Count();
                    dateTmp.totalWorkTime = new TimeSpan(listdata.Where(q => q.TotalWorkTime != null).Sum(q => q.TotalWorkTime.Value.Ticks));
                    var stringtime = "";
                    if (dateTmp.totalWorkTime.Hours < 10)
                    {
                        stringtime += "0" + dateTmp.totalWorkTime.Hours;
                    }
                    else
                    {
                        stringtime += dateTmp.totalWorkTime.Hours;
                    }

                    stringtime += ":";
                    if (dateTmp.totalWorkTime.Minutes < 10)
                    {
                        stringtime += "0" + dateTmp.totalWorkTime.Minutes;
                    }
                    else
                    {
                        stringtime += dateTmp.totalWorkTime.Minutes;
                    }

                    dateTmp.strTotalWorkTime = stringtime;

                }

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecord,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = listResult
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetDetailsDashBoard(JQueryDataTableParamModel param, int storeId, String startTime, String endTime)
        {
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();

            TimeSpan difference = endDate - startDate;

            int totalRecord = (int)difference.TotalDays + 1;


            // Get First date
            if (param.iDisplayStart > 0)
            {
                endDate = endDate.AddDays(-param.iDisplayStart);
            }


            // Get end date
            var tmpTimestart = endDate.AddDays(-param.iDisplayLength).GetStartOfDate();
            if (tmpTimestart.CompareTo(startDate) > 0)
            {
                startDate = tmpTimestart;
            }
            difference = endDate - startDate;
            var totalResult = (int)difference.TotalDays + 1;

            var totalDisplay = totalResult;

            var attendanceApi = new AttendanceApi();
            var listAttendance = attendanceApi.GetAttendanceByTimeRangeAndStore(storeId, startDate, endDate).OrderByDescending(q => q.ShiftMin).GroupBy(q => DbFunctions.TruncateTime(q.ShiftMin)).ToList();

            var datetimeNow = Utils.GetCurrentDateTime();
            var listResult = new List<Details>();
            var count = param.iDisplayStart + 1;

            for (DateTime i = endDate; i > startDate; i = i.AddDays(-1))
            {
                var dateTmp = new Details();
                dateTmp.stt = count++;
                dateTmp.date = i.ToString("dd/MM/yyyy");
                dateTmp.strTotalWorkTime = "00:00";
                listResult.Add(dateTmp);
            }
            foreach (var item in listAttendance)
            {
                var datestring = item.Key.Value.ToString("dd/MM/yyyy");
                var dateTmp = listResult.FirstOrDefault(q => q.date.Equals(datestring));
                var listdata = item;
                dateTmp.numberOfAttendance = listdata.GroupBy(q => q.EmployeeId).Count();
                dateTmp.numberOfNA = listdata.Where(q => q.CheckMax == null || q.CheckMin == null || q.TotalWorkTime == null || q.TotalWorkTime.Value.Ticks <= 0).Count();
                dateTmp.numberOfPresent = listdata.Where(q => q.CheckMin != null || q.CheckMax != null).Count();
                dateTmp.numberOfLate = listdata.Where(q => q.CheckMin > (q.ShiftMin.Add(q.ExpandTime))).Count();
                dateTmp.numberOfLeaveEarly = listdata.Where(q => q.CheckMax < (q.ShiftMax.Add(-q.ExpandTime))).Count();
                dateTmp.numberOfAbsent = listdata.Where(q => q.CheckMax == null && q.CheckMin == null && q.ShiftMax < datetimeNow).Count();
                // dateTmp.numberOfComeTrueTime = listdata.Where(q => q.CheckMin < (q.ShiftMin.Add(q.ExpandTime)) && q.CheckMin >= q.ShiftMin).Count();
                //  dateTmp.numberOfLeaveTrueTime = listdata.Where(q => q.CheckMax > (q.ShiftMax.Add(-q.ExpandTime))).Count();
                dateTmp.totalWorkTime = new TimeSpan(listdata.Where(q => q.TotalWorkTime != null).Sum(q => q.TotalWorkTime.Value.Ticks));
                var stringtime = "";
                if (dateTmp.totalWorkTime.Hours < 10)
                {
                    stringtime += "0" + dateTmp.totalWorkTime.Hours;
                }
                else
                {
                    stringtime += dateTmp.totalWorkTime.Hours;
                }

                stringtime += ":";
                if (dateTmp.totalWorkTime.Minutes < 10)
                {
                    stringtime += "0" + dateTmp.totalWorkTime.Minutes;
                }
                else
                {
                    stringtime += dateTmp.totalWorkTime.Minutes;
                }

                dateTmp.strTotalWorkTime = stringtime;

            }

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecord,
                iTotalDisplayRecords = totalDisplay,
                aaData = listResult
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetDetailsInfringeInBrand(JQueryDataTableParamModel param, int listStoreId, String startTime, String endTime, int brandId)
        {
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();

            var attendanceApi = new AttendanceApi();
            var employeeApi = new EmployeeApi();
            var storeApi = new StoreApi();
            if (listStoreId == 0)
            {
                var listemployees = employeeApi.GetEmployeeByBrandId(brandId);
                var listAttendance = attendanceApi.GetActive().Join(storeApi.GetActive(), atten => atten.StoreId, store => store.ID, (atten, store) => new { atten, store })
                .Where(q => q.store.BrandId == brandId && q.atten.ShiftMin >= startDate && q.atten.ShiftMin <= endDate);

                var listjoin = from tableEmp in listemployees
                               join tableatt in listAttendance
                               on tableEmp.Id equals tableatt.atten.EmployeeId into ps
                               from tableatt in ps.DefaultIfEmpty()
                               select new
                               {
                                   Id = tableEmp.Id,
                                   Name = tableEmp.Name,
                                   Attedance = tableatt
                               };

                var totaldata = listjoin.Count();

                if (!String.IsNullOrEmpty(param.sSearch))
                {
                    listjoin = listjoin.Where(q => q.Name.Contains(param.sSearch));
                }
                var listFinal = listjoin.GroupBy(q => q.Id);

                var totalResult = listFinal.Count();

                listFinal = listFinal.OrderByDescending(q => q.Key).Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

                var datetimeNow = Utils.GetCurrentDateTime();
                var listResult = new List<Infringe>();
                var count = param.iDisplayStart + 1;
                foreach (var item in listFinal)
                {
                    var detail = new Infringe();
                    var attendanceEmp = item.FirstOrDefault();
                    detail.id = attendanceEmp.Id;
                    detail.name = attendanceEmp.Name;
                    detail.totalWorkTime = "00:00";
                    detail.stt = count++;
                    if (attendanceEmp.Attedance != null)
                    {
                        detail.numberOfAbsent = item.Where(q => q.Attedance.atten.CheckMax == null && q.Attedance.atten.CheckMin == null && q.Attedance.atten.ShiftMax < datetimeNow).Count();
                        detail.numberOfAttendance = item.Count();
                        detail.numberworkdays = item.GroupBy(q => q.Attedance.atten.ShiftMin.Date).Count();
                        detail.numberOfLate = item.Where(q => q.Attedance.atten.CheckMin > (q.Attedance.atten.ShiftMin.Add(q.Attedance.atten.ExpandTime))).Count();
                        detail.numberOfLeaveEarly = item.Where(q => q.Attedance.atten.CheckMax < (q.Attedance.atten.ShiftMax.Add(-q.Attedance.atten.ExpandTime))).Count();
                        detail.numberOfNA = item.Where(q => q.Attedance.atten.CheckMax == null || q.Attedance.atten.CheckMin == null || q.Attedance.atten.TotalWorkTime == null || q.Attedance.atten.TotalWorkTime.Value.Ticks <= 0).Count();
                        detail.numberOfPresent = item.Where(q => q.Attedance.atten.CheckMin != null || q.Attedance.atten.CheckMax != null).Count();
                        detail.numberOfTotalInfringe = detail.numberOfLate + detail.numberOfLeaveEarly;
                        var totalworks = new TimeSpan(item.Where(q => q.Attedance.atten.TotalWorkTime != null).Sum(q => q.Attedance.atten.TotalWorkTime.Value.Ticks));
                        var stringtime = "";
                        var tmp = Int32.Parse(totalworks.TotalHours.ToString("N").Split('.')[0]);

                        if (tmp < 10)
                        {
                            stringtime += "0" + totalworks.TotalHours.ToString("N").Split('.')[0];
                        }
                        else
                        {
                            stringtime += totalworks.TotalHours.ToString("N").Split('.')[0];
                        }

                        stringtime += ":";
                        if (totalworks.Minutes < 10)
                        {
                            stringtime += "0" + totalworks.Minutes;
                        }
                        else
                        {
                            stringtime += totalworks.Minutes;
                        }

                        detail.totalWorkTime = stringtime;
                    }

                    listResult.Add(detail);
                }

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalResult,
                    iTotalDisplayRecords = totalResult,
                    aaData = listResult
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var listemployees = employeeApi.GetEmployeeByStoreId(listStoreId);
                IEnumerable<Attendance> listAttendance = attendanceApi.GetAttendanceByStoreByTimeRange(listStoreId, startDate, endDate);

                var listjoin = from tableEmp in listemployees
                               join tableatt in listAttendance
                               on tableEmp.Id equals tableatt.EmployeeId into ps
                               from tableatt in ps.DefaultIfEmpty()
                               select new
                               {
                                   Id = tableEmp.Id,
                                   Name = tableEmp.Name,
                                   Attedance = tableatt
                               };

                var totaldata = listjoin.Count();

                if (!String.IsNullOrEmpty(param.sSearch))
                {
                    listjoin = listjoin.Where(q => q.Name.Contains(param.sSearch));
                }
                var listFinal = listjoin.GroupBy(q => q.Id);

                var totalResult = listFinal.Count();

                listFinal = listFinal.OrderByDescending(q => q.Key).Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

                var datetimeNow = Utils.GetCurrentDateTime();
                var listResult = new List<Infringe>();
                var count = param.iDisplayStart + 1;
                foreach (var item in listFinal)
                {
                    var detail = new Infringe();
                    var attendanceEmp = item.FirstOrDefault();
                    detail.id = attendanceEmp.Id;
                    detail.name = attendanceEmp.Name;
                    detail.totalWorkTime = "00:00";
                    detail.stt = count++;
                    if (attendanceEmp.Attedance != null)
                    {
                        detail.numberOfAbsent = item.Where(q => q.Attedance.CheckMax == null && q.Attedance.CheckMin == null && q.Attedance.ShiftMax < datetimeNow).Count();
                        detail.numberOfAttendance = item.Count();
                        detail.numberworkdays = item.GroupBy(q => q.Attedance.ShiftMin.Date).Count();
                        detail.numberOfLate = item.Where(q => q.Attedance.CheckMin > (q.Attedance.ShiftMin.Add(q.Attedance.ExpandTime))).Count();
                        detail.numberOfLeaveEarly = item.Where(q => q.Attedance.CheckMax < (q.Attedance.ShiftMax.Add(-q.Attedance.ExpandTime))).Count();
                        detail.numberOfNA = item.Where(q => q.Attedance.CheckMax == null || q.Attedance.CheckMin == null || q.Attedance.TotalWorkTime == null || q.Attedance.TotalWorkTime.Value.Ticks <= 0).Count();
                        detail.numberOfPresent = item.Where(q => q.Attedance.CheckMin != null || q.Attedance.CheckMax != null).Count();
                        detail.numberOfTotalInfringe = detail.numberOfLate + detail.numberOfLeaveEarly;
                        var totalworks = new TimeSpan(item.Where(q => q.Attedance.TotalWorkTime != null).Sum(q => q.Attedance.TotalWorkTime.Value.Ticks));
                        var stringtime = "";
                        var tmp = Int32.Parse(totalworks.TotalHours.ToString("N").Split('.')[0]);

                        if (tmp < 10)
                        {
                            stringtime += "0" + totalworks.TotalHours.ToString("N").Split('.')[0];
                        }
                        else
                        {
                            stringtime += totalworks.TotalHours.ToString("N").Split('.')[0];
                        }

                        stringtime += ":";
                        if (totalworks.Minutes < 10)
                        {
                            stringtime += "0" + totalworks.Minutes;
                        }
                        else
                        {
                            stringtime += totalworks.Minutes;
                        }

                        detail.totalWorkTime = stringtime;
                    }

                    listResult.Add(detail);
                }

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalResult,
                    iTotalDisplayRecords = totalResult,
                    aaData = listResult
                }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult GetDetailsInfringe(JQueryDataTableParamModel param, int storeId, String startTime, String endTime)
        {
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();

            var attendanceApi = new AttendanceApi();
            var employeeApi = new EmployeeApi();

            var listemployees = employeeApi.GetEmployeeByStoreId(storeId);
            IEnumerable<Attendance> listAttendance = attendanceApi.GetAttendanceByStoreByTimeRange(storeId, startDate, endDate);

            var listjoin = from tableEmp in listemployees
                           join tableatt in listAttendance
                           on tableEmp.Id equals tableatt.EmployeeId into ps
                           from tableatt in ps.DefaultIfEmpty()
                           select new
                           {
                               Id = tableEmp.Id,
                               Name = tableEmp.Name,
                               Attedance = tableatt
                           };

            var totaldata = listjoin.Count();

            if (!String.IsNullOrEmpty(param.sSearch))
            {
                listjoin = listjoin.Where(q => q.Name.ToUpper().Contains(param.sSearch.ToUpper()));
            }
            var listFinal = listjoin.GroupBy(q => q.Id);

            var totalResult = listFinal.Count();

            listFinal = listFinal.OrderByDescending(q => q.Key).Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

            var datetimeNow = Utils.GetCurrentDateTime();
            var listResult = new List<Infringe>();
            var count = param.iDisplayStart + 1;
            foreach (var item in listFinal)
            {
                var detail = new Infringe();
                var attendanceEmp = item.FirstOrDefault();
                detail.id = attendanceEmp.Id;
                detail.name = attendanceEmp.Name;
                detail.totalWorkTime = "00:00";
                detail.stt = count++;
                if (attendanceEmp.Attedance != null)
                {
                    detail.numberOfAbsent = item.Where(q => q.Attedance.CheckMax == null && q.Attedance.CheckMin == null && q.Attedance.ShiftMax < datetimeNow).Count();
                    detail.numberOfAttendance = item.Count();
                    detail.numberworkdays = item.GroupBy(q => q.Attedance.ShiftMin.Date).Count();
                    // detail.numberOfComeTrueTime = item.Where(q => q.Attedance.CheckMin < (q.Attedance.ShiftMin.Add(q.Attedance.ExpandTime)) && q.Attedance.CheckMin >= q.Attedance.ShiftMin).Count();
                    detail.numberOfLate = item.Where(q => q.Attedance.CheckMin > (q.Attedance.ShiftMin.Add(q.Attedance.ExpandTime))).Count();
                    detail.numberOfLeaveEarly = item.Where(q => q.Attedance.CheckMax < (q.Attedance.ShiftMax.Add(-q.Attedance.ExpandTime))).Count();
                    //  detail.numberOfLeaveTrueTime = item.Where(q => q.Attedance.CheckMax > (q.Attedance.ShiftMax.Add(-q.Attedance.ExpandTime))).Count();
                    detail.numberOfNA = item.Where(q => q.Attedance.CheckMax == null || q.Attedance.CheckMin == null || q.Attedance.TotalWorkTime == null || q.Attedance.TotalWorkTime.Value.Ticks <= 0).Count();
                    detail.numberOfPresent = item.Where(q => q.Attedance.CheckMin != null || q.Attedance.CheckMax != null).Count();
                    detail.numberOfTotalInfringe = detail.numberOfLate + detail.numberOfLeaveEarly;
                    var totalworks = new TimeSpan(item.Where(q => q.Attedance.TotalWorkTime != null).Sum(q => q.Attedance.TotalWorkTime.Value.Ticks));
                    var stringtime = "";
                    var tmp = Int32.Parse(totalworks.TotalHours.ToString("N").Split('.')[0]);

                    if (tmp < 10)
                    {
                        stringtime += "0" + totalworks.TotalHours.ToString("N").Split('.')[0];
                    }
                    else
                    {
                        stringtime += totalworks.TotalHours.ToString("N").Split('.')[0];
                    }

                    stringtime += ":";
                    if (totalworks.Minutes < 10)
                    {
                        stringtime += "0" + totalworks.Minutes;
                    }
                    else
                    {
                        stringtime += totalworks.Minutes;
                    }

                    detail.totalWorkTime = stringtime;
                }

                listResult.Add(detail);
            }

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalResult,
                iTotalDisplayRecords = totalResult,
                aaData = listResult
            }, JsonRequestBehavior.AllowGet);
        }
    }
}