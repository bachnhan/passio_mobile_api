using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Orders.Controllers
{
    public class OrderController : DomainBasedController
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

            if (!string.IsNullOrWhiteSpace(param.sSearch))
            {
                listOrder = listOrder.Where(q => q.InvoiceID.ToLower().Contains(param.sSearch.ToLower()));
            }
            int count = 0;
            count = param.iDisplayStart + 1;

            //try
            //{
            var result = listOrder
                .OrderByDescending(q => q.CheckInDate)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength)
                .ToList();
            var list = result.Select(a => new object[]
                    {
                        count++,
                        string.IsNullOrEmpty(a.InvoiceID) ? "Không xác định" : a.InvoiceID,
                        a.OrderDetailsTotalQuantity,
                        a.FinalAmount,
                        a.Payments.Select(q => ((PaymentTypeEnum) q.Type).DisplayName()).Distinct().ToArray(),
                        a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                        a.OrderType,
                        a.CheckInPerson,
                        a.RentID,
                        a.OrderStatus,
                        //a.Customer!=null ? a.Customer.Name : "",
                        //a.Customer!=null ? a.Customer.Address : "",
                        //a.Customer!=null ? a.Customer.Phone : "",
                        //a.Notes !=null? a.Notes : "",
                        //a.TotalAmount,
                        //a.FinalAmount,
                        //(a.Discount + a.DiscountOrderDetail),
                        //a.Store.Name
                    });
            var totalRecords = listOrder.Count();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = list
            }, JsonRequestBehavior.AllowGet);
            //}
            //catch (Exception e)
            //{
            //    Console.Write(e.Message);
            //    return Json(new
            //    {
            //        sEcho = param.sEcho,
            //        iTotalRecords = totalRecords,
            //        iTotalDisplayRecords = totalRecords,
            //        aaData = new List<dynamic>()
            //    }, JsonRequestBehavior.AllowGet);
            //}
        }

        //public ActionResult OrderDetail(int brandId, int Id)
        //{
        //    var orderApi = new OrderApi();
        //    var order = orderApi.GetOrderByIdAndBrandId(brandId,Id);
        //    return View(order);
        //}


        public JsonResult LoadOrderDetail(int id)
        {
            var orderlApi = new OrderApi();
            var order = orderlApi.Get(id);
            var orderDetail = order.OrderDetails.OrderBy(q => q.OrderDate);
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

            var lblData = new
            {
                cusName = order.Customer != null ? order.Customer.Name : "N/A",
                cusAddr = string.IsNullOrEmpty(order.DeliveryAddress) ? "N/A" : order.DeliveryAddress,
                cusPhone = order.Customer != null ? order.Customer.Phone : "N/A",
                notes = order.Notes != null ? order.Notes : "",
                finalAmount = order.FinalAmount,
                totalAmount = order.TotalAmount,
                totalDiscount = (order.Discount + order.DiscountOrderDetail),
                store = order.Store.Name,
                payment = order.Payments.GroupBy(q => q.Type)
                                .Select(a => new
                                {
                                    type = ((PaymentTypeEnum)a.Key).DisplayName(),
                                    amount = a.Sum(z => z.Amount)
                                }).ToArray(),

                checkInTime = (order.CheckInDate != null) ? order.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                invoiceId = string.IsNullOrEmpty(order.InvoiceID) ? "Không xác định" : order.InvoiceID,
                checkInPerson = (order.CheckInPerson != null) ? order.CheckInPerson : "N/A",
                orderStatus = order.OrderType,
            };

            return Json(new
            {
                dataTable = list,
                aaData = list,//fill data into aaData
                lblData = lblData
            }, JsonRequestBehavior.AllowGet);
        }

        // Hàm xuất file excel
        public ActionResult ExportOrderDetailTableToExcel(int _id, int storeId, int brandId)
        {
            var storeService = this.Service<IStoreService>();
            var orderService = this.Service<IOrderService>();
            var orderDetailService = this.Service<IOrderDetailService>();
            var storeApi = new StoreApi();
            var orderApi = new OrderApi();
            var order = orderApi.Get(_id);
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
                    var fileDownloadName = "ChiTietHoaDon_" + order.InvoiceID + "_CH_" + storeName + "_" + (order.CheckInDate!=null?order.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"): "N/A")+ ".xlsx";
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

        public ActionResult ExportOrderToExcel(int brandId, int storeId, int selectedstoreId, string _date, int status, int type)
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
            var listOrderEnum = listOrder.ToList();
            int count = 0;
            var listOrderCashPayment = listOrderEnum.Where(q => q.Payments!=null && q.Payments.Select(a => a.Type).ToList().Contains((int)PaymentTypeEnum.Cash)).Select(a => new
            {
                STT = count++,
                InvoiceID = string.IsNullOrEmpty(a.InvoiceID) ? "Không xác định" : a.InvoiceID,
                Quantity = a.OrderDetailsTotalQuantity,
                FinalAmount = a.FinalAmount,
                //PaymentType = a.Payments.Select(q => ((PaymentTypeEnum) q.Type).DisplayName()).Distinct().ToArray(),
                CheckInDate = a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                OrderType = ((OrderTypeEnum)a.OrderType).DisplayName(),
                CheckInPerson = a.CheckInPerson,
                RentId = a.RentID,
                OrderStatus = ((OrderStatusEnum)a.OrderStatus).DisplayName(),
                //CustomerName = a.Customer != null ? a.Customer.Name : "N/A",
                //CustomerAddress = a.Customer != null ? a.Customer.Address : "N/A",
                //CustomerPhone = a.Customer != null ? a.Customer.Phone : "",
                Note = a.Notes != null ? a.Notes : "N/A",
                //StoreName = a.Store.Name
            });
            count = 0;
            var listOrderMemberCardPayment = listOrderEnum.Where(q => q.Payments != null && q.Payments.Select(a => a.Type).ToList().Contains((int)PaymentTypeEnum.MemberPayment)).Select(a => new
            {
                STT = count++,
                InvoiceID = string.IsNullOrEmpty(a.InvoiceID) ? "Không xác định" : a.InvoiceID,
                Quantity = a.OrderDetailsTotalQuantity,
                FinalAmount = a.FinalAmount,
                //PaymentType = a.Payments.Select(q => ((PaymentTypeEnum) q.Type).DisplayName()).Distinct().ToArray(),
                CheckInDate = a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                OrderType = ((OrderTypeEnum)a.OrderType).DisplayName(),
                CheckInPerson = a.CheckInPerson,
                RentId = a.RentID,
                OrderStatus = ((OrderStatusEnum)a.OrderStatus).DisplayName(),
                //CustomerName = a.Customer != null ? a.Customer.Name : "N/A",
                //CustomerAddress = a.Customer != null ? a.Customer.Address : "N/A",
                //CustomerPhone = a.Customer != null ? a.Customer.Phone : "",
                Note = a.Notes != null ? a.Notes : "N/A",
                //StoreName = a.Store.Name
            });
            count = 0;
            var listOrderBankPayment = listOrderEnum.Where(q => q.Payments != null && (q.Payments.Select(a => a.Type).ToList().Contains((int)PaymentTypeEnum.MasterCard)
                                        || q.Payments.Select(a => a.Type).ToList().Contains((int)PaymentTypeEnum.VisaCard))).Select(a => new
                                        {
                                            STT = count++,
                                            InvoiceID = string.IsNullOrEmpty(a.InvoiceID) ? "Không xác định" : a.InvoiceID,
                                            Quantity = a.OrderDetailsTotalQuantity,
                                            FinalAmount = a.FinalAmount,
                                            //PaymentType = a.Payments.Select(q => ((PaymentTypeEnum) q.Type).DisplayName()).Distinct().ToArray(),
                                            CheckInDate = a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                                            OrderType = ((OrderTypeEnum)a.OrderType).DisplayName(),
                                            CheckInPerson = a.CheckInPerson,
                                            RentId = a.RentID,
                                            OrderStatus = ((OrderStatusEnum)a.OrderStatus).DisplayName(),
                                            //CustomerName = a.Customer != null ? a.Customer.Name : "N/A",
                                            //CustomerAddress = a.Customer != null ? a.Customer.Address : "N/A",
                                            //CustomerPhone = a.Customer != null ? a.Customer.Phone : "",
                                            Note = a.Notes != null ? a.Notes : "N/A",
                                            //StoreName = a.Store.Name
                                        });

            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                #region Thanh toán tiền mặt
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Thanh toán bằng tiền mặt");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Mã hóa đơn";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thanh toán";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Hình thức thanh toán";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thời gian";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Loại hóa đơn";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tình trạng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Nhân viên";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Khách hàng";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Địa chỉ";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Điện thoại";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Ghi chú";
                //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Cửa hàng";
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
                foreach (var data in listOrderCashPayment)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.STT;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.InvoiceID;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.FinalAmount);
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PaymentType;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CheckInDate;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.OrderType;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.OrderStatus;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CheckInPerson;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerName;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerAddress;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerPhone;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Note;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.StoreName;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion

                #region Thanh toán bằng thẻ thành viên
                ExcelWorksheet ws1 = package.Workbook.Worksheets.Add("Thanh toán bằng Thẻ thành viên");
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #region Headers
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Mã hóa đơn";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thanh toán";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Hình thức thanh toán";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thời gian";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Loại hóa đơn";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tình trạng";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Nhân viên";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Khách hàng";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Địa chỉ";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Điện thoại";
                ws1.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Ghi chú";
                //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Cửa hàng";
                EndHeaderChar = StartHeaderChar;
                EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws1.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws1.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws1.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws1.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws1.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells                
                foreach (var data in listOrderMemberCardPayment)
                {
                    ws1.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.STT;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.InvoiceID;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.FinalAmount);
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PaymentType;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CheckInDate;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.OrderType;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.OrderStatus;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CheckInPerson;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerName;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerAddress;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerPhone;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Note;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.StoreName;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws1.Cells[ws.Dimension.Address].AutoFitColumns();
                ws1.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws1.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws1.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws1.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion

                #region Thanh toán bằng thẻ ngân hàng
                ExcelWorksheet ws2 = package.Workbook.Worksheets.Add("Thanh toán bằng Thẻ ngân hàng");
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #region Headers
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Mã hóa đơn";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thanh toán";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Hình thức thanh toán";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thời gian";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Loại hóa đơn";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tình trạng";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Nhân viên";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Khách hàng";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Địa chỉ";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Điện thoại";
                ws2.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Ghi chú";
                //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Cửa hàng";
                EndHeaderChar = StartHeaderChar;
                EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws2.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws2.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws2.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws2.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws2.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells                
                foreach (var data in listOrderBankPayment)
                {
                    ws2.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.STT;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.InvoiceID;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.FinalAmount);
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PaymentType;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CheckInDate;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.OrderType;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.OrderStatus;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CheckInPerson;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerName;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerAddress;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerPhone;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Note;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.StoreName;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws2.Cells[ws.Dimension.Address].AutoFitColumns();
                ws2.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws2.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws2.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws2.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion


                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var storeApi = new StoreApi();
                var storeName = "";
                if (storeId == 0)
                {
                    if (selectedstoreId == 0)
                    {
                        storeName = "Hệ thống";
                    }
                    else
                    {
                        storeName = storeApi.GetStoreById(selectedstoreId).Name;
                    }
                }
                else
                {
                    storeName = storeApi.GetStoreById(storeId).Name;
                }
                var fileDownloadName = "Danh sách hóa đơn: "+storeName+" ("+_date+").xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
            #endregion

        }

        public ActionResult ExportOrderToExcel2(int brandId, int storeId, int selectedstoreId, string sTime, string eTime, int status, int type)
            {
            var orderApi = new OrderApi();
            IQueryable<Order> listOrder = null;
            var startTime = sTime.ToDateTime().GetStartOfDate();
            var endTime = eTime.ToDateTime().GetEndOfDate();
            //filter status and type
            if (selectedstoreId == 0)
            {
                listOrder = orderApi.GetRentsByTimeRange(storeId, startTime, endTime);
            }
            else
            {
                listOrder = orderApi.GetRentsByTimeRange(selectedstoreId, startTime, endTime, brandId);
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
            var listOrderEnum = listOrder.ToList();
            int count = 0;
            var listOrderCashPayment = listOrderEnum.Where(q => q.Payments != null && q.Payments.Select(a => a.Type).ToList().Contains((int)PaymentTypeEnum.Cash)).Select(a => new
            {
                STT = count++,
                InvoiceID = string.IsNullOrEmpty(a.InvoiceID) ? "Không xác định" : a.InvoiceID,
                Quantity = a.OrderDetailsTotalQuantity,
                FinalAmount = a.FinalAmount,
                //PaymentType = a.Payments.Select(q => ((PaymentTypeEnum) q.Type).DisplayName()).Distinct().ToArray(),
                CheckInDate = a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                OrderType = ((OrderTypeEnum)a.OrderType).DisplayName(),
                CheckInPerson = a.CheckInPerson,
                RentId = a.RentID,
                OrderStatus = ((OrderStatusEnum)a.OrderStatus).DisplayName(),
                //CustomerName = a.Customer != null ? a.Customer.Name : "N/A",
                //CustomerAddress = a.Customer != null ? a.Customer.Address : "N/A",
                //CustomerPhone = a.Customer != null ? a.Customer.Phone : "",
                Note = a.Notes != null ? a.Notes : "N/A",
                //StoreName = a.Store.Name
            });
            count = 0;
            var listOrderMemberCardPayment = listOrderEnum.Where(q => q.Payments != null && q.Payments.Select(a => a.Type).ToList().Contains((int)PaymentTypeEnum.MemberPayment)).Select(a => new
            {
                STT = count++,
                InvoiceID = string.IsNullOrEmpty(a.InvoiceID) ? "Không xác định" : a.InvoiceID,
                Quantity = a.OrderDetailsTotalQuantity,
                FinalAmount = a.FinalAmount,
                //PaymentType = a.Payments.Select(q => ((PaymentTypeEnum) q.Type).DisplayName()).Distinct().ToArray(),
                CheckInDate = a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                OrderType = ((OrderTypeEnum)a.OrderType).DisplayName(),
                CheckInPerson = a.CheckInPerson,
                RentId = a.RentID,
                OrderStatus = ((OrderStatusEnum)a.OrderStatus).DisplayName(),
                //CustomerName = a.Customer != null ? a.Customer.Name : "N/A",
                //CustomerAddress = a.Customer != null ? a.Customer.Address : "N/A",
                //CustomerPhone = a.Customer != null ? a.Customer.Phone : "",
                Note = a.Notes != null ? a.Notes : "N/A",
                //StoreName = a.Store.Name
            });
            count = 0;
            var listOrderBankPayment = listOrderEnum.Where(q => q.Payments != null && (q.Payments.Select(a => a.Type).ToList().Contains((int)PaymentTypeEnum.MasterCard)
                                        || q.Payments.Select(a => a.Type).ToList().Contains((int)PaymentTypeEnum.VisaCard))).Select(a => new
                                        {
                                            STT = count++,
                                            InvoiceID = string.IsNullOrEmpty(a.InvoiceID) ? "Không xác định" : a.InvoiceID,
                                            Quantity = a.OrderDetailsTotalQuantity,
                                            FinalAmount = a.FinalAmount,
                                            //PaymentType = a.Payments.Select(q => ((PaymentTypeEnum) q.Type).DisplayName()).Distinct().ToArray(),
                                            CheckInDate = a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                                            OrderType = ((OrderTypeEnum)a.OrderType).DisplayName(),
                                            CheckInPerson = a.CheckInPerson,
                                            RentId = a.RentID,
                                            OrderStatus = ((OrderStatusEnum)a.OrderStatus).DisplayName(),
                                            //CustomerName = a.Customer != null ? a.Customer.Name : "N/A",
                                            //CustomerAddress = a.Customer != null ? a.Customer.Address : "N/A",
                                            //CustomerPhone = a.Customer != null ? a.Customer.Phone : "",
                                            Note = a.Notes != null ? a.Notes : "N/A",
                                            //StoreName = a.Store.Name
                                        });

            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                #region Thanh toán tiền mặt
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Thanh toán bằng tiền mặt");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Mã hóa đơn";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thanh toán";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Hình thức thanh toán";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thời gian";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Loại hóa đơn";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tình trạng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Nhân viên";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Khách hàng";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Địa chỉ";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Điện thoại";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Ghi chú";
                //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Cửa hàng";
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
                foreach (var data in listOrderCashPayment)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.STT;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.InvoiceID;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.FinalAmount);
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PaymentType;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CheckInDate;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.OrderType;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.OrderStatus;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CheckInPerson;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerName;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerAddress;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerPhone;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Note;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.StoreName;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion

                #region Thanh toán bằng thẻ thành viên
                ExcelWorksheet ws1 = package.Workbook.Worksheets.Add("Thanh toán bằng Thẻ thành viên");
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #region Headers
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Mã hóa đơn";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thanh toán";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Hình thức thanh toán";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thời gian";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Loại hóa đơn";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tình trạng";
                ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Nhân viên";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Khách hàng";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Địa chỉ";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Điện thoại";
                ws1.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Ghi chú";
                //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Cửa hàng";
                EndHeaderChar = StartHeaderChar;
                EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws1.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws1.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws1.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws1.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws1.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells                
                foreach (var data in listOrderMemberCardPayment)
                {
                    ws1.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.STT;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.InvoiceID;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.FinalAmount);
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PaymentType;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CheckInDate;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.OrderType;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.OrderStatus;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CheckInPerson;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerName;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerAddress;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerPhone;
                    ws1.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Note;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.StoreName;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws1.Cells[ws.Dimension.Address].AutoFitColumns();
                ws1.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws1.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws1.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws1.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion

                #region Thanh toán bằng thẻ ngân hàng
                ExcelWorksheet ws2 = package.Workbook.Worksheets.Add("Thanh toán bằng Thẻ ngân hàng");
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #region Headers
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Mã hóa đơn";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thanh toán";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Hình thức thanh toán";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thời gian";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Loại hóa đơn";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tình trạng";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Nhân viên";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Khách hàng";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Địa chỉ";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Điện thoại";
                ws2.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Ghi chú";
                //ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Cửa hàng";
                EndHeaderChar = StartHeaderChar;
                EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws2.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws2.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws2.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws2.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws2.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells                
                foreach (var data in listOrderBankPayment)
                {
                    ws2.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.STT;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.InvoiceID;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.FinalAmount);
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PaymentType;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CheckInDate;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.OrderType;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.OrderStatus;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CheckInPerson;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerName;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerAddress;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CustomerPhone;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Note;
                    //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.StoreName;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws2.Cells[ws.Dimension.Address].AutoFitColumns();
                ws2.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws2.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws2.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws2.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion


                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var storeApi = new StoreApi();
                var storeName = "";
                if (storeId == 0)
                {
                    if (selectedstoreId == 0)
                    {
                        storeName = "Hệ thống";
                    }
                    else
                    {
                        storeName = storeApi.GetStoreById(selectedstoreId).Name;
                    }
                }
                else
                {
                    storeName = storeApi.GetStoreById(storeId).Name;
                }
                var fileDownloadName = "Danh sách hóa đơn: " + storeName + " (" + startTime + "->" + endTime  + ").xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
            #endregion

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
            IEnumerable<Order> listOrder = orderApi.GetRentsByTimeRange(storeId, startTime, endTime, brandId)
                .Where(o => o.OrderPromotionMappings.Any(m => m.PromotionId == promotionId)).ToList();
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

            int count = param.iDisplayStart + 1;
            try
            {
                var searchList = listOrder
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.InvoiceID.ToLower().Contains(param.sSearch.ToLower()));
                var totalDisplayRecords = searchList.Count();

                var rs = searchList
                    .OrderByDescending(q => q.CheckInDate)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength);

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
                foreach (var a in rs)
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
                    iTotalDisplayRecords = totalDisplayRecords,
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