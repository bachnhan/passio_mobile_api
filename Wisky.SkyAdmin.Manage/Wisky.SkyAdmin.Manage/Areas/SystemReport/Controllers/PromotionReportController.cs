using HmsService.Models;
using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using HmsService.Models.Entities;
using System.Data.Entity;
using System.Globalization;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.IO;

namespace Wisky.SkyAdmin.Manage.Areas.SystemReport.Controllers
{
    public class PromotionReportController : Controller
    {
        #region PromotionIndex
        // GET: SystemReport/PromotionReport
        public ActionResult Index(int brandId)
        {
            var storeApi = new StoreApi();
            var promotionApi = new PromotionApi();

            #region GetAllStore
            var storeList = storeApi.GetStoreByBrandId(brandId).Select(q => new SelectListItem
            {
                Text = q.Name,
                Value = q.ID.ToString()
            }).ToList();

            ViewBag.StoreList = storeList;
            #endregion

            #region GetAllPromotion
            var promoList = promotionApi.GetActivePromotion(brandId).Select(q => new SelectListItem
            {
                Text = q.PromotionName,
                Value = q.PromotionID.ToString(),
                //StartDate = q.FromDate.ToString("dd/MM/yyyy"),
                //EndDate = q.ToDate.ToString("dd/MM/yyyy"),
            }).ToList();

            ViewBag.PromotionList = promoList;
            #endregion

            return View();
        }

        public JsonResult LoadPromotion(int brandId, int selectedStoreId)
        {
            var promotionApi = new PromotionApi();
            var promotionStoreMappingApi = new PromotionStoreMappingApi();
            if (selectedStoreId <= 0)
            {
                var promoList = promotionApi.GetActivePromotion(brandId).Select(q => new
                {
                    Text = q.PromotionName,
                    Value = q.PromotionID.ToString(),
                    StartDate = q.FromDate,
                    EndDate = q.ToDate,

                }).ToArray();
                return Json(new
                {
                    promoList
                }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var promoList = promotionStoreMappingApi.GetAllActivePromotionIDByStoreID(selectedStoreId).AsEnumerable()
                    .Join(promotionApi.GetPromotionByBrandId(brandId).AsEnumerable(),
                    q => q,
                    p => p.PromotionID,
                    (q, p) => new
                    {
                        Text = p.PromotionName,
                        Value = p.PromotionID.ToString(),
                        StartDate = p.FromDate,
                        EndDate = p.ToDate,
                    }).ToList();
                return Json(new
                {
                    promoList
                }, JsonRequestBehavior.AllowGet);
            }


        }
        public JsonResult LoadStore(int brandId)
        {
            var storeApi = new StoreApi();

            var stores = storeApi.GetStoreByBrandId(brandId).Select(q => new SelectListItem
            {
                Text = q.Name,
                Value = q.ID.ToString()
            }).ToList();

            return Json(new
            {
                stores = stores,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportPromotionExcel(JQueryDataTableParamModel param, string startTime, string endTime, int brandId, int promoID, int selectedStoreId)
        {
            var customerApi = new CustomerApi();
            var orderApi = new OrderApi();
            var orderPromotionMappingApi = new OrderPromotionMappingApi();
            DateTime now = DateTime.Now;
            var sDate = startTime.ToDateTime().GetStartOfDate();
            var eDate = endTime.ToDateTime().GetEndOfDate();
            eDate = eDate > now ? now : eDate;
            var customerOrders = orderApi.GetAllOrdersByDate(sDate, eDate, brandId);
            if (selectedStoreId > 0)
            {
                customerOrders = customerOrders.Where(q => q.StoreID == selectedStoreId);
            }
            var promotionOrderIDs = orderPromotionMappingApi.GetOrderIdByPromoId(promoID);
            var result = customerOrders
                .Where(q => promotionOrderIDs.Contains(q.RentID))
                .GroupBy(p => p.CustomerID)
                .Select(r => new
                {
                    CustomerName = r.FirstOrDefault().Customer.Name ?? "N/A",
                    OrderQty = r.Count(),
                    SumAmount = r.Sum(d => d.TotalAmount),
                    SumDiscount = r.Sum(d => d.Discount + d.DiscountOrderDetail),
                    SumFinal = r.Sum(d => d.FinalAmount),
                    PromoID = promoID,
                    Id = r.Key.Value,
                }).ToList();

            #region Export to Excel
            string filepath = HttpContext.Server.MapPath(@"/Resource/PromotionReportTemplate.xlsx");
            var filestream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite);
            string storeName;
            var storeApi = new StoreApi();
            if (selectedStoreId > 0)
            {
                storeName = storeApi.GetStoreNameByID(selectedStoreId);
            }
            else
                storeName = "AllStore";
            var startDate = startTime.Replace("/", "-");
            var endDate = endTime.Replace("/", "-");
            var dateRange = "(" + startDate + (startDate == endDate ? "" : " - " + endDate) + ")";
            string fileName = "BaoCaoKhuyenMai_" + "Store_" + storeName + "_" + dateRange + ".xlsx";
            using (ExcelPackage package = new ExcelPackage(filestream))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets[1];
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 6;
                int stt = 1;
                ws.Cells["B3"].Value = dateRange;
                ws.Cells["F3"].Value = storeName;
                #region Set values for cells                
                foreach (var data in result)
                {
                    StartHeaderChar = 'A';
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = stt++;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerName;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.OrderQty);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.SumAmount);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.SumDiscount);
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.SumFinal);
                }
                var EndHeaderChar = StartHeaderChar;
                var EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 6;
                for (char j = 'C'; j <= EndHeaderChar; j++)
                {
                    for (int k = 7; k <= EndHeaderNumber; k++)
                    {
                        ws.Cells["" + (j) + (k)].Value = Convert.ToDecimal(ws.Cells["" + (j) + (k)].Value);
                    }
                }
                #endregion

