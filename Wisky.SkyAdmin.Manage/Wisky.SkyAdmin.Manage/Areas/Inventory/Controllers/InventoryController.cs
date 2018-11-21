using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using Microsoft.Ajax.Utilities;
using OfficeOpenXml;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebGrease.Css.Extensions;
using Wisky.SkyAdmin.Manage.Controllers;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data.Entity;

namespace WiSky.SkyAdmin.Manage.Areas.Inventory.Controllers
{
    //Sử dụng ref trong async method
    public class Ref<T>
    {
        public Ref() { }
        public Ref(T value) { Value = value; }
        public T Value { get; set; }
        public override string ToString()
        {
            T value = Value;
            return value == null ? "" : value.ToString();
        }
        public static implicit operator T(Ref<T> r) { return r.Value; }
        public static implicit operator Ref<T>(T value) { return new Ref<T>(value); }
    }

    public class InventoryController : DomainBasedController
    {
        #region Tồn kho cuối ngày
        // GET: CRM/Inventory
        public ActionResult Index(int storeId, int brandId)
        {
            ViewBag.StoreId = storeId.ToString();
            var productItemCategoryApi = new ProductItemCategoryApi();
            var templateApi = new InventoryTemplateReportApi();
            IEnumerable<StoreViewModel> stores = null;
            // Khi storeId = 0 thì mới load tất cả Store lên web
            if (storeId == 0)
            {
                var storeApi = new StoreApi();
                stores = storeApi.GetStoreByBrandId(brandId).ToList();
            }

            ViewBag.Stores = stores;
            ViewBag.InventoryTemplates = templateApi.GetBrandActiveTemplate(brandId);
            ViewBag.ProductCategories = productItemCategoryApi.GetItemCategories().Where(q => q.BrandId == brandId);
            ViewBag.InstockDate = Utils.GetCurrentDateTime().AddDays(-1).ToString("dd/MM/yyyy");
            return View();
        }

        //public async Task<JsonResult> GetInventory(JQueryDataTableParamModel param, string cateId, string dateTime, int storeId, int brandId, int? selectedStoreId)
        //{
        //    var api = new InventoryDateReportApi();
        //    var invenDaReItemApi = new InventoryDateReportItemApi();
        //    InventoryDateReportViewModel reportByTimeRange = new InventoryDateReportViewModel();
        //    var isLastReport = false;
        //    //var lastCheckDate = reportByTimeRange != null
        //    //    ? reportByTimeRange.CreateDate.ToString("dd/MM/yyyy")
        //    //    : Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");
        //    DateTime startDate, endDate;

        //    // Kiểm tra store hiện tại là store nào
        //    var curStoreId = selectedStoreId != null ? selectedStoreId.Value : storeId;

        //    //Kiểm tra dateTime có được chọn hay không. Nếu có thì lấy theo ngày được chọn, không thì lấy theo ngày mặc định hiện tại.
        //    if (!dateTime.IsNullOrWhiteSpace())
        //    {
        //        //Chọn ngày hiện tại thì tăng lên 1 để add-2 đúng
        //        startDate = dateTime.ToDateTime().GetStartOfDate();
        //        //dateTime.ToDateTime().GetEndOfDate()
        //        endDate = dateTime.ToDateTime().GetEndOfDate();
        //        //Lấy list reportByTimeRange
        //        reportByTimeRange = await api.GetInventoryDateReportByTimeRange(startDate, endDate, curStoreId);
        //        //.FirstOrDefault(q => q.CreateDate >= startDate && q.CreateDate <= endDate && q.StoreId == storeId);
        //        var lastReport = api.GetLastReport(curStoreId);

        //        //Kiem tra reportByTimeRange co phai report mới nhất hay không
        //        isLastReport = (lastReport != null && reportByTimeRange != null && lastReport.CreateDate == reportByTimeRange.CreateDate) ? true : false;
        //    }
        //    else
        //    {
        //        startDate = Utils.GetCurrentDateTime().GetStartOfDate().AddDays(-1);
        //        endDate = Utils.GetCurrentDateTime().GetEndOfDate().AddDays(-1);
        //        reportByTimeRange = api.GetLastReport(curStoreId);
        //        isLastReport = true;
        //    }
        //    var dateReportApi = new DateReportApi();

        //    //Lấy số lượng cuối cùng mới nhất từ datereport
        //    var StoreFinal = (dateReportApi
        //        .GetDateReportTimeRangeAndStore(startDate, endDate, curStoreId).FirstOrDefault());
        //    double StoreFinalAmount = 0;
        //    if (StoreFinal != null)
        //    {
        //        StoreFinalAmount = (double)StoreFinal.FinalAmount;
        //    }
        //    else
        //    {
        //        StoreFinalAmount = 1;
        //    }
        //    //if (StoreFinalAmount == null || StoreFinalAmount == 0)
        //    //{
        //    //    StoreFinalAmount = 1;
        //    //}

        //    //lấy report mới nhất ở ngày trước ngày hiện tại
        //    var beforeLastReport = await api.GetInventoryDateReportByTimeRange(startDate.AddDays(-1), endDate.AddDays(-1), curStoreId);

        //    //Lấy inDayCheckingItems: list checkingItems trong khoảng thời gian từ startDate tới endDate            
        //    var inventoryCheckItemApi = new InventoryCheckingItemApi();
        //    var inDayCheckingItems = inventoryCheckItemApi.GetStoreInventoryCheckingItemByTimeRange(startDate, endDate, curStoreId, brandId);
        //    //Lấy list Product items
        //    var productItemApi = new ProductItemApi();
        //    //IEnumerable<ProductItemViewModel> productItems = productItemApi.GetProductItemByStore(curStoreId, brandId);
        //    IQueryable<ProductItemViewModel> productItems = productItemApi.GetAvailableProductItemsModelByBrand(brandId);

        //    //Lấy array cateid được chọn.
        //    int[] categoriesId = null;
        //    if (!cateId.IsNullOrWhiteSpace())
        //    {
        //        categoriesId = cateId.Split(',').Select(q => int.Parse(q)).ToArray();
        //        productItems = productItems.Where(a => categoriesId.Contains(a.CatID ?? 0));
        //    }
        //    if (!string.IsNullOrWhiteSpace(param.sSearch)) {
        //        productItems = productItems.Where(a => a.ItemName.Contains(param.sSearch));
        //    }
        //    var query = productItems
        //                .OrderBy(q => q.ItemName)
        //                .ThenBy(q => q.ItemID)
        //                .Skip(param.iDisplayStart).Take(param.iDisplayLength)
        //                //.OrderByDescending(q => q.CreateDate)
        //                //.OrderBy(q => q.IndexPriority)
        //                .ToList();

        //    List<dynamic> listDt = new List<dynamic>();
        //    var count = param.iDisplayStart;
        //    var InStockDate = "";
        //    //Nếu beforeLastReport tồn tại thì lấy InStockDate là startDate-1
        //    if (beforeLastReport != null)
        //    {
        //        InStockDate = startDate.AddDays(-1).ToString("dd/MM");
        //    }

        //    foreach (var productItem in query)
        //    {
        //        var inStockQuantity = 0.0;

        //        var checkingQuantity = "";
        //        var checkingDate = "";

        //        //Nếu list inDayCheckingItems > 0 thì lấy checkingDate trong InventoryChecking mới nhất
        //        if (inDayCheckingItems.Count() > 0)
        //        {
        //            checkingDate = inDayCheckingItems.FirstOrDefault().InventoryChecking.CheckingDate.ToString("dd/MM/yyyy");
        //        }

        //        //Với mỗi ItemID của inDayCheckingItems = item của ProductItem => cộng quantity vào chuỗi checkingQuantity
        //        inDayCheckingItems.ForEach(q =>
        //        {
        //            if (q.ItemID == productItem.ItemID)
        //            {
        //                checkingQuantity += q.Quantity + " ";
        //            }
        //        });
        //        if (reportByTimeRange != null)
        //        {
        //            if (beforeLastReport != null)
        //            {
        //                //Xét reportByTimeRange tồn tại rồi xét beforeLastReport tồn tại thì lấy Item đầu tiên trong list bằng vs id của productItem
        //                //var beforeLstReItem = invenDaReItemApi.GetItemByReportId(beforeLastReport.ReportID);
        //                //if (beforeLstReItem.Count() != 0)
        //                //{
        //                //    var checkExist = beforeLstReItem.FirstOrDefault(q => q.ItemID == productItem.ItemID);
        //                //    if (checkExist != null)
        //                //    {
        //                //        inStockQuantity = beforeLstReItem.FirstOrDefault(q => q.ItemID == productItem.ItemID).RealAmount ?? 0;
        //                //        //inStockQuantity = beforeLastReport.InventoryDateReportItem.FirstOrDefault(q => q.ItemID == productItem.ItemID).RealAmount ?? 0;
        //                //    }
        //                //    else
        //                //    {
        //                //        inStockQuantity = 0;
        //                //    }
        //                //}
        //                //else
        //                //{
        //                //    inStockQuantity = 0;
        //                //}
        //                //FirstOrDefault(q => q.ItemID == productItem.ItemID)
        //                //Nếu checkExist tồn tại thì lấy inStockQuantity là RealAmount, nếu ko thì là 0
        //                var checkExist = beforeLastReport.InventoryDateReportItems.FirstOrDefault(q => q.ItemID == productItem.ItemID);
        //                if (checkExist != null)
        //                {
        //                    inStockQuantity = beforeLastReport.InventoryDateReportItems.FirstOrDefault(q => q.ItemID == productItem.ItemID).RealAmount ?? 0;
        //                }
        //                else
        //                {
        //                    inStockQuantity = 0;
        //                }

        //            }

        //            var SoldQuantity = 0.0; var ReturnQuantity = 0.0; var DraftQuantity = 0.0; var OutChangeQuantity = 0.0;
        //            var InQuantity = 0.0; var InChangeQuantity = 0.0;
        //            var theoryQuantity = 0.0; var realQuantity = 0.0;
        //            var itemId = -1;
        //            var isChanged = false;
        //            double COS = 0;

        //            var reportItem = reportByTimeRange.InventoryDateReportItems.FirstOrDefault(q => q.ItemID == productItem.ItemID);
        //            //var invenDaReItem = reTiRaItem.FirstOrDefault(x => x.ItemID == productItem.ItemID);

        //            if (reportItem == null)
        //            {
        //                listDt.Add(new
        //                {
        //                    No = ++count,
        //                    itemName = productItem.ItemName,
        //                    unit = productItem.Unit,
        //                    unitPrice = productItem.Price,
        //                    TotalRealAmount = 0,
        //                    categoryName = productItem.ItemCategory.CateName,
        //                    inStockQuantity = 0,
        //                    SoldQuantity = 0,
        //                    ReturnQuantity = 0,
        //                    DraftQuantity = 0,
        //                    OutChangeQuantity = 0,
        //                    TotalExport = 0,
        //                    InQuantity = 0,
        //                    InChangeQuantity = 0,
        //                    TotalImport = 0,
        //                    theoryQuantity = 0,
        //                    realQuantity = 0,
        //                    checkingQuantity = "",
        //                    itemId = productItem.ItemID,
        //                    isChanged = false,
        //                    isLastReport,
        //                    InStockDate = InStockDate,
        //                    COS = 0,
        //                });

