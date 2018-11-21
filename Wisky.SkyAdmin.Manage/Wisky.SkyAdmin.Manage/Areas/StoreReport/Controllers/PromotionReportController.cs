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
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;

namespace Wisky.SkyAdmin.Manage.Areas.StoreReport.Controllers
{
    public class PromotionReportController : Controller
    {

        #region PromotionIndex
        // GET: StoreReport/PromotionReport
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult LoadPromotion(int brandId, int storeId)
        {
            var promotionApi = new PromotionApi();
            var promotionStoreMappingApi = new PromotionStoreMappingApi();

            var promoList = promotionStoreMappingApi.GetAllActivePromotionIDByStoreID(storeId)
                .Join(promotionApi.GetPromotionByBrandId(brandId),
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
        public JsonResult ListCustomerInPromo(JQueryDataTableParamModel param, string startTime, string endTime, int promoID, int storeId, int brandId)
        {
            if (storeId <= 0 || promoID <= 0)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new List<IConvertible[]>()
                }, JsonRequestBehavior.AllowGet);
            }

            var customerApi = new CustomerApi();
            var promotionApi = new PromotionApi();
            var orderApi = new OrderApi(); ;
            var memberShipApi = new MembershipCardApi();
            var selectedPromo = promotionApi.Get(promoID);
            List<int> customerID = new List<int>();
            //var orderPromoMapping = new OrderPromotionMappingApi();
            //var orderList = orderPromoMapping.GetOrderIdByPromoId(promoID);
            //IEnumerable<Order> customerOrders = null;

            //var sDate = DateTime.ParseExact(startTime, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            //var eDate = DateTime.ParseExact(endTime, "MM/dd/yyyy", CultureInfo.InvariantCulture).GetEndOfDate();
            var sDate = startTime.ToDateTime().GetStartOfDate();
            var eDate = endTime.ToDateTime().GetEndOfDate();
            DateTime now = DateTime.Now;

            //customerOrders = orderApi.GetAllOrderByDateByPromotionIdAndStoreId(sDate, eDate, brandId, promoID, storeId);

            //var customerCardCode = customerOrders.Select(q => q.Att1);
            //List<string> customerMemCode = new List<string>();
            //foreach (var item in customerCardCode)
            //{
            //    if (item != null && item.LastIndexOf(':') >= 0 && item.Substring(0, item.LastIndexOf(":")).Equals(selectedPromo.PromotionCode))
            //    {
            //        customerMemCode.Add(item.Substring(item.LastIndexOf(":") + 1));
            //    }
            //}


            //foreach (var item in customerMemCode)
            //{
            //    var idTemp = memberShipApi.GetMembershipCardByCode(item);
            //    if (idTemp != null)
            //    {
            //        customerID.Add(idTemp.CustomerId.GetValueOrDefault());
            //    }

            //}
            //var customers = customerApi.GetAllCustomerByBrandId(brandId).Where(a => customerID.Contains(a.Customer.CustomerID));

            //int totalRecords = customerID.Count();

            //var rs = (await customers
            //    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Customer.Name.ToLower().Contains(param.sSearch.ToLower()))
            //        .ToListAsync())
            //        .Skip(param.iDisplayStart)
            //        .Take(param.iDisplayLength)
            //        .Select(a => new IConvertible[]
            //            {
            //            //count++,
            //            string.IsNullOrEmpty(a.Customer.Name) ? Properties.Resources.UNDEFINE_VN : a.Customer.Name,
            //            //a.Customer.Accounts.Select(q => new {Name = q.AccountName, isDefault = (a.Customer.AccountID != null && a.Customer.AccountID == q.AccountID)}).ToList(),
            //            customerID.Where(q => q == a.Customer.CustomerID).Count(),
            //            GetSum(customerOrders.ToList(), customerID, a.Customer.CustomerID)[0],//Amount
            //            GetSum(customerOrders.ToList(), customerID, a.Customer.CustomerID)[1],//Discount
            //            GetSum(customerOrders.ToList(), customerID, a.Customer.CustomerID)[2],//Final
            //            promoID,
            //            a.Customer.CustomerID,
            //            //a.VisitCount.GetValueOrDefault(0),
            //            //a.LastLogin?.ToString("dd/MM/yyyy HH:mm") ?? "---",
            //            //a.Customer.CustomerID ,
            //            //a.Customer.CustomerType.BrandId
            //            });
            //int totalDisplay = customers.Count();

            var customerOrders = orderApi.GetAllOrdersByDateWithCard(sDate, eDate, brandId)
                                .Where(q => q.StoreID == storeId);
            var customerOrderWithCard = customerOrders.Where(q => q.CustomerID != null && q.CustomerID != 0
                    && q.Att1 != null && q.Att1.StartsWith(selectedPromo.PromotionCode + ":"));

            var result = customerOrderWithCard.GroupBy(q => q.CustomerID).Select(q => new
            {
                CustomerID = q.Key.Value,
                OrderCount = q.Count(),
                SumAmount = q.Sum(d => d.TotalAmount),
                SumDiscount = q.Sum(d => d.Discount + d.DiscountOrderDetail),
                SumFinal = q.Sum(d => d.FinalAmount),
                PromoID = promoID,
                Id = q.Key.Value,
            }).Join(customerApi.GetAllCustomerByBrandId(brandId), q => q.CustomerID, p => p.CustomerID, (q, p) => new 
                  {
                    CustomerName = p.Name,
                    OrderQty = q.OrderCount,
                    SumAmount = q.SumAmount,
                    SumDiscount = q.SumDiscount,
                    SumFinal = q.SumFinal,
                    PromoId = q.PromoID,
                    Id = q.Id,
                  }).ToList();

