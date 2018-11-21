using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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
using HmsService.ViewModels;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.SystemReport.Controllers
{
    public class PaymentReportController : DomainBasedController
    {
        // GET: StoreReport/PaymentReport
        public ActionResult Index(int brandId, int storeId)
        {
            ViewBag.brandId = brandId;
            ViewBag.storeId = storeId;
            return View();
        }

        public ActionResult ListCashPayment(int brandId, int storeId)
        {
            ViewBag.brandId = brandId;
            ViewBag.storeId = storeId;
            return View();
        }

        public ActionResult ListMemberPayment(int brandId, int storeId)
        {
            return View();
        }

        public JsonResult LoadStoreList(int brandId)
        {
            var storeapi = new StoreApi();
            var stores = storeapi.GetActiveStoreByBrandId(brandId).ToArray();
            return Json(new
            {
                store = stores,
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get data Payment report
        /// </summary>
        /// <param name="param"></param>
        /// <param name="storeId"></param>
        /// <param name="brandId"></param>
        /// <param name="sTime"></param>
        /// <param name="eTime"></param>
        /// <returns>Json data report</returns>
        public JsonResult GetPaymentReportData(JQueryDataTableParamModel param, int? selectedStoreId, int brandId, string sTime, string eTime)
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
                int storeId = 0;
                if (selectedStoreId != null)
                {
                    storeId = selectedStoreId.Value;
                }
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

        public ActionResult ExportPaymentExcel(JQueryDataTableParamModel param, int? selectedStoreId, int brandId, string sTime, string eTime)
        {
            var startTime = sTime.ToDateTime().GetStartOfDate();
            var endTime = eTime.ToDateTime().GetEndOfDate();
            //int totalDisplay = (int)endTime.Subtract(startTime).TotalDays;

            //endTime = endTime.AddDays(-param.iDisplayStart);

            //if (startTime < endTime.AddDays(-param.iDisplayLength + 1))
            //{
            //    startTime = endTime.AddDays(-param.iDisplayLength + 1).GetStartOfDate();
            //}

            var paymenApi = new PaymentApi();
            int storeId = 0;
            if (selectedStoreId != null)
            {
                storeId = selectedStoreId.Value;
            }
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
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngân hàng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Phiếu quà tặng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Khách nợ";
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
                if (selectedStoreId > 0)
                {
                    storeName = storeApi.GetStoreNameByID(selectedStoreId.Value);
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

        [HttpPost]
        public async Task<JsonResult> GetCashPaymentData(JQueryDataTableParamModel param, int? selectedStoreId, int? enumPaymentType, int storeId, int brandId, string sTime, string eTime)
        {

            var startTime = sTime.ToDateTime().GetStartOfDate();
            var endTime = eTime.ToDateTime().GetEndOfDate();
            var paymentApi = new PaymentApi();
            var curStoreId = selectedStoreId ?? 0;//default 0
            var curPaymentType = enumPaymentType ?? 0;
            IQueryable<HmsService.Models.Entities.Payment> payments = null;

            if(curStoreId > 0)
            {
                payments = paymentApi.BaseService.GetPaymentStoreByTimeRange(startTime, endTime, curStoreId).Where(q => q.Order.OrderStatus == (int)OrderStatusEnum.Finish);
            }
            else
            {
                payments = paymentApi.BaseService.GetPaymentBrandByTimeRange(startTime, endTime, brandId).Where(q => q.Order.OrderStatus == (int)OrderStatusEnum.Finish);
            }
            var totalDisplay = await payments.CountAsync();

            if (curStoreId != 0)
            {
                payments = payments.Where(q => (q.Order.Store.ID == curStoreId));
            }
            if (curPaymentType != 0)
            {
                payments = payments.Where(q => q.Type == curPaymentType);
            }

            var count = param.iDisplayStart + 1;
            var rs = (await payments.OrderByDescending(q => q.PayTime)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength)
                .ToListAsync())
                .Select(q => new IConvertible[]
                {
                        count++,
                        q.ToRentID != null ? q.Order.InvoiceID : q.Cost.CostCode ?? "N/A",
                        q.Cost != null ? q.Cost.CostID.ToString() : "N/A",
                        q.Amount > 0 ? q.Amount : 0,//thu > 0
                        q.Amount <= 0 ? q.Amount : 0,//chi <= 0
                        q.PayTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        Utils.DisplayName((PaymentTypeEnum)q.Type),
                        q.ToRentID != null ? q.ToRentID : q.CostID
                }).ToList();
            var totalFilteredDisplay = await payments.CountAsync();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalDisplay,
                iTotalDisplayRecords = totalFilteredDisplay,
                aaData = rs,
            }, JsonRequestBehavior.AllowGet);
        }


        public async Task<JsonResult> GetMemberPaymentData(JQueryDataTableParamModel param, int? selectedStoreId, int storeId, int brandId, string sTime, string eTime)
        {
            var startTime = sTime.ToDateTime().GetStartOfDate();
            var endTime = eTime.ToDateTime().GetEndOfDate();
            var paymentApi = new PaymentApi();
            var orderApi = new OrderApi();

            var curStoreId = selectedStoreId == null ? storeId : selectedStoreId.Value;
            var orders = orderApi.GetOrdersByTimeRange(storeId, startTime, endTime, brandId);
            var payments = paymentApi.GetStorePaymentByDateRange(curStoreId, startTime, endTime, brandId).Where(p => p.ToRentID != null).Join(orders,
                p => p.ToRentID,
                o => o.RentID,
                (p, o) => new { p, o }
                );

            var rs = (await payments.Where(q => q.o.OrderType == (int)OrderTypeEnum.OrderCard || q.p.Type == (int)PaymentTypeEnum.MemberPayment)
                .ToListAsync())
                .Select(q =>
                {
                    var type = String.Empty;

                    if (q.o.OrderType == (int)OrderTypeEnum.OrderCard)
                    {
                        type = "Nạp thẻ";
                    }
                    else
                    {
                        type = "Dùng thẻ";
                    }

                    return new
                    {
                        id = q.p.PaymentID,
                        orderId = q.o.RentID,
                        amount = q.p.Amount,
                        type = type,
                        time = q.p.PayTime,
                    };
                });

            var listresult = new List<object>();

            var totalDisplay = await payments.CountAsync();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalDisplay,
                iTotalDisplayRecords = totalDisplay,
                aaData = listresult,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult LoadTotalPaymentReport(int brandId, int storeIdCode, string startDate, string endDate)
        {
            //var transactionApi = new TransactionApi();
            //var transaction = transactionApi.GetAllTransactionByStoreIDIEnum(brandId, storeIdCode, startTime, endTime);

            var paymentApi = new PaymentApi();
            var startTime = startDate.ToDateTime().GetStartOfDate();
            var endTime = endDate.ToDateTime().GetEndOfDate();
            var payments = paymentApi.GetEntityStorePaymentInDateRange(storeIdCode, startTime, endTime, brandId);

            SumarizeDataCashOther incomeSum = new SumarizeDataCashOther();
            SumarizeDataCashOther outcomeSum = new SumarizeDataCashOther();
            SumarizeDataCashOther insTransaction = new SumarizeDataCashOther();
            SumarizeDataCashOther descTransaction = new SumarizeDataCashOther();

            if(payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash && q.Amount > 0).Count() > 0)
            {
                incomeSum.cashValue = payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash && q.Amount > 0).Sum(q => q.Amount);
                insTransaction.cashValue = payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash && q.Amount > 0).Count();
            }
            if (payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash && q.Amount > 0).Count() > 0)
            {
                incomeSum.otherValue = payments.Where(q => q.Type != (int)PaymentTypeEnum.Cash && q.Amount > 0).Sum(q => q.Amount);
                insTransaction.otherValue = payments.Where(q => q.Type != (int)PaymentTypeEnum.Cash && q.Amount <= 0).Count();
            }
            if (payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash && q.Amount <= 0).Count() > 0)
            {
                outcomeSum.cashValue = payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash && q.Amount <= 0).Sum(q => q.Amount);
                descTransaction.cashValue = payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash && q.Amount <= 0).Count();
            }
            if (payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash && q.Amount <= 0).Count() > 0)
            {
                outcomeSum.otherValue = payments.Where(q => q.Type != (int)PaymentTypeEnum.Cash && q.Amount <= 0).Sum(q => q.Amount);
                descTransaction.cashValue = payments.Where(q => q.Type != (int)PaymentTypeEnum.Cash && q.Amount <= 0).Count();
            }

            //Detail data
            //SumarizeData cashData = new SumarizeData();
            //SumarizeData memshipData = new SumarizeData();
            //SumarizeData bankData = new SumarizeData();
            //SumarizeData ewalletData = new SumarizeData();

            ////
            //if (payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash && q.Amount > 0).Count() > 0)
            //{
            //    cashData.income = payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash && q.Amount > 0).Sum(q => q.Amount);
            //}
            //if (payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash && q.Amount <= 0).Count() > 0)
            //{
            //    cashData.expense = payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash && q.Amount <= 0).Sum(q => q.Amount);
            //}
            //cashData.transaction = payments.Where(q => q.Type == (int)PaymentTypeEnum.Cash).Count();
            ////
            //if (payments.Where(q => q.Type == (int)PaymentTypeEnum.MemberPayment && q.Amount > 0).Count() > 0)
            //{
            //    memshipData.income = payments.Where(q => q.Type == (int)PaymentTypeEnum.MemberPayment && q.Amount > 0).Sum(q => q.Amount);
            //}
            //if (payments.Where(q => q.Type == (int)PaymentTypeEnum.MemberPayment && q.Amount <= 0).Count() > 0)
            //{
            //    memshipData.expense = payments.Where(q => q.Type == (int)PaymentTypeEnum.MemberPayment && q.Amount <= 0).Sum(q => q.Amount);
            //}
            //memshipData.transaction = payments.Where(q => q.Type == (int)PaymentTypeEnum.MemberPayment).Count();
            ////
            //if (payments.Where(q => q.Type == ((int)PaymentTypeEnum.VisaCard) || (q.Type == (int)PaymentTypeEnum.MasterCard) && q.Amount > 0).Count() > 0)
            //{
            //    bankData.income = payments.Where(q => q.Type == ((int)PaymentTypeEnum.VisaCard) || (q.Type == (int)PaymentTypeEnum.MasterCard) && q.Amount > 0).Sum(q => q.Amount);
            //}
            //if (payments.Where(q => q.Type == ((int)PaymentTypeEnum.VisaCard) || (q.Type == (int)PaymentTypeEnum.MasterCard) && q.Amount <= 0).Count() > 0)
            //{
            //    bankData.expense = payments.Where(q => q.Type == ((int)PaymentTypeEnum.VisaCard) || (q.Type == (int)PaymentTypeEnum.MasterCard) && q.Amount <= 0).Sum(q => q.Amount);
            //}
            //bankData.transaction = payments.Where(q => q.Type == ((int)PaymentTypeEnum.VisaCard) || (q.Type == (int)PaymentTypeEnum.MasterCard)).Count();
            ////
            //if (payments.Where(q => q.Type == (int)PaymentTypeEnum.ThirdParty && q.Amount > 0).Count() > 0)
            //{
            //    ewalletData.income = payments.Where(q => q.Type == (int)PaymentTypeEnum.ThirdParty && q.Amount > 0).Sum(q => q.Amount);
            //}
            //if (payments.Where(q => q.Type == (int)PaymentTypeEnum.ThirdParty && q.Amount <= 0).Count() > 0)
            //{
            //    ewalletData.expense = payments.Where(q => q.Type == (int)PaymentTypeEnum.ThirdParty && q.Amount <= 0).Sum(q => q.Amount);
            //}
            //ewalletData.transaction = payments.Where(q => q.Type == (int)PaymentTypeEnum.ThirdParty).Count();

            return Json(new
            {
                //Detail data
                //cashData,
                //memshipData,
                //bankData,
                //ewalletData
                //Total data
                incomeSum,
                outcomeSum,
                insTransaction,
                descTransaction

            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetListStore()
        {
            var brandId = int.Parse(RouteData.Values["brandId"].ToString());
            var storeApi = new StoreApi();
            var listStore = storeApi.GetAllActiveStore(brandId).ToList();
            var result = listStore.Select(q => new
            {
                storeId = q.ID,
                storeName = q.Name
            });
            return Json(new { listresult = result }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadCost(int id)
        {
            var costApi = new CostApi();
            var cost = costApi.Get(id);
            var storeName = (new StoreApi()).GetStoreNameByID(cost.StoreId.Value);
            return Json(new
            {
                paidPerson = cost.PaidPerson,
                store = (!string.IsNullOrEmpty(storeName)) ? storeName : "N/A",
                description = cost.CostDescription,
                time = (cost.CostDate != null) ? cost.CostDate.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                id = cost.CostID,
                amount = cost.Amount,
                type = cost.CostType
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreatingSpendForm(string costCategoryType, int brandId)
        {
            var categoryApi = new CostCategoryApi();

            var model = new CostViewModel();
            int type = Convert.ToInt32(costCategoryType);
            var categories = categoryApi.GetCostCategories().Where(a => (a.Type == type) && a.Active == true && a.BrandId == brandId);
            model.CostCategoryType = type;
            model.Categories = categories;
            return PartialView("CreatingSpendForm", model);
        }

        [HttpPost]
        public async Task<ActionResult> CreatingSpendForm(CostViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var api = new CostApi();
                var costCategoryApi = new CostCategoryApi();
                var catId = await costCategoryApi.GetCostCategoryById(model.CatID);
                model.CostType = catId.Type;
                model.CostStatus = (int)CostStatusEnum.Approved;
                if (model.StoreId == 0)
                {
                    model.StoreId = null;
                }
                model.CostDate = Utils.GetCurrentDateTime();

                var costId = api.CreateCost(model);
                try
                {
                    var payment = new HmsService.Models.Entities.Payment()
                    {
                        Amount = -model.Amount,
                        CurrencyCode = "VND",
                        FCAmount = (decimal)model.Amount,
                        PayTime = model.CostDate,
                        Status = (int)PaymentStatusEnum.Approved,
                        Type = (int)PaymentTypeEnum.Cash,
                        CostID = costId
                    };
                    var paymentApi = new PaymentApi();
                    int paymenId = paymentApi.CreatePaymentReturnId(payment);
                    return Json(new
                    {
                        success = true
                    });
                }
                catch (Exception e1)
                {
                    api.Delete(costId);
                    return Json(new
                    {
                        success = false
                    });
                }
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false
                });
            }

        }

        //Tạo phiếu thu
        public ActionResult CreatingReceiptForm(string costCategoryType, int brandId)
        {
            var categoryApi = new CostCategoryApi();
            var model = new CostViewModel();
            int type = Convert.ToInt32(costCategoryType);
            var categories = categoryApi.GetCostCategories().Where(a => (a.Type == type) && a.Active == true && a.BrandId == brandId);
            model.CostCategoryType = type;
            model.Categories = categories;
            return PartialView("CreatingReceiptForm", model);
        }

        [HttpPost]
        public async Task<ActionResult> CreatingReceiptForm(CostViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View(model);
            }
            // if model valid create payment 
            try
            {
                CostCategoryApi costCategoryApi = new CostCategoryApi();
                var catId = await costCategoryApi.GetCostCategoryById(model.CatID);
                model.CostType = catId.Type;
                if (model.StoreId == 0)
                {
                    model.StoreId = null;
                }

                model.CostDate = Utils.GetCurrentDateTime();
                model.CostStatus = (int)CostStatusEnum.Approved;
                var costApi = new CostApi();
                int costId = costApi.CreateCost(model);

                try
                {
                    var payment = new HmsService.Models.Entities.Payment()
                    {
                        Amount = model.Amount,
                        CurrencyCode = "VND",
                        FCAmount = (decimal)model.Amount,
                        PayTime = model.CostDate,
                        Status = (int)PaymentStatusEnum.Approved,
                        Type = (int)PaymentTypeEnum.Cash,
                        CostID = costId
                    };
                    var paymentApi = new PaymentApi();
                    int paymenId = paymentApi.CreatePaymentReturnId(payment);
                    return Json(new
                    {
                        success = true
                    });
                }
                catch (Exception e1)
                {
                    costApi.Delete(costId);
                    return Json(new
                    {
                        success = false
                    });
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false
                });
            }
        }

        public JsonResult LoadDebtOrder(int brandId)
        {
            var orderApi = new OrderApi();
            var listOrder = orderApi.GetAllDebtOrderByBrand(brandId).Select(q => new
            {
                rentId = q.RentID,
                invoiceId = q.InvoiceID
            }).ToList();
            return Json(new
            {
                listOrder = listOrder
            }, JsonRequestBehavior.AllowGet);
        }
    }

    //public async Task<JsonResult> UpdateReportDB(int brandId, string sTime, int curStore)
    //{
    //    bool result = false;
    //    var reportApi = new PaymentReportApi();
    //    var paymentApi = new PaymentApi();
    //    var storeApi = new StoreApi();
    //    var stores = storeApi.BaseService.GetActiveStoreByBrandId(brandId).Select(q => q.ID)
    //        .OrderBy(a => a).ToArray();
    //    var firstPaymentDate = paymentApi.BaseService.Get().Select(q => q.PayTime).Min();
    //    var startTime = string.IsNullOrEmpty(sTime) ? firstPaymentDate : sTime.ToDateTime().GetStartOfDate();
    //    var yesterday = Utils.GetCurrentDateTime().GetEndOfDate().AddDays(-1);
    //    DateTime dateWhenStop = startTime;
    //    var storeWhenStop = curStore;
    //    try
    //    {
    //        for (var d = new DateTime(2017, 5, 9, 0,0,0); d <= yesterday; d = d.AddDays(1))
    //        {
    //            dateWhenStop = d;
    //            var sw = new Stopwatch();
    //            sw.Start();
    //            for (int i = 0; i < stores.Count(); i++)
    //            {
    //                storeWhenStop = stores[i];
    //                if(curStore > 0)
    //                {
    //                    i = stores.ToList().FindIndex(a => a == curStore);
    //                    curStore = 0;
    //                }
    //                var existedPayments = paymentApi.GetEntityStorePaymentInDateRange(stores[i], d.GetStartOfDate(), d.GetEndOfDate(), brandId).ToList();
    //                if (existedPayments.Count() > 0)
    //                {
    //                    var CashAmount = existedPayments.Where(a => a.Type == (int)PaymentTypeEnum.Cash || a.Type == (int)PaymentTypeEnum.ExchangeCash).Sum(a => a.Amount);
    //                    var MemberCardAmount = existedPayments.Where(a => a.Type == (int)PaymentTypeEnum.MemberPayment).Sum(a => a.Amount);
    //                    var VoucherAmount = existedPayments.Where(a => a.Type == (int)PaymentTypeEnum.Voucher).Sum(a => a.Amount);
    //                    var BankAmount = existedPayments.Where(a => a.Type == (int)PaymentTypeEnum.MasterCard || a.Type == (int)PaymentTypeEnum.VisaCard).Sum(a => a.Amount);
    //                    var DebtAmount = existedPayments.Where(a => a.Type == (int)PaymentTypeEnum.Debt).Sum(a => a.Amount);
    //                    var OtherAmount = existedPayments.Where(a => a.Type == (int)PaymentTypeEnum.Other).Sum(a => a.Amount);
    //                    var paymentReport = new PaymentReport
    //                    {
    //                        Date = d.GetEndOfDate(),
    //                        StoreID = stores[i],
    //                        CreateBy = "system",
    //                        Status = 1,
    //                        CashAmount = CashAmount,
    //                        MemberCardAmount = MemberCardAmount,
    //                        VoucherAmount = VoucherAmount,
    //                        BankAmount = BankAmount,
    //                        DebtAmount = DebtAmount,
    //                        OtherAmount = OtherAmount
    //                    };
    //                    Debug.WriteLine("Store {0} has totalAmount: {1}", stores[i], CashAmount + MemberCardAmount + VoucherAmount + BankAmount + DebtAmount + OtherAmount);
    //                    await reportApi.BaseService.CreateAsync(paymentReport);
    //                }

    //            }
    //            Debug.WriteLine("Time to evaluate in {0}: {1}",d.Date, sw.ElapsedMilliseconds);
    //            sw.Stop();
    //        }
    //        result = true;
    //    }
    //    catch (Exception e)
    //    {
    //        result = false;
    //        return Json(new
    //        {
    //            success = result,
    //            message = "Failed",
    //            date = dateWhenStop.ToShortDateString(),
    //            store = storeWhenStop
    //        }, JsonRequestBehavior.AllowGet);
    //    }
    //    return Json(new
    //    {
    //        success = result,
    //        message = "TC"
    //    }, JsonRequestBehavior.AllowGet);
    //}

    public class PaymentReportData
    {
        public string time { get; set; }
        public double cash { get; set; }
        public double creditCard { get; set; }
        public double bank { get; set; }
        public double voucher { get; set; }
        public double debt { get; set; }
        public double orther { get; set; }

    }

    public class SumarizeData
    {
        public double income { get; set; }
        public double expense { get; set; }
        public int transaction { get; set; }
    }

    public class SumarizeDataCashOther
    {
        public double cashValue { get; set; }
        public double otherValue { get; set; }
    }
}