                //Set style for excel
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                       ":" + (char.ConvertFromUtf32(EndHeaderChar + 1)) + EndHeaderNumber.ToString()]
                       .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + (char.ConvertFromUtf32(EndHeaderChar + 1)) + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SandyBrown);
                ws.Cells["" + StartHeaderChar + (StartHeaderNumber - 1).ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                MemoryStream ms = new MemoryStream();
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }

        public JsonResult ListCustomerInPromo(JQueryDataTableParamModel param, string startTime, string endTime, int brandId, int promoID, int selectedStoreId)
        {
            var customerApi = new CustomerApi();
            var orderApi = new OrderApi();
            var orderPromotionMappingApi = new OrderPromotionMappingApi();
            DateTime now = DateTime.Now;
            var sDate = startTime.ToDateTime().GetStartOfDate();
            var eDate = endTime.ToDateTime().GetEndOfDate();
            eDate = eDate > now ? now : eDate;
            IEnumerable<int> promotionOrderIDs = orderPromotionMappingApi.GetOrderIdByPromoId(promoID);
            IEnumerable<Order> customerOrders = orderApi.GetAllOrdersByDate(sDate, eDate, brandId).Where(q => promotionOrderIDs.Contains(q.RentID));
            if (selectedStoreId > 0)
            {
                customerOrders = customerOrders.Where(q => q.StoreID == selectedStoreId);
            }
           
            var result = customerOrders
                .GroupBy(p => p.CustomerID)
                .Select(r => new
                {
                    CustomerName = (r.FirstOrDefault().Customer == null) ? "N/A" : r.FirstOrDefault().Customer.Name,
                    OrderQty = r.Count(),
                    SumAmount = r.Sum(d => d.TotalAmount),
                    SumDiscount = r.Sum(d => d.Discount + d.DiscountOrderDetail),
                    SumFinal = r.Sum(d => d.FinalAmount),
                    PromoID = promoID,
                    Id = r.Key.Value,
                });

            var rs = result.Where(q => string.IsNullOrEmpty(param.sSearch)
            || q.CustomerName.ToString().ToLower().Contains(param.sSearch.ToLower()))
            .OrderBy(r => r.Id).Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
            int totalDisplay = result.Count();
            

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = param.iDisplayLength,
                iTotalDisplayRecords = totalDisplay,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        //public double[] GetSum(List<Order> customerOrders, List<int> customerIDs, int customerId)
        //{
        //    double sumDiscount = 0, sumAmount = 0, sumFinal = 0;
        //    List<double> result = new List<double>();
        //    int length = customerIDs.Count;
        //    for (int i = 0; i < length; i++)
        //    {
        //        if (customerIDs[i] == customerId)
        //        {
        //            sumDiscount += customerOrders[i].Discount;
        //            sumAmount += customerOrders[i].TotalAmount;
        //            sumFinal += customerOrders[i].FinalAmount;
        //        }
        //    }
        //    result.Add(sumAmount);
        //    result.Add(sumDiscount);
        //    result.Add(sumFinal);
        //    return result.ToArray();
        //}
        private string GetCustomerName(int customerID)
        {
            var customerApi = new CustomerApi();
            return customerApi.Get(customerID).Name;
        }

        private class CustomerDetailViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int OrderCount { get; set; }
            public double SumAmount { get; set; }
            public double SumDiscount { get; set; }
            public double SumFinal { get; set; }
            public int PromoID { get; set; }
            public int CustomerID { get; set; }
        }
        public class PromotionReportViewModel
        {
            public string CustomerName { get; set; }
            public int OrderQty { get; set; }
            public double SumAmount { get; set; }
            public double SumDiscount { get; set; }
            public double SumFinal { get; set; }
            public int PromoID { get; set; }
            public int Id { get; set; }
        }



        #endregion

        #region CustomerOrder

        public ActionResult CustomerOrder(int customerId, int promotionId, string sTime, string eTime, int selectedStoreId)
        {
            CustomerApi cApi = new CustomerApi();
            var model = cApi.GetCustomerById(customerId);
            ViewData["customerName"] = model.Name;
            ViewData["customerId"] = customerId;
            ViewData["promotionId"] = promotionId;
            ViewData["sTime"] = sTime;
            ViewData["eTime"] = eTime;
            ViewData["selectedStoreId"] = selectedStoreId;

            return View();
        }

        public JsonResult LoadOrderByPromotion(JQueryDataTableParamModel param, int brandId, int selectedStoreId, int promotionId, int status, int type, int customerId, string sTime, string eTime)
        {

            var orderApi = new OrderApi();
            var membershipCardApi = new MembershipCardApi();
            var customerApi = new CustomerApi();
            var promotionApi = new PromotionApi();
            try
            {
                var promotion = promotionApi.GetPromotionById(promotionId);
                //var membershipCards = membershipCardApi.GetMembershipCardActiveByCustomerIdByBrandId(customerId, brandId);

                //var startTime = DateTime.ParseExact(sTime, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                //var endTime = DateTime.ParseExact(eTime, "MM/dd/yyyy", CultureInfo.InvariantCulture).GetEndOfDate();
                var startTime = sTime.ToDateTime().GetStartOfDate();
                var endTime = eTime.ToDateTime().GetEndOfDate();
                endTime = (endTime > DateTime.Now) ? DateTime.Now : endTime;

                //filter status and type
                //var listOrder = orderApi.GetRentsByTimeRange(0, startTime, endTime, brandId).Where(o => o.OrderPromotionMappings.Any(m => m.PromotionId == promotionId));
                var listOrder = orderApi.GetRentsByTimeRange(selectedStoreId, startTime, endTime, brandId)
                    .Where(o => o.CustomerID == customerId && o.Att1.StartsWith(promotion.PromotionCode + ":"));
                var totalRecords = listOrder.Count();
                if (status != 0 && type != 0)
                {
                    listOrder = listOrder.Where(q => q.OrderStatus == status && q.OrderType == type);
                }
                else if (status == 0 && type != 0)
                {
                    listOrder = listOrder.Where(q => q.OrderType == type);
                }
                else if (type == 0 && status != 0)
                {
                    listOrder = listOrder.Where(q => q.OrderStatus == status);
                }

                //var result = listOrder.ToList();


                //listOrder = listOrder.Where(q => membershipCards.Any(
                //    m => q.Att1.Equals(promotion.PromotionCode + ":" + m.MembershipCardCode)));

                //if (selectedStoreId > 0)
                //{
                //    listOrder = listOrder.Where(a => a.StoreID == selectedStoreId);
                //}

                var searchList = listOrder
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.InvoiceID.ToLower().Contains(param.sSearch.ToLower())).ToList();
                int totalDisplay = searchList.Count();

                int count = param.iDisplayStart;
                var rs = searchList
                    .OrderByDescending(q => q.CheckInDate)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .Select(a => new IConvertible[]
                        {
                        ++count, // 0
                        string.IsNullOrEmpty(a.InvoiceID) ? "N/A" : a.InvoiceID, // 1
                        a.OrderDetailsTotalQuantity, // 2
                        a.FinalAmount, // 3
                        a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"), // 4
                        a.OrderType, // 5
                        a.CheckInPerson, // 6
                        a.RentID, // 7
                        a.OrderStatus, // 8
                        string.IsNullOrEmpty(a.DeliveryAddress) ? "N/A" : a.DeliveryAddress, // 9
                        a.Customer!=null ? a.Customer.Phone : "N/A", // 10
                        a.Notes !=null? a.Notes : "", // 11
                        a.TotalAmount,// 12
                        (a.Discount + a.DiscountOrderDetail), // 13
                        a.Store.Name // 14
                        });


                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        #endregion
    }
}