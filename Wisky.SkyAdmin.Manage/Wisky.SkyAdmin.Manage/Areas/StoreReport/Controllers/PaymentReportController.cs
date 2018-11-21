using HmsService.Models;
using HmsService.Sdk;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Areas.SystemReport.Controllers;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.StoreReport.Controllers
{
    public class PaymentReportController : DomainBasedController
    {
        // GET: StoreReport/PaymentReport
        public ActionResult Index(int brandId, int storeId)
        {
            ViewBag.Stores = null;
            return View();
        }

        public JsonResult GetPaymentReportData(JQueryDataTableParamModel param, int storeId, int brandId, string sTime, string eTime)
        {
            try
            {
                var startTime = sTime.ToDateTime().GetStartOfDate();
                var endTime = eTime.ToDateTime().GetEndOfDate();
                int totalDisplay = (int)endTime.Subtract(startTime).TotalDays;

                endTime = endTime.AddDays(-param.iDisplayStart);

                if (startTime < endTime.AddDays(-param.iDisplayLength + 1))
                {
                    startTime = endTime.AddDays(-param.iDisplayLength + 1).GetStartOfDate();
                }



                var paymenApi = new PaymentApi();
                var paymentInDateRange = paymenApi.GetEntityStorePaymentInDateRange(storeId, startTime, endTime, brandId).GroupBy(q => DbFunctions.TruncateTime(q.PayTime)).ToList();
                var datetime = startTime.AddDays(-1);
                var listresult = new List<PaymentReportData>();
                for (var d = endTime; d > startTime; d = d.AddDays(-1))
                {
                    var dataReportTmp = new PaymentReportData();
                    dataReportTmp.time = d.ToString("dd/MM/yyyy");
                    foreach (var item in paymentInDateRange)
                    {
                        if (item.Key.Value.ToString("dd/MM/yyyy") == dataReportTmp.time)
                        {
                            dataReportTmp.cash = item.Where(a => a.Type == (int)PaymentTypeEnum.Cash).Sum(a => a.Amount) + item.Where(a => a.Type == (int)PaymentTypeEnum.ExchangeCash).Sum(a => a.Amount);
                            dataReportTmp.creditCard = item.Where(a => a.Type == (int)PaymentTypeEnum.MemberPayment).Sum(a => a.Amount);//Sử dụng điểm từ MemberCard để mua hàng
                            dataReportTmp.voucher = item.Where(a => a.Type == (int)PaymentTypeEnum.Voucher).Sum(a => a.Amount);
                            dataReportTmp.bank = item.Where(a => a.Type == (int)PaymentTypeEnum.MasterCard).Sum(a => a.Amount)
                                + item.Where(a => a.Type == (int)PaymentTypeEnum.VisaCard).Sum(a => a.Amount);
                            dataReportTmp.debt = item.Where(a => a.Type == (int)PaymentTypeEnum.Debt).Sum(a => a.Amount);
                            dataReportTmp.orther = item.Where(a => a.Type == (int)PaymentTypeEnum.Other).Sum(a => a.Amount);
                            // lấy phần tử đầu tiên, vì đã group by 
                            break;
                        }
                    }
                    listresult.Add(dataReportTmp);
                }

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalDisplay,
                    iTotalDisplayRecords = totalDisplay,
                    aaData = listresult,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ExportPaymentStoreExcel(JQueryDataTableParamModel param, int storeId, int brandId, string sTime, string eTime)
        {
            var startTime = sTime.ToDateTime().GetStartOfDate();
            var endTime = eTime.ToDateTime().GetEndOfDate();

            var paymenApi = new PaymentApi();
            var paymentInDateRange = paymenApi.GetEntityStorePaymentInDateRange(storeId, startTime, endTime, brandId).GroupBy(q => DbFunctions.TruncateTime(q.PayTime)).ToList();
            var datetime = startTime.AddDays(-1);
            var listresult = new List<PaymentReportData>();
            for (var d = endTime; d > startTime; d = d.AddDays(-1))
            {
                var dataReportTmp = new PaymentReportData();
                dataReportTmp.time = d.ToString("dd/MM/yyyy");
                foreach (var item in paymentInDateRange)
                {
                    if (item.Key.Value.ToString("dd/MM/yyyy") == dataReportTmp.time)
                    {
                        dataReportTmp.cash = item.Where(a => a.Type == (int)PaymentTypeEnum.Cash).Sum(a => a.Amount) + item.Where(a => a.Type == (int)PaymentTypeEnum.ExchangeCash).Sum(a => a.Amount);
                        dataReportTmp.creditCard = item.Where(a => a.Type == (int)PaymentTypeEnum.MemberPayment).Sum(a => a.Amount);//Sử dụng điểm từ MemberCard để mua hàng
                        dataReportTmp.voucher = item.Where(a => a.Type == (int)PaymentTypeEnum.Voucher).Sum(a => a.Amount);
                        dataReportTmp.bank = item.Where(a => a.Type == (int)PaymentTypeEnum.MasterCard).Sum(a => a.Amount)
                            + item.Where(a => a.Type == (int)PaymentTypeEnum.VisaCard).Sum(a => a.Amount);
                        dataReportTmp.debt = item.Where(a => a.Type == (int)PaymentTypeEnum.Debt).Sum(a => a.Amount);
                        dataReportTmp.orther = item.Where(a => a.Type == (int)PaymentTypeEnum.Other).Sum(a => a.Amount);
                        // lấy phần tử đầu tiên, vì đã group by 
                        break;
                    }
                }
                listresult.Add(dataReportTmp);
            }

            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo theo hình thức thanh toán");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thanh toán tiền mặt";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thẻ thành viên";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Vourcher";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "MasterCard";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "VisaCard";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Khác";
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
                foreach (var data in listresult)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.time;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                                    "{0:0,0}", data.cash);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                                    "{0:0,0}", data.creditCard);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                                    "{0:0,0}", data.bank);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                                    "{0:0,0}", data.voucher);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                                    "{0:0,0}", data.debt);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture,
                                                                                                                    "{0:0,0}", data.orther);
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
                var dateRange = "(" + sTime + (sTime == eTime ? "" : " - " + eTime) + ")";
                string fileName = "BaoCaoThanhToan_" + "Store_" + storeName + dateRange + ".xlsx";
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

        public async Task<JsonResult> GetAllPaymentData(JQueryDataTableParamModel param, int storeId, int brandId, string sTime, string eTime)
        {
            var startTime = sTime.ToDateTime().GetStartOfDate();
            var endTime = eTime.ToDateTime().GetEndOfDate();
            storeId = storeId == 0 ? -1 : storeId;
            var paymentApi = new PaymentApi();
            var payments = paymentApi.GetStorePaymentVMInDateRange(storeId, startTime, endTime, brandId);
            var query = payments.GroupBy(q => DbFunctions.TruncateTime(q.PayTime));
            var rs = (await query
                        .OrderBy(q => q.Key)
                        .Skip(param.iDisplayStart)
                        .Take(param.iDisplayLength)
                        .ToListAsync())
                        .Select(q =>
                            new
                            {
                                Date = q.Key.Value.ToString("dd/MM/yyyy"),
                                CashAmount = q.Where(a => a.Type == (int)PaymentTypeEnum.Cash || a.Type == (int)PaymentTypeEnum.ExchangeCash).Sum(a => a.Amount),
                                CardAmount = q.Where(a => a.Type == (int)PaymentTypeEnum.Card).Sum(a => a.Amount),
                                MemberCard = q.Where(a => a.Type == (int)PaymentTypeEnum.MemberPayment).Sum(a => a.Amount),//Sử dụng điểm từ MemberCard để mua hàng
                                Voucher = q.Where(a => a.Type == (int)PaymentTypeEnum.Voucher).Sum(a => a.Amount),
                                AccountReceivable = q.Where(a => a.Type == (int)PaymentTypeEnum.AccountReceivable).Sum(a => a.Amount),
                                Other = q.Where(a => a.Type == (int)PaymentTypeEnum.Other).Sum(a => a.Amount),
                                MasterCard = q.Where(a => a.Type == (int)PaymentTypeEnum.MasterCard).Sum(a => a.Amount),
                                VisaCard = q.Where(a => a.Type == (int)PaymentTypeEnum.VisaCard).Sum(a => a.Amount),
                                PaymentMember = q.Where(a => a.Type == (int)PaymentTypeEnum.PaymentMember).Sum(a => a.Amount)//Nạp tiền vào tk để quy đổi thành điểm
                            });
            var totalDisplay = await query.CountAsync();

            //var orders = orderApi.GetRentsByTimeRange(storeId, startTime, endTime, brandId);
            //if (!string.IsNullOrWhiteSpace(param.sSearch))
            //{
            //    orders = orders.Where(q => q.InvoiceID.Contains(param.sSearch));
            //}
            //var rs = orders.Join(query, r => r.RentID, q => q.OrderId, (r, q) => new
            //{
            //    orderId = q.OrderId,
            //    date = r.CheckInDate,
            //    quantity = r.OrderDetailsTotalQuantity,
            //    paymentAmount = q.PaymentAmount,
            //    finalAmount = r.FinalAmount,
            //    orderType = r.OrderType,
            //    orderStatus = r.OrderStatus,
            //    cashier = r.CheckInPerson
            //}).ToList();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalDisplay,
                iTotalDisplayRecords = totalDisplay,
                aaData = rs,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}