        //    var InvenDateReportItemApi = new InventoryDateReportItemApi();
        //                var reportItems = new InventoryDateReportItemViewModel()
        //                {
        //                    ReportID = reportByTimeRange.ReportID,
        //                    Quantity = 0,
        //                    ItemID = productItem.ItemID,
        //                    CancelAmount = 0,
        //                    ExportAmount = 0,
        //                    ChangeInventoryAmount = 0,
        //                    ImportAmount = 0,
        //                    RealAmount = 0,
        //                    ReturnAmount = 0,
        //                    SoldAmount = 0,
        //                    TheoryAmount = 0,
        //                    TotalExport = 0,
        //                    TotalImport = 0,
        //                    ReceivedChangeInventoryAmount = 0,
        //                    Price = productItem.Price ?? 0
        //                };
        //                //await InvenDateReportItemApi.CreateAsync(reportItems);
        //            }
        //            else
        //            {
        //                //var tmp = reportByTimeRange.InventoryDateReportItem.FirstOrDefault(x => x.ItemID == productItem.ItemID);

        //                // ItemReport existed
        //                //if (tmp != null)
        //                //{
        //                SoldQuantity = reportItem.SoldAmount ?? 0;
        //                ReturnQuantity = reportItem.ReturnAmount ?? 0;
        //                DraftQuantity = reportItem.CancelAmount ?? 0;
        //                OutChangeQuantity = reportItem.ChangeInventoryAmount ?? 0;
        //                InQuantity = reportItem.ImportAmount ?? 0;
        //                InChangeQuantity = reportItem.ReceivedChangeInventoryAmount ?? 0;
        //                theoryQuantity = reportItem.TheoryAmount ?? 0;
        //                realQuantity = reportItem.RealAmount ?? 0;
        //                itemId = reportItem.ItemID;
        //                isChanged = (reportItem.IsSelected != null && (bool)reportItem.IsSelected) ? true : false;
        //                COS = (double)(productItem.Price * SoldQuantity / StoreFinalAmount);
        //                //Bao cao da ton tai
        //                listDt.Add(new
        //                {
        //                    No = ++count,
        //                    itemName = productItem.ItemName,
        //                    unit = productItem.Unit,
        //                    unitPrice = reportItem.Price,
        //                    TotalRealAmount = string.Format(CultureInfo.InvariantCulture,
        //                "{0:0,0}", realQuantity * reportItem.Price),
        //                    categoryName = productItem.ItemCategory.CateName,
        //                    inStockQuantity = inStockQuantity,
        //                    SoldQuantity = SoldQuantity,
        //                    ReturnQuantity = ReturnQuantity,
        //                    DraftQuantity = DraftQuantity,
        //                    OutChangeQuantity = OutChangeQuantity,
        //                    TotalExport = (SoldQuantity + ReturnQuantity + DraftQuantity + OutChangeQuantity),
        //                    InQuantity = InQuantity,
        //                    InChangeQuantity = InChangeQuantity,
        //                    TotalImport = (InQuantity + InChangeQuantity),
        //                    theoryQuantity = theoryQuantity,
        //                    realQuantity = realQuantity,
        //                    //checkingQuantity = checkingQuantity, old code dung cho kiem ke
        //                    checkingQuantity = realQuantity - theoryQuantity,
        //                    checkingDate = checkingDate,
        //                    itemId = itemId,
        //                    isChanged = isChanged,
        //                    isLastReport = isLastReport,
        //                    InStockDate = InStockDate,
        //                    COS = COS.ToString("0.00%"),
        //                });
        //            }
        //        }
        //        else
        //        {
        //            listDt.Add(new
        //            {
        //                No = ++count,
        //                itemName = productItem.ItemName,
        //                unit = productItem.Unit,
        //                unitPrice = productItem.Price,
        //                TotalRealAmount = 0,
        //                categoryName = productItem.ItemCategory.CateName,
        //                inStockQuantity = 0,
        //                SoldQuantity = 0,
        //                ReturnQuantity = 0,
        //                DraftQuantity = 0,
        //                OutChangeQuantity = 0,
        //                TotalExport = 0,
        //                InQuantity = 0,
        //                InChangeQuantity = 0,
        //                TotalImport = 0,
        //                theoryQuantity = 0,
        //                realQuantity = 0,
        //                checkingQuantity = "",
        //                itemId = productItem.ItemID,
        //                isChanged = false,
        //                isLastReport,
        //                InStockDate = InStockDate,
        //                COS = 0,
        //            });
        //        }
        //    }

        //    var totalRecords = query.Count;
        //    var totalDisplayRecords = productItems.Count();

        //    return Json(new
        //    {
        //        sEcho = param.sEcho,
        //        iTotalRecords = totalRecords,
        //        iTotalDisplayRecords = totalDisplayRecords,
        //        aaData = listDt,
        //    }, JsonRequestBehavior.AllowGet);
        //}

