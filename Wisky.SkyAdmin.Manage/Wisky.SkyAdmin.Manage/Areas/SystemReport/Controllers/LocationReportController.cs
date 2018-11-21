using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Windows.Forms;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using Wisky.SkyAdmin.Manage.Controllers;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Web;
using SkyWeb.DatVM.Mvc;
using System.Data.Entity;
using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities.Services;

namespace Wisky.SkyAdmin.Manage.Areas.SystemReport.Controllers
{
    public class LocationReportController : Controller
    {
        // GET: SystemReport/LocationReport
        public ActionResult Index()
        {
            return View();
        }

        //Thong ke Theo Khu vuc dua Tinh/Thanh pho
        public ActionResult ExportLocationTableToExcelFollowTempForBrand(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int provinceID, int level, int districtID)
        {
            MemoryStream ms = new MemoryStream();
            var brandAPI = new BrandApi();
            var brandName = brandAPI.Get(brandId).BrandName;
            var provinceApi = new ProvinceApi();
            var provinceName = provinceApi.Get().Where(q => q.ProvinceCode == provinceID).Select(r => r.ProvinceName).FirstOrDefault();
            var areaName = provinceName;
            string fileName = "BáoCáoTheoTheoKhuVực_" + brandName + "_";
            if (level == 1)
            {
                var dic = new Dictionary<string, List<LocaltionReportFollowTempModel>>();
                var districtApi = new DistrictApi();
                var districts = districtApi.GetAllDistrictByProvinceID(provinceID).OrderBy(r => r.DistrictName);
                //Get data
                foreach (var district in districts)
                {
                    List<LocaltionReportFollowTempModel> listExcel = new List<LocaltionReportFollowTempModel>();
                    for (int i = 6; i < 23; i++)
                    {
                        listExcel.Add(new LocaltionReportFollowTempModel()
                        {
                            StartTime = i,
                            EndTime = (i + 1)

                        });
                    }

                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();

                    TimeSpan spanTime = endDate - startDate;
                    IEnumerable<Order> rents;
                    var orderAPI = new OrderApi();
                    rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                            .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish && a.DistrictCode == district.DistrictCode);

                    var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                    {
                        OrderType = r.Key.OrderType,
                        OrderTime = r.Key.Time,
                        TotalOrder = r.Count(),
                        Money = r.Sum(a => a.FinalAmount),
                        Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail),
                    }).ToList();

                    foreach (var item in listExcel)
                    {
                        var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                        item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                        item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                        var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                        item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                        item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                        var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                        item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                        item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                        var atStoreDiscount = result.Where(r => r.OrderType == 4 && r.OrderTime == item.StartTime);
                        var takeAwayDiscount = result.Where(r => r.OrderType == 5 && r.OrderTime == item.StartTime);
                        var deliveryDiscount = result.Where(r => r.OrderType == 6 && r.OrderTime == item.StartTime);

                        item.AtStoreDiscount = atStoreDiscount.Sum(a => a.Discount);
                        item.TakeAwayDiscount = takeAwayDiscount.Sum(a => a.Discount);
                        item.DeliveryDiscount = deliveryDiscount.Sum(a => a.Discount);

                        item.TotalAtStoreQuantity = item.AtStore;
                        item.TotalTakeAwayQuantity = item.TakeAway;
                        item.TotalDeliveryQuantity = item.Delivery;

                        item.FinalAtStorePrice = item.PriceAtStore;
                        item.FinalTakeAwayPrice = item.PriceTakeAway;
                        item.FinalDeliveryPrice = item.PriceDelivery;

                        item.TotalAtStorePrice = item.AtStoreDiscount + item.FinalAtStorePrice;
                        item.TotalTakeAwayPrice = item.TakeAwayDiscount + item.FinalTakeAwayPrice;
                        item.TotalDeliveryPrice = item.DeliveryDiscount + item.PriceDelivery;
                    }
                    dic.Add(district.DistrictName, listExcel);
                }


