using HmsService.Sdk;
using HmsService.ViewModels;
using HmsService.Models;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Globalization;

namespace Wisky.SkyAdmin.Manage.Areas.Transaction.Controllers
{
    public class TransactionController : DomainBasedController
    {
        public class ReportExcelView
        {
            public int STT;
            public String tenTaiKhoan;
            public String tenKhachHang;
            public String menhGia;
            public String ngayGiaoDich;
            public String ghiChu;
            public String trangThai;
        }
        public ActionResult ExportTransactionToExcel(string startTime, string endTime, int storeId, int brandId)
        {
            //var listTransaction = new List<DateReportViewModel>();
            var api = new TransactionApi();
            var resultTang = new List<ReportExcelView>();
            var resultGiam = new List<ReportExcelView>();
            var resultRollback = new List<ReportExcelView>();
            var resultActiveCard = new List<ReportExcelView>();
            var sTime = startTime.ToDateTime().GetStartOfDate();
            var eTime = endTime.ToDateTime().GetEndOfDate();
            #region Get data
            var listTransaction = api.GetAllTransactionByStoreIDIEnum(brandId,storeId, sTime, eTime);
            int count = 1;
            //tang
            var membershipCardApi = new MembershipCardApi();
            var accountApi = new AccountApi();
            foreach (var item in listTransaction.Where(p => p.IsIncreaseTransaction & p.Status == (int)TransactionStatus.Approve))
            {
                resultTang.Add(new ReportExcelView
                {
                    STT = count++,
                    tenTaiKhoan = item.Account.AccountName,
                    tenKhachHang = membershipCardApi.GetMembershipCardById(accountApi.Get(item.AccountId)
                            .MembershipCardId.GetValueOrDefault()).CustomerName,
                    menhGia = item.Amount.ToString(),
                    ngayGiaoDich = item.Date.ToString("dd/MM/yyyy HH:mm:ss"),
                    ghiChu = item.Notes,
                    trangThai = (item.Status == 0) ? "Chưa duyệt" : (item.Status == 1) ? "Đã duyệt" : "Đã hủy"
                });
            }
            //giam
            count = 1;
            foreach (var item in listTransaction.Where(p => p.IsIncreaseTransaction == false & p.Status == (int)TransactionStatus.Approve))
            {
                resultGiam.Add(new ReportExcelView
                {
                    STT = count++,
                    tenTaiKhoan = item.Account.AccountName,
                    tenKhachHang = membershipCardApi.GetMembershipCardById(accountApi.Get(item.AccountId)
                            .MembershipCardId.GetValueOrDefault()).CustomerName,
                    menhGia = item.Amount.ToString(),
                    ngayGiaoDich = item.Date.ToString("dd/MM/yyyy HH:mm:ss"),
                    ghiChu = item.Notes,
                    trangThai = (item.Status == 0) ? "Chưa duyệt" : (item.Status == 1) ? "Đã duyệt" : "Đã hủy"
                });
            }
            //roll back
            count = 1;
            foreach (var item in listTransaction.Where(p => p.TransactionType == (int)TransactionTypeEnum.RollBack & p.Status == (int)TransactionStatus.Approve))
            {
                resultRollback.Add(new ReportExcelView
                {
                    STT = count++,
                    tenTaiKhoan = item.Account.AccountName,
                    tenKhachHang = membershipCardApi.GetMembershipCardById(accountApi.Get(item.AccountId)
                            .MembershipCardId.GetValueOrDefault()).CustomerName,
                    menhGia = item.Amount.ToString(),
                    ngayGiaoDich = item.Date.ToString("dd/MM/yyyy HH:mm:ss"),
                    ghiChu = item.Notes,
                    trangThai = (item.Status == 0) ? "Chưa duyệt" : (item.Status == 1) ? "Đã duyệt" : "Đã hủy"
                });
            }
            //ActiveCard
            count = 1;
            foreach (var item in listTransaction.Where(p => p.TransactionType == (int)TransactionTypeEnum.ActiveCard & p.Status == (int)TransactionStatus.Approve))
            {
                resultActiveCard.Add(new ReportExcelView
                {
                    STT = count++,
                    tenTaiKhoan = item.Account.AccountName,
                    tenKhachHang = membershipCardApi.GetMembershipCardById(accountApi.Get(item.AccountId)
                            .MembershipCardId.GetValueOrDefault()).CustomerName,
                    menhGia = item.Amount.ToString(),
                    ngayGiaoDich = item.Date.ToString("dd/MM/yyyy HH:mm:ss"),
                    ghiChu = item.Notes,
                    trangThai = (item.Status == 0) ? "Chưa duyệt" : (item.Status == 1) ? "Đã duyệt" : "Đã hủy"
                });
            }
            #endregion

            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                #region Giao dich tang
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("GDTang");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên tài khoản";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên khách hàng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Mệnh giá";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày giao dịch";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ghi chú";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Trạng thái";
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
                foreach (var data in resultTang)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.STT;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.tenTaiKhoan;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.tenKhachHang;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.menhGia;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.ngayGiaoDich;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.ghiChu;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.trangThai;
                    StartHeaderChar = 'A';
                }
                string storeName;
                var storeApi = new StoreApi();
                storeName = storeApi.GetStoreById(storeId).Name;
                var sDate = startTime.Replace("/", "-");
                var eDate = endTime.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                string fileName = "BaoCaoGiaoDich_" + "CuaHang_" + storeName + dateRange + ".xlsx";
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion
                #region Giao dich giảm
                ExcelWorksheet ws2 = package.Workbook.Worksheets.Add("GDGiam");
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #region Headers
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên tài khoản";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên khách hàng";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Mệnh giá";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày giao dịch";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ghi chú";
                ws2.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Trạng thái";
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
                foreach (var data in resultGiam)
                {
                    ws2.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.STT;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.tenTaiKhoan;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.tenKhachHang;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.menhGia;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.ngayGiaoDich;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.ghiChu;
                    ws2.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.trangThai;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws2.Cells[ws2.Dimension.Address].AutoFitColumns();
                ws2.Cells[ws2.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws2.Cells[ws2.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws2.Cells[ws2.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws2.Cells[ws2.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion
                #region Giao dich rollback
                ExcelWorksheet ws3 = package.Workbook.Worksheets.Add("GDDieuChinh");
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #region Headers
                ws3.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws3.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên tài khoản";
                ws3.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên khách hàng";
                ws3.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Mệnh giá";
                ws3.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày giao dịch";
                ws3.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ghi chú";
                ws3.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Trạng thái";
                EndHeaderChar = StartHeaderChar;
                EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws3.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws3.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws3.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws3.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws3.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells                
                foreach (var data in resultRollback)
                {
                    ws3.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.STT;
                    ws3.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.tenTaiKhoan;
                    ws3.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.tenKhachHang;
                    ws3.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.menhGia;
                    ws3.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.ngayGiaoDich;
                    ws3.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.ghiChu;
                    ws3.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.trangThai;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws3.Cells[ws3.Dimension.Address].AutoFitColumns();
                ws3.Cells[ws3.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws3.Cells[ws3.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws3.Cells[ws3.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws3.Cells[ws3.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion
                #region Giao dich ActiveCard
                ExcelWorksheet ws4 = package.Workbook.Worksheets.Add("GDBanDau");
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #region Headers
                ws4.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws4.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên tài khoản";
                ws4.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên khách hàng";
                ws4.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Mệnh giá";
                ws4.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày giao dịch";
                ws4.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ghi chú";
                ws4.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Trạng thái";
                EndHeaderChar = StartHeaderChar;
                EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws4.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                ws4.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                ws4.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws4.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws4.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells                
                foreach (var data in resultActiveCard)
                {
                    ws4.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.STT;
                    ws4.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.tenTaiKhoan;
                    ws4.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.tenKhachHang;
                    ws4.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.menhGia;
                    ws4.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.ngayGiaoDich;
                    ws4.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.ghiChu;
                    ws4.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.trangThai;
                    StartHeaderChar = 'A';
                }
                #endregion

                //Set style for excel
                ws4.Cells[ws4.Dimension.Address].AutoFitColumns();
                ws4.Cells[ws4.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws4.Cells[ws4.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws4.Cells[ws4.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws4.Cells[ws4.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }

        [HttpPost]
        public ActionResult GetStoreCondinate(int storeId)
        {
            var storeApi = new StoreApi();
            var store = storeApi.Get(storeId);
            var result = new
            {
                ID = store.ID,
                Name = store.Name,
                Address = store.Address,
                Longitude = store.Lon,
                Latitude = store.Lat,
            };
            return Json(result);
        }


        // GET: CRM/Transaction
        public ActionResult Index(int brandId, int storeId)
        {
            var transactionStatusList = new List<TransactionEnumViewModel>();
            var allTransactionStatus = Enum.GetValues(typeof(TransactionStatus));
            foreach (var status in allTransactionStatus)
            {
                transactionStatusList.Add(new TransactionEnumViewModel
                {
                    Name = ((TransactionStatus)status).DisplayName(),
                    Value = (int)status
                });
            }

            var transactionAmountList = new List<TransactionEnumViewModel>();
            var allTransactionAmount = Enum.GetValues(typeof(TransactionAmount));
            foreach (var amount in allTransactionAmount)
            {
                transactionAmountList.Add(new TransactionEnumViewModel
                {
                    Name = ((TransactionAmount)amount).DisplayName(),
                    Value = (int)amount
                });
            }

            ViewBag.TransactionStatusList = transactionStatusList;
            ViewBag.TransactionAmountList = transactionAmountList;
            ViewBag.storeId = storeId;
            return View();
        }

        public ActionResult GetAllTransactions(int storeId)
        {
            try
            {
                var transactionApi = new TransactionApi();

                var count = 1;
                var result = transactionApi.GetAllTransactionByStoreId(storeId).ToList().Select(q => new IConvertible[] {
                    count++,
                    q.Account.AccountName,
                    q.Amount,
                    q.Date.ToString("dd/MM/yyyy"),
                    q.Status
                });

                return Json(new { success = true, list = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liện hệ admin" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateTransaction(int brandId, int storeId, string membershipCardCode, decimal amount,int? form, int type, string description)
        {
            try
            {
                var transactionApi = new TransactionApi();
                var membershipCardApi = new MembershipCardApi();
                var currentCard = membershipCardApi.GetMembershipCardByCode(membershipCardCode);
                var formTrasaction = true;
                if (form !=null)
                {
                    if (form == 1)
                    {
                        formTrasaction = true;
                    }
                    else
                    {
                        formTrasaction = false;
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Giao dịch thất bại!" });
                }
                
                foreach (var account in currentCard.Accounts)
                {
                    if (account.Type == (int)AccountTypeEnum.CreditAccount)
                    {
                        transactionApi.Create(new TransactionViewModel
                        {
                            AccountId = account.AccountID,
                            Amount = amount,
                            StoreId = storeId,
                            BrandId = brandId,
                            Date = DateTime.Now,
                            IsIncreaseTransaction = formTrasaction,
                            TransactionType = type,
                            Notes = description,
                            Status = (int) TransactionStatus.New
                        });

                        return Json(new { success = true, message = "Giao dịch thành công!" });
                    }
                }
                return Json(new { success = false, message = "Giao dịch thất bại!" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liện hệ admin" });
            }
        }

        public ActionResult CheckMembershipCardCode(int brandId, string membershipCardCode)
        {
            try
            {
                var membershipCardApi = new MembershipCardApi();
                var currentCard = membershipCardApi.GetMembershipCardByCode(membershipCardCode.Trim());

                if (currentCard != null && currentCard.BrandId == brandId)
                {
                    if (currentCard.Accounts != null && currentCard.Accounts.Count > 0)
                    {
                        foreach (var account in currentCard.Accounts)
                        {
                            if (account.Type == (int)AccountTypeEnum.CreditAccount)
                            {
                                var customer = currentCard.Customer;
                                return Json(new { success = true, AccountName = account.AccountName, Customer = new { Name = customer.Name, Phone = customer.Phone } }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }

                    return Json(new { success = false, message = "Không tồn tại tài khoảng thanh toán!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Thẻ không tồn tại!" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liện hệ admin" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAllTransactionsByDateRange(JQueryDataTableParamModel param, int storeId, string startTime, string endTime, int transactionStatus, int transactionType, int transactionMode)
        {
            try
            {
                var transactionApi = new TransactionApi();
                var customerApi = new CustomerApi();
                var accountApi = new AccountApi();
                var membershipCardApi = new MembershipCardApi();

                DateTime sTime = startTime.ToDateTime().GetStartOfDate();
                DateTime eTime = endTime.ToDateTime().GetEndOfDate();
                var result = transactionApi.GetAllTransactionByStoreIdWithDateRangeEntity(storeId, sTime, eTime);
                var count = param.iDisplayStart + 1;

                if (transactionStatus != -1)
                {
                    result = result.Where(a => a.Status == transactionStatus).OrderBy(q => q.Date);
                }

                if (transactionType != -1)
                {
                    result = result.Where(a => a.IsIncreaseTransaction == (transactionType == 0)).OrderBy(q => q.Date);
                }

                if (transactionMode != -1)
                {
                    result = result.Where(a => a.TransactionType == transactionMode).OrderBy(q => q.Date);
                }

                IQueryable<HmsService.Models.Entities.Transaction> filteredResult;
                double searchCode;
                if (double.TryParse(param.sSearch, out searchCode))
                {
                    filteredResult = result
                    .OrderByDescending(q => q.Date)
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Account.MembershipCard.MembershipCardCode.ToLower().Contains(searchCode.ToString().ToLower()));
                }
                else
                {
                    filteredResult = result
                    .OrderByDescending(q => q.Date)
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Account.AccountName.ToLower().Contains(param.sSearch.ToLower()));
                }

                var list = filteredResult.Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList()
                    .Select(q => new IConvertible[] {
                        count++,
                        //q.AccountId,
                        q.Account.AccountName,
                        //customerApi.Get(q.AccountId).Name,
                        membershipCardApi.GetMembershipCardById(accountApi.Get(q.AccountId)
                            .MembershipCardId.GetValueOrDefault()).CustomerName,
                        q.Amount,
                        q.Date.ToString("dd/MM/yyyy HH:mm:ss"),
                        q.Notes,                        
                        (q.UserId==null) ? "Hệ Thống" : q.UserId,
                        q.Status,
                        q.IsIncreaseTransaction,
                        q.AccountId,
                    });
                var list2 = result
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Account.AccountName.ToLower().Contains(param.sSearch.ToLower()))
                    .ToList()
                    .Select(q => new IConvertible[] {
                        q.Amount,
                        q.Status,
                        q.IsIncreaseTransaction
                    });

                var totalRecords = result.Count();
                var displayRecords = filteredResult.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = displayRecords,
                    aaData = list,
                    totalData = list2,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                //return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liện hệ admin" }, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public ActionResult Create(int Id)
        {
            TransactionEditViewModel model = new TransactionEditViewModel();
            model.AccountId = Id;
            model.IsIncreaseTransaction = true;
            return View(model);
        }
        //TODO: AccountViewModel chưa có MembershipCard để lấy CustomerID
        //[HttpPost]
        //public async Task<ActionResult> Create(TransactionEditViewModel model)
        //{
        //    if (!this.ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //    var transactionApi = new TransactionApi();
        //    var accountApi = new AccountApi();
        //    await transactionApi.CreateTransactionAsync(model);
        //    var account = await accountApi.GetAsync(model.AccountId);
        //    return RedirectToAction("CustomerDetail", "Customer", new { Id = account.CustomerID });
        //}

        public async Task<ActionResult> Edit(int Id)
        {
            var transactionApi = new TransactionApi();
            var model = new TransactionEditViewModel(await transactionApi.GetAsync(Id), this.Mapper);
            return View(model);
        }
        //TODO: accountviewmodel chua co Membershipcard
        //[HttpPost]
        //public async Task<ActionResult> Edit(TransactionEditViewModel model)
        //{
        //    if (!this.ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //    var transactionApi = new TransactionApi();
        //    var accountApi = new AccountApi();
        //    await transactionApi.EditTransactionAsync(model);
        //    var account = await accountApi.GetAsync(model.AccountId);
        //    return RedirectToAction("CustomerDetail", "Customer", new { Id = account.CustomerID });
        //}


    }


    public class TransactionEnumViewModel
    {
        public dynamic Value { get; set; }
        public string Name { get; set; }
    }


}