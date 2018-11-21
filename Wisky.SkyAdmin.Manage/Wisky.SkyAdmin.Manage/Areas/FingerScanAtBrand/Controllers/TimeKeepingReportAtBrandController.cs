using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Wisky.SkyAdmin.Manage.Areas.FingerScan.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScanAtBrand.Controllers
{
    public class TimeKeepingReportAtBrandController : Controller
    {
        // GET: FingerScanAtBrand/TimeKeepingReportAtBrand
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult MachineConnectPage()
        {
            return View("MachineConnect");
        }
        //public ActionResult ReportMachineConnect(string startDate, string endDate, int brandId)
        //{
        //    var startTime = startDate.ToDateTime().GetStartOfDate();
        //    var endTime = endDate.ToDateTime().GetEndOfDate();

        //    var machineConnectApi = new MachineConnectApi();
        //    var machineconnectList = machineConnectApi.GetActive().Where(q => q.BrandID == brandId && q.ConnectTime >= startTime
        //    && q.ConnectTime <= endTime);
        //    var finalResult = new List<string[]>();
        //    int stt = 0;
        //    foreach (var item in machineconnectList)
        //    {
        //        stt++;
        //        var connectResult = "Không thành công";
        //        if (item.ConnectResult != null && item.ConnectResult == true)
        //        {
        //            connectResult = "Thành công";
        //        }
        //        string[] sarray = { stt.ToString(), item.StoreID.ToString(), item.MachineCode, item.MachineName, item.ConnectTime.ToString(), connectResult };
        //        finalResult.Add(sarray);
        //    }
        //    return Json(new
        //    {
        //        result = finalResult,
        //    }, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult GetTimekeeping(int storeSelected, string startDate, string endDate, int brandId)
        {

            var startTime = startDate.ToDateTime().GetStartOfDate();
            var endTime = endDate.ToDateTime().GetEndOfDate();

            var attendanceApi = new AttendanceApi();
            var employeeApi = new EmployeeApi();
            var storeApi = new StoreApi();

            var storeName = storeApi.Get(storeSelected).Name;
            var listEmployees = storeSelected != 0
                ? employeeApi.GetEmployeeByStoreId(storeSelected)
                : employeeApi.BaseService.GetActive(q => q.BrandId == brandId).ToList();
            var totalRecord = listEmployees.Count();

            var totalResult = listEmployees.Count();

            var listAttendances = storeSelected != 0
                ? attendanceApi.GetAttendanceByStoreByTimeRange(storeSelected, startTime, endTime)
                : attendanceApi.GetAttendanceByTimeRangeAndBrand(storeApi.GetActiveStoreByBrandId(brandId).Select(q => q.ID).ToList(), startTime, endTime);

            var listjoin = from tableEmp in listEmployees
                           join tableatt in listAttendances
                           on tableEmp.Id equals tableatt.EmployeeId into ps
                           from tableatt in ps.DefaultIfEmpty()
                           select new
                           {
                               Id = tableEmp.Id,
                               EnrollNumber = tableEmp.EmpEnrollNumber,
                               Name = tableEmp.Name,
                               Attedance = tableatt
                           };
            var listFinalJoin = listjoin.GroupBy(q => q.Id);

            var listStringDate = new List<String>();

            for (DateTime i = startTime; i < endTime; i = i.AddDays(1))
            {
                listStringDate.Add(i.ToString("dd/MM"));
            }
            //listStringDate.Add();

            var listResult = new List<FingerReportViewModel>();
            var displayStart = 1;
            foreach (var item in listFinalJoin)
            {
                var fingerReport = new FingerReportViewModel(startTime, endTime);
                fingerReport.No = displayStart++;
                fingerReport.StoreName = storeName;
                var firtAttendance = item.FirstOrDefault();
                fingerReport.EmployeeName = firtAttendance.Name;
                fingerReport.EmpID = firtAttendance.Id;
                fingerReport.EmpEnroll = firtAttendance.EnrollNumber;
                if (firtAttendance.Attedance != null)
                {
                    // Have Attendance 
                    foreach (var att in item)
                    {
                        if (att.Attedance != null)
                        {
                            var date = att.Attedance.ShiftMin.Day;
                            if (att.Attedance.TotalWorkTime != null)
                            {
                                var fingerReportTmp = fingerReport.Month.Where(q => q.DateString.Equals(att.Attedance.ShiftMin.ToString("dd/MM"))).FirstOrDefault();

                                fingerReportTmp.timework = fingerReportTmp.timework.Add(att.Attedance.TotalWorkTime.Value);
                            }
                        }
                    }
                }

                listResult.Add(fingerReport);
            }
            var finalResult = new List<string[]>();

            foreach (var item in listResult)
            {
                var tmpResult = new string[listStringDate.Count() + 5];
                tmpResult[0] = "" + item.No;
                tmpResult[1] = "" + item.EmpEnroll;
                tmpResult[2] = "" + item.EmployeeName;                
                var totalTime = new TimeSpan(item.Month.Sum(q => q.timework.Ticks));
                var stringtime = "";
                var tmp = Int32.Parse(totalTime.TotalHours.ToString("N").Split('.')[0]);

                if (tmp < 10)
                {
                    stringtime += "0" + totalTime.TotalHours.ToString("N").Split('.')[0];
                }
                else
                {
                    stringtime += totalTime.TotalHours.ToString("N").Split('.')[0];
                }

                stringtime += ":";
                if (totalTime.Minutes < 10)
                {
                    stringtime += "0" + totalTime.Minutes;
                }
                else
                {
                    stringtime += totalTime.Minutes;
                }
                var tmpMinute = int.Parse(stringtime.Split(':')[1]);
                if (tmpMinute > 60)
                {
                    stringtime = (int.Parse(stringtime.Split(':')[0]) + (int)(tmpMinute / 60)).ToString("N");
                    stringtime += ":" + (tmpMinute % 60).ToString("N");
                }

                for (int i = 0; i < item.Month.Count; i++)
                {
                    tmpResult[i + 3] = item.Month[i].timework.Ticks != 0 ? item.Month[i].timework.Hours.ToString("00") + ":" + item.Month[i].timework.Minutes.ToString("00") : "";
                }
                tmpResult[item.Month.Count + 3] = stringtime;
                tmpResult[tmpResult.Count() - 1] = "<button type='submit' onclick = 'exportOneEmployee(" + item.EmpID + ")' class='width230 btn btn-primary btn-sm waves-effect'>" +
                                                    "<i class='fa fa - download'></i>" +
                                                    "Xuất Excel</button>";

                finalResult.Add(tmpResult);
            }

            return Json(new
            {
                stringdate = listStringDate,
                result = finalResult
            }, JsonRequestBehavior.AllowGet);
        }
        //public ActionResult ExportMachineConnectTableToExcelFollowTemp(JQueryDataTableParamModel param, int brandId, string startDate, string endDate)
        //{
        //    var startTime = startDate.ToDateTime().GetStartOfDate();
        //    var endTime = endDate.ToDateTime().GetEndOfDate();

        //    var machineConnectApi = new MachineConnectApi();
        //    var machineconnectList = machineConnectApi.GetActive().Where(q => q.BrandID == brandId && q.ConnectTime >= startTime
        //    && q.ConnectTime <= endTime);
        //    var finalResult = new List<string[]>();
        //    int stt = 0;
        //    foreach (var item in machineconnectList)
        //    {
        //        stt++;
        //        var connectResult = "Không thành công";
        //        if (item.ConnectResult != null && item.ConnectResult == true)
        //        {
        //            connectResult = "Thành công";
        //        }
        //        string[] sarray = { stt.ToString(), item.StoreID.ToString(), item.MachineCode, item.MachineName, item.ConnectTime.ToString(), connectResult };
        //        finalResult.Add(sarray);
        //    }

        //    //ExportToExcel
        //    string filepath = HttpContext.Server.MapPath(@"/Resource/MACHINE CONNECT REPORT.xlsx");
        //    var filestream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);
        //    var sDate = startDate.Replace("/", "-");
        //    var eDate = endDate.Replace("/", "-");
        //    var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
        //    using (ExcelPackage package = new ExcelPackage(filestream))
        //    {
        //        ExcelWorksheet ws = package.Workbook.Worksheets[1];
        //        char StartHeaderChar = 'A';
        //        int StartHeaderNumber = 5;
        //        //Set values for cells     
        //        ws.Cells["C3"].Value = dateRange;
        //        foreach (var item in finalResult)
        //        {
        //            StartHeaderChar = 'A';
        //            StartHeaderNumber++;
        //            foreach (var unit in item)
        //            {
        //                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = unit;
        //            }
        //        }
        //        char EndHeaderChar = ++StartHeaderChar;
        //        int EndHeaderNumber = StartHeaderNumber;
        //        StartHeaderChar = 'A';
        //        StartHeaderNumber = 6;

        //        //Set style for rows and columns
        //        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
        //            ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
        //        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
        //            ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
        //        //ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
        //        //    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
        //        //    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //        //ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
        //        //    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
        //        //    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SandyBrown);
        //        //ws.View.FreezePanes(4, 7);
        //        ws.Cells["A1:" + StartHeaderChar + StartHeaderNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //        //Set style for excel
        //        ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        //        ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        //        ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        //        ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

        //        var brandAPI = new BrandApi();
        //        var brandName = brandAPI.Get(brandId).BrandName;
        //        string fileName = "Báo cáo kết nối Máy chấm công" + brandName + dateRange + ".xlsx";

        //        MemoryStream ms = new MemoryStream();
        //        package.SaveAs(ms);
        //        filestream.Close();
        //        ms.Seek(0, SeekOrigin.Begin);
        //        var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        return this.File(ms, contentType, fileName);
        //    }
        //}
        public ActionResult ExportToExcel2(int storeSelected, string startDate, string endDate, int brandId)
        {
            #region getdata

            var startTime = startDate.ToDateTime().GetStartOfDate();
            var endTime = endDate.ToDateTime().GetEndOfDate();

            var empApi = new EmployeeApi();
            var attApi = new AttendanceApi();
            var storeapi = new StoreApi();

            //var listEmp = empApi.GetActive().Where(q => q.MainStoreId == storeSelected);
            var listEmp = empApi.GetEmployeeByStoreId(storeSelected, brandId);
            IEnumerable<Attendance> listAtt = attApi.GetAttendanceByStoreByTimeRange(storeSelected, startTime, endTime);

            var listjoin = from tableEmp in listEmp
                           join tableatt in listAtt
                           on tableEmp.Id equals tableatt.EmployeeId into ps
                           from tableatt in ps.DefaultIfEmpty()
                           select new
                           {
                               Id = tableEmp.Id,
                               Name = tableEmp.Name,
                               StoreId = tableEmp.MainStoreId,
                               Attedance = tableatt
                           };

            var listFinalJoin = listjoin.GroupBy(q => q.Id);

            List<ReportEmployee> listRE = new List<ReportEmployee>();
            int dayInMonth = (int)(endTime - startTime).TotalDays;
            //list day of week
            List<string> listDayOfWeek = new List<string>();
            List<string> listStringDate = new List<string>();
            DateTime dateD = new DateTime();
            var culture = new System.Globalization.CultureInfo("vi-VN");
            for (DateTime date = startTime; date.Date <= endTime.Date; date = date.AddDays(1))
            {
                listDayOfWeek.Add(culture.DateTimeFormat.GetDayName(date.DayOfWeek));
                listStringDate.Add(date.ToString("dd/MM"));
            }
            foreach (var item in listFinalJoin)
            {
                var firstAttendance = item.FirstOrDefault();
                //list report gom co ten nhan vien va gio lam viec cua cac ngay trong thang trong
                var reEmp = new ReportEmployee(startTime, endTime);
                reEmp.empName = firstAttendance.Name;
                //list gio lam viec cua nhan vien theo tung ngay trong thang
                if (firstAttendance.Attedance != null) // Xac dinh co diem danh trong thang
                {
                    foreach (var attendance in item)
                    {
                        if (attendance != null)
                        {
                            var date = attendance.Attedance.ShiftMin.Day;
                            if (attendance.Attedance.TotalWorkTime != null)
                            {
                                var fingerReportTmp = reEmp.time.Where(q => q.DateString.Equals(attendance.Attedance.ShiftMin.ToString("dd/MM"))).FirstOrDefault();
                                fingerReportTmp.timework = fingerReportTmp.timework.Add(attendance.Attedance.TotalWorkTime.Value);
                                reEmp.totalTime = reEmp.totalTime.Add(attendance.Attedance.TotalWorkTime.Value);
                            }
                        }
                    }
                }
                if (firstAttendance.StoreId == 0)
                {
                    reEmp.storename = "Hệ thống";
                }
                else
                {
                    reEmp.storename = storeapi.GetStoreNameByID(firstAttendance.StoreId);
                }

                listRE.Add(reEmp);
            }

            #endregion
            return ExportDateStoreReportToExcel2(listRE, listDayOfWeek, startTime.ToString("dd/MM/yyyy"), endTime.ToString("dd/MM/yyyy"), listStringDate);

        }
        public ActionResult ExportDateStoreReportToExcel2(List<ReportEmployee> data, List<string> daysOfWeek, string stime, string etime, List<string> listStringDate)
        {
            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo");
                #region header
                char startchar = 'A'; char endchar = 'Z';
                ws.Cells["A1:A3"].Merge = true;
                ws.Cells["A1:A3"].Value = "STT";
                ws.Cells["A1:A3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A1:A3"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
                ws.Cells["A1:A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["B1:B3"].Merge = true;
                ws.Cells["B1:B3"].Value = "Họ và Tên";
                ws.Cells["B1:B3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["B1:B3"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
                ws.Cells["B1:B3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["C1:C3"].Merge = true;
                ws.Cells["C1:C3"].Value = "Thuộc Chi Nhánh";
                ws.Cells["C1:C3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["C1:C3"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
                ws.Cells["C1:C3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                if (daysOfWeek.Count > 1)

                {
                    ws.Cells["D1:" + GetColNameFromIndex(daysOfWeek.Count + 3) + "1"].Merge = true;
                    ws.Cells["D1:" + GetColNameFromIndex(daysOfWeek.Count + 3) + "1"].Value = "NGÀY";
                    ws.Cells["D1:" + GetColNameFromIndex(daysOfWeek.Count + 3) + "1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells["D1:" + GetColNameFromIndex(daysOfWeek.Count + 3) + "1"].Style.Fill.BackgroundColor.SetColor(Color.Aquamarine);
                    ws.Cells["D1:" + GetColNameFromIndex(daysOfWeek.Count + 3) + "1"].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Center;
                }
                else
                {
                    ws.Cells["D1"].Value = "NGÀY";
                    ws.Cells["D1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells["D1"].Style.Fill.BackgroundColor.SetColor(Color.Aquamarine);
                }

                int startIndex = 4;
                for (int i = 0; i < daysOfWeek.Count(); i++)
                {
                    ws.Cells[GetColNameFromIndex(startIndex) + "2"].Value = listStringDate[i];
                    ws.Cells[GetColNameFromIndex(startIndex) + "3"].Value = daysOfWeek[i];
                    startIndex = startIndex + 1;
                }
                ws.Cells["D1:" + GetColNameFromIndex(daysOfWeek.Count + 3) + "2"].Style.Fill.PatternType =
                    ExcelFillStyle.Solid;
                ws.Cells["D1:" + GetColNameFromIndex(daysOfWeek.Count + 3) + "3"].Style.Fill.PatternType =
                    ExcelFillStyle.Solid;
                ws.Cells["D1:" + GetColNameFromIndex(daysOfWeek.Count + 3) + "2"].Style.Fill.BackgroundColor.SetColor(Color.Aquamarine);
                ws.Cells["D1:" + GetColNameFromIndex(daysOfWeek.Count + 3) + "3"].Style.Fill.BackgroundColor.SetColor(Color.Aquamarine);
                string tongCong = GetColNameFromIndex(daysOfWeek.Count + 4);
                ws.Cells[tongCong + "1:" + tongCong + "3"].Merge = true;
                ws.Cells[tongCong + "1:" + tongCong + "3"].Value = "Tổng cộng";
                ws.Cells[tongCong + "1:" + tongCong + "3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[tongCong + "1:" + tongCong + "3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[tongCong + "1:" + tongCong + "3"].Style.Fill.BackgroundColor.SetColor(Color.GreenYellow);




                #endregion
                //Set style for excel

                #region set value for cells

                int indexRow = 4;
                int count = 1;
                foreach (var item in data)
                {
                    int indexCol = 1;
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = count++;
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item.empName;
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item.storename;
                    ws.Cells["" + GetColNameFromIndex(daysOfWeek.Count + 4) + indexRow].Value = Math.Round(item.totalTime.TotalHours, 1);
                    foreach (var time in item.time)
                    {
                        ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = Math.Round(time.timework.TotalHours, 1);
                    }
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
                string fileName = "BaoCaoChamCong" + "-tu" + stime + "den" + etime + ".xlsx";
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }
        public ActionResult ExportToExcel3(int brandId, int storeSelected, string startDate, string endDate)
        {
            #region getdata

            var startTime = startDate.ToDateTime().GetStartOfDate();
            var endTime = endDate.ToDateTime().GetEndOfDate();

            var empApi = new EmployeeApi();
            var attApi = new AttendanceApi();
            var storeapi = new StoreApi();

            var listEmp = storeSelected != 0
                ? empApi.GetActive().Where(q => q.MainStoreId == storeSelected)
                : empApi.GetActive().Where(q => q.BrandId == brandId);

            IEnumerable<Attendance> listAtt = storeSelected != 0
                ? attApi.GetAttendanceByStoreByTimeRange(storeSelected, startTime, endTime)
                : attApi.GetAttendanceByTimeRangeAndBrand(storeapi.GetStoreByBrandId(brandId).Select(p => p.ID).ToList(), startTime, endTime);
            var listjoin = from tableEmp in listEmp
                           join tableatt in listAtt
                           on tableEmp.Id equals tableatt.EmployeeId into ps
                           from tableatt in ps.DefaultIfEmpty()
                           select new
                           {
                               Id = tableEmp.Id,
                               Name = tableEmp.Name,
                               Salary = tableEmp.Salary,
                               StoreId = tableEmp.MainStoreId,
                               Attedance = tableatt
                           };

            var listFinalJoin = listjoin.GroupBy(q => q.Id);

            List<ReportEmployee> listRE = new List<ReportEmployee>();
            int dayInMonth = (int)(endTime - startTime).TotalDays;
            //list day of week
            List<string> listDayOfWeek = new List<string>();
            List<string> listStringDate = new List<string>();
            DateTime dateD = new DateTime();
            var culture = new System.Globalization.CultureInfo("vi-VN");
            for (DateTime date = startTime; date.Date <= endTime.Date; date = date.AddDays(1))
            {
                listDayOfWeek.Add(culture.DateTimeFormat.GetDayName(date.DayOfWeek));
                listStringDate.Add(date.ToString("dd/MM"));
            }
            foreach (var item in listFinalJoin)
            {
                var firstAttendance = item.FirstOrDefault();
                //list report gom co ten nhan vien va gio lam viec cua cac ngay trong thang trong
                var reEmp = new ReportEmployee(startTime, endTime);
                reEmp.empName = firstAttendance.Name;

                //list gio lam viec cua nhan vien theo tung ngay trong thang
                if (firstAttendance.Attedance != null) // Xac dinh co diem danh trong thang
                {
                    foreach (var attendance in item)
                    {
                        if (attendance != null)
                        {
                            var date = attendance.Attedance.ShiftMin.Day;
                            if (attendance.Attedance.TotalWorkTime != null)
                            {
                                var fingerReportTmp = reEmp.time.Where(q => q.DateString.Equals(attendance.Attedance.ShiftMin.ToString("dd/MM"))).FirstOrDefault();
                                fingerReportTmp.timework = fingerReportTmp.timework.Add(attendance.Attedance.TotalWorkTime.Value);
                                if (attendance.Attedance.CheckMin != null)
                                {
                                    DateTime checkmin = (DateTime)attendance.Attedance.CheckMin;
                                    fingerReportTmp.CheckMin = TimeSpan.FromTicks(checkmin.Ticks);
                                }
                                if (attendance.Attedance.CheckMax != null)
                                {
                                    DateTime checkMax = (DateTime)attendance.Attedance.CheckMax;
                                    fingerReportTmp.CheckMax = TimeSpan.FromTicks(checkMax.Ticks);
                                }
                                if (attendance.Attedance.IsOverTime == true)
                                {
                                    reEmp.totalOverTime = reEmp.totalOverTime.Add(attendance.Attedance.TotalWorkTime.Value);
                                }
                                else
                                {
                                    reEmp.totalTime = reEmp.totalTime.Add(attendance.Attedance.TotalWorkTime.Value);
                                }
                            }
                        }
                    }
                }
                reEmp.storename = storeapi.GetStoreNameByID(firstAttendance.StoreId);
                var tmpEmp = empApi.Get(firstAttendance.Id);
                reEmp.dateStart = tmpEmp.DateStartWork != null ? tmpEmp.DateStartWork.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật";
                reEmp.empEnrollNumber = tmpEmp.EmpEnrollNumber;
                var emplGroupApi = new EmployeeGroupApi();
                reEmp.empGroup = emplGroupApi.Get().Where(q => q.Id == tmpEmp.EmployeeGroupId).Select(r => r.NameGroup).FirstOrDefault();
                reEmp.salary = tmpEmp.Salary;
                listRE.Add(reEmp);
            }

            #endregion
            return ExportDateStoreReportToExcel3(listRE, listDayOfWeek, startTime.ToString("dd/MM/yyyy"), endTime.ToString("dd/MM/yyyy"), listStringDate, startTime.ToString("dd/MM/yyyy") + "-" + endTime.ToString("dd/MM/yyyy"), storeSelected);

        }
        public ActionResult ExportDateStoreReportToExcel3(List<ReportEmployee> data, List<string> daysOfWeek, string stime, string etime, List<string> listStringDate, string month, int selectedStoreId)
        {
            string filepath = HttpContext.Server.MapPath(@"/Resource/BANG_CHAM_CONG_3.xlsx");
            var attApi = new AttendanceApi();

            var filestream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);
            var storeApi = new StoreApi();
            using (ExcelPackage package = new ExcelPackage(filestream))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets[1];
                ws.Cells["A2"].Value = "Thời Gian: " + month;
                ws.Cells["I2"].Value = "Allstore";
                if (selectedStoreId != 0)
                {
                    ws.Cells["I2"].Value = storeApi.GetStoreNameByID(selectedStoreId);
                }
                var row = 5;
                int count = 0;
                var endCol = data.FirstOrDefault().time.Count();
                ws.InsertColumn(10, data.FirstOrDefault().time.Count() - 1);
                var firstrow = data.FirstOrDefault();
                var countCol = 8;
                ExcelRange rgOriginal = ws.Cells["A4"];
                foreach (var item in firstrow.time)
                {
                    countCol++;
                    ws.Cells[4, countCol].Value = item.DateString;
                    ws.Cells[4, countCol].StyleID = rgOriginal.StyleID;
                    ws.Cells[4, countCol, 5, countCol].Merge = true;
                }
                foreach (var item in data)
                {
                    row = row + 3;
                    count++;
                    //ws.Cells["A8:A9"].Merge = true;                    
                    ws.Cells[row - 2, 1].Value = count;
                    ws.Cells[row - 2, 2].Value = item.storename;
                    ws.Cells[row - 2, 3].Value = item.empGroup;
                    ws.Cells[row - 2, 4].Value = item.empEnrollNumber;
                    ws.Cells[row - 2, 5].Value = item.empName;
                    ws.Cells[row - 2, 6].Value = item.dateStart;
                    ws.Cells[row - 2, 8].Value = item.salary;

                    ws.Cells[row - 2, 1, row, 1].Merge = true;
                    ws.Cells[row - 2, 2, row, 2].Merge = true;
                    ws.Cells[row - 2, 3, row, 3].Merge = true;
                    ws.Cells[row - 2, 4, row, 4].Merge = true;
                    ws.Cells[row - 2, 5, row, 5].Merge = true;
                    ws.Cells[row - 2, 6, row, 6].Merge = true;
                    ws.Cells[row - 2, 7, row, 7].Merge = true;
                    ws.Cells[row - 2, 8, row, 8].Merge = true;

                    double total = 0;
                    double totalOverTime = 0;
                    for (int i = 0; i < item.time.Count; i++)
                    {
                        int index = i + 9;
                        if (item.time[i].timework.Ticks == 0)
                        {
                            ws.Cells[row - 2, index].Value = "-";
                            ws.Cells[row - 2, index].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[row, index].Value = "-";
                            ws.Cells[row, index].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        else
                        {
                            ws.Cells[row - 2, index].Value = Math.Round((item.time[i].timework.Hours + item.time[i].timework.Minutes / 100.0 + item.time[i].timework.Seconds / 10000.0) * (item.time[i].timework > TimeSpan.Zero ? 1 : -1), 2);
                            ws.Cells[row, index].Value = new DateTime(item.time[i].CheckMin.Ticks).ToString("HH:mm") + " - " + new DateTime(item.time[i].CheckMax.Ticks).ToString("HH:mm");
                            ws.Cells[row, index].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        if (item.time[i].timeOverTime.Ticks == 0)
                        {
                            ws.Cells[row - 1, index].Value = "-";
                            ws.Cells[row - 1, index].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        else
                        {
                            ws.Cells[row - 1, index].Value = Math.Round((item.time[i].timeOverTime.Hours + item.time[i].timeOverTime.Minutes / 100.0 + item.time[i].timeOverTime.Seconds / 10000.0) * (item.time[i].timeOverTime > TimeSpan.Zero ? 1 : -1), 2);
                        }
                        total = total + Math.Round((item.time[i].timework.Hours + item.time[i].timework.Minutes / 100.0 + item.time[i].timework.Seconds / 10000.0) * (item.time[i].timework > TimeSpan.Zero ? 1 : -1), 2);
                        totalOverTime = totalOverTime + Math.Round((item.time[i].timeOverTime.Hours + item.time[i].timeOverTime.Minutes / 100.0 + item.time[i].timeOverTime.Seconds / 10000.0) * (item.time[i].timeOverTime > TimeSpan.Zero ? 1 : -1), 2);
                    }
                    ws.Cells[row - 2, endCol + 9].Value = total;
                    ws.Cells[row - 2, endCol + 9, row, endCol + 9].Merge = true;
                    ws.Cells[row - 2, endCol + 10].Value = totalOverTime;
                    ws.Cells[row - 2, endCol + 10, row, endCol + 10].Merge = true;
                    ws.Cells[row - 2, endCol + 11, row, endCol + 11].Merge = true;
                    ws.Cells[row - 2, endCol + 12, row, endCol + 12].Merge = true;
                    ws.Cells[row - 2, endCol + 13, row, endCol + 13].Merge = true;
                    ws.Cells[row - 2, endCol + 14, row, endCol + 14].Merge = true;
                    ws.Cells[row - 2, endCol + 15, row, endCol + 15].Merge = true;
                    ws.Cells[row - 2, endCol + 16, row, endCol + 16].Merge = true;
                    ws.Cells[row - 2, endCol + 17, row, endCol + 17].Merge = true;
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.WrapText = true;

                //Ghi Chu
                row = row + 3;
                ws.Cells[row, 1].Value = "Ghi Chú";
                ws.Cells[row, 1, row, 2].Merge = true;
                row++;
                ws.Cells[row, 1].Value = "Nghỉ phép năm";
                ws.Cells[row, 1, row, 2].Merge = true;
                ws.Cells[row, 3].Value = "AL";
                ws.Cells[row, 5].Value = "Nghỉ hết hàng";
                ws.Cells[row, 5, row, 6].Merge = true;
                ws.Cells[row, 7].Value = "TMP";
                ///
                row++;
                ws.Cells[row, 1].Value = "Nghỉ tang";
                ws.Cells[row, 1, row, 2].Merge = true;
                ws.Cells[row, 3].Value = "FL";
                ws.Cells[row, 5].Value = "Không quét thẻ";
                ws.Cells[row, 5, row, 6].Merge = true;
                ws.Cells[row, 7].Value = "NON";
                //
                row++;
                ws.Cells[row, 1].Value = "Nghỉ phép năm";
                ws.Cells[row, 1, row, 2].Merge = true;
                ws.Cells[row, 3].Value = "AL";
                ws.Cells[row, 5].Value = "Nghỉ hết hàng";
                ws.Cells[row, 5, row, 6].Merge = true;
                ws.Cells[row, 7].Value = "TMP";
                //
                row++;
                ws.Cells[row, 1].Value = "Nghỉ khám thai";
                ws.Cells[row, 1, row, 2].Merge = true;
                ws.Cells[row, 3].Value = "PT";
                ws.Cells[row, 5].Value = "Nghỉ bù";
                ws.Cells[row, 5, row, 6].Merge = true;
                ws.Cells[row, 7].Value = "COM";
                // 
                row++;
                ws.Cells[row, 1].Value = "Nghỉ thai sản";
                ws.Cells[row, 1, row, 2].Merge = true;
                ws.Cells[row, 3].Value = "ML";
                ws.Cells[row, 5].Value = "Nghỉ dưỡng sức";
                ws.Cells[row, 5, row, 6].Merge = true;
                ws.Cells[row, 7].Value = "SH";
                // 
                row++;
                ws.Cells[row, 1].Value = "Nghỉ không phép";
                ws.Cells[row, 1, row, 2].Merge = true;
                ws.Cells[row, 3].Value = "NPL";
                ws.Cells[row, 5].Value = "Nghỉ kế hoạch hóa gia đình";
                ws.Cells[row, 5, row, 6].Merge = true;
                ws.Cells[row, 7].Value = "FP";
                //
                row++;
                ws.Cells[row, 1].Value = "Nghỉ con ốm SC";
                ws.Cells[row, 1, row, 2].Merge = true;
                ws.Cells[row, 3].Value = "SC";
                ws.Cells[row, 5].Value = "Nghỉ theo lịch";
                ws.Cells[row, 5, row, 6].Merge = true;
                ws.Cells[row, 7].Value = "AB";
                ///
                row++;
                ws.Cells[row, 1].Value = "Nghỉ ốm";
                ws.Cells[row, 1, row, 2].Merge = true;
                ws.Cells[row, 3].Value = "SL";
                ws.Cells[row, 5].Value = "Nghỉ công tác";
                ws.Cells[row, 5, row, 6].Merge = true;
                ws.Cells[row, 7].Value = "BT";
                //
                row++;
                ws.Cells[row, 1].Value = "Nghỉ cưới";
                ws.Cells[row, 1, row, 2].Merge = true;
                ws.Cells[row, 3].Value = "WL";
                ws.Cells[row, 5].Value = "Nghỉ không lương";
                ws.Cells[row, 5, row, 6].Merge = true;
                ws.Cells[row, 7].Value = "UP";
                //
                row++;
                ws.Cells[row, 1].Value = "Nghỉ tai nạn lao động";
                ws.Cells[row, 1, row, 2].Merge = true;
                ws.Cells[row, 3].Value = "LA";
                ws.Cells[row, 5].Value = "Nghỉ trách nhiệm";
                ws.Cells[row, 5, row, 6].Merge = true;
                ws.Cells[row, 7].Value = "DO";
                //
                row++;
                ws.Cells[row, 1].Value = "Nghỉ sẩy thai";
                ws.Cells[row, 1, row, 2].Merge = true;
                ws.Cells[row, 3].Value = "MS";
                ws.Cells[row, 5].Value = "Không quét vào";
                ws.Cells[row, 5, row, 6].Merge = true;
                ws.Cells[row, 7].Value = "B";
                //
                row++;
                ws.Cells[row, 1].Value = "Nghỉ huấn luyện";
                ws.Cells[row, 1, row, 2].Merge = true;
                ws.Cells[row, 3].Value = "TL";
                ws.Cells[row, 5].Value = "Không quét ra";
                ws.Cells[row, 5, row, 6].Merge = true;
                ws.Cells[row, 7].Value = "A";


                MemoryStream ms = new MemoryStream();

                package.SaveAs(ms);
                filestream.Close();
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = "BaoCaoChamCong" + "-tu " + stime + "den " + etime + ".xlsx";
                return this.File(ms, contentType, fileName);
            }
        }

        public ActionResult ExportToExcel4(int brandId, int storeSelected, string startDate, string endDate)
        {
            #region getdata

            var startTime = startDate.ToDateTime().GetStartOfDate();
            var endTime = endDate.ToDateTime().GetEndOfDate();

            var empApi = new EmployeeApi();
            var attApi = new AttendanceApi();
            var storeapi = new StoreApi();

            //var emp = empApi.GetEmployee(empID);
            List<ReportEmployee> listRE = new List<ReportEmployee>();
            List<string> listDayOfWeek = new List<string>();
            List<string> listStringDate = new List<string>();                                   
            
            //Nếu chọn nút Xuất trong 1 nhân viên thì chỉ xuất 1 nv
            var listEmp = storeSelected != 0
                ? empApi.GetEmployeeByStoreId(storeSelected)
                : empApi.BaseService.GetActive(q => q.BrandId == brandId).ToList();
            var empListID = listEmp.Select(m => m.Id);
                IEnumerable<Attendance> listAtt = storeSelected != 0
                    ? attApi.BaseService.Get(q => q.Active == true && q.ShiftMin >= startTime && q.ShiftMin <= endTime && empListID.Contains(q.EmployeeId))
                    : attApi.GetAttendanceByTimeRangeAndBrand(storeapi.GetStoreByBrandId(brandId).Select(p => p.ID).ToList(), startTime, endTime);
                var listjoin = from tableEmp in listEmp
                               join tableatt in listAtt
                               on tableEmp.Id equals tableatt.EmployeeId into ps
                               from tableatt in ps.DefaultIfEmpty()
                               select new
                               {
                                   Id = tableEmp.Id,
                                   Name = tableEmp.Name,
                                   Salary = tableEmp.Salary,
                                   StoreId = tableEmp.MainStoreId,
                                   WorkingStoreId = tableatt!=null?tableatt.StoreId: 0,
                                   Regency = tableEmp.EmployeeRegency,
                                   Attedance = tableatt
                               };

                var listFinalJoin = listjoin.GroupBy(q => q.Id);

                int dayInMonth = (int)(endTime - startTime).TotalDays;
                //list day of week
                DateTime dateD = new DateTime();
                var culture = new System.Globalization.CultureInfo("vi-VN");
                for (DateTime date = startTime; date.Date <= endTime.Date; date = date.AddDays(1))
                {
                    listDayOfWeek.Add(culture.DateTimeFormat.GetDayName(date.DayOfWeek));
                    listStringDate.Add(date.ToString("dd/MM"));
                }
                foreach (var item in listFinalJoin)
                {
                var attenListByStore = item.GroupBy(q => q.WorkingStoreId);
                    foreach(var itemstore in attenListByStore)
                    {
                    var firstAttendance = itemstore.FirstOrDefault();
                    //list report gom co ten nhan vien va gio lam viec cua cac ngay trong thang trong

                    var reEmp = new ReportEmployee(startTime, endTime);
                    reEmp.empName = firstAttendance.Name;
                    //list gio lam viec cua nhan vien theo tung ngay trong thang
                    if (firstAttendance.Attedance != null) // Xac dinh co diem danh trong thang
                    {
                        foreach (var attendance in itemstore)
                        {
                            if (attendance != null)
                            {
                                var date = attendance.Attedance.ShiftMin.Day;
                                if (attendance.Attedance.TotalWorkTime != null)
                                {
                                    var fingerReportTmp = reEmp.time.Where(q => q.DateString.Equals(attendance.Attedance.ShiftMin.ToString("dd/MM"))).FirstOrDefault();
                                    fingerReportTmp.timework = fingerReportTmp.timework.Add(attendance.Attedance.TotalWorkTime.Value);
                                    if (attendance.Attedance.Status == 1)
                                    {
                                        fingerReportTmp.timeworkApproved = fingerReportTmp.timeworkApproved.Add(attendance.Attedance.TotalWorkTime.Value);
                                    }
                                    if (attendance.Attedance.CheckMin != null)
                                    {
                                        DateTime checkmin = (DateTime)attendance.Attedance.CheckMin;
                                        fingerReportTmp.CheckMin = TimeSpan.FromTicks(checkmin.Ticks);
                                    }
                                    if (attendance.Attedance.CheckMax != null)
                                    {
                                        DateTime checkMax = (DateTime)attendance.Attedance.CheckMax;
                                        fingerReportTmp.CheckMax = TimeSpan.FromTicks(checkMax.Ticks);
                                    }
                                    if (attendance.Attedance.IsOverTime == true)
                                    {
                                        reEmp.totalOverTime = reEmp.totalOverTime.Add(attendance.Attedance.TotalWorkTime.Value);
                                    }
                                    else
                                    {
                                        reEmp.totalTime = reEmp.totalTime.Add(attendance.Attedance.TotalWorkTime.Value);
                                        if (attendance.Attedance.Status == 1)
                                        {
                                            reEmp.totalTimeAprroved = reEmp.totalTimeAprroved.Add(attendance.Attedance.TotalWorkTime.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if(firstAttendance.WorkingStoreId == 0)
                    {
                        reEmp.storename = storeapi.GetStoreNameByID(firstAttendance.StoreId);
                    }
                    else
                    {
                        reEmp.storename = storeapi.GetStoreNameByID(firstAttendance.WorkingStoreId);
                    }
                    
                    var tmpEmp = empApi.Get(firstAttendance.Id);
                    reEmp.dateStart = tmpEmp.DateStartWork != null ? tmpEmp.DateStartWork.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật";
                    reEmp.empEnrollNumber = tmpEmp.EmpEnrollNumber;
                    var emplGroupApi = new EmployeeGroupApi();
                    reEmp.empGroup = emplGroupApi.Get().Where(q => q.Id == tmpEmp.EmployeeGroupId).Select(r => r.NameGroup).FirstOrDefault();
                    reEmp.salary = tmpEmp.Salary;
                    reEmp.empRegency = tmpEmp.EmployeeRegency;
                    reEmp.mainStore = storeapi.Get().Where(q => q.ID == tmpEmp.MainStoreId).FirstOrDefault().Name;
                    //reEmp. = tmpEmp.MainStoreId;
                    listRE.Add(reEmp);
                }   
                }

            if (storeSelected == 0)
            {
                listRE = listRE.OrderBy(q => q.storename).ToList();
            }
            else
            {
                listRE = listRE.OrderBy(q => q.empEnrollNumber).ToList();
            }
            #endregion
            return ExportDateStoreReportToExcel4(listRE, listDayOfWeek, startTime.ToString("dd/MM/yyyy"), endTime.ToString("dd/MM/yyyy"), listStringDate, startTime.ToString("dd/MM/yyyy") + "-" + endTime.ToString("dd/MM/yyyy"), storeSelected);
        }

        public ActionResult ExportSingleToExcel(int brandId, int storeSelected, string startDate, string endDate, int empID)
        {
            #region getdata
            var startTime = startDate.ToDateTime().GetStartOfDate();
            var endTime = endDate.ToDateTime().GetEndOfDate();
            brandId = 1;
            storeSelected = 0;
            var empApi = new EmployeeApi();
            var attApi = new AttendanceApi();
            var storeapi = new StoreApi();

            //var emp = empApi.GetEmployee(empID);
            List<ReportEmployee> listRE = new List<ReportEmployee>();
            List<string> listDayOfWeek = new List<string>();
            List<string> listStringDate = new List<string>();

            //Nếu chọn nút Xuất trong 1 nhân viên thì chỉ xuất 1 nv
            var listEmp = storeSelected != 0
                ? empApi.GetEmployeeByStoreId(storeSelected)
                : empApi.BaseService.GetActive(q => q.BrandId == brandId).ToList();

            IEnumerable<Attendance> listAtt = storeSelected != 0
                ? attApi.GetAttendanceByStoreByTimeRange(storeSelected, startTime, endTime)
                : attApi.GetAttendanceByTimeRangeAndBrand(storeapi.GetStoreByBrandId(brandId).Select(p => p.ID).ToList(), startTime, endTime);
            var listjoin = from tableEmp in listEmp
                           join tableatt in listAtt
                           on tableEmp.Id equals tableatt.EmployeeId into ps
                           from tableatt in ps.DefaultIfEmpty()
                           where tableEmp.Id == empID
                           select new
                           {
                               Id = tableEmp.Id,
                               Name = tableEmp.Name,
                               Salary = tableEmp.Salary,
                               StoreId = tableEmp.MainStoreId,
                               Regency = tableEmp.EmployeeRegency,
                               Attedance = tableatt
                           };

            var listFinalJoin = listjoin.GroupBy(q => q.Id);

            int dayInMonth = (int)(endTime - startTime).TotalDays;
            //list day of week
            DateTime dateD = new DateTime();
            var culture = new System.Globalization.CultureInfo("vi-VN");
            for (DateTime date = startTime; date.Date <= endTime.Date; date = date.AddDays(1))
            {
                listDayOfWeek.Add(culture.DateTimeFormat.GetDayName(date.DayOfWeek));
                listStringDate.Add(date.ToString("dd/MM"));
            }
            foreach (var item in listFinalJoin)
            {
                var firstAttendance = item.FirstOrDefault();
                //list report gom co ten nhan vien va gio lam viec cua cac ngay trong thang trong
                var reEmp = new ReportEmployee(startTime, endTime);
                reEmp.empName = firstAttendance.Name;

                //list gio lam viec cua nhan vien theo tung ngay trong thang
                if (firstAttendance.Attedance != null) // Xac dinh co diem danh trong thang
                {
                    foreach (var attendance in item)
                    {
                        if (attendance != null)
                        {
                            var date = attendance.Attedance.ShiftMin.Day;
                            if (attendance.Attedance.TotalWorkTime != null)
                            {
                                var fingerReportTmp = reEmp.time.Where(q => q.DateString.Equals(attendance.Attedance.ShiftMin.ToString("dd/MM"))).FirstOrDefault();
                                fingerReportTmp.timework = fingerReportTmp.timework.Add(attendance.Attedance.TotalWorkTime.Value);
                                if (attendance.Attedance.Status == 1)
                                {
                                    fingerReportTmp.timeworkApproved = fingerReportTmp.timeworkApproved.Add(attendance.Attedance.TotalWorkTime.Value);
                                }
                                if (attendance.Attedance.CheckMin != null)
                                {
                                    DateTime checkmin = (DateTime)attendance.Attedance.CheckMin;
                                    fingerReportTmp.CheckMin = TimeSpan.FromTicks(checkmin.Ticks);
                                }
                                if (attendance.Attedance.CheckMax != null)
                                {
                                    DateTime checkMax = (DateTime)attendance.Attedance.CheckMax;
                                    fingerReportTmp.CheckMax = TimeSpan.FromTicks(checkMax.Ticks);
                                }
                                if (attendance.Attedance.IsOverTime == true)
                                {
                                    reEmp.totalOverTime = reEmp.totalOverTime.Add(attendance.Attedance.TotalWorkTime.Value);
                                }
                                else
                                {
                                    reEmp.totalTime = reEmp.totalTime.Add(attendance.Attedance.TotalWorkTime.Value);
                                    if (attendance.Attedance.Status == 1)
                                    {
                                        reEmp.totalTimeAprroved = reEmp.totalTimeAprroved.Add(attendance.Attedance.TotalWorkTime.Value);
                                    }
                                }
                            }
                        }
                    }
                }
                var mainStoreID = -1;
                if (firstAttendance.Attedance != null)
                {
                    mainStoreID = empApi.BaseService.Get(firstAttendance.Attedance.EmployeeId).MainStoreId;
                }
                else
                {
                    mainStoreID = firstAttendance.StoreId;
                }
                if (mainStoreID != firstAttendance.StoreId)
                {
                    reEmp.storename = "Hỗ Trợ Từ: " + storeapi.GetStoreNameByID(mainStoreID);
                }
                else
                {
                    reEmp.storename = storeapi.GetStoreNameByID(firstAttendance.StoreId);
                }
                var tmpEmp = empApi.Get(firstAttendance.Id);
                reEmp.dateStart = tmpEmp.DateStartWork != null ? tmpEmp.DateStartWork.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật";
                reEmp.empEnrollNumber = tmpEmp.EmpEnrollNumber;
                var emplGroupApi = new EmployeeGroupApi();
                reEmp.empGroup = emplGroupApi.Get().Where(q => q.Id == tmpEmp.EmployeeGroupId).Select(r => r.NameGroup).FirstOrDefault();
                reEmp.salary = tmpEmp.Salary;
                reEmp.empRegency = tmpEmp.EmployeeRegency;
                listRE.Add(reEmp);
            }
            #endregion
            return ExportDateStoreReportToExcel4(listRE, listDayOfWeek, startTime.ToString("dd/MM/yyyy"), endTime.ToString("dd/MM/yyyy"), listStringDate, startTime.ToString("dd/MM/yyyy") + "-" + endTime.ToString("dd/MM/yyyy"), storeSelected);
        }
        public void colorThing(ref ExcelRange cell, Color color)
        {
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(color);
        }
        public ActionResult ExportDateStoreReportToExcel4(List<ReportEmployee> data, List<string> daysOfWeek, string stime, string etime, List<string> listStringDate, string month, int selectedStoreId)
        {
            string filepath = HttpContext.Server.MapPath(@"/Resource/BANG_CHAM_CONG_4.xlsx");
            var attApi = new AttendanceApi();
            var filestream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);
            var storeApi = new StoreApi();
            using (ExcelPackage package = new ExcelPackage(filestream))
            {
                string[] nameOfSheet = new string[2] { "WorkTime", "CheckTime" };
                foreach (string name in nameOfSheet)
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets[name];
                    ws.Cells["A2"].Value = "Thời Gian: " + month;
                    ws.Cells["I2"].Value = "Allstore";
                    if (selectedStoreId != 0)
                    {
                        ws.Cells["I2"].Value = storeApi.GetStoreNameByID(selectedStoreId);
                    }
                    var row = 5;
                    if (name.Equals("CheckTime"))
                    {
                        row = 6;
                    }
                    int count = 0;
                    int endInfoCol = 10;
                    var endCol = data.FirstOrDefault().time.Count();
                    if (name.Equals("WorkTime"))
                    {
                        ws.InsertColumn(endInfoCol + 1, endCol - 1);
                    }
                    if (name.Equals("CheckTime"))
                    {
                        ws.InsertColumn(endInfoCol + 1, 2 * endCol - 2);
                    }
                    var firstrow = data.FirstOrDefault();
                    var countCol = endInfoCol;
                    ExcelRange rgOriginal = ws.Cells["A4"];
                    if (name.Equals("WorkTime"))
                    {
                        foreach (var item in firstrow.time)
                        {
                            ws.Cells[4, countCol].Value = item.DateString;
                            ws.Cells[4, countCol].StyleID = rgOriginal.StyleID;
                            ws.Cells[4, countCol, 5, countCol].Merge = true;
                            countCol++;
                        }
                        foreach (var item in data)
                        {
                            row = row + 1;
                            count++;
                            //ws.Cells["A8:A9"].Merge = true;                    
                            ws.Cells[row, 1].Value = count;
                            ws.Cells[row, 2].Value = item.storename;
                            ws.Cells[row, 3].Value = item.mainStore;
                            ws.Cells[row, 4].Value = item.empGroup;
                            if (selectedStoreId != 0)
                            {
                                if (item.storename != ws.Cells["I2"].Value.ToString())
                                {
                                    ws.Cells[row, 4].Value = "Nhân viên hỗ trợ";
                                }
                            }
                            else
                            {
                                var empApi = new EmployeeApi();
                                var tmpEmp = empApi.BaseService.Get(q => q.EmpEnrollNumber == item.empEnrollNumber).FirstOrDefault();
                                if (tmpEmp != null)
                                {
                                    if(storeApi.GetStoreNameByID(tmpEmp.MainStoreId) != item.storename)
                                    {
                                        ws.Cells[row, 4].Value = "Nhân viên hỗ trợ";
                                    }
                                }
                            }
                            ws.Cells[row, 5].Value = item.empRegency;
                            ws.Cells[row, 6].Value = item.empEnrollNumber;
                            ws.Cells[row, 7].Value = item.empName;
                            ws.Cells[row, 8].Value = item.dateStart;
                            ws.Cells[row, 10].Value = item.salary;
                            
                            //ws.Cells[row - 2, 1, row, 1].Merge = true;
                            //ws.Cells[row - 2, 2, row, 2].Merge = true;
                            //ws.Cells[row - 2, 3, row, 3].Merge = true;
                            //ws.Cells[row - 2, 4, row, 4].Merge = true;
                            //ws.Cells[row - 2, 5, row, 5].Merge = true;
                            //ws.Cells[row - 2, 6, row, 6].Merge = true;
                            //ws.Cells[row - 2, 7, row, 7].Merge = true;
                            //ws.Cells[row - 2, 8, row, 8].Merge = true;
                            //ws.Cells[row - 2, 9, row, 9].Merge = true;

                            double total = 0;
                            double totalOverTime = 0;
                            double totalApproved = 0;
                            for (int i = 0; i < item.time.Count; i++)
                            {
                                int index = i + endInfoCol;
                                //if (item.time[i].timeworkApproved.Ticks == 0)
                                //{
                                //    ws.Cells[row - 2, index].Value = "-";
                                //    ws.Cells[row - 2, index].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                //}
                                //else
                                //{
                                //    ws.Cells[row - 2, index].Value = Math.Round((item.time[i].timeworkApproved.Hours + item.time[i].timeworkApproved.Minutes / 100.0 + item.time[i].timeworkApproved.Seconds / 10000.0) * (item.time[i].timeworkApproved > TimeSpan.Zero ? 1 : -1), 2);
                                //    //ws.Cells[row, index].Value = new DateTime(item.time[i].CheckMin.Ticks).ToString("HH:mm") + " - " + new DateTime(item.time[i].CheckMax.Ticks).ToString("HH:mm");
                                //}
                                //if (item.time[i].timeOverTime.Ticks == 0)
                                //{
                                //    ws.Cells[row - 1, index].Value = "-";
                                //    ws.Cells[row - 1, index].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                //}
                                //else
                                //{
                                //    ws.Cells[row - 1, index].Value = Math.Round((item.time[i].timeOverTime.Hours + item.time[i].timeOverTime.Minutes / 100.0 + item.time[i].timeOverTime.Seconds / 10000.0) * (item.time[i].timeOverTime > TimeSpan.Zero ? 1 : -1), 2);
                                //}

                                //if (item.time[i].timework.Ticks == 0)
                                //{
                                //    ws.Cells[row, index].Value = "-";
                                //    ws.Cells[row, index].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                //}
                                //else
                                //{
                                //    ws.Cells[row, index].Value = Math.Round((item.time[i].timework.Hours + item.time[i].timework.Minutes / 100.0 + item.time[i].timework.Seconds / 10000.0) * (item.time[i].timework > TimeSpan.Zero ? 1 : -1), 2);
                                //}
                                //total = total + Math.Round((item.time[i].timework.Hours + item.time[i].timework.Minutes / 100.0 + item.time[i].timework.Seconds / 10000.0) * (item.time[i].timework > TimeSpan.Zero ? 1 : -1), 2);
                                //totalOverTime = totalOverTime + Math.Round((item.time[i].timeOverTime.Hours + item.time[i].timeOverTime.Minutes / 100.0 + item.time[i].timeOverTime.Seconds / 10000.0) * (item.time[i].timeOverTime > TimeSpan.Zero ? 1 : -1), 2);
                                //totalApproved = totalApproved + Math.Round((item.time[i].timeworkApproved.Hours + item.time[i].timeworkApproved.Minutes / 100.0 + item.time[i].timeworkApproved.Seconds / 10000.0) * (item.time[i].timeworkApproved > TimeSpan.Zero ? 1 : -1), 2);
                                double itemHour = Math.Round(item.time[i].timework.TotalHours, 2);
                                if (item.time[i].timework.Ticks == 0)
                                {
                                    ws.Cells[row, index].Value = "-";
                                    ws.Cells[row, index].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                }
                                else
                                {
                                    // ws.Cells[row, index].Value = Math.Round((item.time[i].timework.Hours + item.time[i].timework.Minutes/ 60.0) * (item.time[i].timework > TimeSpan.Zero ? 1 : -1), 2);
                                    ws.Cells[row, index].Value = itemHour;
                                    total += itemHour;
                                }
                            }
                            //ws.Cells[row - 2, endCol + endInfoCol].Value = totalApproved;
                            ws.Cells[row, endCol + endInfoCol].Value = total;
                            if (selectedStoreId != 0)
                                {
                                    if (item.storename != ws.Cells["I2"].Value.ToString())
                                    {
                                        ExcelRange ex = ws.Cells[row, 1, row, endCol + endInfoCol + 8];
                                        colorThing(ref ex, Color.LightGreen);
                                    }
                                }
                                else
                                {
                                    var empApi = new EmployeeApi();
                                    var tmpEmp = empApi.BaseService.Get(q => q.EmpEnrollNumber == item.empEnrollNumber && q.Active).FirstOrDefault();
                                    if (tmpEmp != null)
                                    {
                                        if (storeApi.GetStoreNameByID(tmpEmp.MainStoreId) != item.storename)
                                        {
                                            ExcelRange ex = ws.Cells[row, 1, row, endCol + endInfoCol + 8];
                                            colorThing(ref ex, Color.LightGreen);
                                        }
                                    }
                                }
                            //ws.Cells[row - 1, endCol + endInfoCol, row, endCol + endInfoCol].Merge = true;
                            //ws.Cells[row - 2, endCol + endInfoCol + 1].Value = totalOverTime;
                            //ws.Cells[row - 2, endCol + endInfoCol + 1, row, endCol + endInfoCol + 1].Merge = true;
                            //ws.Cells[row - 2, endCol + endInfoCol + 2, row, endCol + endInfoCol + 2].Merge = true;
                            //ws.Cells[row - 2, endCol + endInfoCol + 3, row, endCol + endInfoCol + 3].Merge = true;
                            //ws.Cells[row - 2, endCol + endInfoCol + 4, row, endCol + endInfoCol + 4].Merge = true;
                            //ws.Cells[row - 2, endCol + endInfoCol + 5, row, endCol + endInfoCol + 5].Merge = true;
                            //ws.Cells[row - 2, endCol + endInfoCol + 6, row, endCol + endInfoCol + 6].Merge = true;
                            //ws.Cells[row - 2, endCol + endInfoCol + 7, row, endCol + endInfoCol + 7].Merge = true;
                            //ws.Cells[row - 2, endCol + endInfoCol + 8, row, endCol + endInfoCol + 8].Merge = true;
                        }
                    }
                    if (name.Equals("CheckTime"))
                    {
                        foreach (var item in firstrow.time)
                        {
                            ws.Cells[4, countCol].Value = item.DateString;
                            ws.Cells[4, countCol].StyleID = rgOriginal.StyleID;
                            ws.Cells[4, countCol, 5, countCol + 1].Merge = true;
                            ws.Cells[6, countCol].Value = "Check In";
                            ws.Cells[6, countCol].StyleID = rgOriginal.StyleID;
                            ws.Cells[6, countCol + 1].Value = "Check Out";
                            ws.Cells[6, countCol + 1].StyleID = rgOriginal.StyleID;
                            countCol += 2;
                        }
                        foreach (var item in data)
                        {
                            row = row + 1;
                            count++;
                            //ws.Cells["A8:A9"].Merge = true;                    
                            ws.Cells[row, 1].Value = count;
                            ws.Cells[row, 2].Value = item.storename;
                            ws.Cells[row, 3].Value = item.empGroup;
                            ws.Cells[row, 4].Value = item.empRegency;
                            ws.Cells[row, 5].Value = item.empEnrollNumber;
                            ws.Cells[row, 6].Value = item.empName;
                            ws.Cells[row, 7].Value = item.dateStart;
                            ws.Cells[row, 9].Value = item.salary;

                            for (int i = 0; i < item.time.Count; i++)
                            {
                                int index = 2 * i + endInfoCol;
                                if (item.time[i].timework.Ticks == 0)
                                {
                                    ws.Cells[row, index].Value = "-";
                                    ws.Cells[row, index].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    ws.Cells[row, index + 1].Value = "-";
                                    ws.Cells[row, index + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                }
                                else
                                {
                                    ws.Cells[row, index].Value = new DateTime(item.time[i].CheckMin.Ticks).ToString("HH:mm");
                                    ws.Cells[row, index + 1].Value = new DateTime(item.time[i].CheckMax.Ticks).ToString("HH:mm");
                                }
                            }
                        }
                    }
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.WrapText = true;

                    //Ghi Chu
                    row = row + 3;
                    ws.Cells[row, 1].Value = "Ghi Chú";
                    ws.Cells[row, 1, row, 2].Merge = true;
                    row++;
                    ws.Cells[row, 1].Value = "Nghỉ phép năm";
                    ws.Cells[row, 1, row, 2].Merge = true;
                    ws.Cells[row, 3].Value = "AL";
                    ws.Cells[row, 5].Value = "Nghỉ hết hàng";
                    ws.Cells[row, 5, row, 6].Merge = true;
                    ws.Cells[row, 7].Value = "TMP";
                    ///
                    row++;
                    ws.Cells[row, 1].Value = "Nghỉ tang";
                    ws.Cells[row, 1, row, 2].Merge = true;
                    ws.Cells[row, 3].Value = "FL";
                    ws.Cells[row, 5].Value = "Không quét thẻ";
                    ws.Cells[row, 5, row, 6].Merge = true;
                    ws.Cells[row, 7].Value = "NON";
                    //
                    row++;
                    ws.Cells[row, 1].Value = "Nghỉ phép năm";
                    ws.Cells[row, 1, row, 2].Merge = true;
                    ws.Cells[row, 3].Value = "AL";
                    ws.Cells[row, 5].Value = "Nghỉ hết hàng";
                    ws.Cells[row, 5, row, 6].Merge = true;
                    ws.Cells[row, 7].Value = "TMP";
                    //
                    row++;
                    ws.Cells[row, 1].Value = "Nghỉ khám thai";
                    ws.Cells[row, 1, row, 2].Merge = true;
                    ws.Cells[row, 3].Value = "PT";
                    ws.Cells[row, 5].Value = "Nghỉ bù";
                    ws.Cells[row, 5, row, 6].Merge = true;
                    ws.Cells[row, 7].Value = "COM";
                    // 
                    row++;
                    ws.Cells[row, 1].Value = "Nghỉ thai sản";
                    ws.Cells[row, 1, row, 2].Merge = true;
                    ws.Cells[row, 3].Value = "ML";
                    ws.Cells[row, 5].Value = "Nghỉ dưỡng sức";
                    ws.Cells[row, 5, row, 6].Merge = true;
                    ws.Cells[row, 7].Value = "SH";
                    // 
                    row++;
                    ws.Cells[row, 1].Value = "Nghỉ không phép";
                    ws.Cells[row, 1, row, 2].Merge = true;
                    ws.Cells[row, 3].Value = "NPL";
                    ws.Cells[row, 5].Value = "Nghỉ kế hoạch hóa gia đình";
                    ws.Cells[row, 5, row, 6].Merge = true;
                    ws.Cells[row, 7].Value = "FP";
                    //
                    row++;
                    ws.Cells[row, 1].Value = "Nghỉ con ốm SC";
                    ws.Cells[row, 1, row, 2].Merge = true;
                    ws.Cells[row, 3].Value = "SC";
                    ws.Cells[row, 5].Value = "Nghỉ theo lịch";
                    ws.Cells[row, 5, row, 6].Merge = true;
                    ws.Cells[row, 7].Value = "AB";
                    ///
                    row++;
                    ws.Cells[row, 1].Value = "Nghỉ ốm";
                    ws.Cells[row, 1, row, 2].Merge = true;
                    ws.Cells[row, 3].Value = "SL";
                    ws.Cells[row, 5].Value = "Nghỉ công tác";
                    ws.Cells[row, 5, row, 6].Merge = true;
                    ws.Cells[row, 7].Value = "BT";
                    //
                    row++;
                    ws.Cells[row, 1].Value = "Nghỉ cưới";
                    ws.Cells[row, 1, row, 2].Merge = true;
                    ws.Cells[row, 3].Value = "WL";
                    ws.Cells[row, 5].Value = "Nghỉ không lương";
                    ws.Cells[row, 5, row, 6].Merge = true;
                    ws.Cells[row, 7].Value = "UP";
                    //
                    row++;
                    ws.Cells[row, 1].Value = "Nghỉ tai nạn lao động";
                    ws.Cells[row, 1, row, 2].Merge = true;
                    ws.Cells[row, 3].Value = "LA";
                    ws.Cells[row, 5].Value = "Nghỉ trách nhiệm";
                    ws.Cells[row, 5, row, 6].Merge = true;
                    ws.Cells[row, 7].Value = "DO";
                    //
                    row++;
                    ws.Cells[row, 1].Value = "Nghỉ sẩy thai";
                    ws.Cells[row, 1, row, 2].Merge = true;
                    ws.Cells[row, 3].Value = "MS";
                    ws.Cells[row, 5].Value = "Không quét vào";
                    ws.Cells[row, 5, row, 6].Merge = true;
                    ws.Cells[row, 7].Value = "B";
                    //
                    row++;
                    ws.Cells[row, 1].Value = "Nghỉ huấn luyện";
                    ws.Cells[row, 1, row, 2].Merge = true;
                    ws.Cells[row, 3].Value = "TL";
                    ws.Cells[row, 5].Value = "Không quét ra";
                    ws.Cells[row, 5, row, 6].Merge = true;
                    ws.Cells[row, 7].Value = "A";
                }

                MemoryStream ms = new MemoryStream();

                package.SaveAs(ms);
                filestream.Close();
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = "BaoCaoChamCong" + "-tu " + stime + "den " + etime + ".xlsx";
                return this.File(ms, contentType, fileName);
            }
        }
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
    }
}