            var res = result.Where(a => string.IsNullOrEmpty(param.sSearch) || a.CustomerName.ToString().ToLower().Contains(param.sSearch.ToLower()));

            int totalRecords = result.Count();

            int totalDisplay = res.Count();

            var rs = res.Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength);

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalDisplay,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExportPromotionStoreExcel(JQueryDataTableParamModel param, string startTime, string endTime, int brandId, int promoID, int storeId)
        {
            var customerApi = new CustomerApi();
            var promotionApi = new PromotionApi();
            var orderApi = new OrderApi(); ;
            var memberShipApi = new MembershipCardApi();
            var selectedPromo = promotionApi.Get(promoID);
            List<int> customerID = new List<int>();
            var sDate = startTime.ToDateTime().GetStartOfDate();
            var eDate = endTime.ToDateTime().GetEndOfDate();
            DateTime now = DateTime.Now;

            var customerOrders = orderApi.GetAllOrdersByDateWithCard(sDate, eDate, brandId)
                                .Where(q => q.StoreID == storeId);
            var customerOrderWithCard = customerOrders.Where(q => q.CustomerID != null && q.CustomerID != 0
                    && q.Att1 != null && q.Att1.StartsWith(selectedPromo.PromotionCode + ":"));

            var result = customerOrderWithCard.GroupBy(q => q.CustomerID).Select(q => new
            {
                CustomerID = q.Key.Value,
                OrderCount = q.Count(),
                SumAmount = q.Sum(d => d.TotalAmount),
                SumDiscount = q.Sum(d => d.Discount + d.DiscountOrderDetail),
                SumFinal = q.Sum(d => d.FinalAmount),
                PromoID = promoID,
                Id = q.Key.Value,
            }).Join(customerApi.GetAllCustomerByBrandId(brandId), q => q.CustomerID, p => p.CustomerID, (q, p) => new CustomerDetailViewModel
            {
                Name = p.Name,
                OrderCount = q.OrderCount,
                SumAmount = q.SumAmount,
                SumDiscount = q.SumDiscount,
                SumFinal = q.SumFinal,
                PromoID = q.PromoID,
                Id = q.Id,
            }).ToList();
            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo theo khuyến mãi");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên khách hàng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lần sử dụng thẻ";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số tiền";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số tiền giảm giá";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Thanh toán";
                var EndHeaderChar = StartHeaderChar;
                var EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells                
                foreach (var data in result)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.Name;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.OrderCount);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.SumAmount);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.SumDiscount);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                        "{0:0,0}", data.SumFinal);
                    StartHeaderChar = 'A';
                }
                string storeName;
                var storeApi = new StoreApi();
                if (storeId > 0)
                {
                    storeName = storeApi.GetStoreNameByID(storeId);
                }
                else
                    storeName = "Tổng quan các của hàng";
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                string fileName = "BaoCaoTheoNgay_" + "Store_" + storeName + dateRange + ".xlsx";
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }

        public double[] GetSum(List<Order> customerOrders, List<int> customerIDs, int customerId)
        {
            double sumDiscount = 0, sumAmount = 0, sumFinal = 0;
            List<double> result = new List<double>();
            for (int i = 0; i < customerIDs.Count(); i++)
            {
                if (customerIDs[i] == customerId)
                {
                    sumDiscount += customerOrders[i].Discount;
                    sumAmount += customerOrders[i].TotalAmount;
                    sumFinal += customerOrders[i].FinalAmount;
                }
            }
            result.Add(sumAmount);
            result.Add(sumDiscount);
            result.Add(sumFinal);
            return result.ToArray();
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

        #endregion

        #region CustomerOrder

        public ActionResult CustomerOrder(int customerId, int promotionId, string sTime, string eTime)
        {
            CustomerApi cApi = new CustomerApi();
            var model = cApi.GetCustomerById(customerId);
            ViewData["customerName"] = model.Name;
            ViewData["customerId"] = customerId;
            ViewData["promotionId"] = promotionId;
            ViewData["sTime"] = sTime;
            ViewData["eTime"] = eTime;

            return View();
        }

        public JsonResult LoadOrderByPromotion(JQueryDataTableParamModel param, int brandId, int storeId, int promotionId, int status, int type, int customerId, string sTime, string eTime)
        {
            if (storeId <= 0)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new List<IConvertible[]>()
                }, JsonRequestBehavior.AllowGet);
            }

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
                var listOrder = orderApi.GetRentsByTimeRange(storeId, startTime, endTime, brandId)
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

                //listOrder = listOrder.Where(q => membershipCards.Any(
                //    m => q.Att1.Equals(promotion.PromotionCode + ":" + m.MembershipCardCode)));

                var searchList = listOrder
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.InvoiceID.ToLower().Contains(param.sSearch.ToLower())).ToList();
                    
                int totalDisplay = searchList.Count();

                int count = param.iDisplayStart + 1;
                var list = searchList
                    .OrderByDescending(q => q.CheckInDate)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList().Select(a => new IConvertible[]
                        {
                        count++, // 0
                        string.IsNullOrEmpty(a.InvoiceID) ? "Không xác định" : a.InvoiceID, // 1
                        a.OrderDetailsTotalQuantity, // 2
                        a.FinalAmount, // 3
                        a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"), // 4
                        a.OrderType, // 5
                        a.CheckInPerson, // 6
                        a.RentID, // 7
                        a.OrderStatus, // 8
                        a.Customer!=null ? a.Customer.Address : "", // 9
                        a.Customer!=null ? a.Customer.Phone : "", // 10
                        a.Notes !=null? a.Notes : "", // 11
                        a.TotalAmount,// 12
                        (a.Discount + a.DiscountOrderDetail), // 14
                        });


                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = list
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