//var rs = (await payments.OrderByDescending(q => q.PayTime)
//    .Skip(param.iDisplayStart)
//    .Take(param.iDisplayLength)
//    .ToListAsync())
//    .Select(q =>
//    {
//        var costId = "";
//        var type = "";

//        if (q.CostID != null)
//        {
//            costId = q.Cost.CostID.ToString();
//            if (q.Cost.CostType == (int)CostTypeEnum.ReceiveCost || q.Cost.CostType == (int)CostTypeEnum.ReceiveCostTranferOut)
//            {
//                type = CostTypeEnum.ReceiveCost.DisplayName();
//            }
//            else
//            {
//                type = CostTypeEnum.SpendingCost.DisplayName();
//            }
//        }
//        else
//        {
//            costId = q.ToRentID.ToString();
//            type = "Hóa đơn";
//        }
//        return new
//        {
//            index = ++index + param.iDisplayStart,
//            id = q.ToRentID != null ? q.Order.InvoiceID : q.Cost.CostCode ?? "N/A",
//            costId = costId,
//            income = q.Amount > 0 ? q.Amount : 0,//thu > 0
//            expenses = q.Amount <= 0 ? q.Amount : 0,//chi <= 0
//            time = q.PayTime.ToString("dd/MM/yyyy HH:mm:ss"),
//            paymentType = type,
//        };
//    });

//var resultData = rs.Select(q => new IConvertible[] {
//    q.index,
//    q.id,
//    q.costId,
//    q.income,
//    q.expenses,
//    q.time,
//    String.IsNullOrEmpty(q.paymentType) ? "N/A" : q.paymentType
//}).ToList();
//var totalFilteredDisplay = await payments.CountAsync();