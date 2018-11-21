using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.CostManager.Controllers
{
    [Authorize(Roles = "BrandManager, Manager")]
    public class CostManageController : DomainBasedController
    {


        #region Cost Category
        // GET: CostManager/CostManage
        public ActionResult IndexCostManage(string storeId)
        {
            ViewBag.storeId = storeId;
            return View();
        }

        public JsonResult GetDataCostCategory(JQueryDataTableParamModel param, int? status, int? brandId)
        {
            var api = new CostCategoryApi();
            var listCategory = api.GetCostCategories().Where(q => q.Active == true && q.BrandId == brandId).OrderBy(q => q.Type);
            if (status != 3)
            {
                listCategory = listCategory.Where(q => q.Type == status && q.Active == true && q.BrandId == brandId).OrderBy(a => a.Type);
            }
            var search = listCategory
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.CatName.ToLower().Contains(param.sSearch.ToLower()));
            int count = 0;
            count = param.iDisplayStart + 1;
            try
            {
                var rs = search
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList()
                    .Select(a => new IConvertible[]
                        {
                        count++,
                        string.IsNullOrEmpty(a.CatName) ? "Không xác định" : a.CatName,
                        a.Type,
                        a.CatID
                        });
                var totalRecords = listCategory.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = search.Count(),
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #region Create
        public ActionResult Create()
        {
            var model = new CostCategoryViewModel();
            PrepareCreate(model);
            return PartialView("Create", model);
        }

        [HttpPost]
        public async Task<ActionResult> Create(CostCategoryViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View(model);
            }
            var api = new CostCategoryApi();
            await api.CreateAsync(model);
            return RedirectToAction("IndexCostManage", "CostManage");
        }
        private void PrepareCreate(CostCategoryViewModel model)
        {
            var customerApi = new CostCategoryApi();
            var costCategoryType = Enum.GetValues(typeof(CostTypeEnum)).Cast<CostTypeEnum>().ToList();

            model.ListType = new List<SelectListItem>();

            model.ListType.Add(new SelectListItem()
            {
                Text = "Thu",
                Value = "1",
                Selected = false
            });

            model.ListType.Add(new SelectListItem()
            {
                Text = "Chi",
                Value = "2",
                Selected = false
            });
        }
        #endregion

        #region Edit

        public async Task<ActionResult> Edit(int Id)
        {
            var api = new CostCategoryApi();
            var model = await api.GetAsync(Id);
            model.ListType = new List<SelectListItem>();

            model.ListType.Add(new SelectListItem()
            {
                Text = "Thu",
                Value = "1",
                Selected = false
            });

            model.ListType.Add(new SelectListItem()
            {
                Text = "Chi",
                Value = "2",
                Selected = false
            });
            return PartialView("Edit", model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(CostCategoryViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View(model);
            }
            var api = new CostCategoryApi();
            await api.UpdateCostManageAsync(model);
            return RedirectToAction("IndexCostManage", "CostManage");
        }
        #endregion

        #region
        public async Task<ActionResult> Delete(int Id, int brandId)
        {
            var api = new CostCategoryApi();
            var costApi = new CostApi();
            var model = await api.GetAsync(Id);
            var costs = costApi.GetCostbyCostCategoryandBrand(brandId, Id);
            var checkedList = costs.Select(q => q.CostStatus != (int)CostStatusEnum.Deleted);
            if (checkedList.Count() == 0)
            {
                api.Deactivate(Id);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion
        #endregion

        #region Cost Management

        public ActionResult CostManagement()
        {
            //ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            //var categories = _costCategoryService.GetCostCategories();
            DateTime d = new DateTime(2016, 4, 28, 21, 21, 10);
            //var dateString = Utils.GetCurrentDateTime().Month + "/" + Utils.GetCurrentDateTime().Day + "/" + Utils.GetCurrentDateTime().Year + " " + "11" + ":" + "56";
            var date = Utils.GetCurrentDateTime() - d;
            var time = date.Hours * 3600 + date.Minutes * 60;
            ViewBag.Date = date;
            ViewBag.Time = time;
            return View();
        }

        public JsonResult GetData(JQueryDataTableParamModel param, string startTime, string endTime, int strID, int brandId, int costType)
        {
            var costApi = new CostApi();
            var categoryApi = new CostCategoryApi();
            var storeApi = new StoreApi();

            DateTime startDate, endDate;

            if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
            {
                startDate = Utils.GetCurrentDateTime().GetStartOfDate();
                endDate = Utils.GetCurrentDateTime().GetEndOfDate();
            }
            else if (startTime.Equals(endTime))
            {
                startDate = startTime.ToDateTime().GetStartOfDate();
                endDate = endTime.ToDateTime().GetEndOfDate();
            }
            else
            {
                startDate = startTime.ToDateTime();
                endDate = endTime.ToDateTime();
            }
            IQueryable<Cost> costList;
            if (strID != 0)
            {
                if (costType != 3)
                {
                    costList = costApi.GetCostByRangeTimeAndCostType(startDate, endDate, strID, costType);
                }
                else
                {
                    costList = costApi.GetCostByRangeTimeStoreId(startDate, endDate, strID);
                }
            }
            else
            {
                costList = costApi.GetCostByRangeTimeBrandIdAndCostType(startDate, endDate, brandId, costType);
            }
            int count = param.iDisplayStart + 1;
            //var category = categoryApi.GetCostCategories();
            var store = storeApi.Get();
            var totalRecord = costList.Count();
            if (param.sSearch != null)
            {
                costList = costList.Where(q => (q.CostDescription != null && q.CostDescription.Contains(param.sSearch))
                || (q.CostCode != null && q.CostCode.Contains(param.sSearch))
                || (q.PaidPerson != null && q.PaidPerson.Contains(param.sSearch)));
            }
            var totalDisplay = costList.Count();
            var list = costList.OrderByDescending(a => a.CostDate).Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList()
                .Select(q => new
                {
                    STT = count++,
                    Description = q.CostDescription,
                    //Cate = category.Where(p => p.CatID == q.CatID).Select(p => p.CatName),
                    CostCode = q.CostCode,
                    Cate = q.CostCategory.CatName,
                    Amount = q.Amount.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")),
                    Date = q.CostDate.ToString("dd/MM/yyyy HH:mm"),
                    PaidPerson = q.PaidPerson,
                    LoggedUser = q.LoggedPerson,
                    ApprovedPerson = q.ApprovedPerson,
                    CostID = q.CostID,
                    CostCategoryType = q.CostCategoryType,
                    StoreName = store.Where(p => p.ID == q.StoreId).FirstOrDefault().Name,
                    CostType = q.CostType,
                    InventoryReceiptName = (q.CostInventoryMappings.FirstOrDefault() != null ? q.CostInventoryMappings.FirstOrDefault().InventoryReceipt.Name : "N/A")
                });

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecord,
                iTotalDisplayRecords = totalDisplay,
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportCostExcel(int selectedStoreId, int brandId, string startTime, string endTime)
        {
            var costApi = new CostApi();
            var categoryApi = new CostCategoryApi();
            var storeApi = new StoreApi();
            DateTime startDate, endDate;

            if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
            {
                startDate = Utils.GetCurrentDateTime().GetStartOfDate();
                endDate = Utils.GetCurrentDateTime().GetEndOfDate();
            }
            else if (startTime.Equals(endTime))
            {
                startDate = startTime.ToDateTime().GetStartOfDate();
                endDate = endTime.ToDateTime().GetEndOfDate();
            }
            else
            {
                startDate = startTime.ToDateTime();
                endDate = endTime.ToDateTime();
            }
            IQueryable<Cost> receiptCostList;
            IQueryable<Cost> spendCostList;




            if (selectedStoreId != 0)
            {
                receiptCostList = costApi.GetCostByRangeTimeAndCostType(startDate, endDate, selectedStoreId, (int)CostTypeEnum.ReceiveCost);
                spendCostList = costApi.GetCostByRangeTimeAndCostType(startDate, endDate, selectedStoreId, (int)CostTypeEnum.SpendingCost);
            }
            else
            {
                receiptCostList = costApi.GetCostByRangeTimeBrandIdAndCostType(startDate, endDate, brandId, (int)CostTypeEnum.ReceiveCost);
                spendCostList = costApi.GetCostByRangeTimeBrandIdAndCostType(startDate, endDate, brandId, (int)CostTypeEnum.SpendingCost);
            }
            int count = 1;
            //var category = categoryApi.GetCostCategories();
            var store = storeApi.Get();
            var receiptCostListExcel = receiptCostList.OrderByDescending(a => a.CostDate).ToList()
                .Select(q => new
                {
                    STT = count++,
                    Description = q.CostDescription,
                    //Cate = category.Where(p => p.CatID == q.CatID).Select(p => p.CatName),
                    CostCode = q.CostCode,
                    Cate = q.CostCategory.CatName,
                    Amount = q.Amount.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")),
                    Date = q.CostDate.ToString("dd/MM/yyyy HH:mm"),
                    PaidPerson = q.PaidPerson,
                    LoggedUser = q.LoggedPerson,
                    ApprovedPerson = q.ApprovedPerson,
                    CostID = q.CostID,
                    CostCategoryType = q.CostCategoryType,
                    StoreName = store.Where(p => p.ID == q.StoreId).FirstOrDefault().Name,
                    CostType = q.CostType
                });
            int count2 = 1;
            var spendCostListExcel = spendCostList.OrderByDescending(a => a.CostDate).ToList()
                .Select(q => new
                {
                    STT = count2++,
                    Description = q.CostDescription,
                    //Cate = category.Where(p => p.CatID == q.CatID).Select(p => p.CatName),
                    CostCode = q.CostCode,
                    Cate = q.CostCategory.CatName,
                    Amount = q.Amount.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")),
                    Date = q.CostDate.ToString("dd/MM/yyyy HH:mm"),
                    PaidPerson = q.PaidPerson,
                    LoggedUser = q.LoggedPerson,
                    ApprovedPerson = q.ApprovedPerson,
                    CostID = q.CostID,
                    CostCategoryType = q.CostCategoryType,
                    StoreName = store.Where(p => p.ID == q.StoreId).FirstOrDefault().Name,
                    CostType = q.CostType
                });

            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                #region Thu
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Danh sách thu");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Mã phiếu";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Danh mục";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Cửa hàng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Nội dung thu/chi";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tiền chi";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thời gian";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Người thu chi";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Nhân viên";
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
                foreach (var data in receiptCostListExcel)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.STT;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CostCode;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Cate;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.StoreName;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Description;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Amount;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Date;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PaidPerson;
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.LoggedUser;
                    StartHeaderChar = 'A';
                }
                string storeName;
                storeName = selectedStoreId == 0 ? "Hệ Thống" : storeApi.GetStoreById(selectedStoreId).Name;
                var sDate = startDate.ToString("dd/MM/yyyy");
                var eDate = endDate.ToString("dd/MM/yyyy");
                var dateRange = "(" + sDate + (sDate == eDate ? "" : " - " + eDate) + ")";
                string fileName = "BaoCaoThuChi_" + storeName + dateRange + ".xlsx";
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion
                #region Chi
                ExcelWorksheet ws2 = package.Workbook.Worksheets.Add("Danh sách chi");
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #region Headers
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Mã phiếu";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Danh mục";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Cửa hàng";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Nội dung thu/chi";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tiền chi";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Thời gian";
                ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Người thu chi";
                ws2.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Nhân viên";
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
                foreach (var data in spendCostListExcel)
                {
                    ws2.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.STT;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.CostCode;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Cate;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.StoreName;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Description;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Amount;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Date;
                    ws2.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.PaidPerson;
                    ws2.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.LoggedUser;
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
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion

        }
        public JsonResult GetCostOverView(string startTime, string endTime, int store, int brandId)
        {
            DateTime startDate, endDate;
            startDate = startTime.ToDateTime().GetStartOfDate();
            endDate = endTime.ToDateTime().GetEndOfDate();
            CostApi costApi = new CostApi();
            PaymentApi paymentApi = new PaymentApi();
            var cost = costApi.GetCostByRangeTimeStoreBrand(startDate, endDate, store, brandId);
            CostOverViewViewModel model = new CostOverViewViewModel();
            var receiptCost = cost.Where(q => q.CostType == (int)CostTypeEnum.ReceiveCost);
            model.TotalReceipt = (receiptCost.Count() != 0 ? Math.Round(receiptCost.Sum(q => q.Amount), 2) : 0);
            var spendCost = cost.Where(q => q.CostType == (int)CostTypeEnum.SpendingCost);
            model.TotalSpend = (spendCost.Count() != 0 ? Math.Round(spendCost.Sum(q => q.Amount), 2) : 0);
            var payment = paymentApi.GetStorePaymentByTimeRangeAndType(store, startDate, endDate, brandId, (int)PaymentTypeEnum.Debt);
            model.TotalDebt = (payment.Count() != 0 ? Math.Round(payment.Sum(q => q.Amount), 2) : 0);
            return Json(new { model }, JsonRequestBehavior.AllowGet);
        }

        //Tạo phiếu chi
        public ActionResult CreatingSpendForm(string costCategoryType, int brandId)
        {
            var categoryApi = new CostCategoryApi();

            var model = new CostViewModel();
            int type = Convert.ToInt32(costCategoryType);
            var categories = categoryApi.GetCostCategories().Where(a => a.Type == type && a.Active == true && a.BrandId == brandId);
            //model.CostDate = Utils.GetCurrentDateTime();
            model.CostCategoryType = type;
            model.Categories = categories;
            return PartialView("CreatingSpendForm", model);
        }

        //public ActionResult CostOverview(string startTime, string endTime, int status, int storeId)
        //{
        //    var model = new CostOverViewModel();
        //    DateTime startDate, endDate;
        //    if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
        //    {
        //        startDate = DateTime.Now.GetStartOfDate();
        //        endDate = DateTime.Now.GetEndOfDate();
        //    }
        //    else if (startTime.Equals(endTime))
        //    {
        //        startDate = DateTime.Parse(startTime).GetStartOfDate();
        //        endDate = DateTime.Parse(endTime).GetEndOfDate();
        //    }
        //    else
        //    {
        //        startDate = DateTime.Parse(startTime);
        //        endDate = DateTime.Parse(endTime);
        //    }
        //    var costApi = new CostApi();
        //    var costList = costApi.GetCostCategoriesByRangeTime(startDate, endDate, storeId);
        //    var amountReceive = costList.Where(a => a.CostType == (int)CostTypeEnum.ReceiveCost).Sum(b => b.Amount);
        //    var amountSpend = costList.Where(a => a.CostType == (int)CostTypeEnum.SpendingCost).Sum(b => b.Amount);

        //    model.AmountReceipt = amountReceive;
        //    model.AmountSpend = amountSpend;
        //    model.Status = status;
        //    model.StartTime = startTime;
        //    model.EndTime = endTime;
        //    return PartialView("_CostOverview", model);
        //}

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
                api.Create(model);

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
            var categories = categoryApi.GetCostCategories().Where(a => a.Type == type && a.Active == true && a.BrandId == brandId);
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

        public JsonResult CancelCost(string CostID)
        {

            var costApi = new CostApi();
            var paymentApi = new PaymentApi();
            var costInventoryMappingApi = new CostInventoryMappingApi();
            int costId = int.Parse(CostID);
            try
            {
                var cost = costApi.GetCostById(costId).FirstOrDefault();
                var payment = cost.Payments.ToList();
                foreach (var item in payment)
                {
                    if (item.ToRentID != null)
                    {
                        var debtPayment = paymentApi.GetPaymentByToRentAndType(item.ToRentID.GetValueOrDefault(), (int)PaymentTypeEnum.Debt).FirstOrDefault();
                        paymentApi.Delete(item.PaymentID);
                        debtPayment.Amount = debtPayment.Amount + item.Amount;
                        debtPayment.FCAmount = (decimal)(debtPayment.Amount);
                        debtPayment.Order.PaymentStatus = (int)OrderPaymentStatusEnum.Debt;
                        paymentApi.UpdatePayment(debtPayment);
                    }
                    else
                    {
                        paymentApi.Delete(item.PaymentID);
                    }
                }
                var costInventoryMapping = cost.CostInventoryMappings.ToList();
                foreach (var item2 in costInventoryMapping)
                {
                    costInventoryMappingApi.DeleteByEntity(item2);
                }
                costApi.Delete(costId);
            }
            catch (Exception e)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            //_costService.DeleteCost(id);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            //return Json(new { success = false }, JsonRequestBehavior.AllowGet);
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
        #endregion

        public ActionResult CreateReceipt(int storeId, int brandId)
        {
            var username = User.Identity.Name;
            ViewBag.Creators = new AspNetUserApi().GetActive().Where(q => q.BrandId == brandId && q.UserName != username).ToList();

            var costCategoryApi = new CostCategoryApi();
            ViewBag.CostCategory = costCategoryApi.GetActiveCostCategoriesByBrandId(brandId, (int)CostTypeEnum.ReceiveCost);

            var orderApi = new OrderApi();
            var customerApi = new CustomerApi();
            //var customerList = orderApi.GetAllDebtOrderByBrand(brandId).GroupBy(q => new { q.Customer.Name, q.CustomerID }).Select(q=> new { q.Key.Name, q.Key.CustomerID }).ToList();
            var customerList = orderApi.GetAllDebtOrderByBrand(brandId).GroupBy(q => new { q.Customer.Name, q.CustomerID }).ToList();
            List<CustomerListViewModel> cusList = new List<CustomerListViewModel>();
            foreach (var item in customerList)
            {
                cusList.Add(new CustomerListViewModel
                {
                    customerId = item.Key.CustomerID.Value,
                    customerName = item.Key.Name
                });
            }
            ViewBag.CustomerList = cusList;
            return View();
        }

        public ActionResult CreateSpend(int storeId, int brandId)
        {
            var username = User.Identity.Name;
            ViewBag.Creators = new AspNetUserApi().GetActive().Where(q => q.BrandId == brandId && q.UserName != username).ToList();

            var costCategoryApi = new CostCategoryApi();
            ViewBag.CostCategory = costCategoryApi.GetActiveCostCategoriesByBrandId(brandId, (int)CostTypeEnum.SpendingCost);

            //var orderApi = new OrderApi();
            //var customerList = orderApi.GetAllDebtOrderByBrand(brandId).GroupBy(q => new { q.Customer.Name, q.CustomerID }).Select(q=> new { q.Key.Name, q.Key.CustomerID }).ToList();
            //var customerList = orderApi.GetAllDebtOrderByBrand(brandId).GroupBy(q => new { q.Customer.Name, q.CustomerID }).ToList();
            //List<CustomerListViewModel> cusList = new List<CustomerListViewModel>();
            //foreach (var item in customerList)
            //{
            //    cusList.Add(new CustomerListViewModel
            //    {
            //        customerId = item.Key.CustomerID.Value,
            //        customerName = item.Key.Name
            //    });
            //}
            //ViewBag.CustomerList = cusList;
            return View();
        }

        public JsonResult LoadDebtOrderPayment(int brandId, string[] customerId, string payDate)
        {
            var payDateFormatted = Utils.ToDateTime(payDate).GetEndOfDate();
            var orderApi = new OrderApi();
            var listCustomerId = new List<int>();
            foreach (var item in customerId)
            {
                listCustomerId.Add(int.Parse(item));
            }
            var listOrder = orderApi.GetAllDebtOrderByBrandAndCustomerId(brandId, listCustomerId).Where(q => q.CheckInDate <= payDateFormatted).ToList().Select(q => new DebtOrderViewModel
            {
                InvoiceId = q.InvoiceID.ToString(),
                RentId = q.RentID,
                CheckInDate = q.CheckInDate != null ? q.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm") : "N/A",
                TotalAmount = q.TotalAmount,
                PaymentID = q.Payments.Where(a => a.Type == (int)PaymentTypeEnum.Debt).FirstOrDefault().PaymentID,
                ReceivablesAmount = q.Payments.Where(a => a.Type == (int)PaymentTypeEnum.Debt).ToList().Sum(a => a.Amount)
            });
            return Json(new
            {
                listOrder
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult CreateReceipt(string model)
        {
            var data = JsonConvert.DeserializeObject<CostCreateViewModel>(model);
            data.CostType = (int)CostTypeEnum.ReceiveCost;
            var now = Utils.GetCurrentDateTime();

            var orderApi = new OrderApi();
            var paymentApi = new PaymentApi();
            var costApi = new CostApi();
            int costId = -1;
            //Tao Cost
            try
            {
                CostViewModel costModel = new CostViewModel
                {
                    Amount = data.Amount,
                    CatID = data.CatId,
                    CostCode = data.CostCode,
                    CostDate = now,
                    CostStatus = (int)CostStatusEnum.Approved,
                    CostDescription = data.CostDescription,
                    LoggedPerson = data.LoggedPerson,
                    PaidPerson = data.PaidPerson,
                    StoreId = data.StoreId,
                    CostCategoryType = data.CostCategoryType,
                    CostType = (int)CostTypeEnum.ReceiveCost
                };
                costId = costApi.CreateCost(costModel);
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            if (data.listPayment.Count() == 0)
            {
                try
                {
                    var payment = new HmsService.Models.Entities.Payment()
                    {
                        Amount = data.Amount,
                        CurrencyCode = "VND",
                        FCAmount = (decimal)data.Amount,
                        PayTime = now,
                        Status = (int)PaymentStatusEnum.Approved,
                        Type = (int)PaymentTypeEnum.Cash,
                        CostID = costId
                    };
                    int paymenId = paymentApi.CreatePaymentReturnId(payment);
                }
                catch
                {
                    if (costId != -1)
                    {
                        costApi.Delete(costId);
                    }
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                List<HmsService.Models.Entities.Payment> listUpdatePayment = new List<HmsService.Models.Entities.Payment>();
                List<int> listCreatePaymentId = new List<int>();
                //Duyet tung payment tra no
                foreach (var item in data.listPayment)
                {
                    //Update payment no
                    try
                    {
                        //Lay payment no
                        var debPayment = paymentApi.GetPaymentById(item.paymentId).Where(q => q.Type == (int)PaymentTypeEnum.Debt).FirstOrDefault();
                        //Add data vao list backup
                        listUpdatePayment.Add(debPayment);
                        // Kiem tra tien tra bang tien no
                        if (item.amount > 0 && item.amount == item.receivablesAmount)
                        {
                            //Update payment no = 0
                            debPayment.Amount = 0;
                            debPayment.FCAmount = 0;
                            //Update order status = finish
                            debPayment.Order.OrderStatus = (int)OrderStatusEnum.Finish;
                            debPayment.Order.PaymentStatus = (int)OrderPaymentStatusEnum.Finish;
                        }
                        else if (item.amount > 0 && item.amount < item.receivablesAmount)
                        {
                            //Update payment no = 0
                            debPayment.Amount = debPayment.Amount - item.amount;
                            debPayment.FCAmount = (decimal)(debPayment.Amount);
                        }
                        //Update payment + order nợ
                        var paymentId = paymentApi.UpdatePayment(debPayment);

                    }
                    catch
                    {
                        foreach (var id in listCreatePaymentId)
                        {
                            paymentApi.Delete(id);
                        }
                        foreach (var payment in listUpdatePayment)
                        {
                            paymentApi.UpdatePayment(payment);
                        }
                        if (costId != -1)
                        {
                            costApi.Delete(costId);
                        }
                        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                    }

                    // Tao payment cho tien vua them vao
                    try
                    {
                        var payment = new HmsService.Models.Entities.Payment()
                        {
                            Amount = item.amount,
                            ToRentID = int.Parse(item.rentId),
                            CurrencyCode = "VND",
                            FCAmount = (decimal)item.amount,
                            PayTime = now,
                            Status = (int)PaymentStatusEnum.Approved,
                            Type = (int)PaymentTypeEnum.PayDebt,
                            CostID = costId
                        };
                        int paymenId = paymentApi.CreatePaymentReturnId(payment);
                        listCreatePaymentId.Add(paymenId);
                    }
                    catch
                    {
                        foreach (var id in listCreatePaymentId)
                        {
                            paymentApi.Delete(id);
                        }
                        foreach (var payment in listUpdatePayment)
                        {
                            paymentApi.UpdatePayment(payment);
                        }
                        if (costId != -1)
                        {
                            costApi.Delete(costId);
                        }
                        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                    }

                }
            }


            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateSpend(string model)
        {
            var data = JsonConvert.DeserializeObject<CostCreateViewModel>(model);
            data.CostType = (int)CostTypeEnum.SpendingCost;
            var now = Utils.GetCurrentDateTime();

            var orderApi = new OrderApi();
            var paymentApi = new PaymentApi();
            var costApi = new CostApi();
            int costId = -1;
            //Tao Cost
            try
            {
                CostViewModel costModel = new CostViewModel
                {
                    Amount = data.Amount,
                    CatID = data.CatId,
                    CostCode = data.CostCode,
                    CostDate = now,
                    CostStatus = (int)CostStatusEnum.Approved,
                    CostDescription = data.CostDescription,
                    LoggedPerson = data.LoggedPerson,
                    PaidPerson = data.PaidPerson,
                    StoreId = data.StoreId,
                    CostCategoryType = data.CostCategoryType,
                    CostType = (int)CostTypeEnum.SpendingCost
                };
                costId = costApi.CreateCost(costModel);
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var payment = new HmsService.Models.Entities.Payment()
                {
                    Amount = data.Amount,
                    CurrencyCode = "VND",
                    FCAmount = (decimal)data.Amount,
                    PayTime = now,
                    Status = (int)PaymentStatusEnum.Approved,
                    Type = (int)PaymentTypeEnum.Cash,
                    CostID = costId
                };
                int paymenId = paymentApi.CreatePaymentReturnId(payment);
            }
            catch
            {
                if (costId != -1)
                {
                    costApi.Delete(costId);
                }
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }


            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetListStore()
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
    }


    public class DebtOrderViewModel
    {
        public string InvoiceId { get; set; }
        public int RentId { get; set; }
        public int? PaymentID { get; set; }
        public string CheckInDate { get; set; }
        public double TotalAmount { get; set; } //Tổng tiền hóa đơn
        public double ReceivablesAmount { get; set; } //Số tiền còn lại cần phải thu
        public double Amount { get; set; } //Số tiền thu

    }

    public class CustomerListViewModel
    {
        public int customerId { get; set; }
        public string customerName { get; set; }
    }

    public class PaymentUpdated
    {
        public int PaymentId { get; set; }
        public double Amount { get; set; }
        public decimal FCAmount { get; set; }
    }

}