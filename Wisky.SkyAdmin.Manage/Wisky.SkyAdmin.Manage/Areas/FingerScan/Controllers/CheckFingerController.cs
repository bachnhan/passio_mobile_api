using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan.Controllers
{
    public class CheckFingerController : Controller
    {
        // GET: FingerScan/CheckFinger
        public ActionResult Index()
        {
            return View();

        }
        public ActionResult GetAllCheckFinger(int storeId, String startTime, String endTime, int? empId, int? timeFrameId)
        {
            var startDate = startTime.ToDateTime();
            var endDate = endTime.ToDateTime();
            startDate = startDate.GetStartOfDate();
            endDate = endDate.GetEndOfDate();
            var checkFingerApi = new CheckFingerApi();
            var employeeApi = new EmployeeApi();
            var fingerMachineApi = new FingerScanMachineApi();
            var timeFrameApi = new TimeFrameApi();
            var count = 1;

            if (empId == -1 && timeFrameId == -1)
            {
                var checkFinger = checkFingerApi.GetActive().ToList().OrderByDescending(q => q.DateTime)
                    .Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate)
                    .Select(a => new IConvertible[]{
                        count++,
                        fingerMachineApi.Get(a.FingerScanMachineId).MachineCode,
                        fingerMachineApi.Get(a.FingerScanMachineId).Name,
                        employeeApi.Get(a.EmployeeId).Name,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm")
                    }).ToList();
                return Json(checkFinger);
            }
            else if (empId > -1 && timeFrameId == -1)
            {

                var checkFinger2 = checkFingerApi.GetActive().ToList().OrderByDescending(q => q.DateTime)
                    .Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate && a.EmployeeId == empId)
                    .Select(a => new IConvertible[]{
                        count++,
                        fingerMachineApi.Get(a.FingerScanMachineId).MachineCode,
                        fingerMachineApi.Get(a.FingerScanMachineId).Name,
                        employeeApi.Get(a.EmployeeId).Name,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm")
                    }).ToList();
                
                return Json(checkFinger2);
            }
            else if (empId == -1 && timeFrameId > -1)
            {
                var timeFrameTimeStart = timeFrameApi.Get(timeFrameId).StartTime;
                var timeFrameTimeEnd = timeFrameTimeStart + timeFrameApi.Get(timeFrameId).Duration;
                var checkFinger2 = checkFingerApi.GetActive().ToList().OrderByDescending(q => q.DateTime)
                    .Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate && a.DateTime.TimeOfDay >= timeFrameTimeStart && a.DateTime.TimeOfDay <= timeFrameTimeEnd)
                    .Select(a => new IConvertible[]{
                        count++,
                        fingerMachineApi.Get(a.FingerScanMachineId).MachineCode,
                        fingerMachineApi.Get(a.FingerScanMachineId).Name,
                        employeeApi.Get(a.EmployeeId).Name,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm")
                    }).ToList();
                return Json(checkFinger2);
            }
            else 
            {
                var timeFrameTimeStart = timeFrameApi.Get(timeFrameId).StartTime;
                var timeFrameTimeEnd = timeFrameTimeStart + timeFrameApi.Get(timeFrameId).Duration;
                var checkFinger2 = checkFingerApi.GetActive().ToList().OrderByDescending(q => q.DateTime)
                    .Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate && a.EmployeeId == empId && a.DateTime.TimeOfDay >= timeFrameTimeStart && a.DateTime.TimeOfDay <= timeFrameTimeEnd)
                    .Select(a => new IConvertible[]{
                        count++,
                        fingerMachineApi.Get(a.FingerScanMachineId).MachineCode,
                        fingerMachineApi.Get(a.FingerScanMachineId).Name,
                        employeeApi.Get(a.EmployeeId).Name,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm")
                    }).ToList();
              
                return Json(checkFinger2);
            }


        }
        public ActionResult GetAllCheckFingerServerSide(JQueryDataTableParamModel param, int storeId, String startTime, String endTime, int? empId, int? timeFrameId)
        {
            var startDate = startTime.ToDateTime();
            var endDate = endTime.ToDateTime();
            startDate = startDate.GetStartOfDate();
            endDate = endDate.GetEndOfDate();

            var checkFingerApi = new CheckFingerApi();
            var employeeApi = new EmployeeApi();
            var fingerMachineApi = new FingerScanMachineApi();
            var timeFrameApi = new TimeFrameApi();
            var count = 1;

            if (empId == -1 && timeFrameId == -1)
            {
                int count1 = 0;
                count1 = param.iDisplayStart + 1;
                var checkFinger = checkFingerApi.GetActive().Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate).OrderByDescending(q => q.DateTime).ToList();
                var result= checkFinger.Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .Select(a => new IConvertible[]{
                        count1++,
                        employeeApi.Get(a.EmployeeId).Name,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm"),
                        fingerMachineApi.Get(a.FingerScanMachineId).Name
                    }).ToList();
                var totalRecords = checkFinger.Count;
               
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }
            else if (empId > -1 && timeFrameId == -1)
            {
                int count2 = 0;
                count2 = param.iDisplayStart + 1;
                var checkFinger2 = checkFingerApi.GetActive().ToList()
                    .Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate &&
                                a.EmployeeId == empId)
                    .OrderByDescending(q => q.DateTime)
                    .ToList();
                var result = checkFinger2.Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .Select(a => new IConvertible[]{
                        count2++,
                        employeeApi.Get(a.EmployeeId).Name,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm"),
                        fingerMachineApi.Get(a.FingerScanMachineId).Name
                    }).ToList();
                var totalRecords = checkFinger2.Count;
                
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }
            else if (empId == -1 && timeFrameId > -1)
            {
                int count3 = 0;
                count3 = param.iDisplayStart + 1;
                var timeFrameTimeStart = timeFrameApi.Get(timeFrameId).StartTime;
                var timeFrameTimeEnd = timeFrameTimeStart + timeFrameApi.Get(timeFrameId).Duration;
                var checkFinger2 = checkFingerApi.GetActive().ToList()
                    .Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate &&
                                a.DateTime.TimeOfDay >= timeFrameTimeStart && a.DateTime.TimeOfDay <= timeFrameTimeEnd)
                    .OrderByDescending(q => q.DateTime)
                    .ToList();
                var result = checkFinger2.Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .Select(a => new IConvertible[]{
                        count3++,
                        employeeApi.Get(a.EmployeeId).Name,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm"),
                        fingerMachineApi.Get(a.FingerScanMachineId).Name
                    }).ToList();
                var totalRecords = checkFinger2.Count;
                

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                int count4 = 0;
                count4 = param.iDisplayStart + 1;
                var timeFrameTimeStart = timeFrameApi.Get(timeFrameId).StartTime;
                var timeFrameTimeEnd = timeFrameTimeStart + timeFrameApi.Get(timeFrameId).Duration;
                var checkFinger2 = checkFingerApi.GetActive().ToList()
                    .Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate &&
                                a.EmployeeId == empId && a.DateTime.TimeOfDay >= timeFrameTimeStart &&
                                a.DateTime.TimeOfDay <= timeFrameTimeEnd)
                    .OrderByDescending(q => q.DateTime)
                    .ToList();
                var result = checkFinger2.Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .Select(a => new IConvertible[]{
                        count4++,
                        employeeApi.Get(a.EmployeeId).Name,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm"),
                        fingerMachineApi.Get(a.FingerScanMachineId).Name
                    }).ToList();
                var totalRecords = checkFinger2.Count;
               
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
            }


        }
        public ActionResult GetEmployeeInStore(int storeId)
        {
            var empApi = new EmployeeApi();
            var list = empApi.GetActive().Where(q => q.MainStoreId == storeId).ToList();
            return Json(list);
        }


        public ActionResult ExportToExcelReport(int? empId, String startTime, String endTime, int storeId, int? timeFrameId)
        {
            var startDate = startTime.ToDateTime();
            var endDate = endTime.ToDateTime();
            startDate = startDate.GetStartOfDate();
            endDate = endDate.GetEndOfDate();

            var checkFingerApi = new CheckFingerApi();
            var employeeApi = new EmployeeApi();
            var fingerMachineApi = new FingerScanMachineApi();
            var timeFrameApi = new TimeFrameApi();
            var storeApi = new StoreApi();
            var nameStore = storeApi.Get(storeId).ShortName;
            var count = 1;
            String title = "";
            var sdate = startDate.ToString("dd-MM-yyyy");
            var edate = endDate.ToString("dd-MM-yyyy");
            if (sdate == edate)
            {
                title = "Thông tin điểm danh ngày " + sdate + " - Cửa hàng " + nameStore;
            }
            else
            {
                title = "Thông tin điểm danh từ ngày " + sdate + " đến ngày  " + edate + " - Cửa hàng " + nameStore;
            }

            if (empId == -1 && timeFrameId == -1)
            {
              
                var checkFinger = checkFingerApi.GetActive().Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate).OrderByDescending(q => q.DateTime).ToList();
                var result = checkFinger
                    .Select(a => new IConvertible[]{
                        count++,
                        employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm"),
                        fingerMachineApi.Get(a.FingerScanMachineId).Name
                    }).ToList();


                return ExportReportToExcelFinger(result, title);
            }
            else if (empId > -1 && timeFrameId == -1)
            {
                var checkFinger2 = checkFingerApi.GetActive().ToList()
                    .Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate &&
                                a.EmployeeId == empId)
                    .OrderByDescending(q => q.DateTime)
                    .ToList();
                var result = checkFinger2
                     .Select(a => new IConvertible[]{
                        count++,
                        employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm"),
                        fingerMachineApi.Get(a.FingerScanMachineId).Name
                     }).ToList();


                return ExportReportToExcelFinger(result, title);
            }
            else if (empId == -1 && timeFrameId > -1)
            {
                
                var timeFrameTimeStart = timeFrameApi.Get(timeFrameId).StartTime;
                var timeFrameTimeEnd = timeFrameTimeStart + timeFrameApi.Get(timeFrameId).Duration;
                var checkFinger2 = checkFingerApi.GetActive().ToList()
                    .Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate &&
                                a.DateTime.TimeOfDay >= timeFrameTimeStart && a.DateTime.TimeOfDay <= timeFrameTimeEnd)
                    .OrderByDescending(q => q.DateTime)
                    .ToList();
                var result = checkFinger2
                    .Select(a => new IConvertible[]{
                        count++,
                        employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm"),
                        fingerMachineApi.Get(a.FingerScanMachineId).Name
                    }).ToList();


                return ExportReportToExcelFinger(result, title);
            }
            else
            {
                
                var timeFrameTimeStart = timeFrameApi.Get(timeFrameId).StartTime;
                var timeFrameTimeEnd = timeFrameTimeStart + timeFrameApi.Get(timeFrameId).Duration;
                var checkFinger2 = checkFingerApi.GetActive().ToList()
                    .Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate &&
                                a.EmployeeId == empId && a.DateTime.TimeOfDay >= timeFrameTimeStart &&
                                a.DateTime.TimeOfDay <= timeFrameTimeEnd)
                    .OrderByDescending(q => q.DateTime)
                    .ToList();
                var result = checkFinger2
                    .Select(a => new IConvertible[]{
                        count++,
                        employeeApi.Get(a.EmployeeId).Name,
                        a.EmpEnrollNumber,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm"),
                        fingerMachineApi.Get(a.FingerScanMachineId).Name
                    }).ToList();
                

                return ExportReportToExcelFinger(result, title);
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
                ws.Cells["E1:E2"].Value = "Máy scan";
                ws.Cells["E1:E2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["E1:E2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["E1:E2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["E1:E2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

               

                
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

        //public ActionResult CheckFinger(string fingerCode, string scanMachineCode)
        //{
        //    bool result = false;
        //    bool result2 = false;
        //    //tao checkFinger
        //    var empApi = new EmployeeApi();
        //    var emp = empApi.GetActive().Where(q => q.EmpEnrollNumber == fingerCode).ToList();
        //    var fingerScanMachineApi = new FingerScanMachineApi();
        //    var fingerScanMachine = fingerScanMachineApi.GetActive().Where(q => q.MachineCode == scanMachineCode).ToList();
        //    var checkFingerApi = new CheckFingerApi();
        //    var dateNow = DateTime.Now;
        //    try
        //    {
        //        checkFingerApi.Create(new CheckFingerViewModel()
        //        {
        //            EmployeeId = emp[0].Id,
        //            Active = true,
        //            DateTime = dateNow,
        //            FingerScanMachineId = fingerScanMachine[0].Id,
        //            StoreId = fingerScanMachine[0].StoreId
        //        });
        //        result = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result = false;
        //    }
        //    //update attendance
        //    try
        //    {
        //        var attApi = new AttendanceApi();
        //        var att = attApi.GetActive().Where(q => q.EmployeeId == emp[0].Id
        //                                                && q.ShiftId == shift[0].Id).ToList();
        //        if (att[0].CheckMin == null)
        //        {
        //            att[0].CheckMin = dateNow;
        //        }
        //        else att[0].CheckMax = dateNow;
        //        attApi.Edit(att[0].Id,att[0]);
        //        result2 = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result2 = false;
        //    }

        //    return Json(result.ToString()+", "+result2.ToString(),JsonRequestBehavior.AllowGet);
        //}
    }
}