        public async Task<JsonResult> GetInventory(JQueryDataTableParamModel param, int? selectedTemplate, string cateId, string dateTime, int storeId, int brandId, int? selectedStoreId)
        {
            var api = new InventoryDateReportApi();
            var invenDaReItemApi = new InventoryDateReportItemApi();
            IQueryable<InventoryDateReportViewModel> reportByTimeRange;
            var allowCheck = false;
            //var lastCheckDate = reportByTimeRange != null
            //    ? reportByTimeRange.CreateDate.ToString("dd/MM/yyyy")
            //    : Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");
            DateTime startDate, endDate;

            // Kiểm tra store hiện tại là store nào
            var curStoreId = selectedStoreId != null ? selectedStoreId.Value : storeId;
            var date = dateTime.ToDateTime();
            var yesterday = Utils.GetCurrentDateTime().AddDays(-1);
            //Kiểm tra dateTime có được chọn hay không. Nếu có thì lấy theo ngày được chọn, không thì lấy theo ngày mặc định hiện tại.
            if (!dateTime.IsNullOrWhiteSpace() && date.GetStartOfDate() < yesterday.GetStartOfDate())
            {
                //Chọn ngày hiện tại thì tăng lên 1 để add-2 đúng
                startDate = dateTime.ToDateTime().GetStartOfDate();
                //dateTime.ToDateTime().GetEndOfDate()
                endDate = dateTime.ToDateTime().GetEndOfDate();
                //Lấy list reportByTimeRange
                //reportByTimeRange = api.GetQueryInventoryDateReportByTimeRange(startDate, endDate, curStoreId, brandId);
                //.FirstOrDefault(q => q.CreateDate >= startDate && q.CreateDate <= endDate && q.StoreId == storeId);
                //var lastReport = api.GetLastReport(curStoreId);
                //allowCheck = false;
                //Kiem tra reportByTimeRange co phai report mới nhất hay không
                //allowCheck = (lastReport != null && reportByTimeRange != null && lastReport.CreateDate == reportByTimeRange.SelectCreateDate) ? true : false;
            }
            else
            {
                startDate = yesterday.GetStartOfDate();
                endDate = yesterday.GetEndOfDate();
                //reportByTimeRange = api.GetLastReport(curStoreId);
            }

            allowCheck = (curStoreId == 0 || yesterday.GetStartOfDate() > date.GetStartOfDate()) ? false : true;
            reportByTimeRange = api.GetQueryInventoryDateReportByTimeRange(startDate, endDate, curStoreId, brandId);

            var dateReportApi = new DateReportApi();

            //Lấy số lượng cuối cùng mới nhất từ datereport
            var StoreFinal = dateReportApi
                .GetDateReportByTimeRange(startDate, endDate, brandId, curStoreId);
            double StoreFinalAmount = 0;
            if (StoreFinal.Count() > 0)
            {
                StoreFinalAmount = StoreFinal.Sum(q => q.FinalAmount ?? 0);
            }

            //else
            //{
            //    StoreFinalAmount = 1;
            //}
            //if (StoreFinalAmount == null || StoreFinalAmount == 0)
            //{
            //    StoreFinalAmount = 1;
            //}

            //lấy report mới nhất ở ngày trước ngày hiện tại
            var beforeLastReport = api.GetQueryInventoryDateReportByTimeRange(startDate.AddDays(-1), endDate.AddDays(-1), curStoreId, brandId);

            //Lấy inDayCheckingItems: list checkingItems trong khoảng thời gian từ startDate tới endDate            
            var inventoryCheckItemApi = new InventoryCheckingItemApi();
            var inDayCheckingItems = inventoryCheckItemApi.GetStoreInventoryCheckingItemByTimeRange(startDate, endDate, curStoreId, brandId);
            //Lấy list Product items
            var productItemApi = new ProductItemApi();
            //IEnumerable<ProductItemViewModel> productItems = productItemApi.GetProductItemByStore(curStoreId, brandId);
            IQueryable<ProductItemViewModel> productItems = productItemApi.GetAvailableProductItemsModelByBrand(brandId);

            //Lấy array cateid được chọn.



            //var query = productItems
            //            .OrderBy(q => q.ItemName)
            //            .ThenBy(q => q.ItemID)
            //            .Skip(param.iDisplayStart).Take(param.iDisplayLength)
            //            //.OrderByDescending(q => q.CreateDate)
            //            //.OrderBy(q => q.IndexPriority)
            //            .ToList();

            IEnumerable<ProductItemViewModel> query = new List<ProductItemViewModel>();
            if (selectedTemplate != null)
            {
                var templateApi = new TemplateReportProductItemMappingApi();
                var templateItemsQuery = templateApi.GetQueryTemplateItems(selectedTemplate.Value);
                if (!string.IsNullOrWhiteSpace(param.sSearch))
                {
                    templateItemsQuery = templateItemsQuery.Where(a => a.ProductItem.ItemName.Contains(param.sSearch));
                }
                var templateItems = templateItemsQuery
                                    .OrderByDescending(q => q.MappingIndex)
                                    .ThenBy(q => q.ProductItem.ItemName)
                                    .ThenBy(q => q.ProductItemId)
                                    .Skip(param.iDisplayStart).Take(param.iDisplayLength)
                                    .ToList();
                var itemIds = templateItems.Select(q => q.ProductItemId);
                productItems = productItems.Where(a => itemIds.Contains(a.ItemID));
                var itemsJoin = productItems.ToList().Join(templateItems, l => l.ItemID, r => r.ProductItemId, (l, r) => new
                {
                    model = l,
                    TemplatePriority = r.MappingIndex
                });

                query = itemsJoin
                    .OrderByDescending(q => q.TemplatePriority)
                    .ThenBy(q => q.model.ItemName)
                    .ThenBy(q => q.model.ItemID)
                    .Select(q => q.model);
            }
            else
            {
                int[] categoriesId = null;
                if (!cateId.IsNullOrWhiteSpace())
                {
                    categoriesId = cateId.Split(',').Select(q => int.Parse(q)).ToArray();
                    productItems = productItems.Where(a => categoriesId.Contains(a.CatID ?? 0));
                }
                if (!string.IsNullOrWhiteSpace(param.sSearch))
                {
                    productItems = productItems.Where(a => a.ItemName.Contains(param.sSearch));
                }
                query = productItems
                        .OrderBy(q => q.ItemName)
                        .ThenBy(q => q.ItemID)
                        .Skip(param.iDisplayStart).Take(param.iDisplayLength)
                        //.OrderByDescending(q => q.CreateDate)
                        //.OrderBy(q => q.IndexPriority)
                        .ToList();
            }

            List<dynamic> listDt = new List<dynamic>();
            var count = param.iDisplayStart;
            var InStockDate = "";
            //Nếu beforeLastReport tồn tại thì lấy InStockDate là startDate-1
            if (await beforeLastReport.CountAsync() == 0)
            {
                InStockDate = startDate.AddDays(-1).ToString("dd/MM");
            }

            foreach (var productItem in query)
            {
                var inStockQuantity = 0.0;

                //var checkingQuantity = "";
                //var checkingDate = "";

                //Nếu list inDayCheckingItems > 0 thì lấy checkingDate trong InventoryChecking mới nhất
                //if (inDayCheckingItems.Count() > 0)
                //{
                //    checkingDate = inDayCheckingItems.FirstOrDefault().InventoryChecking.CheckingDate.ToString("dd/MM/yyyy");
                //}

                //Với mỗi ItemID của inDayCheckingItems = item của ProductItem => cộng quantity vào chuỗi checkingQuantity
                //inDayCheckingItems.ForEach(q =>
                //{
                //    if (q.ItemID == productItem.ItemID)
                //    {
                //        checkingQuantity += q.Quantity + " ";
                //    }
                //});
                if (await reportByTimeRange.CountAsync() > 0)
                {
                    if (await beforeLastReport.CountAsync() > 0)
                    {
                        //Xét reportByTimeRange tồn tại rồi xét beforeLastReport tồn tại thì lấy Item đầu tiên trong list bằng vs id của productItem
                        //var beforeLstReItem = invenDaReItemApi.GetItemByReportId(beforeLastReport.ReportID);
                        //if (beforeLstReItem.Count() != 0)
                        //{
                        //    var checkExist = beforeLstReItem.FirstOrDefault(q => q.ItemID == productItem.ItemID);
                        //    if (checkExist != null)
                        //    {
                        //        inStockQuantity = beforeLstReItem.FirstOrDefault(q => q.ItemID == productItem.ItemID).RealAmount ?? 0;
                        //        //inStockQuantity = beforeLastReport.InventoryDateReportItem.FirstOrDefault(q => q.ItemID == productItem.ItemID).RealAmount ?? 0;
                        //    }
                        //    else
                        //    {
                        //        inStockQuantity = 0;
                        //    }
                        //}
                        //else
                        //{
                        //    inStockQuantity = 0;
                        //}
                        //FirstOrDefault(q => q.ItemID == productItem.ItemID)
                        //Nếu checkExist tồn tại thì lấy inStockQuantity là RealAmount, nếu ko thì là 0

                        var checkExist = beforeLastReport.SelectMany(q => q.InventoryDateReportItems)
                        .Where(q => q.ItemID == productItem.ItemID).ToList();
                        //if (checkExist != null)
                        //{
                        inStockQuantity = checkExist.Sum(q => q.RealAmount ?? 0);
                        //}
                        //else
                        //{
                        //    inStockQuantity = 0;
                        //}

                    }

                    var SoldQuantity = 0.0; var ReturnQuantity = 0.0; var DraftQuantity = 0.0; var OutChangeQuantity = 0.0;
                    var InQuantity = 0.0; var InChangeQuantity = 0.0;
                    var theoryQuantity = 0.0; var realQuantity = 0.0;
                    var itemId = -1;
                    var isChanged = false;
                    double COS = 0;

                    var reportItem = reportByTimeRange.SelectMany(q => q.InventoryDateReportItems)
                        .Where(q => q.ItemID == productItem.ItemID).ToList();
                    //var invenDaReItem = reTiRaItem.FirstOrDefault(x => x.ItemID == productItem.ItemID);

                    if (reportItem == null)
                    {
                        listDt.Add(new
                        {
                            No = ++count,
                            itemName = productItem.ItemName,
                            unit = productItem.Unit,
                            unitPrice = productItem.Price,
                            TotalRealAmount = 0,
                            categoryName = productItem.ItemCategory.CateName,
                            inStockQuantity = 0,
                            SoldQuantity = 0,
                            ReturnQuantity = 0,
                            DraftQuantity = 0,
                            OutChangeQuantity = 0,
                            TotalExport = 0,
                            InQuantity = 0,
                            InChangeQuantity = 0,
                            TotalImport = 0,
                            theoryQuantity = 0,
                            realQuantity = 0,
                            checkingQuantity = "",
                            itemId = productItem.ItemID,
                            isChanged = false,
                            allowCheck = allowCheck,
                            InStockDate = InStockDate,
                            COS = 0,
                        });

                        var InvenDateReportItemApi = new InventoryDateReportItemApi();
                        //var reportItems = new InventoryDateReportItemViewModel()
                        //{
                        //    ReportID = reportByTimeRange.ReportID,
                        //    Quantity = 0,
                        //    ItemID = productItem.ItemID,
                        //    CancelAmount = 0,
                        //    ExportAmount = 0,
                        //    ChangeInventoryAmount = 0,
                        //    ImportAmount = 0,
                        //    RealAmount = 0,
                        //    ReturnAmount = 0,
                        //    SoldAmount = 0,
                        //    TheoryAmount = 0,
                        //    TotalExport = 0,
                        //    TotalImport = 0,
                        //    ReceivedChangeInventoryAmount = 0,
                        //    Price = productItem.Price ?? 0
                        //};
                        //await InvenDateReportItemApi.CreateAsync(reportItems);
                    }
                    else
                    {
                        //var tmp = reportByTimeRange.InventoryDateReportItem.FirstOrDefault(x => x.ItemID == productItem.ItemID);

                        // ItemReport existed
                        //if (tmp != null)
                        //{
                        SoldQuantity = reportItem.Sum(q => q.SoldAmount ?? 0);
                        ReturnQuantity = reportItem.Sum(q => q.ReturnAmount ?? 0);
                        DraftQuantity = reportItem.Sum(q => q.CancelAmount ?? 0);
                        OutChangeQuantity = reportItem.Sum(q => q.ChangeInventoryAmount ?? 0);
                        InQuantity = reportItem.Sum(q => q.ImportAmount ?? 0);
                        InChangeQuantity = reportItem.Sum(q => q.ReceivedChangeInventoryAmount ?? 0);
                        theoryQuantity = reportItem.Sum(q => q.TheoryAmount ?? 0);
                        realQuantity = reportItem.Sum(q => q.RealAmount ?? 0);
                        itemId = productItem.ItemID;
                        //isChanged = (reportItem.IsSelected != null && (bool)reportItem.IsSelected) ? true : false;
                        isChanged = true;
                        COS = StoreFinalAmount > 0 ? (double)(productItem.Price * SoldQuantity / StoreFinalAmount) : 0;
                        //Bao cao da ton tai
                        listDt.Add(new
                        {
                            No = ++count,
                            itemName = productItem.ItemName,
                            unit = productItem.Unit,
                            //unitPrice = productItem.Price,
                            TotalRealAmount = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", realQuantity * productItem.Price),
                            //categoryName = productItem.ItemCategory.CateName,
                            inStockQuantity = inStockQuantity,
                            SoldQuantity = SoldQuantity,
                            ReturnQuantity = ReturnQuantity,
                            DraftQuantity = DraftQuantity,
                            OutChangeQuantity = OutChangeQuantity,
                            TotalExport = (SoldQuantity + ReturnQuantity + DraftQuantity + OutChangeQuantity),
                            InQuantity = InQuantity,
                            InChangeQuantity = InChangeQuantity,
                            TotalImport = (InQuantity + InChangeQuantity),
                            theoryQuantity = theoryQuantity,
                            realQuantity = realQuantity,
                            //checkingQuantity = checkingQuantity, old code dung cho kiem ke
                            checkingQuantity = realQuantity - theoryQuantity,
                            //checkingDate = checkingDate,
                            itemId = itemId,
                            isChanged = isChanged,
                            allowCheck = allowCheck,
                            //InStockDate = InStockDate,
                            COS = string.Format(CultureInfo.InvariantCulture,
                                    "{0:0,0}", (inStockQuantity - realQuantity + (InQuantity + InChangeQuantity)) * productItem.Price),
                        });
                    }
                }
                else
                {
                    listDt.Add(new
                    {
                        No = ++count,
                        itemName = productItem.ItemName,
                        unit = productItem.Unit,
                        unitPrice = productItem.Price,
                        TotalRealAmount = 0,
                        categoryName = productItem.ItemCategory.CateName,
                        inStockQuantity = 0,
                        SoldQuantity = 0,
                        ReturnQuantity = 0,
                        DraftQuantity = 0,
                        OutChangeQuantity = 0,
                        TotalExport = 0,
                        InQuantity = 0,
                        InChangeQuantity = 0,
                        TotalImport = 0,
                        theoryQuantity = 0,
                        realQuantity = 0,
                        checkingQuantity = "",
                        itemId = productItem.ItemID,
                        isChanged = false,
                        allowCheck = allowCheck,
                        InStockDate = InStockDate,
                        COS = 0,
                    });
                }
            }

            var totalRecords = query.Count();
            var totalDisplayRecords = productItems.Count();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalDisplayRecords,
                aaData = listDt,
            }, JsonRequestBehavior.AllowGet);
        }


        public async Task<JsonResult> EditRealAmount(int itemId, int quantity, int storeId)
        {
            var api = new InventoryDateReportApi();
            var itemApi = new InventoryDateReportItemApi();
            var lastReport = api.GetLastReport(storeId);
            if (lastReport != null)
            {
                var reportItem = itemApi.GetItemByReportId(lastReport.ReportID);

                if (reportItem != null)
                {
                    var reportItemLast = reportItem.FirstOrDefault(q => q.ItemID == itemId);
                    if (reportItemLast != null)
                    {
                        reportItemLast.RealAmount = quantity;
                        reportItemLast.IsSelected = true;

                        await itemApi.UpdateReportItemAsync(reportItemLast, reportItemLast.ItemID, reportItemLast.ReportID);

                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

        }

        //partial view cho thông tin chung của tồn kho cuối ngày
        public ActionResult InfoIndex(string sTime, string eTime, int storeId, int brandId)
        {
            ViewBag.storeId = storeId.ToString();
            //var storeId = RouteData.Values["storeId"].ToString();
            //int id = Convert.ToInt32(storeId);
            var model = new InfoIndex();

            long stockValue = 0;
            double inventoryTotalImportAmount = 0;
            double inventoryTotalExportAmount = 0;
            double stockTheoryValue = 0;
            DateTime startDate, endDate;
            if (!sTime.IsNullOrWhiteSpace() && !eTime.IsNullOrWhiteSpace())
            {
                //Chọn ngày hiện tại thì tăng lên 1 để add-2 đúng
                //startDate = dateTime.ToDateTime().GetStartOfDate();
                //endDate = dateTime.ToDateTime().GetEndOfDate();
                startDate = sTime.ToDateTime().GetStartOfDate();
                endDate = eTime.ToDateTime().GetEndOfDate();
            }
            else
            {
                startDate = Utils.GetCurrentDateTime().GetStartOfDate().AddDays(-1);
                endDate = Utils.GetCurrentDateTime().GetEndOfDate().AddDays(-1);
            }
            var api = new InventoryReceiptApi();
            var dateReportApi = new InventoryDateReportApi();
            var dateReportItemApi = new InventoryDateReportItemApi();

            //Do chưa set automation nên tạm thời xét thêm điều kiện receipt bị hủy hoặc đã duyệt
            //var receipt = api.GetInventoryReceiptByTimeRange(storeId, startDate, endDate)
            //        .Where(a => a.Status == (int)InventoryReceiptStatusEnum.Closed).ToList();
            // || a.Status == (int)InventoryReceiptStatusEnum.Approved || a.Status == (int)InventoryReceiptStatusEnum.Canceled
            var report = dateReportApi.GetQueryInventoryDateReportByTimeRange(startDate, endDate, storeId, brandId)
                                        .SelectMany(q => q.InventoryDateReportItems)
                                        .ToList();
            if (report.Count() > 0)
            {
                model.NumberImport = (int)report.Sum(a => a.TotalImport ?? 0);

                model.NumberExport = -(int)report.Sum(a => a.TotalExport ?? 0);

                model.Tranfer = (int)report.Sum(a => a.ChangeInventoryAmount ?? 0);

                model.GetTranfer = (int)report.Sum(a => a.ReceivedChangeInventoryAmount ?? 0);

                //model.NumberImport = receipt.Where(a => a.ReceiptType == (int)ReceiptType.InInventory).Count();

                //model.NumberExport = receipt.Where(a => (a.InStoreId == null || (a.OutStoreId != null && a.OutStoreId == storeId))
                //&& (a.ReceiptType == (int)ReceiptType.DraftInventory || a.ReceiptType == (int)ReceiptType.OutInventory)).Count();

                //model.Tranfer = receipt.Where(a => (a.InStoreId == null || (a.OutStoreId != null && a.OutStoreId == storeId))
                //&& (a.ReceiptType == (int)ReceiptType.OutChangeInventory)).Count();

                //model.GetTranfer = receipt.Where(a => (a.InStoreId == null || (a.OutStoreId != null && a.InStoreId == storeId))
                //&& (a.ReceiptType == (int)ReceiptType.OutChangeInventory)).Count();
                //if (report != null)
                //{
                //    var dateReportItem = dateReportItemApi.GetItemByReportId(report.ReportID);
                //    if (dateReportItem != null || dateReportItem.Count() != 0)
                //    {
                //        foreach (var item in dateReportItem.ToList())
                //        {
                //            //if(item.Price == null)
                //            //{
                //            //    item.Price = 0;
                //            //}
                //            stockValue += (int)(item.Price * item.RealAmount);
                //        }
                //        model.StockValue = stockValue;
                //    }
                //}
                foreach (var item in report)
                {
                    stockValue += (int)(item.Price * item.RealAmount);
                    inventoryTotalImportAmount += (item.Price * item.TotalImport) ?? 0;
                    inventoryTotalExportAmount += (item.Price * -item.TotalExport) ?? 0;
                    stockTheoryValue += (item.Price * item.TheoryAmount) ?? 0;
                }
                model.StockValue = stockValue;
                model.ImportValue = inventoryTotalImportAmount;
                model.COSValue = stockTheoryValue - stockValue + inventoryTotalExportAmount;
            }
            else
            {
                model.NumberImport = 0;
                model.NumberExport = 0;
                model.StockValue = 0;
                model.Tranfer = 0;
                model.GetTranfer = 0;
            }
            return PartialView("_InfoIndex", model);
        }

        //partial view cho modal chi tiết product
        public ActionResult CheckDetailItem(int brandId, int itemId, string startTime, int storeId, string endTime)
        {
            ViewBag.ItemId = itemId;
            //ViewBag.StoreId = storeId;
            //ViewBag.BrandId = brandId;
            //RouteData.Values["storeId"].ToString()
            ViewBag.StartTime = startTime;
            ViewBag.EndTime = endTime;
            return PartialView("_CheckDetailItem");
        }

        //Load bảng chi tiết từng product
        public ActionResult LoadDTCheckDetailItem(int brandId, int storeId, int itemId, string startTime, string endTime)
        {
            DateTime startDate = startTime.ToDateTime();
            DateTime endDate = endTime.ToDateTime();
            ViewBag.StartTime = startTime;
            ViewBag.EndTime = endTime;
            var api = new InventoryDateReportApi();
            var itemApi = new InventoryDateReportItemApi();
            var reportItem = itemApi.GetQueryItemByItemId(itemId);
            var dateReport = api.GetQueryInventoryDateReportByTimeRange(startDate.GetStartOfDate(), endDate.GetEndOfDate(), storeId, brandId);
            var reportDate = dateReport.GroupJoin(reportItem, report => report.ReportID, item => item.ReportID, (report, dateReportItem) => new
            {
                Date = report.CreateDate,
                Items = dateReportItem
            }).GroupBy(a => a.Date).ToList();
            var model = new List<ItemDetail>();

            foreach (var item in reportDate)
            {
                //var reportItem = itemApi.GetItemByReportId(item.ReportID);
                var itemDetail = item.SelectMany(q => q.Items);
                //item.InventoryDateReportItems.FirstOrDefault(a => a.ItemID == itemId);
                var detail = new ItemDetail()
                {
                    DateTime = item.Key.ToString("dd/MM/yyyy"),
                    Unit = itemDetail != null ? itemDetail.Select(q => q.ProductItem.Unit).FirstOrDefault() : "",
                    ProductName = itemDetail != null ? itemDetail.Select(q => q.ProductItem.ItemName).FirstOrDefault() : "",
                    ImportInventory = itemDetail != null ? (double)itemDetail.Sum(q => q.ImportAmount) : 0,
                    ReceiveInventory = itemDetail != null ? (double)itemDetail.Sum(q => q.ReceivedChangeInventoryAmount) : 0,
                    TotalImport = itemDetail != null ? (double)itemDetail.Sum(q => q.TotalImport) : 0,
                    Sold = itemDetail != null ? (double)itemDetail.Sum(q => q.SoldAmount) : 0,
                    GiveBack = itemDetail != null ? (double)itemDetail.Sum(q => q.ReturnAmount) : 0,
                    Destroy = itemDetail != null ? (double)itemDetail.Sum(q => q.CancelAmount) : 0,
                    ExportInventory = itemDetail != null ? (double)itemDetail.Sum(q => q.ChangeInventoryAmount) : 0,
                    TotalExport = itemDetail != null ? (double)itemDetail.Sum(q => q.TotalExport) : 0
                };
                model.Add(detail);
            }

            return PartialView("_LoadDTCheckDetailItem", model);
        }

        //Lấy danh sách để xuất file excel
        public async Task<List<dynamic>> getListDT(int storeId, int? templateId, string cateId, DateTime dateTime, int brandId)
        {
            var api = new InventoryDateReportApi();
            var reportItemApi = new InventoryDateReportItemApi();
            var productItemApi = new ProductItemApi();

            IQueryable<InventoryDateReportViewModel> reportByTimeRange;
            //var lastCheckDate = reportByTimeRange != null
            //    ? reportByTimeRange.CreateDate.ToString("dd/MM/yyyy")
            //    : Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");
            DateTime startDate, endDate;
            var today = Utils.GetCurrentDateTime().GetStartOfDate();
            if (dateTime.GetStartOfDate() == today) //Nếu chọn ngày hôm nay hoặc string ngày tháng rỗng thì lấy của ngày hôm qua
            {
                //Chọn ngày hiện tại thì tăng lên 1 để add-2 đúng
                startDate = dateTime.GetStartOfDate().AddDays(-1);
                endDate = dateTime.GetEndOfDate().AddDays(-1);
                //.FirstOrDefault(q => q.CreateDate >= startDate && q.CreateDate <= endDate && q.StoreId == storeId);
                //var lastReport = api.GetLastReport(storeId);
                //allowCheck = (lastReport != null && reportByTimeRange != null && lastReport.CreateDate == reportByTimeRange.CreateDate) ? true : false;
            }
            else
            {
                startDate = dateTime.GetStartOfDate();
                endDate = dateTime.GetEndOfDate();
                //reportByTimeRange = api.GetLastReport(storeId);
                //allowCheck = true;
            }

            reportByTimeRange = api.GetQueryInventoryDateReportByTimeRange(startDate, endDate, storeId, brandId);

            //var beforeLastReport = api.GetQueryInventoryDateReportByTimeRange(startDate.AddDays(-1), endDate.AddDays(-1), storeId, brandId);
            //var productItems =
            //    productItemApi.GetProductItems().Where(w => w.IsAvailable == true).OrderBy(pi => pi.ItemName);
            var productItems = productItemApi.GetAvailableProductItemsModelByBrand(brandId);
            //int[] categoriesId = null;
            //if (!cateId.IsNullOrWhiteSpace())
            //{
            //    categoriesId = cateId.Split(',').Select(q => int.Parse(q)).ToArray();
            //}
            int[] categoriesId = null;
            if (!cateId.IsNullOrWhiteSpace())
            {
                categoriesId = cateId.Split(',').Select(q => int.Parse(q)).ToArray();
                productItems = productItems.Where(a => categoriesId.Contains(a.CatID ?? 0));
            }

            if (templateId != null)
            {
                var itemIds = new TemplateReportProductItemMappingApi().GetAllTemplateItems(templateId.Value).Select(q => q.ProductItemId);
                productItems = productItems.Where(a => itemIds.Contains(a.ItemID));
            }
            //var query =
            //   productItems.Where(
            //       a =>
            //           (cateId == null || cateId.Count() == 0 || (categoriesId.Contains(a.CatID ?? 0))))
            //           .ToList();
            IEnumerable<ProductItemViewModel> query = new List<ProductItemViewModel>();
            if (templateId != null)
            {
                var templateApi = new TemplateReportProductItemMappingApi();
                var templateItems = templateApi.GetAllTemplateItems(templateId.Value);

                var itemIds = templateItems.Select(q => q.ProductItemId);
                productItems = productItems.Where(a => itemIds.Contains(a.ItemID));
                var itemsJoin = productItems.ToList().Join(templateItems, l => l.ItemID, r => r.ProductItemId, (l, r) => new
                {
                    model = l,
                    TemplatePriority = r.MappingIndex
                });

                query = itemsJoin
                    .OrderByDescending(q => q.TemplatePriority)
                    .ThenBy(q => q.model.ItemName)
                    .ThenBy(q => q.model.ItemID)
                    .Select(q => q.model);
            }
            else
            {
                query = productItems
                        .OrderBy(q => q.ItemName)
                        .ThenBy(q => q.ItemID)
                        //.OrderByDescending(q => q.CreateDate)
                        //.OrderBy(q => q.IndexPriority)
                        .ToList();
            }
            //.OrderBy(q => q.ItemName);
            //.OrderBy(q => q.IndexPriority);

            List<dynamic> listDt = new List<dynamic>();

            var count = 0;
            foreach (var productItem in query)
            {
                var realQuantity = 0.0;
                if (await reportByTimeRange.CountAsync() > 0)
                {
                    //var inStockQuantity = 0.0;
                    //var SoldQuantity = 0.0; var ReturnQuantity = 0.0; var DraftQuantity = 0.0; var OutChangeQuantity = 0.0;
                    //var InQuantity = 0.0; var InChangeQuantity = 0.0;
                    //var theoryQuantity = 0.0;
                    //if (await beforeLastReport.CountAsync() > 0)
                    //{
                    //    //Nếu checkExist tồn tại thì lấy inStockQuantity là RealAmount, nếu ko thì là 0

                    //    var checkExist = beforeLastReport.SelectMany(q => q.InventoryDateReportItems)
                    //    .Where(q => q.ItemID == productItem.ItemID);

                    //    inStockQuantity = checkExist.Sum(q => q.RealAmount) ?? 0;

                    //}

                    var reportItem = reportByTimeRange.SelectMany(q => q.InventoryDateReportItems)
                        .Where(q => q.ItemID == productItem.ItemID);

                    //SoldQuantity = reportItem.Sum(q => q.SoldAmount ?? 0);
                    //ReturnQuantity = reportItem.Sum(q => q.ReturnAmount ?? 0);
                    //DraftQuantity = reportItem.Sum(q => q.CancelAmount ?? 0);
                    //OutChangeQuantity = reportItem.Sum(q => q.ChangeInventoryAmount ?? 0);
                    //InQuantity = reportItem.Sum(q => q.ImportAmount ?? 0);
                    //InChangeQuantity = reportItem.Sum(q => q.ReceivedChangeInventoryAmount ?? 0);
                    //theoryQuantity = reportItem.Sum(q => q.TheoryAmount ?? 0);
                    realQuantity = reportItem.Sum(q => q.RealAmount ?? 0);

                    //Bao cao da ton tai
                    //listDt.Add(new
                    //{
                    //    No = ++count,
                    //    itemId = productItem.ItemID,
                    //    itemName = productItem.ItemName,
                    //    unit = productItem.Unit,
                    //    //inStockQuantity = inStockQuantity,
                    //    //SoldQuantity = SoldQuantity,
                    //    //TotalExport = (ReturnQuantity + DraftQuantity + OutChangeQuantity),
                    //    //TotalImport = (InQuantity + InChangeQuantity),
                    //    //theoryQuantity = theoryQuantity,
                    //    realQuantity = realQuantity,
                    //});
                }
                //else
                //{
                //    listDt.Add(new
                //    {
                //        No = ++count,
                //        itemId = productItem.ItemID,
                //        itemName = productItem.ItemName,
                //        unit = productItem.Unit,
                //        //inStockQuantity = 0,
                //        //TotalImport = 0,
                //        //TotalExport = 0,
                //        //SoldQuantity = 0,
                //        //theoryQuantity = 0,
                //        realQuantity = 0,
                //    });
                //}
                listDt.Add(new
                {
                    No = ++count,
                    itemId = productItem.ItemID,
                    itemName = productItem.ItemName,
                    unit = productItem.Unit,
                    unit2 = productItem.Unit2,
                    unitConvertRate = productItem.UnitRate,
                    //inStockQuantity = 0,
                    //TotalImport = 0,
                    //TotalExport = 0,
                    //SoldQuantity = 0,
                    //theoryQuantity = 0,
                    realQuantity = realQuantity,
                });
            }
            //else
            //{
            //    listDt.Add(new
            //    {
            //        No = ++count,
            //        itemName = productItem.ItemName,
            //        unit = productItem.Unit,
            //        inStockQuantity = 0,
            //        TotalImport = 0,
            //        TotalExport = 0,
            //        SoldQuantity = 0,
            //        theoryQuantity = 0,
            //        realQuantity = 0,
            //    });
            //}


            //if (reportByTimeRange != null)
            //{
            //    if()
            //    dateTime = reportByTimeRange.Select(q => q.Store.ShortName).FirstOrDefault() + " " + startDate.ToShortDateString();
            //}
            //else
            //{
            //    dateTime = startDate.ToShortDateString();
            //}

            return listDt;
        }
        //Hàm xuất file excel
        public async Task<ActionResult> ExportExcelEPPlus(int storeId, int? selectedStore, int? templateId, string cateId, string dateTime, int brandId)
        {
            var curStoreId = selectedStore != null ? selectedStore.Value : storeId;
            var store = new StoreApi().Get(curStoreId);
            string storeName = curStoreId != 0 ? "cửa hàng " + store.ShortName : "Tất cả cửa hàng";
            var date = string.IsNullOrWhiteSpace(dateTime) ? Utils.GetCurrentDateTime() : dateTime.ToDateTime();
            string excelPrefixFormat = "TK_" + curStoreId.ToString("D4");
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var listDT = await getListDT(curStoreId, templateId, cateId, date, brandId);
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";//A
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "MÃ NGUYÊN LIỆU";//B
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "TÊN NL";//C
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "ĐƠN VỊ TÍNH GỐC";//D
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "TỔNG CỘNG";//E
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "T. ĐẦU";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "NHẬP";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "XUẤT";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "BÁN";
                //ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "TỒN LT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "ĐV TÍNH 1";//F
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "GIÁ TRỊ QUY ĐỔI";//G
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "SỐ LƯỢNG";//H
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "ĐV TÍNH 2";//I
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "GIÁ TRỊ QUY ĐỔI";//J
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "SỐ LƯỢNG";//K
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
                foreach (var data in listDT)
                {
                    // STT	TÊN NGUYÊN LiỆU	ĐV	T. ĐẦU	NHẬP	XuẤT	BÁN	TỒN LT	TỒN TT	(+/-)
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.No;//A
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.itemId;//B
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.itemName;//C
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.unit;//D
                    char totalHearderChar = StartHeaderChar++;//E
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.unit;//F
                    char convertHeaderChar1 = StartHeaderChar;//G
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = 1;//G
                    char qtyHeaderChar = StartHeaderChar;//H
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.realQuantity;//H
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.unit2;//I
                    char convertHeaderChar2 = StartHeaderChar;//J
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.unitConvertRate;//J
                    char qty2HeaderChar = StartHeaderChar;//K
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = 0;//K                   
                    ws.Cells["" + (totalHearderChar) + (StartHeaderNumber)].Formula = //E //H //G //K //J
                        "(" + (qtyHeaderChar) + (StartHeaderNumber) + "*" + (convertHeaderChar1) + (StartHeaderNumber) + "+" + (qty2HeaderChar) + (StartHeaderNumber) + "*" + (convertHeaderChar2) + (StartHeaderNumber) + ")";
                    StartHeaderChar = 'A';
                }
                ws.Column(5).Style.Locked = false;
                ws.Column(7).Style.Locked = false;
                ws.Protection.IsProtected = true;
                ws.Protection.AllowEditObject = false;
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var fileDownloadName = excelPrefixFormat + " - Tồn kho cuối ngày (" + date.ToString("dd-MM-yyyy") + ") " + storeName + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }
        #endregion
        [HttpPost]
        public async Task<ActionResult> UpdateDataViaExcel(int storeId, int brandId, int? selectedStore)
        {
            if (Request.Files.Count != 0)
            {
                var productItemApi = new ProductItemApi();
                var api = new InventoryDateReportApi();
                var itemApi = new InventoryDateReportItemApi();
                var curStoreId = selectedStore != null ? selectedStore.Value : storeId;
                var file = Request.Files[0];
                var filePrefix = file.FileName.Split('-')[0].Trim();
                var dataTokens = filePrefix.Split('_');
                var dataStoreId = 0;
                var itemList = productItemApi.GetAvailableProductItemsByBrand(brandId);
                int.TryParse(dataTokens[1], out dataStoreId);
                if (dataStoreId == curStoreId && dataStoreId != 0)
                {
                    var lastReport = api.GetLastReport(curStoreId);
                    var reportItem = itemApi.GetItemByReportId(lastReport.ReportID).ToList();
                    var ms = file.InputStream;
                    using (ExcelPackage package = new ExcelPackage(ms))
                    {
                        var ws = package.Workbook.Worksheets.First();
                        var totalCol = ws.Dimension.Columns;
                        var totalRow = ws.Dimension.Rows;
                        int realAmountCol = 0;
                        int itemIdCol = 0;
                        for (int i = 1; i <= totalCol; i++)
                        {
                            if (ws.Cells[1, i].Text.ToUpper().Trim() == "MÃ NGUYÊN LIỆU")
                            {
                                itemIdCol = i;
                            }
                            if (ws.Cells[1, i].Text.ToUpper().Trim() == "TỔNG CỘNG")
                            {
                                realAmountCol = i;
                            }
                        }
                        for (int i = 2; i <= totalRow; i++)
                        {

                            double itemRealAmount = 0;
                            double.TryParse(ws.Cells[i, realAmountCol].Text.Trim(), out itemRealAmount);
                            var itemId = 0;
                            var validItemId = int.TryParse(ws.Cells[i, itemIdCol].Text.Trim(), out itemId);
                            if (validItemId)
                            {
                                var reportItemLast = reportItem.Where(q => q.ItemID == itemId).FirstOrDefault();
                                if (reportItemLast != null)
                                {
                                    if (reportItemLast.RealAmount != itemRealAmount)
                                    {
                                        reportItemLast.RealAmount = itemRealAmount;
                                        reportItemLast.IsSelected = true;
                                        await itemApi.UpdateReportItemAsync(reportItemLast, reportItemLast.ItemID, reportItemLast.ReportID);
                                    }
                                }
                            }
                        }
                    }
                    return Json(new { success = true, message = "Cập nhật thành công" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "File được cập nhật không khớp với cửa hàng hiện tại" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { success = false, message = "Không có file nào được lựa chọn" }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Hàng tồn kho hiện tại
        public ActionResult IndexInDate(int storeId, int brandId)
        {
            ViewBag.storeId = storeId.ToString();
            var api = new ProductItemCategoryApi();
            IQueryable<StoreViewModel> stores = new List<StoreViewModel>().AsQueryable();
            // Khi storeId = 0 thì mới load tất cả Store lên web
            if (storeId == 0)
            {
                var storeApi = new StoreApi();
                stores = storeApi.GetAllStore(brandId);
            }

            ViewBag.Stores = stores;
            ViewBag.ProductCategories = api.GetItemCategories().Where(q => q.BrandId == brandId);
            ViewBag.InstockDate = Utils.GetCurrentDateTime();
            return View();
        }
        public JsonResult GetInventoryInDate(JQueryDataTableParamModel param, int brandId, int storeId, string cateId, string dateTime, int? selectedStoreId)
        {
            var result = selectedStoreId;

            var api = new InventoryDateReportApi();
            var itemApi = new InventoryDateReportItemApi();
            IQueryable<InventoryDateReportViewModel> reportByTimeRange;
            var curStoreId = selectedStoreId != null ? selectedStoreId.Value : storeId;

            var allowCheck = false;
            var lastCheckDate = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");
            DateTime startDate, endDate;

            if (!dateTime.IsNullOrWhiteSpace())
            {
                startDate = dateTime.ToDateTime().GetStartOfDate();
                endDate = dateTime.ToDateTime().GetEndOfDate();
                //reportByTimeRange = api.GetQueryInventoryDateReportByTimeRange(startDate, endDate, curStoreId, brandId);
                //var lastReport = api.GetLastReport((int)curStoreId);
                allowCheck = false;
            }
            else
            {
                startDate = Utils.GetCurrentDateTime().GetStartOfDate();
                endDate = Utils.GetCurrentDateTime();
                //reportByTimeRange = api.GetLastReport(curStoreId);
                allowCheck = false;
            }

            reportByTimeRange = api.GetLastReportAllStore(brandId);
            if (curStoreId != 0)
            {
                reportByTimeRange = reportByTimeRange.Where(q => q.StoreId == curStoreId);
            }
            #region ProductItems
            var orderDetailApi = new OrderDetailApi();
            var productApi = new ProductApi();
            IQueryable<OrderDetail> orderDetails;

            if (curStoreId == 0)

            {
                orderDetails =
                orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, startDate, Utils.GetCurrentDateTime()).Where(q => q.Status == (int)OrderStatusEnum.PosFinished);

            }
            else
            {
                orderDetails =
                orderDetailApi.GetOrderDetailsByTimeRange(startDate, Utils.GetCurrentDateTime(), (int)curStoreId).Where(q => q.Status == (int)OrderStatusEnum.PosFinished);
            }


            var dateProducts =
                orderDetails.GroupBy(a => a.ProductID)
                    .Join(productApi.GetProductByBrandId(brandId), a => a.Key, b => b.ProductID, (a, b) => new
                    {
                        ProductId = a.Key,
                        StoreID = storeId,
                        Quantity = a.Sum(c => c.Quantity),
                        //Date = reportDate,
                        TotalAmount = a.Sum(c => c.TotalAmount),
                        FinalAmount = a.Sum(c => c.FinalAmount),
                        Discount = a.Sum(c => c.Discount),
                        ProductName_ = b.ProductName,
                        Product = b,
                        CategoryId_ = b.ProductCategory.CateID
                    }).ToList();
            var compositionsStatistic = dateProducts.SelectMany(a => a.Product.ProductItemCompositionMappings.Select(b => new Tuple<ProductItemCompositionMapping, int>(b, a.Quantity)))
                .GroupBy(a => a.Item1.ItemID);
            var productItemApi = new ProductItemApi();
            var dateItemProduct = compositionsStatistic.Join(productItemApi.GetProductItems().ToList(), a => a.Key, b => b.ItemID, (a, b) => new DateProductItem
            {
                StoreId = storeId,
                ProductItemID = a.Key,
                ProductItemName = b.ItemName,
                Quantity = (int)a.Sum(c => c.Item2 * c.Item1.Quantity),
                Unit = b.Unit
            });

            var invenCheckItemApi = new InventoryCheckingItemApi();
            var inDayCheckingItems = invenCheckItemApi.GetStoreInventoryCheckingItemByTimeRange(startDate.AddDays(-1), endDate.AddDays(-1), curStoreId, brandId);
            //int[] categoriesId = null;
            //if (!cateId.IsNullOrWhiteSpace())
            //{
            //    categoriesId = cateId.Split(',').Select(q => int.Parse(q)).ToArray();
            //}
            var productItems = productItemApi.GetAvailableProductItemsModelByBrand(brandId);/*.OrderBy(pi => pi.ItemName)*/;
            int[] categoriesId = null;
            if (!cateId.IsNullOrWhiteSpace())
            {
                categoriesId = cateId.Split(',').Select(q => int.Parse(q)).ToArray();
                productItems = productItems.Where(a => categoriesId.Contains(a.CatID ?? 0));
            }
            if (!string.IsNullOrWhiteSpace(param.sSearch))
            {
                productItems = productItems.Where(a => a.ItemName.Contains(param.sSearch));
            }
            var query =
               productItems.OrderBy(a => a.IndexPriority)
                            .ThenBy(a => a.ItemID)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength)
                            .ToList();
            #endregion


            List<dynamic> listDt = new List<dynamic>();
            var count = param.iDisplayStart;

            var inReItemApi = new InventoryReceiptItemApi();

            IQueryable<InventoryReceiptItem> receiptItems;
            if (curStoreId == 0)
            {
                receiptItems = inReItemApi.GetReceiptItems()
                   .Where(q => q.InventoryReceipt.CreateDate >= startDate && q.InventoryReceipt.CreateDate <= endDate
                   && (q.InventoryReceipt.Status == (int)InventoryReceiptStatusEnum.Approved || q.InventoryReceipt.Status == (int)InventoryReceiptStatusEnum.Closed));
                //q => (q.InventoryReceipt.StoreId == (int)curStoreId
                //        || (q.InventoryReceipt.InStoreId != null && q.InventoryReceipt.InStoreId == (int)curStoreId))
                //        &&
            }
            else
            {

                receiptItems = inReItemApi.GetReceiptItems()
                    .Where(q => (q.InventoryReceipt.StoreId == (int)curStoreId
                    || (q.InventoryReceipt.InStoreId != null && q.InventoryReceipt.InStoreId == (int)curStoreId))
                    && q.InventoryReceipt.CreateDate >= startDate && q.InventoryReceipt.CreateDate <= endDate
                    && (q.InventoryReceipt.Status == (int)InventoryReceiptStatusEnum.Approved || q.InventoryReceipt.Status == (int)InventoryReceiptStatusEnum.Closed));
            }


            //var invenCheckingApi = new InventoryCheckingApi();
            //IEnumerable<InventoryChecking> InDayChecking;
            //if (curStoreId == 0)
            //{
            //    InDayChecking = invenCheckingApi.GetInventoryChecking().Where(q => q.CheckingDate >= startDate && q.CheckingDate <= endDate);
            //}
            //else
            //{
            //    InDayChecking = invenCheckingApi.GetInventoryChecking().Where(q => q.StoreId == (int)curStoreId && q.CheckingDate >= startDate && q.CheckingDate <= endDate);
            //}
            foreach (var productItem in query)
            {

                var soldItem = dateItemProduct.FirstOrDefault(q => q.ProductItemID == productItem.ItemID);
                var soldItemQuantity = soldItem != null ? soldItem.Quantity : 0;
                var items = receiptItems.Where(q => q.ItemID == productItem.ItemID).ToList();

                var ImportExportQuantity = new ImportExportModel
                {
                    InInventory = 0,
                    InChangeInventory = 0,
                    OutChangeInventory = 0,
                    OutInventory = 0,
                    SoldProduct = soldItemQuantity,
                    DraftInventory = 0,
                    TotalExport = 0,
                    TotalImport = 0,
                };
                var ioQuantity = 0.0;
                foreach (var item in items)
                {
                    switch (item.InventoryReceipt.ReceiptType)
                    {
                        case (int)ReceiptType.InInventory:
                            {
                                //ioQuantity += item.Quantity;
                                ImportExportQuantity.InInventory += item.Quantity;
                                break;
                            }
                        case (int)ReceiptType.DraftInventory:
                            {
                                //ioQuantity -= item.Quantity;
                                ImportExportQuantity.DraftInventory += item.Quantity;
                                break;
                            }
                        case (int)ReceiptType.OutInventory:
                            {
                                //ioQuantity -= item.Quantity;
                                ImportExportQuantity.OutInventory += item.Quantity;
                                break;
                            }
                        default:
                            {
                                if (item.InventoryReceipt.InStoreId != null && item.InventoryReceipt.InStoreId == (int)curStoreId)
                                {
                                    //ioQuantity += item.Quantity;
                                    ImportExportQuantity.InChangeInventory += item.Quantity;
                                }
                                else
                                {
                                    ImportExportQuantity.OutChangeInventory += item.Quantity;
                                    //ioQuantity -= item.Quantity;
                                }
                                break;
                            }
                    }
                }
                ImportExportQuantity.TotalImport = ImportExportQuantity.InChangeInventory + ImportExportQuantity.InInventory;
                ImportExportQuantity.TotalExport = ImportExportQuantity.OutChangeInventory + ImportExportQuantity.OutInventory
                    + ImportExportQuantity.SoldProduct + ImportExportQuantity.DraftInventory;
                ioQuantity = ImportExportQuantity.TotalImport - ImportExportQuantity.TotalExport;

                //var checkingQuantity = "";
                //var CheckingDate = "";
                //if (inDayCheckingItems.Count() > 0)
                //{
                //    CheckingDate = inDayCheckingItems.FirstOrDefault().InventoryChecking.CheckingDate.ToString("dd/MM/yyyy");
                //}
                //inDayCheckingItems.ForEach(q =>
                //{
                //    if (q.ItemID == productItem.ItemID)
                //    {
                //        checkingQuantity += q.Quantity + " ";
                //    }
                //});
                //var InDayCheckingItem = InDayChecking.FirstOrDefault(q => (q.InventoryCheckingItems
                //.FirstOrDefault(p => p.ItemID == productItem.ItemID)) != null);

                var inDaReItemApi = new InventoryDateReportItemApi();
                if (reportByTimeRange != null)
                {
                    var tmp = reportByTimeRange.SelectMany(q => q.InventoryDateReportItems).Where(q => q.ItemID == productItem.ItemID).ToList();
                    double theoryQuantity = 0;
                    double realQuantity = 0;
                    var itemId = 0;
                    var isChanged = false;
                    if (tmp.Count() > 0)
                    {
                        theoryQuantity = tmp.Sum(q => q.TheoryAmount ?? 0);
                        realQuantity = tmp.Sum(q => q.RealAmount ?? 0);
                        itemId = productItem.ItemID;
                        isChanged = tmp.FirstOrDefault().IsSelected != null ? tmp.FirstOrDefault().IsSelected.Value : false;
                    }
                    //var InDayCheckingItemQuantity = (InDayCheckingItem != null) ?
                    //    InDayCheckingItem.InventoryCheckingItems.FirstOrDefault().Quantity : (theoryQuantity + ioQuantity);
                    listDt.Add(new
                    {
                        No = ++count,
                        ImageURL = productItem.ImageUrl,
                        ItemName = productItem.ItemName,
                        ItemUnit = productItem.Unit,
                        CategoryName = productItem.CateName,
                        TheoryQuantity = (theoryQuantity + ioQuantity),
                        RealQuantity = realQuantity,
                        //CheckingQuantity = checkingQuantity,
                        ItemId = itemId,
                        isChanged = isChanged,
                        allowCheck = allowCheck,
                        //CheckingDate = CheckingDate,
                        IOQuantity = ImportExportQuantity,
                        //InDayCheckingItemQuantity = InDayCheckingItemQuantity,
                        Difference = (realQuantity - theoryQuantity) < 0 ? (realQuantity - theoryQuantity)*(-1) : (realQuantity - theoryQuantity),
                    });
                }
                else
                {
                    //var InDayCheckingItemQuantity = (InDayCheckingItem != null) ?
                    //    InDayCheckingItem.InventoryCheckingItems.FirstOrDefault().Quantity : 0;
                    listDt.Add(new
                    {
                        No = ++count,
                        ImageURL = productItem.ImageUrl,
                        ItemName = productItem.ItemName,
                        ItemUnit = productItem.Unit,
                        CategoryName = productItem.CateName,
                        TheoryQuantity = ioQuantity,
                        RealQuantity = 0,
                        //CheckingQuantity = checkingQuantity,
                        ItemId = productItem.ItemID,
                        isChanged = false,
                        allowCheck = allowCheck,
                        //CheckingDate = CheckingDate,
                        IOQuantity = ImportExportQuantity,
                        //InDayCheckingItemQuantity = InDayCheckingItemQuantity,
                        Difference = /*InDayCheckingItemQuantity - */ioQuantity,
                    });
                }
            }

            return Json(new
            {
                param.sEcho,
                // iTotalRecords = orderDetails.Count(),
                iTotalRecords = productItems.Count(),
                iTotalDisplayRecords = productItems.Count(),
                aaData = listDt,
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<JsonResult> EditInventoryChecking(int itemId, int storeId, int quantity)
        {
            var productApi = new ProductItemApi();
            var ProductItem = await productApi.GetProductItemById(itemId);
            if (ProductItem != null)
            {
                var listChecking = new InventoryChecking()
                {
                    CheckingDate = Utils.GetCurrentDateTime(),
                    Status = 0,
                    InventoryCheckingItems = {
                    new InventoryCheckingItem
                    {
                        ItemID = itemId,
                        Quantity = quantity,
                        Price = ProductItem.Price,
                        Unit = ProductItem.Unit,
                    }
                },
                    StoreId = storeId,
                    Creator = HttpContext.User.Identity.Name,
                };
                var invenCheckApi = new InventoryCheckingApi();
                await invenCheckApi.CreateInventoryChecking(listChecking);
                return Json(new { success = true, msg = "Lưu thành công!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, msg = "Lưu thất bại" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult LoadAllStoreById(int brandId)
        {
            var storeApi = new StoreApi();
            //var customerTypeAPI = new CustomerTypeApi();
            //get customer types

            //set brain id = 1 to view page
            var storeList = storeApi.GetActiveStoreByBrandId(brandId).Select(a => new
            {
                storeId = a.ID,
                Name = a.Name
            }).ToList();
            //var typeList = customerTypeAPI.GetAllCustomerTypes(brandID).Select(a => new
            //{
            //    CustomerTypeId = a.ID,
            //    Name = a.CustomerType1
            //}).ToList();
            return Json(storeList);
        }
        #endregion

        #region Danh sách phiếu kho
        public ActionResult InventoryReceiptList(int brandId, int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            //ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            var api = new StoreApi();
            var store = api.GetStoresByBrandId(brandId);
            ViewBag.storeId = storeId.ToString();
            ViewBag.StoreChoose = store;

            return View("InventoryReceiptList");
        }

        public JsonResult GetReceiptsByType(JQueryDataTableParamModel param, int type, string startTime, string endTime, string fillteredStores, int storeId)
        {
            var api = new InventoryReceiptApi();
            //var storeId = int.Parse(RouteData.Values["StoreId"].ToString());

            //ParseExact(startTime, "dd/MM/yyyy", null)
            var startDate = (startTime.IsNullOrWhiteSpace()) ? Utils.GetCurrentDateTime().GetStartOfDate() : startTime.ToDateTime().GetStartOfDate();
            var endDate = (endTime.IsNullOrWhiteSpace()) ? Utils.GetCurrentDateTime() : endTime.ToDateTime().GetEndOfDate();
            IQueryable<InventoryReceipt> receipts;
            if (storeId == 0)
            {
                var user = System.Web.HttpContext.Current.User;
                if (!user.IsInRole("BrandManager") && !user.IsInRole("Inventory"))
                {
                    RedirectToAction("Login", "Account");
                }
                receipts = api.GetInventoryReceiptByTime(startDate, endDate).AsQueryable();
            }
            else
            {
                receipts = api.GetInventoryReceiptByTimeRange(storeId, startDate, endDate).AsQueryable();
            }
            if (!fillteredStores.IsNullOrWhiteSpace())
            {
                var stores = fillteredStores.Split(',').Select(q => int.Parse(q)).ToArray();
                receipts = receipts.Where(q => stores.Contains(q.StoreId ?? 0));
            }
            switch (type)
            {
                case 0:
                    {// Import
                        receipts = receipts.Where(q => q.ReceiptType == (int)ReceiptType.InInventory);
                        break;
                    }
                case 2:
                    {// OutChangeInventory
                        receipts = receipts.Where(q => q.ReceiptType == (int)ReceiptType.OutChangeInventory);
                        break;
                    }
                case 3:
                    {//Export
                        receipts = receipts.Where(q => q.ReceiptType == (int)ReceiptType.OutInventory ||
                        q.ReceiptType == (int)ReceiptType.DraftInventory);
                        break;
                    }
            }

            var totalRecords = receipts.Count();
            var storeApi = new StoreApi();
            var providerApi = new ProviderApi();

            var count = param.iDisplayStart + 1;

            var model = receipts.OrderByDescending(q => q.Creator)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList()
                    .Select(p => new
                    {
                        STT = count++,
                        ReceiptName = p.Name,
                        Creator = p.Creator,
                        Date = p.CreateDate.Value.ToString("G", System.Globalization.CultureInfo.CreateSpecificCulture("nl-BE")),
                        Status = p.Status,
                        Note = p.Notes,
                        Provider = p.ProviderId != 0 && p.ProviderId != null ? providerApi.GetProviderNameByID((int)p.ProviderId) : "",
                        Category = p.ReceiptType,
                        ReceiptID = p.ReceiptID,
                        IsInStore = (p.InStoreId == storeId),
                        SrcStore = storeApi.GetStoreNameByID((int)p.StoreId),
                        DesStore = p.InStoreId != null ? storeApi.GetStoreNameByID((int)p.InStoreId) : ""
                    });
            //var totalDisplay = model.Count();

            return Json(new
            {
                success = true,
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = receipts.Count(),
                data = model
            }, JsonRequestBehavior.AllowGet);
        }

        //View detail of receipt in Tồn Kho cuối ngày
        public async Task<ActionResult> InventoryReceiptItem(int id, int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            var api = new InventoryReceiptItemApi();
            var receiptApi = new InventoryReceiptApi();
            var storeApi = new StoreApi();
            var inventoryReceipt = await receiptApi.GetInventoryReceiptById(id);
            var invetoryReceiptItem = api.GetItemReceiptById(id);
            var model = new InventoryReceiptEditViewModel(inventoryReceipt, this.Mapper);
            model.InventoryReceiptItem = invetoryReceiptItem;
            if (inventoryReceipt.InStoreId != null)
            {
                model.InStoreName = storeApi.GetStoreNameByID((int)inventoryReceipt.InStoreId);
            }
            if (inventoryReceipt.OutStoreId != null)
            {
                model.OutStoreName = storeApi.GetStoreNameByID((int)inventoryReceipt.OutStoreId);
            }
            return View(model);
        }
        #endregion

        #region Chênh lệch theo khoảng thời gian
        public ActionResult IndexTimeRange(int storeId, int brandId)
        {
            ViewBag.storeId = storeId.ToString();

            IQueryable<StoreViewModel> stores = new List<StoreViewModel>().AsQueryable();
            // Khi storeId = 0 thì mới load tất cả Store lên web
            if (storeId == 0)
            {
                var storeApi = new StoreApi();
                stores = storeApi.GetAllStore(brandId);
            }
            ViewBag.Stores = stores;

            var api = new ProductItemCategoryApi();
            ViewBag.ProductCategories = api.GetItemCategories().Where(q => q.BrandId == brandId);
            ViewBag.CurDate = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");
            return View();
        }
        public JsonResult LoadIndexTimeRange(JQueryDataTableParamModel param, string cateId, string startTime, string endTime, int storeId, int brandId, int? selectStore)
        {
            var curStoreId = selectStore != null ? (int)selectStore : storeId;
            var inDaReApi = new InventoryDateReportApi();
            var invenDaReItemApi = new InventoryDateReportItemApi();
            var productApi = new ProductItemApi();
            var itemApi = new InventoryReceiptItemApi();
            var startDate = (startTime.IsNullOrWhiteSpace()) ? Utils.GetCurrentDateTime().GetStartOfDate() : startTime.ToDateTime().GetStartOfDate();
            var endDate = (endTime.IsNullOrWhiteSpace()) ? Utils.GetCurrentDateTime() : endTime.ToDateTime().GetEndOfDate();
            int count = param.iDisplayStart;

            //var productItems = productApi.GetAvailableProductItems().OrderBy(q => q.ItemName).ToList();
            var productItems = productApi.GetAvailableProductItemsModelByBrand(brandId)/*.OrderBy(q => q.ItemName).ToList()*/;

            int[] categoriesId = null;
            if (!cateId.IsNullOrWhiteSpace())
            {
                categoriesId = cateId.Split(',').Select(q => int.Parse(q)).ToArray();
            }
            Stopwatch sw = new Stopwatch();

            sw.Start();
            //int[] categoriesId = null;
            //if (!cateId.IsNullOrWhiteSpace())
            //{
            //    categoriesId = cateId.Split(',').Select(q => int.Parse(q)).ToArray();
            //    productItems = productItems.Where(a => categoriesId.Contains(a.CatID ?? 0));
            //}
            if (!string.IsNullOrWhiteSpace(param.sSearch))
            {
                productItems = productItems.Where(a => a.ItemName.Contains(param.sSearch));
            }
            var query = productItems
                        .OrderBy(q => q.ItemName)
                        .ThenBy(q => q.ItemID)
                        .Skip(param.iDisplayStart).Take(param.iDisplayLength)
                        //.OrderByDescending(q => q.CreateDate)
                        //.OrderBy(q => q.IndexPriority)
                        .ToList();

            sw.Stop();

            Console.WriteLine("Elapsed={0}", sw.Elapsed);


            List<dynamic> listDt = new List<dynamic>();
            var reportByTimeRange = inDaReApi.GetQueryInventoryDateReportByTimeRange(startDate, endDate, curStoreId, brandId).ToList();
            //if (curStoreId!=0)
            //{
            //    reportByTimeRange = inDaReApi.GetQueryInventoryDateReportByTimeRange(startDate, endDate, curStoreId, brandId);
            //}
            //else
            //{
            //    reportByTimeRange = inDaReApi.GetListInventoryDateReportByTimeRange(startDate, endDate);
            //}

            sw.Start();

            // ...
            foreach (var productItem in query)
            {
                if (reportByTimeRange != null)
                {
                    var theoryQuantity = 0.0;
                    var realQuantity = 0.0;
                    double totalRealAmount = 0;
                    var itemId = -1;

                    //Xét reportByTimeRange tồn tại rồi xét beforeLastReport tồn tại thì lấy Item đầu tiên trong list bằng vs id của productItem
                    foreach (var report in reportByTimeRange)
                    {

                        //double COS = 0;
                        var reTiRaItem = report.InventoryDateReportItems.Where(q => q.ItemID == productItem.ItemID);
                        var invenDaReItem = reTiRaItem.FirstOrDefault();
                        //var reTiRaItem = invenDaReItemApi.GetItemByReportId(report.ReportID).Where(q => q.ItemID == productItem.ItemID);
                        //var invenDaReItem = reTiRaItem.FirstOrDefault(x => x.ItemID == productItem.ItemID);

                        theoryQuantity = reTiRaItem.Sum(q => q.TheoryAmount) + theoryQuantity ?? 0;
                        realQuantity = reTiRaItem.Sum(q => q.RealAmount) + realQuantity ?? 0;
                        totalRealAmount = reTiRaItem.Sum(q => q.RealAmount * q.Price) + totalRealAmount ?? 0;
                        //itemId = invenDaReItem.ItemID;
                        //COS = (double)(productItem.Price * SoldQuantity / StoreFinalAmount);
                        //Bao cao da ton tai

                    }
                    listDt.Add(new
                    {
                        No = ++count,
                        imageURL = productItem.ImageUrl,
                        itemName = productItem.ItemName,
                        unit = productItem.Unit,
                        unitPrice = productItem.Price,
                        TotalRealAmount = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", totalRealAmount),
                        categoryName = productItem.CateName,
                        theoryQuantity = theoryQuantity,
                        realQuantity = realQuantity,
                        //checkingQuantity = checkingQuantity, old code dung cho kiem ke
                        checkingQuantity = realQuantity - theoryQuantity,
                        itemId = itemId,
                    });
                }
                else
                {
                    listDt.Add(new
                    {
                        No = ++count,
                        itemName = productItem.ItemName,
                        unit = productItem.Unit,
                        unitPrice = productItem.Price,
                        TotalRealAmount = 0,
                        categoryName = productItem.CateName,
                        theoryQuantity = 0,
                        realQuantity = 0,
                        checkingQuantity = 0,
                        itemId = productItem.ItemID,
                    });
                }
            }

            sw.Stop();

            Console.WriteLine("Elapsed={0}", sw.Elapsed);



            return Json(new
            {
                param.sEcho,
                iTotalRecords = productItems.Count(),
                iTotalDisplayRecords = productItems.Count(),
                aaData = listDt,
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Tồn kho cuối tháng
        public ActionResult IndexMonth(int storeId, int brandId)
        {
            ViewBag.storeId = storeId.ToString();
            var api = new ProductItemCategoryApi();
            var storeApi = new StoreApi();

            IQueryable<StoreViewModel> stores = new List<StoreViewModel>().AsQueryable();
            if (storeId == 0)
            {
                stores = storeApi.GetAllStore(brandId);
            }

            ViewBag.Stores = stores;
            ViewBag.ProductCategories = api.GetItemCategories().Where(q => q.BrandId == brandId);
            ViewBag.FirstDate = new DateTime(Utils.GetCurrentDateTime().Year, Utils.GetCurrentDateTime().Month, 1).ToString("dd/MM/yyyy");
            ViewBag.LastDate = Utils.GetCurrentDateTime().AddDays(-1).ToString("dd/MM/yyyy");
            return View();
        }

        public JsonResult LoadIndexMonth(JQueryDataTableParamModel param, string cateId, string startTime, string endTime, int storeId, int brandId, int? selectedStoreId)
        {
            var inDaReApi = new InventoryDateReportApi();
            var invenDaReItemApi = new InventoryDateReportItemApi();
            var productApi = new ProductItemApi();
            var itemApi = new InventoryReceiptItemApi();
            var today = Utils.GetCurrentDateTime();
            var startDate = (startTime.IsNullOrWhiteSpace()) ? today.GetStartOfMonth() : startTime.ToDateTime().GetEndOfDate();
            var endDate = (endTime.IsNullOrWhiteSpace()) ? today.GetEndOfDate() : endTime.ToDateTime().GetEndOfDate();
            int count = param.iDisplayStart;

            if (endDate.Month == today.Month)
            {
                endDate = endDate.AddDays(-1);
            }

            // Kiểm tra store hiện tại là store nào
            var curStoreId = selectedStoreId != null ? selectedStoreId.Value : storeId;

            // Load những sản phẩm của store hiện tại = theo brand.
            var productItems = productApi.GetAvailableProductItemsModelByBrand(brandId);

            //var StoreFinal = (inDaReApi
            //   .GetInventoryDateReportByTimeRange(startDate, endDate, storeId).FirstOrDefault());
            //double StoreFinalAmount = 0;
            //if (StoreFinal != null)
            //{
            //    StoreFinalAmount = (double)StoreFinal.FinalAmount;
            //}
            //else
            //{
            //    StoreFinalAmount = 1;
            //}

            int[] categoriesId = null;
            if (!cateId.IsNullOrWhiteSpace())
            {
                categoriesId = cateId.Split(',').Select(q => int.Parse(q)).ToArray();
                productItems = productItems.Where(a => categoriesId.Contains(a.CatID ?? 0));
            }
            if (!string.IsNullOrWhiteSpace(param.sSearch))
            {
                productItems = productItems.Where(a => a.ItemName.Contains(param.sSearch));
            }
            var query = productItems
                        .OrderBy(q => q.ItemName)
                        .ThenBy(q => q.ItemID)
                        .Skip(param.iDisplayStart).Take(param.iDisplayLength)
                        //.OrderByDescending(q => q.CreateDate)
                        //.OrderBy(q => q.IndexPriority)
                        .ToList();

            List<dynamic> listDt = new List<dynamic>();
            var reportByTimeRange = inDaReApi.GetQueryInventoryDateReportByTimeRange(startDate, endDate, curStoreId, brandId);

            foreach (var productItem in query)
            {
                if (reportByTimeRange != null && reportByTimeRange.Count() != 0)
                {
                    double totalRealAmount = 0;
                    double lastRealQuantity = 0;
                    double firstRealQuantity = 0;
                    double totalType1 = 0;
                    double totalType2 = 0;

                    //var itemId = -1;

                    var firstRealReport = reportByTimeRange
                                            .Where(q => q.CreateDate == startDate)
                                            .SelectMany(q => q.InventoryDateReportItems)
                                            .Where(q => q.ItemID == productItem.ItemID)
                                            .ToList();
                    if (firstRealReport != null)
                    {
                        firstRealQuantity = firstRealReport.Sum(q => q.RealAmount ?? 0);
                    }
                    var lastRealReport = reportByTimeRange
                                            .Where(q => q.CreateDate == endDate)
                                            .SelectMany(q => q.InventoryDateReportItems)
                                            .Where(q => q.ItemID == productItem.ItemID)
                                            .ToList();
                    if (lastRealReport != null)
                    {
                        lastRealQuantity = lastRealReport.Sum(q => q.RealAmount ?? 0);
                        totalRealAmount = lastRealReport.Sum(q => q.RealAmount * q.Price ?? 0);
                        totalType1 = lastRealReport.Sum(q => q.RealAmount * q.Price ?? 0);
                        totalType2 = lastRealReport.Where(q => q.ProductItem.ItemType == 2).Sum(q => q.RealAmount * q.Price ?? 0);
                    }

                    listDt.Add(new
                    {
                        No = ++count,
                        ItemCode = productItem.ItemCode,
                        itemName = productItem.ItemName,
                        itemType = productItem.ItemType,
                        //== 1 ? "Nguyên vật liệu" : "Vật tư lẻ",
                        unit = productItem.Unit,
                        unitPrice = productItem.Price,
                        TotalRealAmount = string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", totalRealAmount),
                        //categoryName = productItem.ProductItemCategory.CateName,
                        categoryName = productItem.CateName,
                        firstRealQuantity = firstRealQuantity,
                        lastRealQuantity = lastRealQuantity,
                        itemId = productItem.ItemID,
                        totalType1 = totalType1,
                        totalType2 = totalType2
                    });
                }
                else
                {
                    listDt.Add(new
                    {
                        No = ++count,
                        ItemCode = productItem.ItemCode,
                        itemName = productItem.ItemName,
                        itemType = productItem.ItemType,
                        //== 1 ? "Nguyên vật liệu" : "Vật tư lẻ",
                        unit = productItem.Unit,
                        unitPrice = productItem.Price,
                        TotalRealAmount = 0,
                        //categoryName = productItem.ProductItemCategory.CateName,
                        categoryName = productItem.CateName,
                        firstRealQuantity = 0,
                        lastRealQuantity = 0,
                        itemId = productItem.ItemID,
                        totalType1 = 0,
                        totalType2 = 0
                    });
                }
            }

            return Json(new
            {
                param.sEcho,
                iTotalRecords = productItems.Count(),
                iTotalDisplayRecords = productItems.Count(),
                aaData = listDt,
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}