                //ExportToExcel
                string filepath = HttpContext.Server.MapPath(@"/Resource/LocationReportFollowTemp.xlsx");
                var filestream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                using (ExcelPackage package = new ExcelPackage(filestream))
                {
                    package.Workbook.Worksheets["Total"].Cells["C3"].Value = dateRange;
                    package.Workbook.Worksheets["Total"].Cells["J3"].Value = areaName;
                    for (int i = 1; i <= 3; i++)
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets[i];
                        char StartHeaderChar = 'A';
                        int StartHeaderNumber = 6;
                        int stt = 1;
                        //Set values for cells     
                        ws.Cells["C3"].Value = dateRange;
                        ws.Cells["J3"].Value = areaName;
                        foreach (var item in dic)
                        {
                            StartHeaderChar = 'A';
                            StartHeaderNumber++;
                            ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = stt++;
                            ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = item.Key;
                            List<LocaltionReportFollowTempModel> listExcel = item.Value;
                            if (i == 1)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalDeliveryPrice);
                                }
                            }
                            else if (i == 2)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalTakeAwayPrice);
                                }
                            }
                            else if (i == 3)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalAtStorePrice);
                                }
                            }
                        }
                        char EndHeaderChar = ++StartHeaderChar;
                        int EndHeaderNumber = StartHeaderNumber;
                        StartHeaderChar = 'A';
                        StartHeaderNumber = 7;

                        //Set style for rows and columns
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                            .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                            .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SandyBrown);
                        for (char j = 'C'; j < EndHeaderChar; j++)
                        {
                            for (int k = 7; k <= EndHeaderNumber; k++)
                            {
                                ws.Cells["" + (j) + (k)].Value = Convert.ToDecimal(ws.Cells["" + (j) + (k)].Value);
                            }
                        }
                        for (int j = 3; j < 20; j++)
                        {
                            ws.Column(j).Width = 15;
                        }
                        ws.Column(20).Width = 18;
                        //ws.View.FreezePanes(4, 7);
                        ws.Cells["A1:" + StartHeaderChar + StartHeaderNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        //Set style for excel
                        //ws.Cells[ws.Dimension.Address].AutoFitColumns();
                        ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    fileName += areaName + "_" + dateRange + ".xlsx";

                    package.SaveAs(ms);
                    filestream.Close();
                    ms.Seek(0, SeekOrigin.Begin);
                }
            }
            else
            {
                var dic = new Dictionary<string, List<LocaltionReportFollowTempModel>>();
                var wardApi = new WardApi();
                var wards = wardApi.BaseService.GetActive().Where(q => q.DistrictCode == districtID).OrderBy(r => r.WardName).ToList();
                //Get data
                foreach (var ward in wards)
                {
                    List<LocaltionReportFollowTempModel> listExcel = new List<LocaltionReportFollowTempModel>();
                    for (int i = 6; i < 23; i++)
                    {
                        listExcel.Add(new LocaltionReportFollowTempModel()
                        {
                            StartTime = i,
                            EndTime = (i + 1)

                        });
                    }

                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();

                    TimeSpan spanTime = endDate - startDate;
                    IEnumerable<Order> rents;
                    var orderAPI = new OrderApi();
                    rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                            .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish && a.WardCode == ward.WardCode);

                    var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                    {
                        OrderType = r.Key.OrderType,
                        OrderTime = r.Key.Time,
                        TotalOrder = r.Count(),
                        Money = r.Sum(a => a.FinalAmount),
                        Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail),
                    }).ToList();

                    foreach (var item in listExcel)
                    {
                        var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                        item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                        item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                        var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                        item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                        item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                        var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                        item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                        item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                        var atStoreDiscount = result.Where(r => r.OrderType == 4 && r.OrderTime == item.StartTime);
                        var takeAwayDiscount = result.Where(r => r.OrderType == 5 && r.OrderTime == item.StartTime);
                        var deliveryDiscount = result.Where(r => r.OrderType == 6 && r.OrderTime == item.StartTime);

                        item.AtStoreDiscount = atStoreDiscount.Sum(a => a.Discount);
                        item.TakeAwayDiscount = takeAwayDiscount.Sum(a => a.Discount);
                        item.DeliveryDiscount = deliveryDiscount.Sum(a => a.Discount);

                        item.TotalAtStoreQuantity = item.AtStore;
                        item.TotalTakeAwayQuantity = item.TakeAway;
                        item.TotalDeliveryQuantity = item.Delivery;

                        item.FinalAtStorePrice = item.PriceAtStore;
                        item.FinalTakeAwayPrice = item.PriceTakeAway;
                        item.FinalDeliveryPrice = item.PriceDelivery;

                        item.TotalAtStorePrice = item.AtStoreDiscount + item.FinalAtStorePrice;
                        item.TotalTakeAwayPrice = item.TakeAwayDiscount + item.FinalTakeAwayPrice;
                        item.TotalDeliveryPrice = item.DeliveryDiscount + item.PriceDelivery;
                    }
                    dic.Add(ward.WardName, listExcel);
                }


                //ExportToExcel
                string filepath = HttpContext.Server.MapPath(@"/Resource/LocationReportFollowTemp.xlsx");
                var filestream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                var districtApi = new DistrictApi();
                var districtName = districtApi.Get().Where(q => q.DistrictCode == districtID).Select(r => r.DistrictType + " " + r.DistrictName).FirstOrDefault();
                areaName += "_" + districtName;
                using (ExcelPackage package = new ExcelPackage(filestream))
                {
                    package.Workbook.Worksheets["Total"].Cells["C3"].Value = dateRange;
                    package.Workbook.Worksheets["Total"].Cells["J3"].Value = areaName;
                    for (int i = 1; i <= 3; i++)
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets[i];
                        char StartHeaderChar = 'A';
                        int StartHeaderNumber = 6;
                        int stt = 1;
                        //Set values for cells     
                        ws.Cells["C3"].Value = dateRange;
                        ws.Cells["J3"].Value = areaName;
                        foreach (var item in dic)
                        {
                            StartHeaderChar = 'A';
                            StartHeaderNumber++;
                            ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = stt++;
                            ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = item.Key;
                            List<LocaltionReportFollowTempModel> listExcel = item.Value;
                            if (i == 1)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalDeliveryPrice);
                                }
                            }
                            else if (i == 2)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalTakeAwayPrice);
                                }
                            }
                            else if (i == 3)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalAtStorePrice);
                                }
                            }
                        }
                        char EndHeaderChar = ++StartHeaderChar;
                        int EndHeaderNumber = StartHeaderNumber;
                        StartHeaderChar = 'A';
                        StartHeaderNumber = 7;

                        //Set style for rows and columns
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                            .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                            .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SandyBrown);
                        for (char j = 'C'; j < EndHeaderChar; j++)
                        {
                            for (int k = 7; k <= EndHeaderNumber; k++)
                            {
                                ws.Cells["" + (j) + (k)].Value = Convert.ToDecimal(ws.Cells["" + (j) + (k)].Value);
                            }
                        }
                        for (int j = 3; j < 20; j++)
                        {
                            ws.Column(j).Width = 15;
                        }
                        ws.Column(20).Width = 18;
                        //ws.View.FreezePanes(4, 7);
                        ws.Cells["A1:" + StartHeaderChar + StartHeaderNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        //Set style for excel
                        //ws.Cells[ws.Dimension.Address].AutoFitColumns();
                        ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    fileName += areaName + "_" + dateRange + ".xlsx";

                    package.SaveAs(ms);
                    filestream.Close();
                    ms.Seek(0, SeekOrigin.Begin);
                }
            }
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return this.File(ms, contentType, fileName);
        }
        public ActionResult ExportLocationTableToExcelFollowTempForGroup(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int selectedGroupId, int provinceID, int level, int districtID)
        {
            MemoryStream ms = new MemoryStream();
            var brandAPI = new BrandApi();
            var brandName = brandAPI.Get(brandId).BrandName;
            var stroreGroupApi = new StoreGroupApi();
            var storeGroupName = stroreGroupApi.GetStoreGroupByID(selectedGroupId).GroupName;
            var provinceApi = new ProvinceApi();
            var provinceName = provinceApi.Get().Where(q => q.ProvinceCode == provinceID).Select(r => r.ProvinceName).FirstOrDefault();
            var areaName = provinceName;
            string fileName = "BáoCáoTheoTheoKhuVực_" + brandName + "_" + storeGroupName + "_";
            if (level == 1)
            {
                var dic = new Dictionary<string, List<LocaltionReportFollowTempModel>>();
                var districtApi = new DistrictApi();
                var districts = districtApi.GetAllDistrictByProvinceID(provinceID).OrderBy(r => r.DistrictName);
                var storeApi = new StoreApi();
                var storeInGroupIDs = storeApi.GetStoreByGroupId(selectedGroupId).Select(r => r.ID).ToList();
                //Get data
                foreach (var district in districts)
                {
                    List<LocaltionReportFollowTempModel> listExcel = new List<LocaltionReportFollowTempModel>();
                    for (int i = 6; i < 23; i++)
                    {
                        listExcel.Add(new LocaltionReportFollowTempModel()
                        {
                            StartTime = i,
                            EndTime = (i + 1)

                        });
                    }

                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();

                    TimeSpan spanTime = endDate - startDate;
                    IEnumerable<Order> rents;
                    var orderAPI = new OrderApi();
                    rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId).Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish &&
                    q.DistrictCode != null && q.DistrictCode == district.DistrictCode && q.StoreID != null && storeInGroupIDs.Contains((int)q.StoreID));

                    var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                    {
                        OrderType = r.Key.OrderType,
                        OrderTime = r.Key.Time,
                        TotalOrder = r.Count(),
                        Money = r.Sum(a => a.FinalAmount),
                        Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail),
                    }).ToList();

                    foreach (var item in listExcel)
                    {
                        var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                        item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                        item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                        var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                        item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                        item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                        var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                        item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                        item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                        var atStoreDiscount = result.Where(r => r.OrderType == 4 && r.OrderTime == item.StartTime);
                        var takeAwayDiscount = result.Where(r => r.OrderType == 5 && r.OrderTime == item.StartTime);
                        var deliveryDiscount = result.Where(r => r.OrderType == 6 && r.OrderTime == item.StartTime);

                        item.AtStoreDiscount = atStoreDiscount.Sum(a => a.Discount);
                        item.TakeAwayDiscount = takeAwayDiscount.Sum(a => a.Discount);
                        item.DeliveryDiscount = deliveryDiscount.Sum(a => a.Discount);

                        item.TotalAtStoreQuantity = item.AtStore;
                        item.TotalTakeAwayQuantity = item.TakeAway;
                        item.TotalDeliveryQuantity = item.Delivery;

                        item.FinalAtStorePrice = item.PriceAtStore;
                        item.FinalTakeAwayPrice = item.PriceTakeAway;
                        item.FinalDeliveryPrice = item.PriceDelivery;

                        item.TotalAtStorePrice = item.AtStoreDiscount + item.FinalAtStorePrice;
                        item.TotalTakeAwayPrice = item.TakeAwayDiscount + item.FinalTakeAwayPrice;
                        item.TotalDeliveryPrice = item.DeliveryDiscount + item.PriceDelivery;
                    }
                    dic.Add(district.DistrictName, listExcel);
                }


                //ExportToExcel
                string filepath = HttpContext.Server.MapPath(@"/Resource/LocationReportFollowTemp.xlsx");
                var filestream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                using (ExcelPackage package = new ExcelPackage(filestream))
                {
                    package.Workbook.Worksheets["Total"].Cells["C3"].Value = dateRange;
                    package.Workbook.Worksheets["Total"].Cells["J3"].Value = areaName;
                    for (int i = 1; i <= 3; i++)
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets[i];
                        char StartHeaderChar = 'A';
                        int StartHeaderNumber = 6;
                        int stt = 1;
                        //Set values for cells     
                        ws.Cells["C3"].Value = dateRange;
                        ws.Cells["J3"].Value = areaName;
                        foreach (var item in dic)
                        {
                            StartHeaderChar = 'A';
                            StartHeaderNumber++;
                            ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = stt++;
                            ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = item.Key;
                            List<LocaltionReportFollowTempModel> listExcel = item.Value;
                            if (i == 1)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalDeliveryPrice);
                                }
                            }
                            else if (i == 2)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalTakeAwayPrice);
                                }
                            }
                            else if (i == 3)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalAtStorePrice);
                                }
                            }
                        }
                        char EndHeaderChar = ++StartHeaderChar;
                        int EndHeaderNumber = StartHeaderNumber;
                        StartHeaderChar = 'A';
                        StartHeaderNumber = 7;

                        //Set style for rows and columns
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                            .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                            .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SandyBrown);
                        for (char j = 'C'; j < EndHeaderChar; j++)
                        {
                            for (int k = 7; k <= EndHeaderNumber; k++)
                            {
                                ws.Cells["" + (j) + (k)].Value = Convert.ToDecimal(ws.Cells["" + (j) + (k)].Value);
                            }
                        }
                        for (int j = 3; j < 20; j++)
                        {
                            ws.Column(j).Width = 15;
                        }
                        ws.Column(20).Width = 18;
                        //ws.View.FreezePanes(4, 7);
                        ws.Cells["A1:" + StartHeaderChar + StartHeaderNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        //Set style for excel
                        //ws.Cells[ws.Dimension.Address].AutoFitColumns();
                        ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    fileName += areaName + "_" + dateRange + ".xlsx";

                    package.SaveAs(ms);
                    filestream.Close();
                    ms.Seek(0, SeekOrigin.Begin);
                }
            }
            else
            {
                var dic = new Dictionary<string, List<LocaltionReportFollowTempModel>>();
                var wardApi = new WardApi();
                var wards = wardApi.BaseService.GetActive().Where(q => q.DistrictCode == districtID).OrderBy(r => r.WardName).ToList();
                var storeApi = new StoreApi();
                var storeInGroupIDs = storeApi.GetStoreByGroupId(selectedGroupId).Select(r => r.ID).ToList();
                //Get data
                foreach (var ward in wards)
                {
                    List<LocaltionReportFollowTempModel> listExcel = new List<LocaltionReportFollowTempModel>();
                    for (int i = 6; i < 23; i++)
                    {
                        listExcel.Add(new LocaltionReportFollowTempModel()
                        {
                            StartTime = i,
                            EndTime = (i + 1)

                        });
                    }

                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();

                    TimeSpan spanTime = endDate - startDate;
                    IEnumerable<Order> rents;
                    var orderAPI = new OrderApi();
                    rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId).Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish &&
                    q.WardCode != null && q.WardCode == ward.WardCode && q.StoreID != null && storeInGroupIDs.Contains((int)q.StoreID));

                    var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                    {
                        OrderType = r.Key.OrderType,
                        OrderTime = r.Key.Time,
                        TotalOrder = r.Count(),
                        Money = r.Sum(a => a.FinalAmount),
                        Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail),
                    }).ToList();

                    foreach (var item in listExcel)
                    {
                        var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                        item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                        item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                        var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                        item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                        item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                        var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                        item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                        item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                        var atStoreDiscount = result.Where(r => r.OrderType == 4 && r.OrderTime == item.StartTime);
                        var takeAwayDiscount = result.Where(r => r.OrderType == 5 && r.OrderTime == item.StartTime);
                        var deliveryDiscount = result.Where(r => r.OrderType == 6 && r.OrderTime == item.StartTime);

                        item.AtStoreDiscount = atStoreDiscount.Sum(a => a.Discount);
                        item.TakeAwayDiscount = takeAwayDiscount.Sum(a => a.Discount);
                        item.DeliveryDiscount = deliveryDiscount.Sum(a => a.Discount);

                        item.TotalAtStoreQuantity = item.AtStore;
                        item.TotalTakeAwayQuantity = item.TakeAway;
                        item.TotalDeliveryQuantity = item.Delivery;

                        item.FinalAtStorePrice = item.PriceAtStore;
                        item.FinalTakeAwayPrice = item.PriceTakeAway;
                        item.FinalDeliveryPrice = item.PriceDelivery;

                        item.TotalAtStorePrice = item.AtStoreDiscount + item.FinalAtStorePrice;
                        item.TotalTakeAwayPrice = item.TakeAwayDiscount + item.FinalTakeAwayPrice;
                        item.TotalDeliveryPrice = item.DeliveryDiscount + item.PriceDelivery;
                    }
                    dic.Add(ward.WardName, listExcel);
                }


                //ExportToExcel
                string filepath = HttpContext.Server.MapPath(@"/Resource/LocationReportFollowTemp.xlsx");
                var filestream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                var districtApi = new DistrictApi();
                var districtName = districtApi.Get().Where(q => q.DistrictCode == districtID).Select(r => r.DistrictType + " " + r.DistrictName).FirstOrDefault();
                areaName += "_" + districtName;
                using (ExcelPackage package = new ExcelPackage(filestream))
                {
                    package.Workbook.Worksheets["Total"].Cells["C3"].Value = dateRange;
                    package.Workbook.Worksheets["Total"].Cells["J3"].Value = areaName;
                    for (int i = 1; i <= 3; i++)
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets[i];
                        char StartHeaderChar = 'A';
                        int StartHeaderNumber = 6;
                        int stt = 1;
                        //Set values for cells     
                        ws.Cells["C3"].Value = dateRange;
                        ws.Cells["J3"].Value = areaName;
                        foreach (var item in dic)
                        {
                            StartHeaderChar = 'A';
                            StartHeaderNumber++;
                            ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = stt++;
                            ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = item.Key;
                            List<LocaltionReportFollowTempModel> listExcel = item.Value;
                            if (i == 1)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalDeliveryPrice);
                                }
                            }
                            else if (i == 2)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalTakeAwayPrice);
                                }
                            }
                            else if (i == 3)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalAtStorePrice);
                                }
                            }
                        }
                        char EndHeaderChar = ++StartHeaderChar;
                        int EndHeaderNumber = StartHeaderNumber;
                        StartHeaderChar = 'A';
                        StartHeaderNumber = 7;

                        //Set style for rows and columns
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                            .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                            .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SandyBrown);
                        for (char j = 'C'; j < EndHeaderChar; j++)
                        {
                            for (int k = 7; k <= EndHeaderNumber; k++)
                            {
                                ws.Cells["" + (j) + (k)].Value = Convert.ToDecimal(ws.Cells["" + (j) + (k)].Value);
                            }
                        }
                        for (int j = 3; j < 20; j++)
                        {
                            ws.Column(j).Width = 15;
                        }
                        ws.Column(20).Width = 18;
                        //ws.View.FreezePanes(4, 7);
                        ws.Cells["A1:" + StartHeaderChar + StartHeaderNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        //Set style for excel
                        //ws.Cells[ws.Dimension.Address].AutoFitColumns();
                        ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    fileName += areaName + "_" + dateRange + ".xlsx";

                    package.SaveAs(ms);
                    filestream.Close();
                    ms.Seek(0, SeekOrigin.Begin);
                }
            }
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return this.File(ms, contentType, fileName);
        }
        public ActionResult ExportLocationTableToExcelFollowTempForStore(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int selectedStoreId, int provinceID, int level, int districtID)
        {
            MemoryStream ms = new MemoryStream();
            var brandAPI = new BrandApi();
            var brandName = brandAPI.Get(brandId).BrandName;
            var storeApi = new StoreApi();
            var storeName = storeApi.GetStoreById(selectedStoreId).Name;
            var provinceApi = new ProvinceApi();
            var provinceName = provinceApi.Get().Where(q => q.ProvinceCode == provinceID).Select(r => r.ProvinceName).FirstOrDefault();
            var areaName = provinceName;
            string fileName = "BáoCáoTheoTheoKhuVực_" + brandName +" " + storeName + "_";
            if (level == 1)
            {
                var dic = new Dictionary<string, List<LocaltionReportFollowTempModel>>();
                var districtApi = new DistrictApi();
                var districts = districtApi.GetAllDistrictByProvinceID(provinceID).OrderBy(r => r.DistrictName);
                //Get data
                foreach (var district in districts)
                {
                    List<LocaltionReportFollowTempModel> listExcel = new List<LocaltionReportFollowTempModel>();
                    for (int i = 6; i < 23; i++)
                    {
                        listExcel.Add(new LocaltionReportFollowTempModel()
                        {
                            StartTime = i,
                            EndTime = (i + 1)

                        });
                    }

                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();

                    TimeSpan spanTime = endDate - startDate;
                    IEnumerable<Order> rents;
                    var orderAPI = new OrderApi();
                    rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId).Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish &&
                    q.DistrictCode != null && q.DistrictCode == district.DistrictCode && q.StoreID != null && q.StoreID == selectedStoreId);

                    var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                    {
                        OrderType = r.Key.OrderType,
                        OrderTime = r.Key.Time,
                        TotalOrder = r.Count(),
                        Money = r.Sum(a => a.FinalAmount),
                        Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail),
                    }).ToList();

                    foreach (var item in listExcel)
                    {
                        var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                        item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                        item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                        var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                        item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                        item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                        var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                        item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                        item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                        var atStoreDiscount = result.Where(r => r.OrderType == 4 && r.OrderTime == item.StartTime);
                        var takeAwayDiscount = result.Where(r => r.OrderType == 5 && r.OrderTime == item.StartTime);
                        var deliveryDiscount = result.Where(r => r.OrderType == 6 && r.OrderTime == item.StartTime);

                        item.AtStoreDiscount = atStoreDiscount.Sum(a => a.Discount);
                        item.TakeAwayDiscount = takeAwayDiscount.Sum(a => a.Discount);
                        item.DeliveryDiscount = deliveryDiscount.Sum(a => a.Discount);

                        item.TotalAtStoreQuantity = item.AtStore;
                        item.TotalTakeAwayQuantity = item.TakeAway;
                        item.TotalDeliveryQuantity = item.Delivery;

                        item.FinalAtStorePrice = item.PriceAtStore;
                        item.FinalTakeAwayPrice = item.PriceTakeAway;
                        item.FinalDeliveryPrice = item.PriceDelivery;

                        item.TotalAtStorePrice = item.AtStoreDiscount + item.FinalAtStorePrice;
                        item.TotalTakeAwayPrice = item.TakeAwayDiscount + item.FinalTakeAwayPrice;
                        item.TotalDeliveryPrice = item.DeliveryDiscount + item.PriceDelivery;
                    }
                    dic.Add(district.DistrictName, listExcel);
                }


                //ExportToExcel
                string filepath = HttpContext.Server.MapPath(@"/Resource/LocationReportFollowTemp.xlsx");
                var filestream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                using (ExcelPackage package = new ExcelPackage(filestream))
                {
                    package.Workbook.Worksheets["Total"].Cells["C3"].Value = dateRange;
                    package.Workbook.Worksheets["Total"].Cells["J3"].Value = areaName;
                    for (int i = 1; i <= 3; i++)
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets[i];
                        char StartHeaderChar = 'A';
                        int StartHeaderNumber = 6;
                        int stt = 1;
                        //Set values for cells     
                        ws.Cells["C3"].Value = dateRange;
                        ws.Cells["J3"].Value = areaName;
                        foreach (var item in dic)
                        {
                            StartHeaderChar = 'A';
                            StartHeaderNumber++;
                            ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = stt++;
                            ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = item.Key;
                            List<LocaltionReportFollowTempModel> listExcel = item.Value;
                            if (i == 1)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalDeliveryPrice);
                                }
                            }
                            else if (i == 2)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalTakeAwayPrice);
                                }
                            }
                            else if (i == 3)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalAtStorePrice);
                                }
                            }
                        }
                        char EndHeaderChar = ++StartHeaderChar;
                        int EndHeaderNumber = StartHeaderNumber;
                        StartHeaderChar = 'A';
                        StartHeaderNumber = 7;

                        //Set style for rows and columns
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                            .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                            .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SandyBrown);
                        for (char j = 'C'; j < EndHeaderChar; j++)
                        {
                            for (int k = 7; k <= EndHeaderNumber; k++)
                            {
                                ws.Cells["" + (j) + (k)].Value = Convert.ToDecimal(ws.Cells["" + (j) + (k)].Value);
                            }
                        }
                        for (int j = 3; j < 20; j++)
                        {
                            ws.Column(j).Width = 15;
                        }
                        ws.Column(20).Width = 18;
                        //ws.View.FreezePanes(4, 7);
                        ws.Cells["A1:" + StartHeaderChar + StartHeaderNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        //Set style for excel
                        //ws.Cells[ws.Dimension.Address].AutoFitColumns();
                        ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    fileName += areaName + " " + dateRange + ".xlsx";

                    package.SaveAs(ms);
                    filestream.Close();
                    ms.Seek(0, SeekOrigin.Begin);
                }
            }
            else
            {
                var dic = new Dictionary<string, List<LocaltionReportFollowTempModel>>();
                var wardApi = new WardApi();
                var wards = wardApi.BaseService.GetActive().Where(q => q.DistrictCode == districtID).OrderBy(r => r.WardName).ToList();
                //Get data
                foreach (var ward in wards)
                {
                    List<LocaltionReportFollowTempModel> listExcel = new List<LocaltionReportFollowTempModel>();
                    for (int i = 6; i < 23; i++)
                    {
                        listExcel.Add(new LocaltionReportFollowTempModel()
                        {
                            StartTime = i,
                            EndTime = (i + 1)

                        });
                    }

                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();

                    TimeSpan spanTime = endDate - startDate;
                    IEnumerable<Order> rents;
                    var orderAPI = new OrderApi();
                    rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId).Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish &&
                    q.WardCode != null && q.WardCode == ward.WardCode && q.StoreID != null && q.StoreID == selectedStoreId);

                    var result = rents.GroupBy(r => new { r.OrderType, Time = r.CheckinHour }).Select(r => new
                    {
                        OrderType = r.Key.OrderType,
                        OrderTime = r.Key.Time,
                        TotalOrder = r.Count(),
                        Money = r.Sum(a => a.FinalAmount),
                        Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail),
                    }).ToList();

                    foreach (var item in listExcel)
                    {
                        var takeAway = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.TakeAway && r.OrderTime == item.StartTime);
                        item.TakeAway = (takeAway == null) ? 0 : takeAway.TotalOrder;
                        item.PriceTakeAway = (takeAway == null) ? 0 : takeAway.Money;

                        var atStore = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.AtStore && r.OrderTime == item.StartTime);
                        item.AtStore = (atStore == null) ? 0 : atStore.TotalOrder;
                        item.PriceAtStore = (atStore == null) ? 0 : atStore.Money;

                        var delivery = result.FirstOrDefault(r => r.OrderType == (int)OrderTypeEnum.Delivery && r.OrderTime == item.StartTime);
                        item.Delivery = (delivery == null) ? 0 : delivery.TotalOrder;
                        item.PriceDelivery = (delivery == null) ? 0 : delivery.Money;

                        var atStoreDiscount = result.Where(r => r.OrderType == 4 && r.OrderTime == item.StartTime);
                        var takeAwayDiscount = result.Where(r => r.OrderType == 5 && r.OrderTime == item.StartTime);
                        var deliveryDiscount = result.Where(r => r.OrderType == 6 && r.OrderTime == item.StartTime);

                        item.AtStoreDiscount = atStoreDiscount.Sum(a => a.Discount);
                        item.TakeAwayDiscount = takeAwayDiscount.Sum(a => a.Discount);
                        item.DeliveryDiscount = deliveryDiscount.Sum(a => a.Discount);

                        item.TotalAtStoreQuantity = item.AtStore;
                        item.TotalTakeAwayQuantity = item.TakeAway;
                        item.TotalDeliveryQuantity = item.Delivery;

                        item.FinalAtStorePrice = item.PriceAtStore;
                        item.FinalTakeAwayPrice = item.PriceTakeAway;
                        item.FinalDeliveryPrice = item.PriceDelivery;

                        item.TotalAtStorePrice = item.AtStoreDiscount + item.FinalAtStorePrice;
                        item.TotalTakeAwayPrice = item.TakeAwayDiscount + item.FinalTakeAwayPrice;
                        item.TotalDeliveryPrice = item.DeliveryDiscount + item.PriceDelivery;
                    }
                    dic.Add(ward.WardName, listExcel);
                }


                //ExportToExcel
                string filepath = HttpContext.Server.MapPath(@"/Resource/LocationReportFollowTemp.xlsx");
                var filestream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                var districtApi = new DistrictApi();
                var districtName = districtApi.Get().Where(q => q.DistrictCode == districtID).Select(r => r.DistrictType + " " + r.DistrictName).FirstOrDefault();
                areaName += "_" + districtName;
                using (ExcelPackage package = new ExcelPackage(filestream))
                {
                    package.Workbook.Worksheets["Total"].Cells["C3"].Value = dateRange;
                    package.Workbook.Worksheets["Total"].Cells["J3"].Value = areaName;
                    for (int i = 1; i <= 3; i++)
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets[i];
                        char StartHeaderChar = 'A';
                        int StartHeaderNumber = 6;
                        int stt = 1;
                        //Set values for cells     
                        ws.Cells["C3"].Value = dateRange;
                        ws.Cells["J3"].Value = areaName;
                        foreach (var item in dic)
                        {
                            StartHeaderChar = 'A';
                            StartHeaderNumber++;
                            ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = stt++;
                            ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = item.Key;
                            List<LocaltionReportFollowTempModel> listExcel = item.Value;
                            if (i == 1)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalDeliveryPrice);
                                }
                            }
                            else if (i == 2)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalTakeAwayPrice);
                                }
                            }
                            else if (i == 3)
                            {
                                foreach (var data in listExcel)
                                {
                                    ws.Cells["" + (++StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                        "{0:0,0}", data.FinalAtStorePrice);
                                }
                            }
                        }
                        char EndHeaderChar = ++StartHeaderChar;
                        int EndHeaderNumber = StartHeaderNumber;
                        StartHeaderChar = 'A';
                        StartHeaderNumber = 7;

                        //Set style for rows and columns
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                            .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                            ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                            .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SandyBrown);
                        for (char j = 'C'; j < EndHeaderChar; j++)
                        {
                            for (int k = 7; k <= EndHeaderNumber; k++)
                            {
                                ws.Cells["" + (j) + (k)].Value = Convert.ToDecimal(ws.Cells["" + (j) + (k)].Value);
                            }
                        }
                        for (int j = 3; j < 20; j++)
                        {
                            ws.Column(j).Width = 15;
                        }
                        ws.Column(20).Width = 18;
                        //ws.View.FreezePanes(4, 7);
                        ws.Cells["A1:" + StartHeaderChar + StartHeaderNumber].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        //Set style for excel
                        //ws.Cells[ws.Dimension.Address].AutoFitColumns();
                        ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    fileName += areaName + " " + dateRange + ".xlsx";

                    package.SaveAs(ms);
                    filestream.Close();
                    ms.Seek(0, SeekOrigin.Begin);
                }
            }
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return this.File(ms, contentType, fileName);
        }
        public JsonResult LoadLocationReportForStore(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int selectedStoreId, int provinceID)
        {
            var dateNow = Utils.GetCurrentDateTime();
            var locationReport = new List<LocationReportModel>();
            var districtApi = new DistrictApi();
            var districts = districtApi.GetAllDistrictByProvinceID(provinceID).OrderBy(q => q.DistrictName).ToList();
            var storeApi = new StoreApi();
            var storeID = storeApi.GetStoreById(selectedStoreId).ID;

            var isAdmin = HttpContext.User.IsInRole("Administrator");

            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            TimeSpan spanTime = endDate - startDate;

            int count = 1;
            IEnumerable<Order> rents;
            var orderAPI = new OrderApi();
            rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                  .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish && a.StoreID != null && a.StoreID == storeID);

            var result = rents.GroupBy(r => new { Time = r.CheckinHour, District = r.District }).Select(r => new
            {
                OrderTime = r.Key.Time,
                OrderLocation = r.Key.District,
                TotalOrder = r.Count(),
                Money = r.Sum(a => a.FinalAmount),
                Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail)
            }).ToList();

            foreach (var district in districts)
            {
                LocationReportModel model = new LocationReportModel();
                model.LocationName = district.DistrictName;
                model.PriceByHour = new double[17];
                for (int i = 6; i < 23; i++)
                {
                    var hourOrder = result.FirstOrDefault(r => r.OrderTime == i && r.OrderLocation == district);
                    if (hourOrder != null)
                    {
                        model.PriceByHour[i - 6] = hourOrder.Money + hourOrder.Discount;
                        model.TotalQuantity += hourOrder.TotalOrder;
                        model.Discount += hourOrder.Discount;
                        model.FinalPrice += hourOrder.Money;
                        model.TotalPrice += hourOrder.Money + hourOrder.Discount;
                    }
                    else
                    {
                        model.PriceByHour[i - 6] = 0;
                        model.TotalQuantity += 0;
                        model.Discount += 0;
                        model.FinalPrice += 0;
                        model.TotalPrice += 0;
                    }
                }
                locationReport.Add(model);
            }

            var list = locationReport.Select(a => new IConvertible[]
           {
                count++,
                a.LocationName,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[0]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[1]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[2]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[3]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[4]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[5]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[6]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[7]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[8]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[9]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[10]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[11]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[12]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[13]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[14]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[15]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[16]),
                a.TotalQuantity,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalPrice)

           }).ToList();

            return Json(new
            {
                datatable = list,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadLocationReportForGroup(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int selectedGroupId, int provinceID)
        {
            var dateNow = Utils.GetCurrentDateTime();
            var locationReport = new List<LocationReportModel>();
            var districtApi = new DistrictApi();
            var districts = districtApi.GetAllDistrictByProvinceID(provinceID).OrderBy(q => q.DistrictName).ToList();
            var storeApi = new StoreApi();
            var stores = storeApi.GetStoreByGroupId(selectedGroupId).Select(r => r.ID).ToList();

            var isAdmin = HttpContext.User.IsInRole("Administrator");

            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            TimeSpan spanTime = endDate - startDate;

            int count = 1;
            IEnumerable<Order> rents;
            var orderAPI = new OrderApi();
            rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                  .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish && a.StoreID != null && stores.Contains((int)a.StoreID));

            var result = rents.GroupBy(r => new { Time = r.CheckinHour, District = r.District }).Select(r => new
            {
                OrderTime = r.Key.Time,
                OrderLocation = r.Key.District,
                TotalOrder = r.Count(),
                Money = r.Sum(a => a.FinalAmount),
                Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail)
            }).ToList();

            foreach (var district in districts)
            {
                LocationReportModel model = new LocationReportModel();
                model.LocationName = district.DistrictName;
                model.PriceByHour = new double[17];
                for (int i = 6; i < 23; i++)
                {
                    var hourOrder = result.FirstOrDefault(r => r.OrderTime == i && r.OrderLocation == district);
                    if (hourOrder != null)
                    {
                        model.PriceByHour[i - 6] = hourOrder.Money + hourOrder.Discount;
                        model.TotalQuantity += hourOrder.TotalOrder;
                        model.Discount += hourOrder.Discount;
                        model.FinalPrice += hourOrder.Money;
                        model.TotalPrice += hourOrder.Money + hourOrder.Discount;
                    }
                    else
                    {
                        model.PriceByHour[i - 6] = 0;
                        model.TotalQuantity += 0;
                        model.Discount += 0;
                        model.FinalPrice += 0;
                        model.TotalPrice += 0;
                    }
                }
                locationReport.Add(model);
            }

            var list = locationReport.Select(a => new IConvertible[]
           {
                count++,
                a.LocationName,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[0]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[1]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[2]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[3]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[4]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[5]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[6]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[7]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[8]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[9]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[10]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[11]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[12]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[13]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[14]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[15]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[16]),
                a.TotalQuantity,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalPrice)

           }).ToList();

            return Json(new
            {
                datatable = list,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadLocationReportForBrand(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int provinceID)
        {
            var dateNow = Utils.GetCurrentDateTime();
            var locationReport = new List<LocationReportModel>();
            var districtApi = new DistrictApi();
            var districts = districtApi.GetAllDistrictByProvinceID(provinceID).OrderBy(q => q.DistrictName).ToList();

            var isAdmin = HttpContext.User.IsInRole("Administrator");

            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            TimeSpan spanTime = endDate - startDate;

            int count = 1;
            IEnumerable<Order> rents;
            var orderAPI = new OrderApi();
            rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                  .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish);

            var result = rents.GroupBy(r => new { Time = r.CheckinHour, District = r.District }).Select(r => new
            {
                OrderTime = r.Key.Time,
                OrderLocation = r.Key.District,
                TotalOrder = r.Count(),
                Money = r.Sum(a => a.FinalAmount),
                Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail)
            }).ToList();

            foreach (var district in districts)
            {
                LocationReportModel model = new LocationReportModel();
                model.LocationName = district.DistrictName;
                model.PriceByHour = new double[17];
                for (int i = 6; i < 23; i++)
                {
                    var hourOrder = result.FirstOrDefault(r => r.OrderTime == i && r.OrderLocation == district);
                    if (hourOrder != null)
                    {
                        model.PriceByHour[i - 6] = hourOrder.Money + hourOrder.Discount;
                        model.TotalQuantity += hourOrder.TotalOrder;
                        model.Discount += hourOrder.Discount;
                        model.FinalPrice += hourOrder.Money;
                        model.TotalPrice += hourOrder.Money + hourOrder.Discount;
                    }
                    else
                    {
                        model.PriceByHour[i - 6] = 0;
                        model.TotalQuantity += 0;
                        model.Discount += 0;
                        model.FinalPrice += 0;
                        model.TotalPrice += 0;
                    }
                }
                locationReport.Add(model);
            }

            var list = locationReport.Select(a => new IConvertible[]
           {
                count++,
                a.LocationName,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[0]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[1]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[2]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[3]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[4]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[5]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[6]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[7]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[8]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[9]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[10]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[11]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[12]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[13]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[14]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[15]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[16]),
                a.TotalQuantity,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalPrice)

           }).ToList();

            return Json(new
            {
                datatable = list,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadDistrictReportForBrand(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int provinceID, int districtID)
        {
            var dateNow = Utils.GetCurrentDateTime();
            var locationReport = new List<LocationReportModel>();
            var districtApi = new DistrictApi();
            var districtCodes = districtApi.GetAllDistrictByProvinceID(provinceID).Select(r => r.DistrictCode).ToList();
            var wardApi = new WardApi();
            var wards = wardApi.BaseService.GetActive().Where(q => districtCodes.Contains(q.DistrictCode) && q.DistrictCode == districtID).OrderBy(r => r.WardName).ToList();

            var isAdmin = HttpContext.User.IsInRole("Administrator");

            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            TimeSpan spanTime = endDate - startDate;

            int count = 1;
            IEnumerable<Order> rents;
            var orderAPI = new OrderApi();
            rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                  .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish);

            var result = rents.GroupBy(r => new { Time = r.CheckinHour, Ward = r.Ward }).Select(r => new
            {
                OrderTime = r.Key.Time,
                OrderLocation = r.Key.Ward,
                TotalOrder = r.Count(),
                Money = r.Sum(a => a.FinalAmount),
                Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail)
            }).ToList();

            foreach (var ward in wards)
            {
                LocationReportModel model = new LocationReportModel();
                model.LocationName = ward.WardName;
                model.PriceByHour = new double[17];
                for (int i = 6; i < 23; i++)
                {
                    var hourOrder = result.FirstOrDefault(r => r.OrderTime == i && r.OrderLocation == ward);
                    if (hourOrder != null)
                    {
                        model.PriceByHour[i - 6] = hourOrder.Money + hourOrder.Discount;
                        model.TotalQuantity += hourOrder.TotalOrder;
                        model.Discount += hourOrder.Discount;
                        model.FinalPrice += hourOrder.Money;
                        model.TotalPrice += hourOrder.Money + hourOrder.Discount;
                    }
                    else
                    {
                        model.PriceByHour[i - 6] = 0;
                        model.TotalQuantity += 0;
                        model.Discount += 0;
                        model.FinalPrice += 0;
                        model.TotalPrice += 0;
                    }
                }
                locationReport.Add(model);
            }

            var list = locationReport.Select(a => new IConvertible[]
           {
                count++,
                a.LocationName,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[0]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[1]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[2]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[3]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[4]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[5]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[6]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[7]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[8]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[9]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[10]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[11]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[12]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[13]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[14]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[15]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[16]),
                a.TotalQuantity,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalPrice)

           }).ToList();

            return Json(new
            {
                datatable = list,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadDistrictReportForStore(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int provinceID, int districtID, int selectedStoreId)
        {
            var dateNow = Utils.GetCurrentDateTime();
            var locationReport = new List<LocationReportModel>();
            var districtApi = new DistrictApi();
            var districtCodes = districtApi.GetAllDistrictByProvinceID(provinceID).Select(r => r.DistrictCode).ToList();
            var wardApi = new WardApi();
            var wards = wardApi.BaseService.GetActive().Where(q => districtCodes.Contains(q.DistrictCode) && q.DistrictCode == districtID).OrderBy(r => r.WardName).ToList();

            var isAdmin = HttpContext.User.IsInRole("Administrator");

            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            TimeSpan spanTime = endDate - startDate;

            int count = 1;
            IEnumerable<Order> rents;
            var orderAPI = new OrderApi();
            rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                  .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish && a.StoreID != null && a.StoreID == selectedStoreId);

            var result = rents.GroupBy(r => new { Time = r.CheckinHour, Ward = r.Ward }).Select(r => new
            {
                OrderTime = r.Key.Time,
                OrderLocation = r.Key.Ward,
                TotalOrder = r.Count(),
                Money = r.Sum(a => a.FinalAmount),
                Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail)
            }).ToList();

            foreach (var ward in wards)
            {
                LocationReportModel model = new LocationReportModel();
                model.LocationName = ward.WardName;
                model.PriceByHour = new double[17];
                for (int i = 6; i < 23; i++)
                {
                    var hourOrder = result.FirstOrDefault(r => r.OrderTime == i && r.OrderLocation == ward);
                    if (hourOrder != null)
                    {
                        model.PriceByHour[i - 6] = hourOrder.Money + hourOrder.Discount;
                        model.TotalQuantity += hourOrder.TotalOrder;
                        model.Discount += hourOrder.Discount;
                        model.FinalPrice += hourOrder.Money;
                        model.TotalPrice += hourOrder.Money + hourOrder.Discount;
                    }
                    else
                    {
                        model.PriceByHour[i - 6] = 0;
                        model.TotalQuantity += 0;
                        model.Discount += 0;
                        model.FinalPrice += 0;
                        model.TotalPrice += 0;
                    }
                }
                locationReport.Add(model);
            }

            var list = locationReport.Select(a => new IConvertible[]
           {
                count++,
                a.LocationName,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[0]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[1]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[2]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[3]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[4]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[5]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[6]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[7]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[8]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[9]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[10]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[11]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[12]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[13]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[14]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[15]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[16]),
                a.TotalQuantity,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalPrice)

           }).ToList();

            return Json(new
            {
                datatable = list,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadDistrictReportForGroup(JQueryDataTableParamModel param, int brandId, string startTime, string endTime, int provinceID, int districtID, int selectedGroupId)
        {
            var dateNow = Utils.GetCurrentDateTime();
            var locationReport = new List<LocationReportModel>();
            var storeApi = new StoreApi();
            var stores = storeApi.GetStoreByGroupId(selectedGroupId).Select(r => r.ID).ToList();
            var districtApi = new DistrictApi();
            var districtCodes = districtApi.GetAllDistrictByProvinceID(provinceID).Select(r => r.DistrictCode).ToList();
            var wardApi = new WardApi();
            var wards = wardApi.BaseService.GetActive().Where(q => districtCodes.Contains(q.DistrictCode) && q.DistrictCode == districtID).OrderBy(r => r.WardName).ToList();

            var isAdmin = HttpContext.User.IsInRole("Administrator");

            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            TimeSpan spanTime = endDate - startDate;

            int count = 1;
            IEnumerable<Order> rents;
            var orderAPI = new OrderApi();
            rents = orderAPI.GetAllOrdersByDate(startDate, endDate, brandId)
                  .Where(a => a.OrderStatus == (int)OrderStatusEnum.Finish && a.StoreID != null && stores.Contains((int)a.StoreID));

            var result = rents.GroupBy(r => new { Time = r.CheckinHour, Ward = r.Ward }).Select(r => new
            {
                OrderTime = r.Key.Time,
                OrderLocation = r.Key.Ward,
                TotalOrder = r.Count(),
                Money = r.Sum(a => a.FinalAmount),
                Discount = r.Sum(a => a.Discount + a.DiscountOrderDetail)
            }).ToList();

            foreach (var ward in wards)
            {
                LocationReportModel model = new LocationReportModel();
                model.LocationName = ward.WardName;
                model.PriceByHour = new double[17];
                for (int i = 6; i < 23; i++)
                {
                    var hourOrder = result.FirstOrDefault(r => r.OrderTime == i && r.OrderLocation == ward);
                    if (hourOrder != null)
                    {
                        model.PriceByHour[i - 6] = hourOrder.Money + hourOrder.Discount;
                        model.TotalQuantity += hourOrder.TotalOrder;
                        model.Discount += hourOrder.Discount;
                        model.FinalPrice += hourOrder.Money;
                        model.TotalPrice += hourOrder.Money + hourOrder.Discount;
                    }
                    else
                    {
                        model.PriceByHour[i - 6] = 0;
                        model.TotalQuantity += 0;
                        model.Discount += 0;
                        model.FinalPrice += 0;
                        model.TotalPrice += 0;
                    }
                }
                locationReport.Add(model);
            }

            var list = locationReport.Select(a => new IConvertible[]
           {
                count++,
                a.LocationName,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[0]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[1]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[2]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[3]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[4]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[5]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[6]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[7]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[8]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[9]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[10]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[11]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[12]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[13]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[14]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[15]),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.PriceByHour[16]),
                a.TotalQuantity,
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.FinalPrice)

           }).ToList();

            return Json(new
            {
                datatable = list,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadProvinceList()
        {
            var provinceApi = new ProvinceApi();
            var provinces = provinceApi.GetAllProvince().OrderBy(q => q.ProvinceName).ToArray();
            return Json(new
            {
                province = provinces,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadDistrictList(int provinceID)
        {
            var districtApi = new DistrictApi();
            var districts = districtApi.GetAllDistrictByProvinceID(provinceID).OrderBy(q => q.DistrictName).ToArray();
            return Json(new
            {
                districtLength = districts.Length,
                districtCode = districts.Select(q => q.DistrictCode),
                districtName = districts.Select(q => q.DistrictName)
            }, JsonRequestBehavior.AllowGet);
        }
        public class LocaltionReportFollowTempModel
        {
            public int StartTime { get; set; }
            public int EndTime { get; set; }
            public double TakeAway { get; set; }
            public double PriceTakeAway { get; set; }
            public double AtStore { get; set; }
            public double PriceAtStore { get; set; }
            public double Delivery { get; set; }
            public double PriceDelivery { get; set; }
            public double TakeAwayDiscount { get; set; }
            public double TotalTakeAwayQuantity { get; set; }
            public double TotalTakeAwayPrice { get; set; }
            public double FinalTakeAwayPrice { get; set; }
            public double AtStoreDiscount { get; set; }
            public double TotalAtStoreQuantity { get; set; }
            public double TotalAtStorePrice { get; set; }
            public double FinalAtStorePrice { get; set; }
            public double DeliveryDiscount { get; set; }
            public double TotalDeliveryQuantity { get; set; }
            public double TotalDeliveryPrice { get; set; }
            public double FinalDeliveryPrice { get; set; }
            public int ProvincdID { get; set; }
            public int DistrictID { get; set; }
            public int WardID { get; set; }
        }
        public class LocationReportModel
        {
            public double[] PriceByHour { get; set; }
            public double Discount { get; set; }
            public double TotalQuantity { get; set; }
            public double TotalPrice { get; set; }
            public double FinalPrice { get; set; }
            public string LocationName { get; set; }
        }
    }
}