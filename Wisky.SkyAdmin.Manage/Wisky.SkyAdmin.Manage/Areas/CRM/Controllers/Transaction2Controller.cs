using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.CRM.Controllers
{
    public class Transaction2Controller : DomainBasedController
    {
        // GET: Orders/Order
        public ActionResult Index(int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            return View();
        }

        public JsonResult LoadOrder(JQueryDataTableParamModel param, int brandId, int storeId, int selectedstoreId, string _date, int status, int type)
        {

            var orderApi = new OrderApi();
            IQueryable<Order> listOrder = null;
            var rpDate = _date.ToDateTime();
            var startTime = rpDate.GetStartOfDate();
            var endTime = rpDate.GetEndOfDate();
            //filter status and type
            if (storeId == 0)
            {
                listOrder = orderApi.GetRentsByTimeRange(selectedstoreId, startTime, endTime, brandId);
            }
            else
            {
                listOrder = orderApi.GetRentsByTimeRange(storeId, startTime, endTime);
            }

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

            int count = 0;
            count = param.iDisplayStart + 1;
            var totalRecords = listOrder.Count();
            try
            {
                var list = listOrder
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.InvoiceID.ToLower().Contains(param.sSearch.ToLower()))
                    .ToList()
                    .OrderByDescending(q => q.CheckInDate)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength).OrderByDescending(a => a.CheckInDate)
                    .Select(a => new IConvertible[]
                        {
                        count++,
                        string.IsNullOrEmpty(a.InvoiceID) ? "Không xác định" : a.InvoiceID,
                        a.OrderDetailsTotalQuantity,
                        a.FinalAmount,
                        a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                        a.OrderType,
                        a.CheckInPerson,
                        a.RentID,
                        a.OrderStatus,
                        a.Customer!=null ? a.Customer.Name : "",
                        a.Customer!=null ? a.Customer.Address : "",
                        a.Customer!=null ? a.Customer.Phone : "",
                        a.Notes !=null? a.Notes : "",
                        a.TotalAmount,
                        a.FinalAmount,
                        (a.Discount + a.DiscountOrderDetail),
                        a.Store.Name,
                        });

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = list
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        //public ActionResult OrderDetail(int brandId, int Id)
        //{
        //    var orderApi = new OrderApi();
        //    var order = orderApi.GetOrderByIdAndBrandId(brandId,Id);
        //    return View(order);
        //}

        public JsonResult LoadOrderDetail(JQueryDataTableParamModel param, int id)
        {
            var orderDetailApi = new OrderDetailApi();
            var orderDetail = orderDetailApi.GetOrderDetailsByRentId(id)
                .OrderBy(a => a.OrderDate)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength);
            int count = 1;
            var totalCount = orderDetail.Count();
            var list = orderDetail.Select(a => new IConvertible[]
            {
                count++,
                a.Product.ProductName,
                a.UnitPrice,
                a.Quantity,
                a.Status,
                a.Discount,
                a.FinalAmount,
                a.RentID,
            });
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalCount,
                iTotalDisplayRecords = totalCount,
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }

        // Hàm xuất file excel
        public ActionResult ExportOrderTableToExcel(int _id, int storeId, int brandId)
        {
            var storeService = this.Service<IStoreService>();
            var orderService = this.Service<IOrderService>();
            var orderDetailService = this.Service<IOrderDetailService>();
            var storeApi = new StoreApi();
            var orderApi = new OrderApi();
            var orderDetailApi = new OrderDetailApi();
            var storeName = "";
            if (storeId > 0)
            {
                storeName = storeService.Get(storeId).Name;
            }
            else
            {
                storeName = "Service";
            }

            if (storeId > 0)
            {
                var totalModel = orderDetailApi.GetOrderDetailsByRentId(_id)
                  .Select(g => new
                  {
                      productName = g.Product.ProductName,
                      Price = g.UnitPrice,
                      quality = g.Quantity,
                      discount = g.Discount,
                      finalAmount = g.FinalAmount
                  });
                //var totalModel = orderApi.GetRentsByTimeRange2(storeId, fromDate, toDate)
                //  .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish)
                //  .GroupBy(a => a.CheckInPerson)
                //  .Select(g => new
                //  {
                //      FullName = g.Key == null ? "N/A" : aspNetUserApi.GetUserByUsername(g.Key).FullName,
                //      Username = g.Key == null ? "N/A" : g.Key,
                //      TotalBill = g.Count(),
                //      FinalAmount = g.Sum(x => x.FinalAmount)
                //  });

                #region Export to Excel
                int count = 0;
                MemoryStream ms = new MemoryStream();
                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("ChiTietHoaDon");
                    char StartHeaderChar = 'A';
                    int StartHeaderNumber = 1;
                    #region Headers
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên sản phẩm";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giá sản phẩm";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Tình trạng";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giảm giá";
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
                    foreach (var data in totalModel)
                    {
                        ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = ++count;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.productName;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.Price);
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.quality;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.discount);
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.finalAmount);
                        StartHeaderChar = 'A';
                    }
                    #endregion

                    //Set style for excel
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    package.SaveAs(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var fileDownloadName = "ChiTietHoaDon_" + storeName + "_TổngQuanNgày.xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return this.File(ms, contentType, fileDownloadName);
                }
                #endregion
            }
            else
            {
                var totalModel = orderDetailApi.GetOrderDetailsByRentId(_id)
                  .Select(g => new
                  {
                      productName = g.Product.ProductName,
                      Price = g.UnitPrice,
                      quality = g.Quantity,
                      discount = g.Discount,
                      finalAmount = g.FinalAmount
                  });

                #region Export to Excel
                int count = 0;
                MemoryStream ms = new MemoryStream();
                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("TongQuanNhanVien");
                    char StartHeaderChar = 'A';
                    int StartHeaderNumber = 1;
                    #region Headers
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên sản phẩm";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giá sản phẩm";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giảm giá";
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
                    foreach (var data in totalModel)
                    {
                        ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = ++count;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.productName;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.Price);
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.quality;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.discount);
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.finalAmount);
                        StartHeaderChar = 'A';
                    }
                    #endregion

                    //Set style for excel
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    package.SaveAs(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var fileDownloadName = "ChiTietHoaDon" + storeName + "_TổngQuanNgày.xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return this.File(ms, contentType, fileDownloadName);
                }
                #endregion
            }
        }
        #region lấy danh sách các store
        public JsonResult LoadStoreList(int brandId)
        {
            var storeapi = new StoreApi();
            var stores = storeapi.GetActiveStoreByBrandId(brandId).ToArray();
            return Json(new
            {
                store = stores,
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadOrderByPromotion(JQueryDataTableParamModel param, int brandId, int storeId, int promotionId, string _date, int status, int type)
        {

            var orderApi = new OrderApi();
            var membershipCardApi = new MembershipCardApi();
            var customerApi = new CustomerApi();

            var rpDate = _date.ToDateTime();
            var startTime = rpDate.GetStartOfDate();
            var endTime = rpDate.GetEndOfDate();
            //filter status and type
            var listOrder = orderApi.GetRentsByTimeRange(storeId, startTime, endTime, brandId).Where(o => o.OrderPromotionMappings.Any(m => m.PromotionId == promotionId));

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

            int count = 0;
            count = param.iDisplayStart + 1;
            var totalRecords = listOrder.Count();
            try
            {
                var searchList = listOrder
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.InvoiceID.ToLower().Contains(param.sSearch.ToLower()))
                    .OrderByDescending(q => q.CheckInDate)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList();

                //var list = searchList.Select(a => new IConvertible[]
                //        {
                //        count++,
                //        string.IsNullOrEmpty(a.InvoiceID) ? "Không xác định" : a.InvoiceID,
                //        a.OrderDetailsTotalQuantity,
                //        a.FinalAmount,
                //        a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                //        a.OrderType,
                //        a.CheckInPerson,
                //        a.RentID,
                //        a.OrderStatus,
                //        //a.Customer!=null ? a.Customer.Name : "",
                //        //a.Customer!=null ? a.Customer.Address : "",
                //        //a.Customer!=null ? a.Customer.Phone : "",
                //        a.Notes !=null? a.Notes : "",
                //        a.TotalAmount,
                //        a.FinalAmount,
                //        (a.Discount + a.DiscountOrderDetail),
                //        });

                var list = new List<dynamic>();
                foreach (var a in searchList)
                {
                    // Lấy membershipcard code từ Att1
                    var cardCode = "";
                    if (a.Att1 != null && a.Att1.Contains(":"))
                    {
                        cardCode = a.Att1.Substring(a.Att1.LastIndexOf(":") + 1);
                    }
                    var membershipCard = membershipCardApi.GetMembershipCardByCode(cardCode);
                    Customer customer = null;
                    if (membershipCard != null && membershipCard.Active == true
                    && membershipCard.Status == (int)MembershipStatusEnum.Active /* chưa kiểm tra membershipType */)
                    {
                        customer = customerApi.GetCustomerEntityById(membershipCard.CustomerId.GetValueOrDefault());
                    }

                    var rowData = new List<IConvertible>();

                    rowData.Add(count++); // 0
                    rowData.Add(string.IsNullOrEmpty(a.InvoiceID) ? "Không xác định" : a.InvoiceID); // 1
                    rowData.Add(a.OrderDetailsTotalQuantity); // 2
                    rowData.Add(a.FinalAmount); // 3
                    rowData.Add(a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss")); // 4
                    rowData.Add(a.OrderType); // 5
                    rowData.Add(a.CheckInPerson); //6
                    rowData.Add(a.RentID); // 7
                    rowData.Add(a.OrderStatus); // 8
                    rowData.Add(customer != null ? customer.Name : ""); // 9
                    rowData.Add(customer != null ? customer.Address : ""); // 10
                    rowData.Add(customer != null ? customer.Phone : ""); // 11
                    rowData.Add(a.Notes != null ? a.Notes : ""); // 12
                    rowData.Add(a.TotalAmount); // 13
                    rowData.Add(a.FinalAmount); // 14
                    rowData.Add(a.Discount + a.DiscountOrderDetail); // 15

                    list.Add(rowData.ToArray());
                }


                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = list
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}