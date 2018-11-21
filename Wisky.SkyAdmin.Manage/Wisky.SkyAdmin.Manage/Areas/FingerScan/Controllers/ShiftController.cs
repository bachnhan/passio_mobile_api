using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using System.Globalization;
using OfficeOpenXml.Style;
using System.IO;
using OfficeOpenXml;
using System.Drawing;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan.Controllers
{
    public class ShiftController : Controller
    {
        // GET: FingerScan/Shift
        public ActionResult Index()
        {
            return View();
            
        }
        public ActionResult AddShiftFromBrand()
        {
            return View();
        }
        public ActionResult ViewCalendar()
        {
            return View();
        }

        //public JsonResult GetShift(int storeId)
        //{
        //    int count = 1;
        //    var api = new ShiftApi();
        //    var timeFramApi = new TimeFrameApi();
        //    var datatable = api.GetActive().Where(q => q.StoreId == storeId).ToList().Select(q => new IConvertible[] {
        //        count++,
        //        timeFramApi.Get(q.TimeFrameId).Name,
        //        q.StartTime.Value.ToString(),
        //        timeFramApi.Get(q.TimeFrameId).Duration.ToString(),
        //        q.EndTime.Value.ToString(),
        //        q.Id
        //    });
        //    return Json(new { datatable }, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult GetDateShift(int storeId, String startTime, String endTime, String checkstt)
        //{
        //    var startDate = startTime.ToDateTime();
        //    var endDate = endTime.ToDateTime();
        //    var api = new ShiftApi();
        //    var timeFramApi = new TimeFrameApi();
        //    var count = 1;
        //    var today = DateTime.Now.Date;
        //    var now = DateTime.Now.Date;
        //    var checkFinger = api.GetActive()
        //        .Where(a => a.StoreId == storeId && a.StartTime.Value.Date >= startDate && a.StartTime.Value.Date <= endDate).ToList();
        //    switch (checkstt)
        //    {
        //        case "Past":
        //            checkFinger = api.GetActive().ToList().Where(c => c.StoreId == storeId && c.StartTime.Value.Date < today).ToList();
        //            break;
        //        case "Now":
        //            checkFinger = api.GetActive().ToList().Where(c => c.StoreId == storeId && c.StartTime.Value.Date == today).ToList();
        //            break;
        //        case "Future":
        //            checkFinger = api.GetActive().ToList().Where(c => c.StoreId == storeId && c.StartTime.Value.Date > today).ToList();
        //            break;
        //        case null:
        //        default:
        //            break;
        //    }
        //        var result=checkFinger.OrderByDescending(q => q.StartTime.Value.Date).ThenByDescending(q => q.StartTime)
        //        .Select(a => new IConvertible[]{
        //            count++,
        //            timeFramApi.Get(a.TimeFrameId).Name,
        //            a.StartTime.GetValueOrDefault().ToString("dd/MM/yyyy HH:mm"),
        //            //timeFramApi.Get(a.TimeFrameId).Duration.ToString(),
        //            (a.EndTime.GetValueOrDefault() - a.StartTime.GetValueOrDefault()).ToString(),
        //            a.EndTime.GetValueOrDefault().ToString("dd/MM/yyyy HH:mm"),
        //          a.StartTime.GetValueOrDefault().ToString("MM/dd/yyyyy HH:mm"),
        //          a.Id,
        //          a.EndTime.GetValueOrDefault().ToString("MM//dd/yyyy HH:mm"),


        //    });
        //    return Json(result);
        //}
        //public ActionResult AddNewShift()
        //{
        //    return View();
        //}

        //public ActionResult CreateShift(int storeId, int timeFrameId, String strWorkDayStart, String strWorkDayEnd, List<int> listId, TimeSpan start, TimeSpan duration, int expandTime)
        //{
        //    bool result = false;
        //    var api = new ShiftApi();
        //    var timeFramApi = new TimeFrameApi();
        //    var model = new ShiftViewModel();
        //    var workDayStart = Utils.ToDateTime(strWorkDayStart);
        //    var workDayEnd = Utils.ToDateTime(strWorkDayEnd);
        //    var timeFrame = timeFramApi.Get(timeFrameId);
           
        //    try
        //    {
        //        for (DateTime i = workDayStart; i <= workDayEnd; i = i.AddDays(1))
        //        {
        //            var checkDuplicate = api.GetActive().Where(q => q.TimeFrameId == timeFrame.Id && q.StartTime.GetValueOrDefault().Date == i.Date).ToList();
        //            if (checkDuplicate.Count > 0)
        //            {
        //                checkDuplicate[0].Active = false;
        //                api.Edit(checkDuplicate[0].Id, checkDuplicate[0]);
        //            }


        //            model.StoreId = storeId;
        //            model.TimeFrameId = timeFrameId;
        //            //model.Workday = i;
        //            //model.StartTime = timeFramApi.Get(timeFrameId).StartTime;
        //            //model.StartTime = i.GetStartOfDate().Add(timeFrame.StartTime);
        //            model.StartTime = i.GetStartOfDate().Add(start);
        //            model.EndTime = model.StartTime.Value.Add(duration);
        //            model.ExpandTime = expandTime;
        //            model.Active = true;
        //            api.Create(model);
        //            var attendance = new AttendanceController();
        //            foreach (var empId in listId)
        //            {
        //                attendance.CreateAttendance(empId, model.Id);
        //            }

        //        }

        //        result = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result = false;
        //    }
        //    return Json(result);
        //}
        //public ActionResult CreateShiftForStores(int brandId, int timeFrameId, String strWorkDayStart, String strWorkDayEnd, List<int> listId)
        //{
        //    bool result = false;
        //    var api = new ShiftApi();
        //    var timeFramApi = new TimeFrameApi();
        //    var model = new ShiftViewModel();
        //    var workDayStart = Utils.ToDateTime(strWorkDayStart);
        //    var workDayEnd = Utils.ToDateTime(strWorkDayEnd);
        //    var timeFrame = timeFramApi.Get(timeFrameId);
        //    try
        //    {
        //        for (DateTime i = workDayStart; i <= workDayEnd; i = i.AddDays(1))
        //        {
        //            foreach (var storeId in listId)
        //            {
        //                var checkDuplicate = api.GetActive().Where(q =>
        //                    q.TimeFrameId == timeFrame.Id
        //                    && q.StartTime.Value.Date == i.Date
        //                    && q.StoreId == storeId).ToList();
        //                if (checkDuplicate.Count > 0)
        //                {
        //                    checkDuplicate[0].Active = false;
        //                    api.Edit(checkDuplicate[0].Id, checkDuplicate[0]);
        //                }
        //                model.StoreId = storeId;
        //                model.TimeFrameId = timeFrameId;
        //                //model.Workday = i;
        //                //model.StartTime = timeFramApi.Get(timeFrameId).StartTime;
        //                model.StartTime = i.GetStartOfDate().Add(timeFrame.StartTime);
        //                model.EndTime = model.StartTime.Value.Add(timeFrame.Duration);
        //                model.Active = true;
        //                api.Create(model);
        //            }
                    
        //        }

        //        result = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result = false;
        //    }
        //    return Json(result);
        //}
        //public ActionResult prepareEdit(int ShiftId)
        //{
        //    var api = new ShiftApi();
        //    var model = api.Get(ShiftId);
        //    var timeFramApi = new TimeFrameApi();
        //    var nameshift = new TimeFrameApi();

        //    var tmpModel = new
        //    {
        //        id = model.Id,
        //        timeFrameId = model.TimeFrameId,
        //        nameshift = timeFramApi.Get(model.TimeFrameId).Name,
        //        workday = model.StartTime.Value.Date.ToShortDateString(),
        //        startTime = model.StartTime.GetValueOrDefault().ToString("dd/MM/yyyy HH:mm"),
        //        duration = (model.EndTime.GetValueOrDefault() - model.StartTime.GetValueOrDefault()).ToString(@"hh\:mm"),
        //    };
        //    Boolean stt;
        //    if (api.Get(ShiftId).StartTime.Value.Date > DateTime.Now.Date)
        //    {
        //        stt = true;

        //    }
        //    else
        //    {
        //        stt = false;
        //    }
        //    return Json(new { result = tmpModel, stt = stt }, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult EditShift(int storeId, int id, int timeFrameId, string starttime, string duration)
        //{

        //    int result = 0;
        //    var api = new ShiftApi();
        //    var timeFramApi = new TimeFrameApi();
        //    DateTime Datestart = Utils.ToDateTimeHour(starttime);
        //    TimeSpan durationtime = DateTime.ParseExact(duration, "HH:mm", CultureInfo.InvariantCulture).TimeOfDay;
        //    DateTime dateend = Datestart.Add(durationtime);
        //    DateTime workDay = Datestart.Date;


        //    var checkDuplicate = api.GetActive().Where(q => q.TimeFrameId == timeFrameId && q.StartTime.Value.Date == workDay.Date && q.StartTime.Value.TimeOfDay == Datestart.TimeOfDay && q.EndTime.Value.TimeOfDay == dateend.TimeOfDay).ToList();
        //    if (checkDuplicate.Count > 0)

        //    {
        //        result = 2;
        //        return Json(result);
        //    }

        //    var model = new ShiftViewModel()
        //    {
        //        Id = id,
        //        TimeFrameId = timeFrameId,
        //        StoreId = storeId,
        //        StartTime = Datestart,

        //        EndTime = dateend,
        //        Active = true
        //    };
        //    try
        //    {
        //        api.Edit(id, model);
        //        var attedanceApi = new AttendanceApi();
        //        var attList = attedanceApi.GetActive().Where(q => q.ShiftId == id).ToList();
        //        foreach (var item in attList)
        //        {
        //            item.CheckMin = null;
        //            item.CheckMax = null;
        //            attedanceApi.Edit(item.Id, item);
        //        }

        //        result = 1;
        //    }
        //    catch (Exception e)
        //    {
        //        result = 0;
        //    }
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult DeleteShift(int ShiftId)
        //{
        //    var result = true;
        //    var api = new ShiftApi();
        //    var attapi = new AttendanceApi();
        //    var entity = api.Get(ShiftId);
        //    var atten = attapi.GetActive().Where(q => q.ShiftId == ShiftId).ToList();

        //    if (entity == null)
        //    {
        //        result = false;

        //    }
        //    try
        //    {
        //        entity.Active = false;
        //        api.Edit(ShiftId, entity);
        //        foreach (var item in atten)
        //        {
        //            item.Active = false;
        //            attapi.Edit(item.Id, item);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result = false;
        //    }
        //    return Json(result);
        //}

        //public ActionResult GetFutureShift(int storeId, String startTime, String endTime)
        //{
        //    var startDate = startTime.ToDateTime();
        //    var endDate = endTime.ToDateTime();
        //    startDate = startDate.GetStartOfDate();
        //    endDate = endDate.GetEndOfDate();


        //    int count = 1;
        //    var api = new ShiftApi();
        //    var timeFramApi = new TimeFrameApi();
        //    var now = DateTime.Now;
        //    var datatable = api.GetActive().Where(q => q.StoreId == storeId &&q.StartTime.Value.Date >= startDate && q.EndTime.Value.Date <= endDate).ToList().Select(
        //        q => new IConvertible[]
        //        {
        //            count++,
        //            timeFramApi.Get(q.TimeFrameId).Name,
        //            q.StartTime.Value.ToString(),
        //            timeFramApi.Get(q.TimeFrameId).Duration.ToString(),
        //            q.Id
        //        });
        //    return Json(new { datatable }, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult GetTimeFrameInfo(int id)
        {
            var timeFrameApi = new TimeFrameApi();
            var timeFrame = timeFrameApi.GetActive().Where(q => q.Id == id).ToList();
            var model = new
            {
                startTime = timeFrame[0].StartTime.ToString(),
                duration = timeFrame[0].Duration.ToString(),
                //expandTime = timeFrame[0].ExpandTime,
            };
            return Json(model);
        }

        //public ActionResult GetShiftInFo(int shiftId)
        //{
        //    var api = new ShiftApi();
        //    var model = api.Get(shiftId);
        //    var tempModel = new
        //    {
        //        startTime = model.StartTime == null ? "N/a" : model.StartTime.Value.ToString("HH:mm dd/MM/yyyy"),
        //        endTime = model.EndTime == null ? "N/a" : model.EndTime.Value.ToString("HH:mm dd/MM/yyyy"),
        //    };
        //    return Json(tempModel);
        //}

        //public ActionResult ExportToExcel(int shiftId)
        //{
        //    #region getdata
        //    int count = 1;
        //    var api = new AttendanceApi();
        //    var employeeApi = new EmployeeApi();
        //    var shiftApi = new ShiftApi();
        //    var storeapi = new StoreApi();
        //    var shift = shiftApi.Get(shiftId);
            
        //    var datatable = api.GetActive().Where(q => q.ShiftId == shiftId).ToList();
           
        //    var shiftStartTime = shift.StartTime;
        //    var shiftEndTime = shift.EndTime;
        //    var result = datatable.Select(q => new IConvertible[]
        //    {
        //        count++,
        //        employeeApi.Get(q.EmployeeId).Name,
        //         storeapi.GetStoreNameByID( employeeApi.Get(q.EmployeeId).MainStoreId),
        //        q.CheckMin == null ? "Chưa cập nhật" : q.CheckMin.Value.ToString("dd/MM/yyyy HH:mm"),
        //        q.CheckMax == null ? "Chưa cập nhật" : q.CheckMax.Value.ToString("dd/MM/yyyy HH:mm"),
        //        q.TotalWorkTime.ToString(),
        //        q.Id,
        //        (q.CheckMax==null||q.CheckMin==null)?"chưa check đủ":"check đủ",
        //        (q.CheckMin-shiftStartTime>=new TimeSpan(0,0,30,0))? "Đi trễ":"Chuẩn",
        //        (shiftEndTime-q.CheckMax>=new TimeSpan(0,0,30,0))? "Về sớm":"Chuẩn"
        //    }).ToList();
        //    #endregion
        //    return ExportReportToExcel(result);

        //}
        //public ActionResult ExportReportToExcel(List<IConvertible[]> data)
        //{
        //    #region Export to Excel
        //    MemoryStream ms = new MemoryStream();
        //    using (ExcelPackage package = new ExcelPackage(ms))
        //    {
        //        ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo");
        //        #region header
        //        ws.Cells["A1:A2"].Merge = true;
        //        ws.Cells["A1:A2"].Value = "STT";
        //        ws.Cells["A1:A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        ws.Cells["A1:A2"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
        //        ws.Cells["A1:A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

        //        ws.Cells["B1:B2"].Merge = true;
        //        ws.Cells["B1:B2"].Value = "Họ và Tên";
        //        ws.Cells["B1:B2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        ws.Cells["B1:B2"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
        //        ws.Cells["B1:B2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

        //        ws.Cells["C1:C2"].Merge = true;
        //        ws.Cells["C1:C2"].Value = "Tên Cửa Hàng";
        //        ws.Cells["C1:C2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        ws.Cells["C1:C2"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
        //        ws.Cells["C1:C2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

               
        //        ws.Cells["D1:D2"].Merge = true;
        //        ws.Cells["D1:D2"].Value = "Giờ vào";
        //        ws.Cells["D1:D2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        ws.Cells["D1:D2"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
        //        ws.Cells["D1:D2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

        //        ws.Cells["E1:E2"].Merge = true;
        //        ws.Cells["E1:E2"].Value = "Giờ ra";
        //        ws.Cells["E1:E2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        ws.Cells["E1:E2"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
        //        ws.Cells["E1:E2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

        //        ws.Cells["F1:F2"].Merge = true;
        //        ws.Cells["F1:F2"].Value = "Trạng Thái";
        //        ws.Cells["F1:F2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        ws.Cells["F1:F2"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
        //        ws.Cells["F1:F2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

        //        ws.Cells["G1:G2"].Merge = true;
        //        ws.Cells["G1:G2"].Value = "Số Giờ Làm";
        //        ws.Cells["G1:G2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        ws.Cells["G1:G2"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
        //        ws.Cells["G1:G2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


        //        #endregion
        //        //Set style for excel

        //        #region set value for cells

        //        int indexRow = 3;
        //        foreach (var item in data)
        //        {
        //            int indexCol = 1;
        //            ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[0];
        //            ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[1];
        //            ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[2];
        //            ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[3];
        //            ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[4];
        //            //trang thai
        //            var check = item[7];
        //            var vao = item[8];
        //            var ra = item[9];
        //            var chuan = "Đúng giờ";
        //            var tre = "Đi trễ";
        //            var som = "Về sớm";
        //            var strTrangThai = "";
        //            if (check == "chưa check đủ")
        //            {
        //                strTrangThai = "--";
        //            }
        //            else
        //            {
        //                if (vao != "Chuẩn")
        //                    if (strTrangThai == "") strTrangThai = strTrangThai + tre;
        //                    else strTrangThai = strTrangThai + ", " + tre;
        //                if (ra != "Chuẩn")
        //                    if (strTrangThai == "") strTrangThai = strTrangThai + som;
        //                    else strTrangThai = strTrangThai + ", " + som;
        //            }
        //            if (strTrangThai == "") strTrangThai = chuan;

        //            ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = strTrangThai;
        //            //
        //            ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[5];
        //            indexRow = indexRow + 1;
        //        }
        //        #endregion
        //        #region style
        //        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        //        ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        //        ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        //        ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        //        ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

        //        ws.Cells[ws.Dimension.Address].Style.WrapText = true;
        //        string fileName = "BaoCaoChamCong" + ".xlsx";
        //        #endregion
        //        package.SaveAs(ms);
        //        ms.Seek(0, SeekOrigin.Begin);
        //        var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        return this.File(ms, contentType, fileName);
        //    }
        //    #endregion
        //}

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