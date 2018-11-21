using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.CRM.Controllers
{
    public class TransactionController : BaseController
    {
        // GET: CRM/Transaction
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TransactionDetail(int id, int customerId)
        {
            var accApi = new AccountApi();
            var membershipCardApi = new MembershipCardApi();
            var customerApi = new CustomerApi();

            var customer = customerApi.GetCustomerById(customerId);
            CustomerEditViewModel model = new CustomerEditViewModel(customer.ToEntity());
            model.MembershipCard = membershipCardApi.GetMembershipCardById(id);
            model.AllAccounts = accApi.GetAccountsByMembershipCardId(id).Select(a => new SelectListItem
            {
                Text = a.AccountName,
                Value = a.AccountID.ToString(),
            });

            return View(model);
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
        public JsonResult LoadTotalTransaction(int brandId, int storeIdCode, string startDate, string endDate)
        {
            var transactionApi = new TransactionApi();
            var startTime = startDate.ToDateTime().GetStartOfDate();
            var endTime = endDate.ToDateTime().GetEndOfDate();
            var transaction = transactionApi.GetAllTransactionByStoreIDIEnum(brandId, storeIdCode, startTime, endTime);

            var numberIncrease = 0;
            var numberIncreaseOptimize = 0;
            var numberDecreaseOptimize = 0;
            var numberDecrease = 0;
            var numberIncreaseRollback = 0;
            var numberDecreaseRollback = 0;
            var numberActiveCard = 0;
            var numberDecreaseCancel = 0;
            var numberIncreaseCancel = 0;
            decimal revenueIncrease = 0;
            decimal revenueDecrease = 0;
            decimal revenueIncreaseRollback = 0;
            decimal revenueDecreaseRollback = 0;
            decimal revenueActiveCard = 0;
            decimal revenueIncreaseOptimize = 0;
            decimal revenueDecreaseOptimize = 0;
            decimal revenueDecreaseCancel = 0;
            decimal revenueIncreaseCancel = 0;

            foreach (var item in transaction)
            {
                if (item.IsIncreaseTransaction)
                {
                    if (item.Status == (int)TransactionStatus.Approve)
                    {
                        numberIncrease++;
                        revenueIncrease += item.Amount;
                        numberIncreaseOptimize++;
                        revenueIncreaseOptimize += item.Amount;
                        if (item.TransactionType == (int)TransactionTypeEnum.RollBack)
                        {
                            numberIncreaseRollback++;
                            revenueIncreaseRollback += item.Amount;
                        }
                        else
                        {
                            if (item.TransactionType == (int)TransactionTypeEnum.ActiveCard)
                            {
                                numberActiveCard++;
                                revenueActiveCard += item.Amount;
                            }
                        }
                    }
                    else
                    {
                        if (item.Status == (int)TransactionStatus.New)
                        {
                            numberIncreaseOptimize++;
                            revenueIncreaseOptimize += item.Amount;
                        }
                        else
                        {
                            if (item.Status == (int)TransactionStatus.Cancel)
                            {
                                numberIncreaseCancel++;
                                revenueIncreaseCancel += item.Amount;
                            }
                        }
                    }
                }
                else
                {
                    if (item.Status == (int)TransactionStatus.Approve)
                    {
                        numberDecrease++;
                        revenueDecrease += item.Amount;
                        numberDecreaseOptimize++;
                        revenueDecreaseOptimize += item.Amount;
                        if (item.TransactionType == (int)TransactionTypeEnum.RollBack)
                        {
                            numberDecreaseRollback++;
                            revenueDecreaseRollback += item.Amount;
                        }
                    }
                    else
                    {
                        if (item.Status == (int)TransactionStatus.New)
                        {
                            numberDecreaseOptimize++;
                            revenueDecreaseOptimize += item.Amount;
                        }
                        else
                        {
                            if (item.Status == (int)TransactionStatus.Cancel)
                            {
                                numberDecreaseCancel++;
                                revenueDecreaseCancel += item.Amount;
                            }
                        }
                    }
                }

            }

            return Json(new
            {
                numberIncrease = numberIncrease,
                numberIncreaseOptimize = numberIncreaseOptimize,
                numberDecreaseOptimize = numberDecreaseOptimize,
                numberDecrease = numberDecrease,
                numberIncreaseRollback = numberIncreaseRollback,
                numberDecreaseRollback = numberDecreaseRollback,
                numberActiveCard = numberActiveCard,
                numberDecreaseCancel = numberDecreaseCancel,
                numberIncreaseCancel = numberIncreaseCancel,
                revenueIncrease = revenueIncrease,
                revenueDecrease = revenueDecrease,
                revenueIncreaseRollback = revenueIncreaseRollback,
                revenueDecreaseRollback = revenueDecreaseRollback,
                revenueActiveCard = revenueActiveCard,
                revenueIncreaseOptimize = revenueIncreaseOptimize,
                revenueDecreaseOptimize = revenueDecreaseOptimize,
                revenueDecreaseCancel = revenueDecreaseCancel,
                revenueIncreaseCancel = revenueIncreaseCancel,
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadTransaction(JQueryDataTableParamModel param, int brandId, int storeIdCode, int transactionStatus, int transactionType, int transactionMode, string startDate, string endDate)
        {
            var transactionApi = new TransactionApi();
            var accountApi = new AccountApi();
            var customerApi = new CustomerApi();
            var membershipCardApi = new MembershipCardApi();
            var aspNetUserApi = new AspNetUserApi();


            var startTime = startDate.ToDateTime().GetStartOfDate();
            var endTime = endDate.ToDateTime().GetEndOfDate();
            var transaction = transactionApi.GetAllTransactionByStoreIDIQuery(brandId, storeIdCode, startTime, endTime);
            var totalRecords = transaction.Count();
            var customerAPi = new CustomerApi();

            try
            {
                var count = param.iDisplayStart + 1;
                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    transaction = transaction.Where(a => a.Account.MembershipCard.MembershipCardCode.Contains(param.sSearch.ToLower())
                     );
                }
                if (transactionStatus != -1)
                {
                    transaction = transaction.Where(a => a.Status == transactionStatus);
                }
                if (transactionType != -1)
                {
                    transaction = transaction.Where(a => a.IsIncreaseTransaction == (transactionType == 0));
                }
                if (transactionMode != -1)
                {
                    transaction = transaction.Where(a => a.TransactionType == transactionMode);
                }
                
                var totalDisplayRecords = transaction.Count();
                var rs = transaction
                    .OrderByDescending(q => q.Date)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList()
                    .Select(a => new IConvertible[]
                    {
                            count++,
                            membershipCardApi.GetMembershipCardById(accountApi.Get(a.AccountId)
                            .MembershipCardId.GetValueOrDefault()).MembershipCardCode,
                            membershipCardApi.GetMembershipCardById(accountApi.Get(a.AccountId)
                            .MembershipCardId.GetValueOrDefault()).CustomerName,
                            a.Amount,
                            a.Date.ToString("dd/MM/yyyy HH:mm:ss"),
                            string.IsNullOrWhiteSpace(a.Notes) ? "---" : a.Notes,
                            a.StoreId <=0 ? "Hệ Thống" : (Utils.GetStore(a.StoreId).Name),
                            a.Status,
                            (a.UserId==null) ? "Hệ Thống" : a.UserId,
                            a.Id,
                            membershipCardApi.GetMembershipCardById(accountApi.Get(a.AccountId)
                            .MembershipCardId.GetValueOrDefault()).CustomerId,
                            a.IsIncreaseTransaction,
                            //(a.UserId==null) ? "Hệ Thống" : a.UserId,
                           // a.Id,
                            //a.Account.Type
                            a.AccountId,
                            
                    }).ToList();
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalDisplayRecords,
                    aaData = rs,
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Error" });
            }
        }

        public ActionResult ExportToExcel(int brandId, int storeIdCode, string startDate, string endDate)
        {
            var transactionApi = new TransactionApi();
            var startTime = startDate.ToDateTime().GetStartOfDate();
            var endTime = endDate.ToDateTime().GetEndOfDate();
            var transaction = transactionApi.GetAllTransactionByStoreIDIEnum(brandId, storeIdCode, startTime, endTime);
            var resultTang = new List<ReportExcelView>();
            var resultGiam = new List<ReportExcelView>();
            var resultRollback = new List<ReportExcelView>();
            var resultActiveCard = new List<ReportExcelView>();
            #region Get data
            int count = 1;
            //tang
            var membershipCardApi = new MembershipCardApi();
            var accountApi = new AccountApi();
            foreach (var item in transaction.Where(p => p.IsIncreaseTransaction & p.Status == (int)TransactionStatus.Approve))
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
            foreach (var item in transaction.Where(p => p.IsIncreaseTransaction == false & p.Status == (int)TransactionStatus.Approve))
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
            foreach (var item in transaction.Where(p => p.TransactionType == (int)TransactionTypeEnum.RollBack & p.Status == (int)TransactionStatus.Approve))
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
            foreach (var item in transaction.Where(p => p.TransactionType == (int)TransactionTypeEnum.ActiveCard & p.Status == (int)TransactionStatus.Approve))
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
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("GiaoDichTang");
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
                storeName = storeIdCode == 0 ? "Hệ Thống" : storeApi.GetStoreById(storeIdCode).Name;
                var sDate = startDate.Replace("/", "-");
                var eDate = endDate.Replace("/", "-");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                string fileName = "BaoCaoGiaoDich_" + storeName + dateRange + ".xlsx";
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion
                #region Giao dich giảm
                ExcelWorksheet ws2 = package.Workbook.Worksheets.Add("GiaoDichGiam");
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
                ExcelWorksheet ws3 = package.Workbook.Worksheets.Add("GiaoDichRollBack");
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
                ExcelWorksheet ws4 = package.Workbook.Worksheets.Add("GiaoDichActiveCard");
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

            return null;
        }

        //public ActionResult Create(int Id)
        //{
        //    TransactionEditViewModel model = new TransactionEditViewModel();
        //    model.AccountId = Id;
        //    model.IsIncreaseTransaction = true;
        //    return View(model);
        //}
        public ActionResult CreateByAccountId(int id)
        {
            var model = new TransactionEditViewModel();
            var accountApi = new AccountApi();
            var account = accountApi.GetAccountEntityById(id);
            model.AccountId = account.AccountID;
            model.Amount = (int)account.Balance;
            return View(model);
        }
        [HttpPost]
        public async Task<JsonResult> CreateByAccountId(TransactionEditViewModel model)
        {
            var brandId = int.Parse(RouteData.Values["brandId"].ToString());
            var transactionApi = new TransactionApi();
            var accountApi = new AccountApi();
            var membershipCardApi = new MembershipCardApi();
            var account = await accountApi.GetAsync(model.AccountId);
            if (model.Amount == 0 || model.Amount.ToString().Length > 9)
            {
                return Json(new { success = false, message = "Số tiền tăng hoặc giảm cần lớn hơn 0 và cần ít hơn 9 chữ số!" },
                    JsonRequestBehavior.AllowGet);
            }
            model.Amount = (decimal)model.Amount;
            model.Status = (int)TransactionStatus.New;
            var customerId = membershipCardApi.GetMembershipCardById(account.MembershipCardId.GetValueOrDefault()).CustomerId;
            var customer = (new CustomerApi()).GetCustomerEntityById(customerId);
            await transactionApi.CreateTransactionAsync(model, customer, brandId);
            account = await accountApi.GetAsync(model.AccountId);
            return Json(new { success = true, message = "Tạo giao dịch thành công!", balance = account.Balance }, JsonRequestBehavior.AllowGet);
        }
        //public ActionResult Create(int Id)
        //{
        //    TransactionEditViewModel model = new TransactionEditViewModel();
        //    AccountApi accountApi = new AccountApi();
        //    model.ActiveAccounts = accountApi.GetActiveAccountByCusId(Id);
        //    model.IsIncreaseTransaction = true;
        //    return PartialView(model);
        //}

        public ActionResult Create(int id)
        {
            TransactionEditViewModel model = new TransactionEditViewModel();
            AccountApi accountApi = new AccountApi();
            model.ActiveAccounts = accountApi.GetActiveAccountByCardId(id);
            model.IsIncreaseTransaction = true;
            return PartialView(model);
        }

        //[HttpPost]
        //public async Task<ActionResult> Create(TransactionEditViewModel model)
        //{
        //    if (!this.ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //    var transactionApi = new TransactionApi();
        //    var accountApi = new AccountApi();
        //    var account = await accountApi.GetAsync(model.AccountId);
        //    if ((account.Balance == 0 || account.Balance==null ||account.Balance < model.Amount) && model.IsIncreaseTransaction==false)
        //    {
        //        TempData["msg"] = "<script>alert('Số dư của tài khoản hiện đang bé hơn số tiền giảm!');</script>";

        //        return RedirectToAction("CustomerDetail", "Customer", new { Id = account.CustomerID });
        //    }
        //    await transactionApi.CreateTransactionAsync(model);
        //    return RedirectToAction("CustomerDetail", "Customer", new { Id = account.CustomerID });
        //}

        [HttpPost]
        public async Task<JsonResult> Create(TransactionEditViewModel model)
        {
            var transactionApi = new TransactionApi();
            var accountApi = new AccountApi();
            var storeApi = new StoreApi();
            var store = storeApi.Get(model.StoreId);
            var account = await accountApi.GetAsync(model.AccountId);
            if (model.Amount == 0 || model.Amount.ToString().Length > 9)
            {
                return Json(new { success = false, message = "Số tiền tăng hoặc giảm cần lớn hơn 0 và cần ít hơn 9 chữ số!" },
                    JsonRequestBehavior.AllowGet);
            }
            //if ((account.Balance == null || account.Balance < double.Parse(model.Amount.ToString())) && model.IsIncreaseTransaction == false)
            //{
            //    return Json(new { success = false, message = "Số dư của tài khoản hiện đang bé hơn số tiền giảm!" },
            //        JsonRequestBehavior.AllowGet);
            //}
            model.Amount = (decimal)model.Amount;
            model.StoreId = store.ID;
            model.Status = (int)TransactionStatus.Approve;
            var customerId = (new MembershipCardApi()).GetMembershipCardById(account.MembershipCardId.GetValueOrDefault()).CustomerId;
            var customer = (new CustomerApi()).GetCustomerEntityById(customerId);
            await transactionApi.CreateTransactionAsync(model, customer, model.BrandId);
            account = await accountApi.GetAsync(model.AccountId);
            return Json(new { success = true, message = "Tạo giao dịch thành công!", balance = account.Balance }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int Id)
        {
            var transactionApi = new TransactionApi();
            var model = new TransactionEditViewModel(transactionApi.Get(Id), this.Mapper);
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> Approve(int transactionId, bool isApproved)
        {
            try
            {
                var transactionApi = new TransactionApi();
                var model = transactionApi.Get(transactionId);
                if (isApproved)
                {
                    model.Status = (int)TransactionStatus.Approve;
                    var accountApi = new AccountApi();
                    var account = accountApi.Get(model.AccountId);
                    var message = "";
                    var membershipCardApi = new MembershipCardApi();
                    if (account != null)
                    {
                        var membershipCard = membershipCardApi.GetMembershipCardById(account.MembershipCardId.Value);
                        if (model.IsIncreaseTransaction)
                        {
                            account.Balance += model.Amount;
                            message = (new BrandApi().Get(model.BrandId)).BrandName + ". "
                            + DateTime.Now.ToShortDateString() + ";" + DateTime.Now.ToString("HH:mm") + ". "
                            + "TK: xxxxx" + membershipCard.MembershipCardCode.Substring(5, membershipCard.MembershipCardCode.Length - 5) + ". "
                            + "PS:" + "+" + Utils.ToMoney((double)model.Amount) + " VNĐ" + "."
                            + "Tại: " + (new StoreApi().GetStoreById(model.StoreId)).Name + "."
                            + model.Notes == null ? "ND: " + model.Notes : ""
                            + "Số dư tài khoản: " + Utils.ToMoney((double)account.Balance) + " VNĐ";
                        }
                        else
                        {
                            account.Balance -= model.Amount;
                            message = (new BrandApi().Get(model.BrandId)).BrandName + ". "
                           + DateTime.Now.ToShortDateString() + ";" + DateTime.Now.ToString("HH:mm") + ". "
                           + "TK: xxxxx" + membershipCard.MembershipCardCode.Substring(5, membershipCard.MembershipCardCode.Length - 5) + ". "
                           + "PS:" + "-" + Utils.ToMoney((double)model.Amount) + " VNĐ" + "."
                           + "Tại: " + (new StoreApi().GetStoreById(model.StoreId)).Name + "."
                           + model.Notes == null ? "ND: " + model.Notes : ""
                           + "Số dư tài khoản: " + Utils.ToMoney((double)account.Balance) + " VNĐ";
                        }
                        await accountApi.EditAsync(account.AccountID, account);
                        var customerApi = new CustomerApi();
                        var customer = customerApi.Get(membershipCard.CustomerId);
                        try
                        {
                            Utils.SendSMS(customer.Phone, message, model.BrandId);
                        }
                        catch (Exception)
                        {
                        }
                      
                    }
                }
                else
                {
                    model.Status = (int)TransactionStatus.Cancel;
                }
                await transactionApi.EditAsync(transactionId, model);
                return Json(new { success = true });
            }
            catch (System.Exception e)
            {
                return Json(new { success = false });
            }
        }

        public JsonResult CheckAccount(int accountId)
        {
            var accountApi = new AccountApi();
            var result = accountApi.GetAccountEntityById(accountId);
            var balance = (int)result.Balance;
            return Json(new
            {
                typeId = result.Type,
                balance = balance
            }, JsonRequestBehavior.AllowGet);
        }
        //TODO: AccountViewModel chưa co membershipcard để lấy CustomerID
        [HttpPost]
        public async Task<ActionResult> Edit(TransactionEditViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View(model);
            }
            var transactionApi = new TransactionApi();
            var accountApi = new AccountApi();

            var account = await accountApi.GetAsync(model.AccountId);
            if (model.IsIncreaseTransaction == false && account.Balance < model.Amount)
            {
                //return RedirectToAction("CustomerDetail", "Customer", new { Id = account.CustomerID });
                return RedirectToAction("Index", "Transaction");
            }
            await transactionApi.EditTransactionAsync(model);
            //return RedirectToAction("CustomerDetail", "Customer", new { Id = account.CustomerID });
            return RedirectToAction("Index", "Transaction");
        }
        public async Task<JsonResult> Delete(int? Id)
        {
            try
            {
                var transactionApi = new TransactionApi();
                var model = await transactionApi.GetAsync(Id);
                if (model == null)
                {
                    return Json(new { success = false });
                }
                await transactionApi.DeleteAsync(model);
                return Json(new { success = true });
            }
            catch (System.Exception e)
            {
                return Json(new { success = false });
            }
        }
        public ActionResult CheckMembershipCardCode(int brandId, int membershipCardCode)
        {
            try
            {
                var membershipCardApi = new MembershipCardApi();
                var accountApi = new AccountApi();
                string membershipCardCode1 = membershipCardApi.GetMembershipCardById(accountApi.Get(membershipCardCode).MembershipCardId.Value).MembershipCardCode;
                var currentCard = membershipCardApi.GetMembershipCardByCode(membershipCardCode1.Trim());

                if (currentCard != null && currentCard.BrandId == brandId)
                {
                    if (currentCard.Accounts != null && currentCard.Accounts.Count > 0)
                    {
                        foreach (var account in currentCard.Accounts)
                        {
                            if (account.Type == (int)AccountTypeEnum.CreditAccount)
                            {
                                var customer = currentCard.Customer;
                                return Json(new { success = true, AccountName = account.AccountName, Customer = new { Name = customer.Name, Phone = customer.Phone, MembershipCardCode = membershipCardCode1 } }, JsonRequestBehavior.AllowGet);
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
    }
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
}