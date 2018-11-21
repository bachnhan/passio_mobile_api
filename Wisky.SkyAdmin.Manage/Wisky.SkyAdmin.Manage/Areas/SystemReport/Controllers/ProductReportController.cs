using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using Newtonsoft.Json;
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
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.SystemReport.Controllers
{
    [Authorize(Roles = "BrandManager, Manager, StoreReportViewer")]
    public class ProductReportController : DomainBasedController
    {
        // GET: SystemReport/ProductReport
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> LoadAllStore(int? brandId)
        {
            var storeApi = new StoreApi();
            //get stores 
            var storeGroupList = await storeApi.GetActiveStoreByBrandId(brandId.Value)
                                               .Select(a => new
                                               {
                                                   StoreId = a.ID,
                                                   Name = a.Name
                                               }).ToListAsync();

            return Json(storeGroupList);
        }
        public ActionResult IndexProductCategoryReport(int storeId, int brandId)
        {
            var storeApi = new StoreApi();
            var store = storeApi.GetActiveStoreByBrandId(brandId);
            ViewBag.storeId = storeId.ToString();
            return View(store);
        }
        public ActionResult IndexProductLineReport(int storeId, int brandId)
        {
            return View();
        }

        #region báo cáo doanh thu sản phẩm ở brand
        public ActionResult ProductReportOneStore(int brandId, string startTime, string endTime, int storeIdReport, string checkDeal)
        {
            var dateproductApi = new DateProductApi();
            var orderdetailApi = new OrderDetailApi();
            var productApi = new ProductApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listDate = new List<TempSystemProductDateReportItem>();
            var totalProductPrice = new List<DateProduct>();
            int i = 0;
            var listTotalGroup = new List<Double[]>();
            #region List product
            var listAllProduct = productApi.GetAllProductByBrand(brandId).ToList();
            foreach (var itemP in listAllProduct)
            {
                ProductReport.Add(new ProductLineReportModalViewModel
                {
                    ProductId = itemP.ProductID,
                    ProductName = itemP.ProductName,
                });
            }
            #endregion
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                var listOrderdetaillist = orderdetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).Where(a=>a.StoreId == storeIdReport).ToList();
                var listDateProductlist = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.StoreID == storeIdReport).ToList();
                foreach (var item in ProductReport)
                {
                    var listDateReport = new List<TempSystemProductDateReportItem>();
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    // 2. lấy list store
                    double listTotalAmount = 0;
                    double listFinalAmount = 0;
                    double listTotalDiscountFee = 0;
                    double listTotalAmountnow = 0;
                    double listFinalAmountnow = 0;
                    double listTotalDiscountFeenow = 0;
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    var listDateProductPro = listDateProductlist.Where(a => a.ProductId == item.ProductId);
                    var listOrder = listOrderdetaillist.Where(a => a.ProductID == item.ProductId);
                    if (endDate == dateNow.GetEndOfDate())
                    {
                        listTotalAmountnow = listOrder.Select(a => a.TotalAmount).Sum();
                        listFinalAmountnow = listOrder.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFeenow = listOrder.Select(a => a.Discount).Sum();

                        listTotalAmount = listDateProductPro.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProductPro.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProductPro.Select(a => a.Discount).Sum();

                        item.TotalOrder = listTotalAmount + listTotalAmountnow;
                        item.TotalPrice = listFinalAmount + listFinalAmountnow;
                        item.Discount = listTotalDiscountFee + listTotalDiscountFeenow;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount + listTotalAmountnow,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount + listFinalAmountnow,
                            });
                        }
                    }
                    else
                    {
                        listTotalAmount = listDateProductPro.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProductPro.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProductPro.Select(a => a.Discount).Sum();
                        item.TotalOrder = listTotalAmount;
                        item.TotalPrice = listFinalAmount;
                        item.Discount = listTotalDiscountFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount,
                            });
                        }
                    }
                    i++;

                }
                double totalProduct = 0;
                totalProduct = totalProductPrice.Sum(a=>a.TotalAmount);
                if (totalProduct == 0)
                {
                    totalProduct = 1;
                }
                #region kết quả truoc giam gia
                if (checkDeal.Equals("beforeDeal"))
                {
                    var list = ProductReport.OrderByDescending(a => a.TotalOrder * 100 / totalProduct).Select(a => new IConvertible[] {
                        a.ProductName,
                        a.CateName,
                        (a.TotalOrder*100/totalProduct).ToString("0.00"),
                        a.TotalOrder,
                        a.Discount,
                        a.TotalPrice,
                        a.ProductId,
                     }).ToList();
                    var PercentT = ProductReport.Where(a => (a.TotalPrice * 100 / totalProduct) < 5).Sum(a => a.TotalOrder);
                    ProductReport.Add(new ProductLineReportModalViewModel
                    {
                        ProductName = "Phần còn lại",
                        TotalOrder = PercentT,
                    });
                    var listPercent = ProductReport.Where(a => ((a.TotalOrder * 100 / totalProduct) < 5) && !a.ProductName.Equals("Phần còn lại"));
                    var _ProductName = ProductReport.Except(listPercent).Select(a => a.ProductName).ToArray();
                    var _ProductPercent = ProductReport.Except(listPercent).Select(a => a.TotalOrder).ToArray();
                    return Json(new
                    {
                        datatable = list,
                        dataChart = new
                        {
                            _ProductName,
                            _ProductPercent
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                #endregion
                #region kết quả sau giam gia
                else
                {
                    var list = ProductReport.OrderByDescending(a => a.TotalPrice * 100 / totalProduct).Select(a => new IConvertible[]
                      {
                        a.ProductName,
                        a.CateName,
                        (a.TotalPrice*100/totalProduct).ToString("0.00"),
                        a.TotalOrder,
                        a.Discount,
                        a.TotalPrice,
                        a.ProductId,
                 }).ToList();
                    var PercentT = ProductReport.Where(a => (a.TotalPrice * 100 / totalProduct) < 5).Sum(a => a.TotalPrice);
                    ProductReport.Add(new ProductLineReportModalViewModel
                    {
                        ProductName = "Phần còn lại",
                        TotalPrice = PercentT,
                    });
                    var listPercent = ProductReport.Where(a => ((a.TotalPrice * 100 / totalProduct) < 5) && !a.ProductName.Equals("Phần còn lại"));
                    var _ProductName = ProductReport.Except(listPercent).Select(a => a.ProductName).ToArray();
                    var _ProductPercent = ProductReport.Except(listPercent).Select(a => a.TotalPrice).ToArray(); return Json(new
                    {
                        datatable = list,
                        dataChart = new
                        {
                            _ProductName,
                            _ProductPercent
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                #endregion
            }
            catch (Exception ex)
            {
                return Json(new
                { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region báo cáo doanh thu nhóm sản phẩm ở brand
        public ActionResult CategoryReportOneStore(int brandId, string startTime, string endTime, int storeIdReport, string checkDeal)
        {
            var dateproductApi = new DateProductApi();
            var orderdetailApi = new OrderDetailApi();
            var cateProductApi = new ProductCategoryApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listDate = new List<TempSystemProductDateReportItem>();
            var totalProductPrice = new List<DateProduct>();
            int i = 0;
            var listTotalGroup = new List<Double[]>();
            #region List product
            var listAllCatePro = cateProductApi.GetProductCategoriesForReport(brandId).ToList();
            foreach (var itemP in listAllCatePro)
            {
                ProductReport.Add(new ProductLineReportModalViewModel
                {
                    CateId = itemP.CateID,
                    CateName = itemP.CateName,
                });
            }


            #endregion
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                var listOrderdetaillist = orderdetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).Where(a => a.StoreId == storeIdReport).ToList();
                var listDateProductlist = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.StoreID == storeIdReport).ToList();
                foreach (var item in ProductReport)
                {
                    var listDateReport = new List<TempSystemProductDateReportItem>();
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    // 2. lấy list store
                    double listTotalAmount = 0;
                    double listFinalAmount = 0;
                    double listTotalDiscountFee = 0;
                    double listTotalAmountnow = 0;
                    double listFinalAmountnow = 0;
                    double listTotalDiscountFeenow = 0;
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    var listDateProductPro = listDateProductlist.Where(a => a.CategoryId_ == item.CateId);
                    var listOrder = listOrderdetaillist.Where(a => a.Product.CatID == item.CateId);
                    if (endDate == dateNow.GetEndOfDate())
                    {
                        listTotalAmountnow = listOrder.Select(a => a.TotalAmount).Sum();
                        listFinalAmountnow = listOrder.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFeenow = listOrder.Select(a => a.Discount).Sum();

                        listTotalAmount = listDateProductPro.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProductPro.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProductPro.Select(a => a.Discount).Sum();

                        item.TotalOrder = listTotalAmount + listTotalAmountnow;
                        item.TotalPrice = listFinalAmount + listFinalAmountnow;
                        item.Discount = listTotalDiscountFee + listTotalDiscountFeenow;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount + listTotalAmountnow,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount + listFinalAmountnow,
                            });
                        }
                    }
                    else
                    {
                        listTotalAmount = listDateProductPro.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProductPro.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProductPro.Select(a => a.Discount).Sum();
                        item.TotalOrder = listTotalAmount;
                        item.TotalPrice = listFinalAmount;
                        item.Discount = listTotalDiscountFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount,
                            });
                        }
                    }
                    i++;
                }
                double totalProduct = 0;
                totalProduct = totalProductPrice.Sum(a => a.TotalAmount);
                if (totalProduct == 0)
                {
                    totalProduct = 1;
                }
                #region truoc giam gia
                if (checkDeal.Equals("beforeDeal"))
                {
                    var list2 = ProductReport.OrderByDescending(a => a.TotalOrder * 100 / totalProduct).Select(a => new IConvertible[]
                    {
                            a.CateName,
                            a.CateId,
                            (a.TotalOrder*100/(totalProduct)).ToString("0.00"),
                            a.TotalOrder,
                            a.Discount,
                            a.TotalPrice,
                            a.ProductId,
                    }).ToList();
                    var PercentT = ProductReport.Where(a => (a.TotalOrder * 100 / totalProduct) < 5).Sum(a => a.TotalOrder);
                    ProductReport.Add(new ProductLineReportModalViewModel
                    {
                        CateName = "Phần còn lại",
                        TotalOrder = PercentT,
                    });
                    var listPercent = ProductReport.Where(a => ((a.TotalOrder * 100 / totalProduct) < 5) && !a.CateName.Equals("Phần còn lại"));
                    var _ProductName = ProductReport.Except(listPercent).Select(a => a.CateName).ToArray();
                    var _ProductPercent = ProductReport.Except(listPercent).Select(a => a.TotalOrder).ToArray();
                    return Json(new
                    {
                        datatable = list2,
                        dataChart = new
                        {
                            _ProductName,
                            _ProductPercent,
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                #endregion
                #region sau giam gia
                else
                {
                    var list2 = ProductReport.OrderByDescending(a => a.TotalPrice * 100 / totalProduct).Select(a => new IConvertible[]
                    {
                            a.CateName,
                            a.CateId,
                            (a.TotalPrice*100/(totalProduct)).ToString("0.00"),
                            a.TotalOrder,
                            a.Discount,
                            a.TotalPrice,
                            a.ProductId,
                    }).ToList();
                    var PercentT = ProductReport.Where(a => (a.TotalPrice * 100 / totalProduct) < 5).Sum(a => a.TotalPrice);
                    ProductReport.Add(new ProductLineReportModalViewModel
                    {
                        CateName = "Phần còn lại",
                        TotalPrice = PercentT,
                    });
                    var listPercent = ProductReport.Where(a => ((a.TotalPrice * 100 / totalProduct) < 5) && !a.CateName.Equals("Phần còn lại"));
                    var _ProductName = ProductReport.Except(listPercent).Select(a => a.CateName).ToArray();
                    var _ProductPercent = ProductReport.Except(listPercent).Select(a => a.TotalPrice).ToArray();
                    return Json(new
                    {
                        datatable = list2,
                        dataChart = new
                        {
                            _ProductName,
                            _ProductPercent,
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                #endregion
            }
            catch (Exception ex)
            {
                return Json(new
                { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region lấy data sản phẩm ở brand
        public List<ExcellistModelView> ExportProductOneStoreTableToExcel(int brandId, string startTime, string endTime, int storeIdReport, string checkDeal)
        {
            List<ExcellistModelView> list = new List<ExcellistModelView>();
            List<ExcellistModelView> list2 = new List<ExcellistModelView>();
            var dateproductApi = new DateProductApi();
            var orderdetailApi = new OrderDetailApi();
            var productApi = new ProductApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listDate = new List<TempSystemProductDateReportItem>();
            var totalProductPrice = new List<DateProduct>();
            int i = 0;
            var listTotalGroup = new List<Double[]>();
            #region List product
            var listAllProduct = productApi.GetAllProductByBrand(brandId).ToList();
            foreach (var itemP in listAllProduct)
            {
                ProductReport.Add(new ProductLineReportModalViewModel
                {
                    ProductId = itemP.ProductID,
                    ProductName = itemP.ProductName,
                });
            }
            #endregion
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                var listOrderdetaillist = orderdetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).Where(a => a.StoreId == storeIdReport).ToList();
                var listDateProductlist = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.StoreID == storeIdReport).ToList();
                foreach (var item in ProductReport)
                {
                    var listDateReport = new List<TempSystemProductDateReportItem>();
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    // 2. lấy list store
                    double listTotalAmount = 0;
                    double listFinalAmount = 0;
                    double listTotalDiscountFee = 0;
                    double listTotalAmountnow = 0;
                    double listFinalAmountnow = 0;
                    double listTotalDiscountFeenow = 0;
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    var listDateProductPro = listDateProductlist.Where(a => a.ProductId == item.ProductId);
                    var listOrder = listOrderdetaillist.Where(a => a.ProductID == item.ProductId);
                    if (endDate == dateNow.GetEndOfDate())
                    {
                        listTotalAmountnow = listOrder.Select(a => a.TotalAmount).Sum();
                        listFinalAmountnow = listOrder.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFeenow = listOrder.Select(a => a.Discount).Sum();

                        listTotalAmount = listDateProductPro.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProductPro.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProductPro.Select(a => a.Discount).Sum();

                        item.TotalOrder = listTotalAmount + listTotalAmountnow;
                        item.TotalPrice = listFinalAmount + listFinalAmountnow;
                        item.Discount = listTotalDiscountFee + listTotalDiscountFeenow;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount + listTotalAmountnow,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount + listFinalAmountnow,
                            });
                        }
                        list.Add(new ExcellistModelView
                        {
                            Name = item.ProductName,
                            TotalOrder = listTotalAmount + listTotalAmountnow,
                            Discount = listTotalDiscountFee + listTotalDiscountFeenow,
                            TotalPrice = listFinalAmount + listFinalAmountnow,
                        });
                    }
                    else
                    {
                        listTotalAmount = listDateProductPro.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProductPro.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProductPro.Select(a => a.Discount).Sum();
                        item.TotalOrder = listTotalAmount;
                        item.TotalPrice = listFinalAmount;
                        item.Discount = listTotalDiscountFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount,
                            });
                        }
                        list.Add(new ExcellistModelView
                        {
                            Name = item.ProductName,
                            TotalOrder = listTotalAmount,
                            Discount = listTotalDiscountFee,
                            TotalPrice = listFinalAmount,
                        });
                    }
                    i++;
                }
                double totalProduct = 0;
                totalProduct = totalProductPrice.Sum(a => a.TotalAmount);
                if (totalProduct == 0)
                {
                    totalProduct = 1;
                }
                #region kết quả truoc giam gia
                if (checkDeal.Equals("beforeDeal"))
                {
                    foreach(var itemc in list)
                    {
                        itemc.Percent = (itemc.TotalOrder * 100 / totalProduct);
                    }
                }
                else
                {
                    foreach (var itemc in list)
                    {
                        itemc.Percent = (itemc.TotalPrice * 100 / totalProduct);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
            }
            return list;
        }
        #endregion
        #region lấy data nhóm sản phẩm ở brand
        public List<ExcellistModelView> ExportCategoryOneStoreTableToExcel(int brandId, string startTime, string endTime, int storeIdReport, string checkDeal)
        {
            List<ExcellistModelView> list = new List<ExcellistModelView>();
            List<ExcellistModelView> list2 = new List<ExcellistModelView>();
            var dateproductApi = new DateProductApi();
            var orderdetailApi = new OrderDetailApi();
            var cateProductApi = new ProductCategoryApi();
            var productApi = new ProductApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listDate = new List<TempSystemProductDateReportItem>();
            var totalProductPrice = new List<DateProduct>();
            int i = 0;
            var listTotalGroup = new List<Double[]>();
            #region List product
            var listAllCatePro = cateProductApi.GetProductCategoriesForReport(brandId).ToList();
            foreach (var itemP in listAllCatePro)
            {
                ProductReport.Add(new ProductLineReportModalViewModel
                {
                    CateId = itemP.CateID,
                    CateName = itemP.CateName,
                });
            }
            #endregion
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                var listOrderdetaillist = orderdetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).Where(a => a.StoreId == storeIdReport).ToList();
                var listDateProductlist = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.StoreID == storeIdReport).ToList();
                foreach (var item in ProductReport)
                {
                    var listDateReport = new List<TempSystemProductDateReportItem>();
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    // 2. lấy list store
                    double listTotalAmount = 0;
                    double listFinalAmount = 0;
                    double listTotalDiscountFee = 0;
                    double listTotalAmountnow = 0;
                    double listFinalAmountnow = 0;
                    double listTotalDiscountFeenow = 0;
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    var listDateProductPro = listDateProductlist.Where(a => a.CategoryId_ == item.CateId);
                    var listOrder = listOrderdetaillist.Where(a => a.Product.CatID == item.CateId);
                    if (endDate == dateNow.GetEndOfDate())
                    {
                        listTotalAmountnow = listOrder.Select(a => a.TotalAmount).Sum();
                        listFinalAmountnow = listOrder.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFeenow = listOrder.Select(a => a.Discount).Sum();

                        listTotalAmount = listDateProductPro.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProductPro.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProductPro.Select(a => a.Discount).Sum();

                        item.TotalOrder = listTotalAmount + listTotalAmountnow;
                        item.TotalPrice = listFinalAmount + listFinalAmountnow;
                        item.Discount = listTotalDiscountFee + listTotalDiscountFeenow;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount + listTotalAmountnow,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount + listFinalAmountnow,
                            });
                        }
                        list.Add(new ExcellistModelView
                        {
                            Name = item.ProductName,
                            TotalOrder = listTotalAmount + listTotalAmountnow,
                            Discount = listTotalDiscountFee + listTotalDiscountFeenow,
                            TotalPrice = listFinalAmount + listFinalAmountnow,
                        });
                    }
                    else
                    {
                        listTotalAmount = listDateProductPro.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProductPro.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProductPro.Select(a => a.Discount).Sum();
                        item.TotalOrder = listTotalAmount;
                        item.TotalPrice = listFinalAmount;
                        item.Discount = listTotalDiscountFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount,
                            });
                        }
                        list.Add(new ExcellistModelView
                        {
                            Name = item.CateName,
                            TotalOrder = listTotalAmount,
                            Discount = listTotalDiscountFee,
                            TotalPrice = listFinalAmount,
                        });
                    }
                    i++;
                    
                }
                double totalProduct = 0;
                totalProduct = totalProductPrice.Sum(a => a.TotalAmount);
                if (totalProduct == 0)
                {
                    totalProduct = 1;
                }
                #region kết quả truoc giam gia
                if (checkDeal.Equals("beforeDeal"))
                {
                    foreach (var itemc in list)
                    {
                        itemc.Percent = (itemc.TotalOrder * 100 / totalProduct);
                    }
                }
                else
                {
                    foreach (var itemc in list)
                    {
                        itemc.Percent = (itemc.TotalPrice * 100 / totalProduct);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
            }
            return list;
        }
        #endregion
        #region xuất file excel
        public ActionResult ReportProductOneStoreExportExcelEPPlus(int brandId, string startTime, string endTime, int storeIdReport, string checkvalue, string checkDeal)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo doanh thu sản phẩm");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var list = new List<ExcellistModelView>();
                if (checkvalue.Equals("loadProducts"))
                {
                    list = ExportProductOneStoreTableToExcel(brandId, startTime, endTime, storeIdReport, checkDeal);
                }
                else
                {
                    list = ExportCategoryOneStoreTableToExcel(brandId, startTime, endTime, storeIdReport, checkDeal);
                }

                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Sản phẩm/ Nhóm sản phẩm";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tỉ trọng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng tiền trước giảm giá";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giảm giá";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Tổng tiền sau giảm giá";

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
                var listtotal = list.OrderByDescending(a => a.Percent);
                foreach (var data in listtotal)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.Name;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Percent.ToString("0.00");
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrder;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Discount;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalPrice;
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
                startTime = startTime.Replace('/', '-');

                endTime = endTime.Replace('/', '-');
                var fileDownloadName = "Báo cáo doanh thu sản phẩm từ ngày " + startTime + " - " + endTime + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }
        #endregion
        #region báo cáo doanh thu sản phẩm theo group
        public ActionResult ProductReportStoreGroup(int brandId, string startTime, string endTime, int storeGroupIdReport, string checkDeal)
        {
            var storeApi = new StoreApi();
            var dateproductApi = new DateProductApi();
            var orderdetailApi = new OrderDetailApi();
            int i = 0;
            var productApi = new ProductApi();
            var listAllProduct = productApi.GetAllProductByBrand(brandId).ToList();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listDate = new List<TempSystemProductDateReportItem>();
            var totalProductPrice = new List<DateProduct>();
            var groupApi = new StoreGroupApi();
            var listTotalGroup = new List<Double[]>();
            var table1 = storeApi.GetStoreByBrandId(brandId);
            var table2 = dateproductApi.GetDateProductAllStoreByTimeRange(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate());
            var tablejoin = (from t1 in table1
                             join t2 in table2
                             on t1.ID equals t2.StoreID
                             select new
                             {
                                 t2.Date,
                                 t2.StoreID,
                                 t2.ProductId,
                                 t2.Quantity,
                                 t2.TotalAmount,
                                 t2.Discount,
                                 t2.FinalAmount,
                             }).ToList();
            #region List product
            foreach (var itemP in listAllProduct)
            {

                ProductReport.Add(new ProductLineReportModalViewModel
                {
                    ProductId = itemP.ProductID,
                    ProductName = itemP.ProductName,
                });
            }
            
            #endregion
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                var listOrderdetaillist = orderdetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //var listDateProductlist = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).ToList();
                var storeinGroups = storeApi.GetStoreByGroupId(storeGroupIdReport).ToList();
                // 2. lấy list store
                var storeList = storeinGroups.ToList();
                foreach (var item in ProductReport)
                {
                    var listDateReport = new List<TempSystemProductDateReportItem>();
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    double listTotalAmountnow = 0;
                    double listFinalAmountnow = 0;
                    double listTotalDiscountFeenow = 0;
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    var listDateProductPro = tablejoin.Where(a => a.ProductId == item.ProductId);
                    if (endDate == dateNow.GetEndOfDate())
                    {
                        double totalDateAmount = 0;
                        double finalDateAmount = 0;
                        double discountDateFee = 0;
                        foreach (var store in storeList)
                        {
                            var listOrder = listOrderdetaillist.Where(a => a.ProductID == item.ProductId && a.StoreId == store.ID);
                            listTotalAmountnow = listOrder.Select(a => a.TotalAmount).Sum();
                            listFinalAmountnow = listOrder.Select(a => a.FinalAmount).Sum();
                            listTotalDiscountFeenow = listOrder.Select(a => a.Discount).Sum();

                            var listinstore = listDateProductPro.Where(a => a.StoreID == store.ID);
                            var totalAmount = listinstore.Sum(a => a.TotalAmount);
                            var finalAmount = listinstore.Sum(a => a.FinalAmount);
                            var discountFee = listinstore.Sum(a => a.Discount);
                            totalDateAmount += totalAmount + listTotalAmountnow;
                            finalDateAmount += finalAmount + listFinalAmountnow;
                            discountDateFee += discountFee + listTotalDiscountFeenow;
                        }
                        item.TotalOrder = totalDateAmount;
                        item.TotalPrice = finalDateAmount;
                        item.Discount = discountDateFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = totalDateAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = finalDateAmount,
                            });
                        }
                    }
                    else
                    {
                        double totalDateAmount = 0;
                        double finalDateAmount = 0;
                        double discountDateFee = 0;
                        foreach (var store in storeList)
                        {
                            var listinstore = listDateProductPro.Where(a => a.StoreID == store.ID);
                            var totalAmount = listinstore.Sum(a => a.TotalAmount);
                            var finalAmount = listinstore.Sum(a => a.FinalAmount);
                            var discountFee = listinstore.Sum(a => a.Discount);
                            totalDateAmount += totalAmount;
                            finalDateAmount += finalAmount;
                            discountDateFee += discountFee;
                        }
                        item.TotalOrder = totalDateAmount;
                        item.TotalPrice = finalDateAmount;
                        item.Discount = discountDateFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = totalDateAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = finalDateAmount,
                            });
                        }
                    }
                    i++;
                }
                var totalProduct = totalProductPrice.Select(a => a.TotalAmount).Sum();
                if (totalProduct == 0)
                {
                    totalProduct = 1;
                }
                if (checkDeal.Equals("beforeDeal"))
                {
                    var list = ProductReport.OrderByDescending(a => a.TotalOrder * 100 / totalProduct).Select(a => new IConvertible[]
                {
                            a.ProductName,
                            a.CateName,
                            (a.TotalOrder*100/totalProduct).ToString("0.00"),
                            a.TotalOrder,
                            a.Discount,
                            a.TotalPrice,
                            a.ProductId,
                }).ToList();
                    var PercentT1 = ProductReport.Where(a => (a.TotalOrder * 100 / totalProduct) < 5).Sum(a => a.TotalOrder);
                    ProductReport.Add(new ProductLineReportModalViewModel
                    {
                        ProductName = "Phần còn lại",
                        TotalOrder = PercentT1,
                    });
                    var listPercent = ProductReport.Where(a => ((a.TotalOrder * 100 / totalProduct) < 5) && !a.ProductName.Equals("Phần còn lại"));
                    var _ProductName = ProductReport.Except(listPercent).Select(a => a.ProductName).ToArray();
                    var _ProductPercent = ProductReport.Except(listPercent).Select(a => a.TotalOrder).ToArray();
                    return Json(new
                    {
                        datatable = list,
                        dataChart = new
                        {
                            _ProductName,
                            _ProductPercent,
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var list = ProductReport.OrderByDescending(a => a.TotalPrice * 100 / totalProduct).Select(a => new IConvertible[]
                {
                            a.ProductName,
                            a.CateName,
                            (a.TotalPrice*100/totalProduct).ToString("0.00"),
                            a.TotalOrder,
                            a.Discount,
                            a.TotalPrice,
                            a.ProductId,
                }).ToList();
                    var PercentT1 = ProductReport.Where(a => (a.TotalPrice * 100 / totalProduct) < 5).Sum(a => a.TotalPrice);
                    ProductReport.Add(new ProductLineReportModalViewModel
                    {
                        ProductName = "Phần còn lại",
                        TotalPrice = PercentT1,
                    });
                    var listPercent = ProductReport.Where(a => ((a.TotalPrice * 100 / totalProduct) < 5) && !a.ProductName.Equals("Phần còn lại"));
                    var _ProductName = ProductReport.Except(listPercent).Select(a => a.ProductName).ToArray();
                    var _ProductPercent = ProductReport.Except(listPercent).Select(a => a.TotalPrice).ToArray();
                    return Json(new
                    {
                        datatable = list,
                        dataChart = new
                        {
                            _ProductName,
                            _ProductPercent,
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CategoryReportStoreGroup(int brandId, string startTime, string endTime, int storeGroupIdReport, string checkDeal)
        {
            var storeApi = new StoreApi();
            var dateproductApi = new DateProductApi();
            var orderdetailApi = new OrderDetailApi();
            int i = 0;
            var cateProductApi = new ProductCategoryApi();
            var listAllCatePro = cateProductApi.GetProductCategoriesForReport(brandId).ToList();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listDate = new List<TempSystemProductDateReportItem>();
            var totalProductPrice = new List<DateProduct>();
            var groupApi = new StoreGroupApi();
            var listTotalGroup = new List<Double[]>();
            var table1 = storeApi.GetStoreByBrandId(brandId);
            var table2 = dateproductApi.GetDateProductAllStoreByTimeRange(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate());
            var tablejoin = (from t1 in table1
                             join t2 in table2
                             on t1.ID equals t2.StoreID
                             select new
                             {
                                 t2.Date,
                                 t2.StoreID,
                                 t2.CategoryId_,
                                 t2.Quantity,
                                 t2.TotalAmount,
                                 t2.Discount,
                                 t2.FinalAmount,
                             }).ToList();
            #region List product
            var productCategoryApi = new ProductCategoryApi();
            foreach (var itemP in listAllCatePro)
            {
                ProductReport.Add(new ProductLineReportModalViewModel
                {
                    CateId = itemP.CateID,
                    CateName = itemP.CateName,
                });
            }
            #endregion
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                var listOrderdetaillist = orderdetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //var listDateProductlist = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).ToList();
                var storeinGroups = storeApi.GetStoreByGroupId(storeGroupIdReport).ToList();
                // 2. lấy list store
                var storeList = storeinGroups.ToList();
                foreach (var item in ProductReport)
                {
                    var listDateReport = new List<TempSystemProductDateReportItem>();
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    double listTotalAmountnow = 0;
                    double listFinalAmountnow = 0;
                    double listTotalDiscountFeenow = 0;
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    var listDateProduct2 = tablejoin.Where(a => a.CategoryId_ == item.CateId);
                    if (endDate == dateNow.GetEndOfDate())
                    {
                        double totalDateAmount = 0;
                        double finalDateAmount = 0;
                        double discountDateFee = 0;
                        foreach (var store in storeList)
                        {
                            var listOrder = listOrderdetaillist.Where(a => a.Product.CatID == item.CateId && a.StoreId == store.ID);
                            listTotalAmountnow = listOrder.Select(a => a.TotalAmount).Sum();
                            listFinalAmountnow = listOrder.Select(a => a.FinalAmount).Sum();
                            listTotalDiscountFeenow = listOrder.Select(a => a.Discount).Sum();

                            var listinstore = listDateProduct2.Where(a => a.StoreID == store.ID);
                            var totalAmount = listinstore.Sum(a => a.TotalAmount);
                            var finalAmount = listinstore.Sum(a => a.FinalAmount);
                            var discountFee = listinstore.Sum(a => a.Discount);
                            totalDateAmount += totalAmount + listTotalAmountnow;
                            finalDateAmount += finalAmount + listFinalAmountnow;
                            discountDateFee += discountFee + listTotalDiscountFeenow;
                        }
                        item.TotalOrder = totalDateAmount;
                        item.TotalPrice = finalDateAmount;
                        item.Discount = discountDateFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = totalDateAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = finalDateAmount,
                            });
                        }
                    }
                    else
                    {
                        double totalDateAmount = 0;
                        double finalDateAmount = 0;
                        double discountDateFee = 0;
                        foreach (var store in storeList)
                        {
                            var listinstore = listDateProduct2.Where(a => a.StoreID == store.ID);
                            var totalAmount = listinstore.Sum(a => a.TotalAmount);
                            var finalAmount = listinstore.Sum(a => a.FinalAmount);
                            var discountFee = listinstore.Sum(a => a.Discount);
                            totalDateAmount += totalAmount;
                            finalDateAmount += finalAmount;
                            discountDateFee += discountFee;
                        }
                        item.TotalOrder = totalDateAmount;
                        item.TotalPrice = finalDateAmount;
                        item.Discount = discountDateFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = totalDateAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = finalDateAmount,
                            });
                        }
                    }
                    i++;
                }
                var totalProduct = totalProductPrice.Select(a => a.TotalAmount).Sum();
                if (totalProduct == 0)
                {
                    totalProduct = 1;
                }
                if (checkDeal.Equals("beforeDeal"))
                {
                    var list2 = ProductReport.OrderByDescending(a => a.TotalOrder * 100 / totalProduct).Select(a => new IConvertible[]
                {
                            a.CateName,
                            a.CateId,
                           (a.TotalOrder*100/totalProduct).ToString("0.00"),
                            a.TotalOrder,
                            a.Discount,
                            a.TotalPrice,
                }).ToList();
                    var PercentT = ProductReport.Where(a => (a.TotalOrder * 100 / totalProduct) < 5).Sum(a => a.TotalOrder);
                    ProductReport.Add(new ProductLineReportModalViewModel
                    {
                        CateName = "Phần còn lại",
                        TotalOrder = PercentT,
                    });
                    var listPercent = ProductReport.Where(a => ((a.TotalOrder * 100 / totalProduct) < 5) && !a.CateName.Equals("Phần còn lại"));
                    var _ProductName = ProductReport.Except(listPercent).Select(a => a.CateName).ToArray();
                    var _ProductPercent = ProductReport.Except(listPercent).Select(a => a.TotalOrder).ToArray();
                    return Json(new
                    {
                        datatable = list2,
                        dataChart = new
                        {
                            _ProductName,
                            _ProductPercent,
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    {
                        var list2 = ProductReport.OrderByDescending(a => a.TotalPrice * 100 / totalProduct).Select(a => new IConvertible[]
                    {
                            a.CateName,
                            a.CateId,
                           (a.TotalPrice*100/totalProduct).ToString("0.00"),
                            a.TotalOrder,
                            a.Discount,
                            a.TotalPrice,
                    }).ToList();
                        var PercentT = ProductReport.Where(a => (a.TotalPrice * 100 / totalProduct) < 5).Sum(a => a.TotalPrice);
                        ProductReport.Add(new ProductLineReportModalViewModel
                        {
                            CateName = "Phần còn lại",
                            TotalPrice = PercentT,
                        });
                        var listPercent = ProductReport.Where(a => ((a.TotalPrice * 100 / totalProduct) < 5) && !a.CateName.Equals("Phần còn lại"));
                        var _ProductName = ProductReport.Except(listPercent).Select(a => a.CateName).ToArray();
                        var _ProductPercent = ProductReport.Except(listPercent).Select(a => a.TotalPrice).ToArray();
                        return Json(new
                        {
                            datatable = list2,
                            dataChart = new
                            {
                                _ProductName,
                                _ProductPercent,
                            }
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public List<ExcellistModelView> ExportProductStoreGroupTableToExcel(int brandId, string startTime, string endTime, int storeGroupIdReport, string checkDeal)
        {
            List<ExcellistModelView> list = new List<ExcellistModelView>();
            List<ExcellistModelView> list2 = new List<ExcellistModelView>();
            var storeApi = new StoreApi();
            var dateproductApi = new DateProductApi();
            var orderdetailApi = new OrderDetailApi();
            int i = 0;
            var productApi = new ProductApi();
            var listAllProduct = productApi.GetAllProductByBrand(brandId).ToList();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listDate = new List<TempSystemProductDateReportItem>();
            var totalProductPrice = new List<DateProduct>();
            var groupApi = new StoreGroupApi();
            var listTotalGroup = new List<Double[]>();
            var table1 = storeApi.GetStoreByBrandId(brandId);
            var table2 = dateproductApi.GetDateProductAllStoreByTimeRange(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate());
            var tablejoin = (from t1 in table1
                             join t2 in table2
                             on t1.ID equals t2.StoreID
                             select new
                             {
                                 t2.Date,
                                 t2.StoreID,
                                 t2.ProductId,
                                 t2.Quantity,
                                 t2.TotalAmount,
                                 t2.Discount,
                                 t2.FinalAmount,
                             }).ToList();
            #region List product
            foreach (var itemP in listAllProduct)
            {

                ProductReport.Add(new ProductLineReportModalViewModel
                {
                    ProductId = itemP.ProductID,
                    ProductName = itemP.ProductName,
                });
            }

            #endregion
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                var listOrderdetaillist = orderdetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //var listDateProductlist = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).ToList();
                var storeinGroups = storeApi.GetStoreByGroupId(storeGroupIdReport).ToList();
                // 2. lấy list store
                var storeList = storeinGroups.ToList();
                foreach (var item in ProductReport)
                {
                    var listDateReport = new List<TempSystemProductDateReportItem>();
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    double listTotalAmountnow = 0;
                    double listFinalAmountnow = 0;
                    double listTotalDiscountFeenow = 0;
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    var listDateProductPro = tablejoin.Where(a => a.ProductId == item.ProductId);
                    if (endDate == dateNow.GetEndOfDate())
                    {
                        double totalDateAmount = 0;
                        double finalDateAmount = 0;
                        double discountDateFee = 0;
                        foreach (var store in storeList)
                        {
                            var listOrder = listOrderdetaillist.Where(a => a.ProductID == item.ProductId && a.StoreId == store.ID);
                            listTotalAmountnow = listOrder.Select(a => a.TotalAmount).Sum();
                            listFinalAmountnow = listOrder.Select(a => a.FinalAmount).Sum();
                            listTotalDiscountFeenow = listOrder.Select(a => a.Discount).Sum();

                            var listinstore = listDateProductPro.Where(a => a.StoreID == store.ID);
                            var totalAmount = listinstore.Sum(a => a.TotalAmount);
                            var finalAmount = listinstore.Sum(a => a.FinalAmount);
                            var discountFee = listinstore.Sum(a => a.Discount);
                            totalDateAmount += totalAmount + listTotalAmountnow;
                            finalDateAmount += finalAmount + listFinalAmountnow;
                            discountDateFee += discountFee + listTotalDiscountFeenow;
                        }
                        item.TotalOrder = totalDateAmount;
                        item.TotalPrice = finalDateAmount;
                        item.Discount = discountDateFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = totalDateAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = finalDateAmount,
                            });
                        }
                        list.Add(new ExcellistModelView
                        {
                            Name = item.ProductName,
                            TotalOrder = totalDateAmount,
                            Discount = finalDateAmount,
                            TotalPrice = discountDateFee,
                        });
                    }
                    else
                    {
                        double totalDateAmount = 0;
                        double finalDateAmount = 0;
                        double discountDateFee = 0;
                        foreach (var store in storeList)
                        {
                            var listinstore = listDateProductPro.Where(a => a.StoreID == store.ID);
                            var totalAmount = listinstore.Sum(a => a.TotalAmount);
                            var finalAmount = listinstore.Sum(a => a.FinalAmount);
                            var discountFee = listinstore.Sum(a => a.Discount);
                            totalDateAmount += totalAmount;
                            finalDateAmount += finalAmount;
                            discountDateFee += discountFee;
                        }
                        item.TotalOrder = totalDateAmount;
                        item.TotalPrice = finalDateAmount;
                        item.Discount = discountDateFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = totalDateAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = finalDateAmount,
                            });
                        }
                        list.Add(new ExcellistModelView
                        {
                            Name = item.ProductName,
                            TotalOrder = totalDateAmount,
                            Discount = discountDateFee,
                            TotalPrice = finalDateAmount,
                        });
                    }
                    i++;
                }
                var totalProduct = totalProductPrice.Select(a => a.TotalAmount).Sum();
                if (totalProduct == 0)
                {
                    totalProduct = 1;
                }
                #region kết quả truoc giam gia
                if (checkDeal.Equals("beforeDeal"))
                {
                    foreach (var itemc in list)
                    {
                        itemc.Percent = (itemc.TotalOrder * 100 / totalProduct);
                    }
                }
                else
                {
                    foreach (var itemc in list)
                    {
                        itemc.Percent = (itemc.TotalPrice * 100 / totalProduct);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
            }
            return list;
        }
        public List<ExcellistModelView> ExportCategoryStoreGroupTableToExcel(int brandId, string startTime, string endTime, int storeGroupIdReport, string checkDeal)
        {
            List<ExcellistModelView> list = new List<ExcellistModelView>();
            List<ExcellistModelView> list2 = new List<ExcellistModelView>();
            var storeApi = new StoreApi();
            var dateproductApi = new DateProductApi();
            var orderdetailApi = new OrderDetailApi();
            int i = 0;
            var cateProductApi = new ProductCategoryApi();
            var listAllCatePro = cateProductApi.GetProductCategoriesForReport(brandId).ToList();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listDate = new List<TempSystemProductDateReportItem>();
            var totalProductPrice = new List<DateProduct>();
            var groupApi = new StoreGroupApi();
            var listTotalGroup = new List<Double[]>();
            var table1 = storeApi.GetStoreByBrandId(brandId);
            var table2 = dateproductApi.GetDateProductAllStoreByTimeRange(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate());
            var tablejoin = (from t1 in table1
                             join t2 in table2
                             on t1.ID equals t2.StoreID
                             select new
                             {
                                 t2.Date,
                                 t2.StoreID,
                                 t2.CategoryId_,
                                 t2.Quantity,
                                 t2.TotalAmount,
                                 t2.Discount,
                                 t2.FinalAmount,
                             }).ToList();
            #region List product
            var productCategoryApi = new ProductCategoryApi();
            foreach (var itemP in listAllCatePro)
            {
                ProductReport.Add(new ProductLineReportModalViewModel
                {
                    CateId = itemP.CateID,
                    CateName = itemP.CateName,
                });
            }
            #endregion
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                var listOrderdetaillist = orderdetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //var listDateProductlist = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).ToList();
                var storeinGroups = storeApi.GetStoreByGroupId(storeGroupIdReport).ToList();
                // 2. lấy list store
                var storeList = storeinGroups.ToList();
                foreach (var item in ProductReport)
                {
                    var listDateReport = new List<TempSystemProductDateReportItem>();
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    double listTotalAmountnow = 0;
                    double listFinalAmountnow = 0;
                    double listTotalDiscountFeenow = 0;
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    var listDateProduct2 = tablejoin.Where(a => a.CategoryId_ == item.CateId);
                    if (endDate == dateNow.GetEndOfDate())
                    {
                        double totalDateAmount = 0;
                        double finalDateAmount = 0;
                        double discountDateFee = 0;
                        foreach (var store in storeList)
                        {
                            var listOrder = listOrderdetaillist.Where(a => a.Product.CatID == item.CateId && a.StoreId == store.ID);
                            listTotalAmountnow = listOrder.Select(a => a.TotalAmount).Sum();
                            listFinalAmountnow = listOrder.Select(a => a.FinalAmount).Sum();
                            listTotalDiscountFeenow = listOrder.Select(a => a.Discount).Sum();

                            var listinstore = listDateProduct2.Where(a => a.StoreID == store.ID);
                            var totalAmount = listinstore.Sum(a => a.TotalAmount);
                            var finalAmount = listinstore.Sum(a => a.FinalAmount);
                            var discountFee = listinstore.Sum(a => a.Discount);
                            totalDateAmount += totalAmount + listTotalAmountnow;
                            finalDateAmount += finalAmount + listFinalAmountnow;
                            discountDateFee += discountFee + listTotalDiscountFeenow;
                        }
                        item.TotalOrder = totalDateAmount;
                        item.TotalPrice = finalDateAmount;
                        item.Discount = discountDateFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = totalDateAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = finalDateAmount,
                            });
                        }
                        list.Add(new ExcellistModelView
                        {
                            Name = item.CateName,
                            TotalOrder = totalDateAmount,
                            Discount = finalDateAmount,
                            TotalPrice = discountDateFee,
                        });
                    }
                    else
                    {
                        double totalDateAmount = 0;
                        double finalDateAmount = 0;
                        double discountDateFee = 0;
                        foreach (var store in storeList)
                        {
                            var listinstore = listDateProduct2.Where(a => a.StoreID == store.ID);
                            var totalAmount = listinstore.Sum(a => a.TotalAmount);
                            var finalAmount = listinstore.Sum(a => a.FinalAmount);
                            var discountFee = listinstore.Sum(a => a.Discount);
                            totalDateAmount += totalAmount;
                            finalDateAmount += finalAmount;
                            discountDateFee += discountFee;
                        }
                        item.TotalOrder = totalDateAmount;
                        item.TotalPrice = finalDateAmount;
                        item.Discount = discountDateFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = totalDateAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = finalDateAmount,
                            });
                        }
                        list.Add(new ExcellistModelView
                        {
                            Name = item.CateName,
                            TotalOrder = totalDateAmount,
                            Discount = discountDateFee,
                            TotalPrice = finalDateAmount,
                        });
                    }
                    i++;
                }
                var totalProduct = totalProductPrice.Select(a => a.TotalAmount).Sum();
                if (totalProduct == 0)
                {
                    totalProduct = 1;
                }
                #region kết quả truoc giam gia
                if (checkDeal.Equals("beforeDeal"))
                {
                    foreach (var itemc in list)
                    {
                        itemc.Percent = (itemc.TotalOrder * 100 / totalProduct);
                    }
                }
                else
                {
                    foreach (var itemc in list)
                    {
                        itemc.Percent = (itemc.TotalPrice * 100 / totalProduct);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
            }
            return list;
        }

        public ActionResult ReportProductStoreGroupExportExcelEPPlus(int brandId, string startTime, string endTime, int storeGroupIdReport, string checkvalue, string checkDeal)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo doanh thu sản phẩm");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var list = new List<ExcellistModelView>();
                if (checkvalue.Equals("loadProducts"))
                {
                    list = ExportProductStoreGroupTableToExcel(brandId, startTime, endTime, storeGroupIdReport, checkDeal);
                }
                else
                {
                    list = ExportCategoryStoreGroupTableToExcel(brandId, startTime, endTime, storeGroupIdReport, checkDeal);
                }
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Sản phẩm/ Nhóm sản phẩm";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tỉ trọng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng tiền trước giảm giá";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giảm giá";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Tổng tiền sau giảm giá";

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
                var listtotal = list.OrderByDescending(a => a.Percent);
                foreach (var data in listtotal)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.Name;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Percent.ToString("0.00");
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrder;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Discount;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalPrice;
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
                startTime = startTime.Replace('/', '-');

                endTime = endTime.Replace('/', '-');
                var fileDownloadName = "Báo cáo doanh thu sản phẩm từ ngày " + startTime + " - " + endTime + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }
        #endregion
        #region báo cáo doanh thu sản phẩm theo brand
        public ActionResult ProductReportAllStore(int brandId, string startTime, string endTime, string checkDeal)
        {
            var storeApi = new StoreApi();
            var dateproductApi = new DateProductApi();
            var orderdetailApi = new OrderDetailApi();
            int i = 0;
            var productApi = new ProductApi();
            var listAllProduct = productApi.GetAllProductByBrand(brandId).ToList();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listDate = new List<TempSystemProductDateReportItem>();
            var totalProductPrice = new List<DateProduct>();
            var groupApi = new StoreGroupApi();
            var listTotalGroup = new List<Double[]>();
            var table1 = storeApi.GetStoreByBrandId(brandId);
            var table2 = dateproductApi.GetDateProductAllStoreByTimeRange(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate());
            var tablejoin = (from t1 in table1
                             join t2 in table2
                             on t1.ID equals t2.StoreID
                             select new
                             {
                                 t2.Date,
                                 t2.StoreID,
                                 t2.ProductId,
                                 t2.Quantity,
                                 t2.TotalAmount,
                                 t2.Discount,
                                 t2.FinalAmount,
                             }).ToList();
            #region List product
            var productCategoryApi = new ProductCategoryApi();
            foreach (var itemP in listAllProduct)
            {

                ProductReport.Add(new ProductLineReportModalViewModel
                {
                    ProductId = itemP.ProductID,
                    ProductName = itemP.ProductName,
                });
            }
            #endregion
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                var listOrderdetaillist = orderdetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //var listDateProductlist = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).ToList();
                foreach (var item in ProductReport)
                {
                    var listDateReport = new List<TempSystemProductDateReportItem>();
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    // 2. lấy list store
                    double listTotalAmount = 0;
                    double listFinalAmount = 0;
                    double listTotalDiscountFee = 0;
                    double listTotalAmountnow = 0;
                    double listFinalAmountnow = 0;
                    double listTotalDiscountFeenow = 0;
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    var listDateProductPro = tablejoin.Where(a => a.ProductId == item.ProductId);
                    var listOrder = listOrderdetaillist.Where(a => a.ProductID == item.ProductId);
                    if (endDate == dateNow.GetEndOfDate())
                    {
                        listTotalAmountnow = listOrder.Select(a => a.TotalAmount).Sum();
                        listFinalAmountnow = listOrder.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFeenow = listOrder.Select(a => a.Discount).Sum();

                        listTotalAmount = listDateProductPro.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProductPro.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProductPro.Select(a => a.Discount).Sum();

                        item.TotalOrder = listTotalAmount + listTotalAmountnow;
                        item.TotalPrice = listFinalAmount + listFinalAmountnow;
                        item.Discount = listTotalDiscountFee + listTotalDiscountFeenow;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount + listTotalAmountnow,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount + listFinalAmountnow,
                            });
                        }
                    }
                    else
                    {
                        listTotalAmount = listDateProductPro.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProductPro.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProductPro.Select(a => a.Discount).Sum();
                        item.TotalOrder = listTotalAmount;
                        item.TotalPrice = listFinalAmount;
                        item.Discount = listTotalDiscountFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount,
                            });
                        }
                    }
                    i++;
                }
                var totalProduct = totalProductPrice.Select(a => a.TotalAmount).Sum();
                if (totalProduct == 0)
                {
                    totalProduct = 1;
                }
                if (checkDeal.Equals("beforeDeal"))
                {
                    var list = ProductReport.OrderByDescending(a => a.TotalOrder * 100 / totalProduct).Select(a => new IConvertible[]
                {
                            a.ProductName,
                            a.CateName,
                            (a.TotalOrder*100/totalProduct).ToString("0.00"),
                            a.TotalOrder,
                            a.Discount,
                            a.TotalPrice,
                            a.ProductId,
                }).ToList();
                    var PercentT1 = ProductReport.Where(a => (a.TotalOrder * 100 / totalProduct) < 5).Sum(a => a.TotalOrder);
                    ProductReport.Add(new ProductLineReportModalViewModel
                    {
                        ProductName = "Phần còn lại",
                        TotalOrder = PercentT1,
                    });
                    var listPercent = ProductReport.Where(a => ((a.TotalOrder * 100 / totalProduct) < 5) && !a.ProductName.Equals("Phần còn lại"));
                    var _ProductName = ProductReport.Except(listPercent).Select(a => a.ProductName).ToArray();
                    var _ProductPercent = ProductReport.Except(listPercent).Select(a => a.TotalOrder).ToArray();
                    return Json(new
                    {
                        datatable = list,
                        dataChart = new
                        {
                            _ProductName,
                            _ProductPercent,
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var list = ProductReport.OrderByDescending(a => a.TotalPrice * 100 / totalProduct).Select(a => new IConvertible[]
                {
                            a.ProductName,
                            a.CateName,
                            (a.TotalPrice*100/totalProduct).ToString("0.00"),
                            a.TotalOrder,
                            a.Discount,
                            a.TotalPrice,
                            a.ProductId,
                }).ToList();
                    var PercentT1 = ProductReport.Where(a => (a.TotalPrice * 100 / totalProduct) < 5).Sum(a => a.TotalPrice);
                    ProductReport.Add(new ProductLineReportModalViewModel
                    {
                        ProductName = "Phần còn lại",
                        TotalPrice = PercentT1,
                    });
                    var listPercent = ProductReport.Where(a => ((a.TotalPrice * 100 / totalProduct) < 5) && !a.ProductName.Equals("Phần còn lại"));
                    var _ProductName = ProductReport.Except(listPercent).Select(a => a.ProductName).ToArray();
                    var _ProductPercent = ProductReport.Except(listPercent).Select(a => a.TotalPrice).ToArray();
                    return Json(new
                    {
                        datatable = list,
                        dataChart = new
                        {
                            _ProductName,
                            _ProductPercent,
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CategoryReportAllStore(int brandId, string startTime, string endTime, string checkDeal)
        {
            var storeApi = new StoreApi();
            var dateproductApi = new DateProductApi();
            var orderdetailApi = new OrderDetailApi();
            int i = 0;
            var cateProductApi = new ProductCategoryApi();
            var listAllCatePro = cateProductApi.GetProductCategoriesForReport(brandId).ToList();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listDate = new List<TempSystemProductDateReportItem>();
            var totalProductPrice = new List<DateProduct>();
            var groupApi = new StoreGroupApi();
            var listTotalGroup = new List<Double[]>();
            var table1 = storeApi.GetStoreByBrandId(brandId);
            var table2 = dateproductApi.GetDateProductAllStoreByTimeRange(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate());
            var tablejoin = (from t1 in table1
                             join t2 in table2
                             on t1.ID equals t2.StoreID
                             select new
                             {
                                 t2.Date,
                                 t2.StoreID,
                                 t2.CategoryId_,
                                 t2.Quantity,
                                 t2.TotalAmount,
                                 t2.Discount,
                                 t2.FinalAmount,
                             }).ToList();
            #region List product
            var productCategoryApi = new ProductCategoryApi();
            foreach (var itemP in listAllCatePro)
            {
                ProductReport.Add(new ProductLineReportModalViewModel
                {
                    CateId = itemP.CateID,
                    CateName = itemP.CateName,
                });
            }
            #endregion
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                var listOrderdetaillist = orderdetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //var listDateProductlist = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).ToList();
                var storeinGroups = storeApi.GetStoreByBrandId(brandId).Where(a => a.isAvailable == true).ToList();
                // 2. lấy list store
                var storeList = storeinGroups.ToList();
                foreach (var item in ProductReport)
                {
                    var listDateReport = new List<TempSystemProductDateReportItem>();
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    double listTotalAmount = 0;
                    double listFinalAmount = 0;
                    double listTotalDiscountFee = 0;
                    double listTotalAmountnow = 0;
                    double listFinalAmountnow = 0;
                    double listTotalDiscountFeenow = 0;
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    var listDateProduct2 = tablejoin.Where(a => a.CategoryId_ == item.CateId).ToList();
                    var listOrder = listOrderdetaillist.Where(a => a.Product.CatID == item.CateId);
                    if (endDate == dateNow.GetEndOfDate())
                    {
                        listTotalAmountnow = listOrder.Select(a => a.TotalAmount).Sum();
                        listFinalAmountnow = listOrder.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFeenow = listOrder.Select(a => a.Discount).Sum();

                        listTotalAmount = listDateProduct2.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProduct2.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProduct2.Select(a => a.Discount).Sum();

                        item.TotalOrder = listTotalAmount + listTotalAmountnow;
                        item.TotalPrice = listFinalAmount + listFinalAmountnow;
                        item.Discount = listTotalDiscountFee + listTotalDiscountFeenow;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount + listTotalAmountnow,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount + listFinalAmountnow,
                            });
                        }
                    }
                    else
                    {
                        listTotalAmount = listDateProduct2.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProduct2.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProduct2.Select(a => a.Discount).Sum();
                        item.TotalOrder = listTotalAmount;
                        item.TotalPrice = listFinalAmount;
                        item.Discount = listTotalDiscountFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount,
                            });
                        }
                    }
                    i++;
                }
                var totalProduct = totalProductPrice.Select(a => a.TotalAmount).Sum();
                if (totalProduct == 0)
                {
                    totalProduct = 1;
                }
                if (checkDeal.Equals("beforeDeal"))
                {
                    var list2 = ProductReport.OrderByDescending(a => a.TotalOrder * 100 / totalProduct).Select(a => new IConvertible[]
                {
                            a.CateName,
                            a.CateId,
                           (a.TotalOrder*100/totalProduct).ToString("0.00"),
                            a.TotalOrder,
                            a.Discount,
                            a.TotalPrice,
                }).ToList();
                    var PercentT = ProductReport.Where(a => (a.TotalOrder * 100 / totalProduct) < 5).Sum(a => a.TotalOrder);
                    ProductReport.Add(new ProductLineReportModalViewModel
                    {
                        CateName = "Phần còn lại",
                        TotalOrder = PercentT,
                    });
                    var listPercent = ProductReport.Where(a => ((a.TotalOrder * 100 / totalProduct) < 5) && !a.CateName.Equals("Phần còn lại"));
                    var _ProductName = ProductReport.Except(listPercent).Select(a => a.CateName).ToArray();
                    var _ProductPercent = ProductReport.Except(listPercent).Select(a => a.TotalOrder).ToArray(); return Json(new
                    {
                        datatable = list2,
                        dataChart = new
                        {
                            _ProductName,
                            _ProductPercent,
                            listTotalGroup,
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var list2 = ProductReport.OrderByDescending(a => a.TotalPrice * 100 / totalProduct).Select(a => new IConvertible[]
                {
                            a.CateName,
                            a.CateId,
                           (a.TotalPrice*100/totalProduct).ToString("0.00"),
                            a.TotalOrder,
                            a.Discount,
                            a.TotalPrice,
                }).ToList();
                    var PercentT = ProductReport.Where(a => (a.TotalPrice * 100 / totalProduct) >= 5).Sum(a => a.TotalPrice);
                    ProductReport.Add(new ProductLineReportModalViewModel
                    {
                        CateName = "Phần còn lại",
                        TotalPrice = PercentT,
                    });
                    var listPercent = ProductReport.Where(a => ((a.TotalPrice * 100 / totalProduct) < 5) && !a.CateName.Equals("Phần còn lại"));
                    var _ProductName = ProductReport.Except(listPercent).Select(a => a.CateName).ToArray();
                    var _ProductPercent = ProductReport.Except(listPercent).Select(a => a.TotalPrice).ToArray();
                    return Json(new
                    {
                        datatable = list2,
                        dataChart = new
                        {
                            _ProductName,
                            _ProductPercent,
                            listTotalGroup,
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public List<ExcellistModelView> ExportProductAllStoreTableToExcel(int brandId, string startTime, string endTime, string checkDeal)
        {
            List<ExcellistModelView> list = new List<ExcellistModelView>();
            List<ExcellistModelView> list2 = new List<ExcellistModelView>();
            var storeApi = new StoreApi();
            var dateproductApi = new DateProductApi();
            var orderdetailApi = new OrderDetailApi();
            int i = 0;
            var productApi = new ProductApi();
            var listAllProduct = productApi.GetAllProductByBrand(brandId).ToList();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listDate = new List<TempSystemProductDateReportItem>();
            var totalProductPrice = new List<DateProduct>();
            var groupApi = new StoreGroupApi();
            var listTotalGroup = new List<Double[]>();
            var table1 = storeApi.GetStoreByBrandId(brandId);
            var table2 = dateproductApi.GetDateProductAllStoreByTimeRange(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate());
            var tablejoin = (from t1 in table1
                             join t2 in table2
                             on t1.ID equals t2.StoreID
                             select new
                             {
                                 t2.Date,
                                 t2.StoreID,
                                 t2.ProductId,
                                 t2.Quantity,
                                 t2.TotalAmount,
                                 t2.Discount,
                                 t2.FinalAmount,
                             }).ToList();
            #region List product
            var productCategoryApi = new ProductCategoryApi();
            foreach (var itemP in listAllProduct)
            {

                ProductReport.Add(new ProductLineReportModalViewModel
                {
                    ProductId = itemP.ProductID,
                    ProductName = itemP.ProductName,
                });
            }
            #endregion
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                var listOrderdetaillist = orderdetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //var listDateProductlist = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).ToList();
                foreach (var item in ProductReport)
                {
                    var listDateReport = new List<TempSystemProductDateReportItem>();
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    // 2. lấy list store
                    double listTotalAmount = 0;
                    double listFinalAmount = 0;
                    double listTotalDiscountFee = 0;
                    double listTotalAmountnow = 0;
                    double listFinalAmountnow = 0;
                    double listTotalDiscountFeenow = 0;
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    var listDateProductPro = tablejoin.Where(a => a.ProductId == item.ProductId);
                    var listOrder = listOrderdetaillist.Where(a => a.ProductID == item.ProductId);
                    if (endDate == dateNow.GetEndOfDate())
                    {
                        listTotalAmountnow = listOrder.Select(a => a.TotalAmount).Sum();
                        listFinalAmountnow = listOrder.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFeenow = listOrder.Select(a => a.Discount).Sum();

                        listTotalAmount = listDateProductPro.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProductPro.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProductPro.Select(a => a.Discount).Sum();

                        item.TotalOrder = listTotalAmount + listTotalAmountnow;
                        item.TotalPrice = listFinalAmount + listFinalAmountnow;
                        item.Discount = listTotalDiscountFee + listTotalDiscountFeenow;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount + listTotalAmountnow,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount + listFinalAmountnow,
                            });
                        }
                        list.Add(new ExcellistModelView
                        {
                            Name = item.ProductName,
                            TotalOrder = listTotalAmount + listTotalAmountnow,
                            Discount = listTotalDiscountFee + listTotalDiscountFeenow,
                            TotalPrice = listFinalAmount + listFinalAmountnow,
                        });
                    }
                    else
                    {
                        listTotalAmount = listDateProductPro.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProductPro.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProductPro.Select(a => a.Discount).Sum();
                        item.TotalOrder = listTotalAmount;
                        item.TotalPrice = listFinalAmount;
                        item.Discount = listTotalDiscountFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount,
                            });
                        }
                        list.Add(new ExcellistModelView
                        {
                            Name = item.ProductName,
                            TotalOrder = listTotalAmount,
                            Discount = listTotalDiscountFee,
                            TotalPrice = listFinalAmount,
                        });
                    }
                    i++;
                }
                var totalProduct = totalProductPrice.Select(a => a.TotalAmount).Sum();
                if (totalProduct == 0)
                {
                    totalProduct = 1;
                }
                #region kết quả truoc giam gia
                if (checkDeal.Equals("beforeDeal"))
                {
                    foreach (var itemc in list)
                    {
                        itemc.Percent = (itemc.TotalOrder * 100 / totalProduct);
                    }
                }
                else
                {
                    foreach (var itemc in list)
                    {
                        itemc.Percent = (itemc.TotalPrice * 100 / totalProduct);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
            }
            return list;
        }
        public List<ExcellistModelView> ExportCategoryAllStoreTableToExcel(int brandId, string startTime, string endTime, string checkDeal)
        {
            List<ExcellistModelView> list = new List<ExcellistModelView>();
            List<ExcellistModelView> list2 = new List<ExcellistModelView>();
            var storeApi = new StoreApi();
            var dateproductApi = new DateProductApi();
            var orderdetailApi = new OrderDetailApi();
            int i = 0;
            var cateProductApi = new ProductCategoryApi();
            var listAllCatePro = cateProductApi.GetProductCategoriesForReport(brandId).ToList();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listDate = new List<TempSystemProductDateReportItem>();
            var totalProductPrice = new List<DateProduct>();
            var groupApi = new StoreGroupApi();
            var listTotalGroup = new List<Double[]>();
            var table1 = storeApi.GetStoreByBrandId(brandId);
            var table2 = dateproductApi.GetDateProductAllStoreByTimeRange(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate());
            var tablejoin = (from t1 in table1
                             join t2 in table2
                             on t1.ID equals t2.StoreID
                             select new
                             {
                                 t2.Date,
                                 t2.StoreID,
                                 t2.CategoryId_,
                                 t2.Quantity,
                                 t2.TotalAmount,
                                 t2.Discount,
                                 t2.FinalAmount,
                             }).ToList();
            #region List product
            var productCategoryApi = new ProductCategoryApi();
            foreach (var itemP in listAllCatePro)
            {
                ProductReport.Add(new ProductLineReportModalViewModel
                {
                    CateId = itemP.CateID,
                    CateName = itemP.CateName,
                });
            }
            #endregion
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                var listOrderdetaillist = orderdetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //var listDateProductlist = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).ToList();
                var storeinGroups = storeApi.GetStoreByBrandId(brandId).Where(a => a.isAvailable == true).ToList();
                // 2. lấy list store
                var storeList = storeinGroups.ToList();
                foreach (var item in ProductReport)
                {
                    var listDateReport = new List<TempSystemProductDateReportItem>();
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    double listTotalAmount = 0;
                    double listFinalAmount = 0;
                    double listTotalDiscountFee = 0;
                    double listTotalAmountnow = 0;
                    double listFinalAmountnow = 0;
                    double listTotalDiscountFeenow = 0;
                    // 3. duyệt ngày theo store -> lấy dc doanh thu của ngày theo tất cả cửa hàng
                    var listDateProduct2 = tablejoin.Where(a => a.CategoryId_ == item.CateId).ToList();
                    var listOrder = listOrderdetaillist.Where(a => a.Product.CatID == item.CateId);
                    if (endDate == dateNow.GetEndOfDate())
                    {
                        listTotalAmountnow = listOrder.Select(a => a.TotalAmount).Sum();
                        listFinalAmountnow = listOrder.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFeenow = listOrder.Select(a => a.Discount).Sum();

                        listTotalAmount = listDateProduct2.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProduct2.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProduct2.Select(a => a.Discount).Sum();

                        item.TotalOrder = listTotalAmount + listTotalAmountnow;
                        item.TotalPrice = listFinalAmount + listFinalAmountnow;
                        item.Discount = listTotalDiscountFee + listTotalDiscountFeenow;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount + listTotalAmountnow,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount + listFinalAmountnow,
                            });
                        }
                        list.Add(new ExcellistModelView
                        {
                            Name = item.CateName,
                            TotalOrder = listTotalAmount + listTotalAmountnow,
                            Discount = listTotalDiscountFee + listTotalDiscountFeenow,
                            TotalPrice = listFinalAmount + listFinalAmountnow,
                        });
                    }
                    else
                    {
                        listTotalAmount = listDateProduct2.Select(a => a.TotalAmount).Sum();
                        listFinalAmount = listDateProduct2.Select(a => a.FinalAmount).Sum();
                        listTotalDiscountFee = listDateProduct2.Select(a => a.Discount).Sum();
                        item.TotalOrder = listTotalAmount;
                        item.TotalPrice = listFinalAmount;
                        item.Discount = listTotalDiscountFee;
                        if (checkDeal.Equals("beforeDeal"))
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listTotalAmount,
                            });
                        }
                        else
                        {
                            totalProductPrice.Add(new DateProduct()
                            {
                                TotalAmount = listFinalAmount,
                            });
                        }
                        list.Add(new ExcellistModelView
                        {
                            Name = item.CateName,
                            TotalOrder = listTotalAmount,
                            Discount = listTotalDiscountFee,
                            TotalPrice = listFinalAmount,
                        });
                    }
                    i++;
                }
                var totalProduct = totalProductPrice.Select(a => a.TotalAmount).Sum();
                if (totalProduct == 0)
                {
                    totalProduct = 1;
                }
                #region kết quả truoc giam gia
                if (checkDeal.Equals("beforeDeal"))
                {
                    foreach (var itemc in list)
                    {
                        itemc.Percent = (itemc.TotalOrder * 100 / totalProduct);
                    }
                }
                else
                {
                    foreach (var itemc in list)
                    {
                        itemc.Percent = (itemc.TotalPrice * 100 / totalProduct);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
            }
            return list;
        }
        public ActionResult ReportProductAllStoreExportExcelEPPlus(int brandId, string startTime, string endTime, string checkvalue, string checkDeal)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo doanh thu sản phẩm");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var list = new List<ExcellistModelView>();
                if (checkvalue.Equals("loadProducts"))
                {
                    list = ExportProductAllStoreTableToExcel(brandId, startTime, endTime, checkDeal);
                }
                else
                {
                    list = ExportCategoryAllStoreTableToExcel(brandId, startTime, endTime, checkDeal);
                }
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Sản phẩm/ Nhóm sản phẩm";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tỉ trọng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tổng tiền trước giảm giá";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giảm giá";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Tổng tiền sau giảm giá";

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
                var listtotal = list.OrderByDescending(a => a.Percent);
                foreach (var data in listtotal)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.Name;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Percent.ToString("0.00");
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrder;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Discount;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalPrice;
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
                startTime = startTime.Replace('/', '-');

                endTime = endTime.Replace('/', '-');
                var fileDownloadName = "Báo cáo doanh thu sản phẩm từ ngày " + startTime + " - " + endTime + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }

        #region ProductList Report
        public ActionResult CategoryProductLineReportOneStore(int brandId, string startTime, string endTime, int storeIdReport, int selecteditem)
        {
            var dateproductApi = new DateProductApi();
            var orderDetailApi = new OrderDetailApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listTotalOrder = new List<Double[]>();
            var listTotalPrice = new List<Double[]>();
            var productCategoryApi = new ProductCategoryApi();
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                //Get list OrderDetai by DateNow
                var listOrderdetail = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //Get list DateProduct by Store
                var listDateProduct = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.StoreID == storeIdReport && a.CategoryId_ == selecteditem).ToList();
                for (var d = startDate; startDate <= endDate; d.AddDays(1))
                {

                    if (startDate == dateNow.GetStartOfDate())
                    {
                        var listOrder = listOrderdetail.Where(a => a.StoreId == storeIdReport && a.Product.CatID == selecteditem);
                        var quantity = listOrder.Sum(a => a.Quantity);
                        var totalAmount = listOrder.Sum(a => a.TotalAmount);
                        var finalAmount = listOrder.Sum(a => a.FinalAmount);
                        var discountFee = listOrder.Sum(a => a.Discount);
                        ProductReport.Add(new ProductLineReportModalViewModel()
                        {
                            StartTime = startDate.ToString("dd/MM/yyyy"),
                            ProductId = selecteditem,
                            Quantity = quantity,
                            TotalOrder = totalAmount,
                            Discount = discountFee,
                            TotalPrice = finalAmount,
                        });

                    }
                    else
                    {
                        var listdate = listDateProduct.Where(a => a.Date.GetStartOfDate() == startDate.Date);
                        var quantity = listdate.Sum(a => a.Quantity);
                        var totalAmount = listdate.Sum(a => a.TotalAmount);
                        var finalAmount = listdate.Sum(a => a.FinalAmount);
                        var discountFee = listdate.Sum(a => a.Discount);
                        ProductReport.Add(new ProductLineReportModalViewModel()
                        {
                            StartTime = startDate.ToString("dd/MM/yyyy"),
                            CateId = selecteditem,
                            Quantity = quantity,
                            TotalOrder = totalAmount,
                            Discount = discountFee,
                            TotalPrice = finalAmount,
                        });
                    }

                    startDate = startDate.AddDays(1);
                }
                //Get value Chart
                listTotalOrder.Add(ProductReport.Select(a => a.TotalOrder).ToArray());
                listTotalPrice.Add(ProductReport.Select(a => a.TotalPrice).ToArray());
                #region Xuat Json
                var list = ProductReport.Select(a => new IConvertible[]
            {
                        a.StartTime,
                        a.CateId,
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Quantity),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalOrder),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice),
                        a.ProductId,
            }).ToList();
                var _ProductName = productCategoryApi.GetProductCategoryEntityById(selecteditem).CateName.ToString();
                var _Day = ProductReport.Select(a => a.StartTime).ToArray();
                return Json(new
                {
                    datatable = list,
                    dataChart = new
                    {
                        _ProductName,
                        _Day,
                        listTotalOrder,
                        listTotalPrice,
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            catch (Exception ex)
            {
                return Json(new
                { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        public ActionResult ProductLineReportOneStore(int brandId, string startTime, string endTime, int storeIdReport, int selecteditem)
        {
            var dateproductApi = new DateProductApi();
            var orderDetailApi = new OrderDetailApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listTotalOrder = new List<Double[]>();
            var listTotalPrice = new List<Double[]>();
            var productApi = new ProductApi();
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                //Get list OrderDetai by DateNow
                var listOrderdetail = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //Get list DateProduct by Store
                var listDateProduct = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.StoreID == storeIdReport && a.ProductId == selecteditem).ToList();
                for (var d = startDate; startDate <= endDate; d.AddDays(1))
                {
                    if (startDate == dateNow.GetStartOfDate())
                    {
                        var listOrder = listOrderdetail.Where(a => a.StoreId == storeIdReport && a.ProductID == selecteditem);
                        var quantity = listOrder.Sum(a => a.Quantity);
                        var totalAmount = listOrder.Sum(a => a.TotalAmount);
                        var finalAmount = listOrder.Sum(a => a.FinalAmount);
                        var discountFee = listOrder.Sum(a => a.Discount);
                        ProductReport.Add(new ProductLineReportModalViewModel()
                        {
                            StartTime = startDate.ToString("dd/MM/yyyy"),
                            ProductId = selecteditem,
                            Quantity = quantity,
                            TotalOrder = totalAmount,
                            Discount = discountFee,
                            TotalPrice = finalAmount,
                        });

                    }
                    else
                    {
                        var listdate = listDateProduct.Where(a => a.Date.GetStartOfDate() == startDate.Date);
                        var quantity = listdate.Sum(a => a.Quantity);
                        var totalAmount = listdate.Sum(a => a.TotalAmount);
                        var finalAmount = listdate.Sum(a => a.FinalAmount);
                        var discountFee = listdate.Sum(a => a.Discount);
                        ProductReport.Add(new ProductLineReportModalViewModel()
                        {
                            StartTime = startDate.ToString("dd/MM/yyyy"),
                            ProductId = selecteditem,
                            Quantity = quantity,
                            TotalOrder = totalAmount,
                            Discount = discountFee,
                            TotalPrice = finalAmount,
                        });
                    }
                    startDate = startDate.AddDays(1);
                }
                //Get value Chart
                listTotalOrder.Add(ProductReport.Select(a => a.TotalOrder).ToArray());
                listTotalPrice.Add(ProductReport.Select(a => a.TotalPrice).ToArray());
                #region Xuat Json
                var list = ProductReport.Select(a => new IConvertible[]
            {
                        a.StartTime,
                        a.ProductId,
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Quantity),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalOrder),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice),
                        a.ProductId,
            }).ToList();
                var _ProductName = productApi.GetProductById(selecteditem).ProductName.ToString();
                var _Day = ProductReport.Select(a => a.StartTime).ToArray();
                return Json(new
                {
                    datatable = list,
                    dataChart = new
                    {
                        _ProductName,
                        _Day,
                        listTotalOrder,
                        listTotalPrice,
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            catch (Exception ex)
            {
                return Json(new
                { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public List<dynamic> ExportProductLineOneStoreTableToExcel(int brandId, string startTime, string endTime, int storeIdReport, int selecteditem)
        {
            List<dynamic> list = new List<dynamic>();
            var dateproductApi = new DateProductApi();
            var orderDetailApi = new OrderDetailApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listTotalOrder = new List<Double[]>();
            var listTotalPrice = new List<Double[]>();
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var dateNow = Utils.GetCurrentDateTime();
            //Get list OrderDetai by DateNow
            var listOrderdetail = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
            //Get list DateProduct by Store
            var listDateProduct = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.StoreID == storeIdReport).ToList();
            //Get list DateProduct by ProductId in listDateProduct
            var listDateProductPro = listDateProduct.Where(a => a.ProductId == selecteditem).ToList();
            for (var d = startDate; startDate <= endDate; d.AddDays(1))
            {

                if (startDate == dateNow.GetStartOfDate())
                {
                    var listOrder = listOrderdetail.Where(a => a.StoreId == storeIdReport && a.ProductID == selecteditem);
                    var quantity = listOrder.Sum(a => a.Quantity);
                    var totalAmount = listOrder.Sum(a => a.TotalAmount);
                    var finalAmount = listOrder.Sum(a => a.FinalAmount);
                    var discountFee = listOrder.Sum(a => a.Discount);
                    list.Add(new ProductLineReportModalViewModel()
                    {
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        ProductId = selecteditem,
                        Quantity = quantity,
                        TotalOrder = totalAmount,
                        Discount = discountFee,
                        TotalPrice = finalAmount,
                    });

                }
                else
                {
                    var listdate = listDateProductPro.Where(a => a.Date.GetStartOfDate() == startDate.Date);
                    var quantity = listdate.Sum(a => a.Quantity);
                    var totalAmount = listdate.Sum(a => a.TotalAmount);
                    var finalAmount = listdate.Sum(a => a.FinalAmount);
                    var discountFee = listdate.Sum(a => a.Discount);
                    list.Add(new ProductLineReportModalViewModel()
                    {
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        CateId = selecteditem,
                        Quantity = quantity,
                        TotalOrder = totalAmount,
                        Discount = discountFee,
                        TotalPrice = finalAmount,
                    });
                }
                startDate = startDate.AddDays(1);
            }
            return list;
        }
        public List<dynamic> ExportCategoryProductLineOneStoreTableToExcel(int brandId, string startTime, string endTime, int storeIdReport, int selecteditem)
        {
            List<dynamic> list = new List<dynamic>();
            var dateproductApi = new DateProductApi();
            var orderDetailApi = new OrderDetailApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listTotalOrder = new List<Double[]>();
            var listTotalPrice = new List<Double[]>();
            var productCategoryApi = new ProductCategoryApi();
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var dateNow = Utils.GetCurrentDateTime();
            //Get list OrderDetai by DateNow
            var listOrderdetail = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
            //Get list DateProduct by Store
            var listDateProduct = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.StoreID == storeIdReport).ToList();
            //Get list DateProduct by CateID in listDateProduct
            var listDateProductPro = listDateProduct.Where(a => a.CategoryId_ == selecteditem).ToList();
            for (var d = startDate; startDate <= endDate; d.AddDays(1))
            {

                if (startDate == dateNow.GetStartOfDate())
                {
                    var listOrder = listOrderdetail.Where(a => a.StoreId == storeIdReport && a.Product.CatID == selecteditem);
                    var quantity = listOrder.Sum(a => a.Quantity);
                    var totalAmount = listOrder.Sum(a => a.TotalAmount);
                    var finalAmount = listOrder.Sum(a => a.FinalAmount);
                    var discountFee = listOrder.Sum(a => a.Discount);
                    list.Add(new ProductLineReportModalViewModel()
                    {
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        ProductId = selecteditem,
                        Quantity = quantity,
                        TotalOrder = totalAmount,
                        Discount = discountFee,
                        TotalPrice = finalAmount,
                    });

                }
                else
                {
                    var listdate = listDateProductPro.Where(a => a.Date.GetStartOfDate() == startDate.Date);
                    var quantity = listdate.Sum(a => a.Quantity);
                    var totalAmount = listdate.Sum(a => a.TotalAmount);
                    var finalAmount = listdate.Sum(a => a.FinalAmount);
                    var discountFee = listdate.Sum(a => a.Discount);
                    list.Add(new ProductLineReportModalViewModel()
                    {
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        CateId = selecteditem,
                        Quantity = quantity,
                        TotalOrder = totalAmount,
                        Discount = discountFee,
                        TotalPrice = finalAmount,
                    });
                }
                startDate = startDate.AddDays(1);
            }
            return list;
        }
        public ActionResult ReportProductLineOneStoreExportExcelEPPlus(int brandId, string startTime, string endTime, int storeIdReport, int selecteditem, string checkvalue)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo diễn tiến sản phẩm");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var list = new List<dynamic>();
                if (checkvalue == "product")
                {
                    list = ExportProductLineOneStoreTableToExcel(brandId, startTime, endTime, storeIdReport, selecteditem);
                }
                else
                {
                    list = ExportCategoryProductLineOneStoreTableToExcel(brandId, startTime, endTime, storeIdReport, selecteditem);
                }

                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu trước giảm giá";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giảm giá";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Doanh thu";

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
                foreach (var data in list)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.StartTime;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrder;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Discount;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalPrice;
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
                startTime = startTime.Replace('/', '-');

                endTime = endTime.Replace('/', '-');
                var fileDownloadName = "Báo cáo diễn tiến sản phẩm từ ngày " + startTime + " - " + endTime + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }

        public ActionResult ProductLineReportStoreGroup(int brandId, string startTime, string endTime, int storeGroupIdReport, int selecteditem)
        {
            var dateproductApi = new DateProductApi();
            var orderDetailApi = new OrderDetailApi();
            var storeApi = new StoreApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listTotalOrder = new List<Double[]>();
            var listTotalPrice = new List<Double[]>();
            var productApi = new ProductApi();
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                //Get list OrderDetai by DateNow
                var listOrderdetail = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //Get list DateProduct by Brand
                var listDateProduct = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.ProductId == selecteditem).ToList();
                var storeinGroups = storeApi.GetStoreByGroupId(storeGroupIdReport).ToList();
                // 2. lay list store
                var storeList = storeinGroups.ToList();
                // 3. duy?t ngày theo store -> l?y dc doanh thu c?a ngày theo t?t c? c?a hàng
                for (var d = startDate; startDate <= endDate; d.AddDays(1))
                {
                    int totalDateQuantity = 0;
                    double totalDateAmount = 0;
                    double finalDateAmount = 0;
                    double discountDateFee = 0;
                    //Duyet theo Store lay doanh thu San pham
                    foreach (var store in storeList)
                    {
                        if (startDate == dateNow.GetStartOfDate())
                        {
                            var listOrder = listOrderdetail.Where(a => a.StoreId == store.ID && a.ProductID == selecteditem);
                            var quantity = listOrder.Sum(a => a.Quantity);
                            var totalAmount = listOrder.Sum(a => a.TotalAmount);
                            var finalAmount = listOrder.Sum(a => a.FinalAmount);
                            var discountFee = listOrder.Sum(a => a.Discount);
                            totalDateAmount += totalAmount;
                            finalDateAmount += finalAmount;
                            discountDateFee += discountFee;
                            totalDateQuantity += quantity;
                        }
                        else
                        {
                            var listdate = listDateProduct.Where(a => a.Date.Date == startDate.Date && a.StoreID == store.ID).ToList();
                            var quantity = listdate.Sum(a => a.Quantity);
                            var totalAmount = listdate.Sum(a => a.TotalAmount);
                            var finalAmount = listdate.Sum(a => a.FinalAmount);
                            var discountFee = listdate.Sum(a => a.Discount);
                            totalDateAmount += totalAmount;
                            finalDateAmount += finalAmount;
                            discountDateFee += discountFee;
                            totalDateQuantity += quantity;
                        }
                    }
                    ProductReport.Add(new ProductLineReportModalViewModel()
                    {
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        ProductId = selecteditem,
                        Quantity = totalDateQuantity,
                        TotalOrder = totalDateAmount,
                        Discount = discountDateFee,
                        TotalPrice = finalDateAmount,
                    });
                    startDate = startDate.AddDays(1);
                }
                listTotalOrder.Add(ProductReport.Select(a => a.TotalOrder).ToArray());
                listTotalPrice.Add(ProductReport.Select(a => a.TotalPrice).ToArray());
                //}
                #region Xuat Json
                var list = ProductReport.Select(a => new IConvertible[]
            {
                        a.StartTime,
                        a.ProductId,
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Quantity),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalOrder),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice),
                        a.ProductId,
            }).ToList();
                var _ProductName = productApi.GetProductById(selecteditem).ProductName.ToString();
                var _Day = ProductReport.Select(a => a.StartTime).ToArray();
                return Json(new
                {
                    datatable = list,
                    dataChart = new
                    {
                        _ProductName,
                        _Day,
                        listTotalOrder,
                        listTotalPrice,
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            catch (Exception ex)
            {
                return Json(new
                { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CategoryProductLineReportStoreGroup(int brandId, string startTime, string endTime, int storeGroupIdReport, int selecteditem)
        {
            var dateproductApi = new DateProductApi();
            var orderDetailApi = new OrderDetailApi();
            var storeApi = new StoreApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listTotalOrder = new List<Double[]>();
            var listTotalPrice = new List<Double[]>();
            var productApi = new ProductApi();
            var productCategoryApi = new ProductCategoryApi();
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                //Get list OrderDetai by DateNow
                var listOrderdetail = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //Get list DateProduct by Brand
                var listDateProduct = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.CategoryId_ == selecteditem).ToList();
                var storeinGroups = storeApi.GetStoreByGroupId(storeGroupIdReport).ToList();
                // 2. l?y list store
                var storeList = storeinGroups.ToList();
                // 3. duy?t ngày theo store -> l?y dc doanh thu c?a ngày theo t?t c? c?a hàng
                //Get list DateProduct by CateID in listDateProduct
                var listDateProduct2 = listDateProduct.Where(a => a.CategoryId_ == selecteditem).ToList();
                for (var d = startDate; startDate <= endDate; d.AddDays(1))
                {
                    int totalDateQuantity = 0;
                    double totalDateAmount = 0;
                    double finalDateAmount = 0;
                    double discountDateFee = 0;
                    //Duyet theo Store lay doanh thu Nhom san pham
                    foreach (var store in storeList)
                    {
                        if (startDate == dateNow.GetStartOfDate())
                        {
                            var listOrder = listOrderdetail.Where(a => a.StoreId == store.ID && a.Product.CatID == selecteditem);
                            var quantity = listOrder.Sum(a => a.Quantity);
                            var totalAmount = listOrder.Sum(a => a.TotalAmount);
                            var finalAmount = listOrder.Sum(a => a.FinalAmount);
                            var discountFee = listOrder.Sum(a => a.Discount);
                            totalDateAmount += totalAmount;
                            finalDateAmount += finalAmount;
                            discountDateFee += discountFee;
                            totalDateQuantity += quantity;
                        }
                        else
                        {
                            var listdate = listDateProduct.Where(a => a.Date.Date == startDate.Date && a.StoreID == store.ID).ToList();
                            var quantity = listdate.Sum(a => a.Quantity);
                            var totalAmount = listdate.Sum(a => a.TotalAmount);
                            var finalAmount = listdate.Sum(a => a.FinalAmount);
                            var discountFee = listdate.Sum(a => a.Discount);
                            totalDateAmount += totalAmount;
                            finalDateAmount += finalAmount;
                            discountDateFee += discountFee;
                            totalDateQuantity += quantity;
                        }
                    }
                    ProductReport.Add(new ProductLineReportModalViewModel()
                    {
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        ProductId = selecteditem,
                        Quantity = totalDateQuantity,
                        TotalOrder = totalDateAmount,
                        Discount = discountDateFee,
                        TotalPrice = finalDateAmount,
                    });
                    startDate = startDate.AddDays(1);
                }
                listTotalOrder.Add(ProductReport.Select(a => a.TotalOrder).ToArray());
                listTotalPrice.Add(ProductReport.Select(a => a.TotalPrice).ToArray());
                #region Xuat Json
                var list = ProductReport.Select(a => new IConvertible[]
                    {
                        a.StartTime,
                        a.ProductId,
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Quantity),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalOrder),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice),
                        a.ProductId,
                    }).ToList();
                var _ProductName = productCategoryApi.GetProductCategoryEntityById(selecteditem).CateName.ToString();
                var _Day = ProductReport.Select(a => a.StartTime).ToArray();
                return Json(new
                {
                    datatable = list,
                    dataChart = new
                    {
                        _ProductName,
                        _Day,
                        listTotalOrder,
                        listTotalPrice,
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            catch (Exception ex)
            {
                return Json(new
                { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public List<dynamic> ExportProductLineStoreGroupTableToExcel(int brandId, string startTime, string endTime, int storeGroupIdReport, int selecteditem)
        {
            List<dynamic> list = new List<dynamic>();
            var dateproductApi = new DateProductApi();
            var orderDetailApi = new OrderDetailApi();
            var storeApi = new StoreApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listTotalOrder = new List<Double[]>();
            var listTotalPrice = new List<Double[]>();
            var productApi = new ProductApi();
            var productCategoryApi = new ProductCategoryApi();

            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var dateNow = Utils.GetCurrentDateTime();

            var listOrderdetail = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
            var listDateProduct = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.ProductId == selecteditem).ToList();
            var storeinGroups = storeApi.GetStoreByGroupId(storeGroupIdReport).ToList();
            // 2. l?y list store
            var storeList = storeinGroups.ToList();
            for (var d = startDate; startDate <= endDate; d.AddDays(1))
            {
                int totalDateQuantity = 0;
                double totalDateAmount = 0;
                double finalDateAmount = 0;
                double discountDateFee = 0;
                foreach (var store in storeList)
                {

                    if (startDate == dateNow.GetStartOfDate())
                    {
                        var listOrder = listOrderdetail.Where(a => a.StoreId == store.ID && a.ProductID == selecteditem);
                        var quantity = listOrder.Sum(a => a.Quantity);
                        var totalAmount = listOrder.Sum(a => a.TotalAmount);
                        var finalAmount = listOrder.Sum(a => a.FinalAmount);
                        var discountFee = listOrder.Sum(a => a.Discount);
                        totalDateAmount += totalAmount;
                        finalDateAmount += finalAmount;
                        discountDateFee += discountFee;
                        totalDateQuantity += quantity;
                    }
                    else
                    {
                        var listdate = listDateProduct.Where(a => a.Date.Date == startDate.Date && a.StoreID == store.ID).ToList();
                        var quantity = listdate.Sum(a => a.Quantity);
                        var totalAmount = listdate.Sum(a => a.TotalAmount);
                        var finalAmount = listdate.Sum(a => a.FinalAmount);
                        var discountFee = listdate.Sum(a => a.Discount);
                        totalDateAmount += totalAmount;
                        finalDateAmount += finalAmount;
                        discountDateFee += discountFee;
                        totalDateQuantity += quantity;
                    }
                }
                list.Add(new ProductLineReportModalViewModel()
                {
                    StartTime = startDate.ToString("dd/MM/yyyy"),
                    ProductId = selecteditem,
                    Quantity = totalDateQuantity,
                    TotalOrder = totalDateAmount,
                    Discount = discountDateFee,
                    TotalPrice = finalDateAmount,
                });
                startDate = startDate.AddDays(1);
            }
            return list;
        }
        public List<dynamic> ExportCategoryProductLineStoreGroupTableToExcel(int brandId, string startTime, string endTime, int storeGroupIdReport, int selecteditem)
        {
            List<dynamic> list = new List<dynamic>();
            var dateproductApi = new DateProductApi();
            var orderDetailApi = new OrderDetailApi();
            var storeApi = new StoreApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listTotalOrder = new List<Double[]>();
            var listTotalPrice = new List<Double[]>();
            var productCategoryApi = new ProductCategoryApi();
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var tempStartDate = startDate;
            var dateNow = Utils.GetCurrentDateTime();
            var listOrderdetail = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
            var listDateProduct = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).ToList();
            var storeinGroups = storeApi.GetStoreByGroupId(storeGroupIdReport).ToList();
            // 2. l?y list store
            var storeList = storeinGroups.ToList();
            var listDateProduct2 = listDateProduct.Where(a => a.CategoryId_ == selecteditem).ToList();
            // 3. duy?t ngày theo store -> l?y dc doanh thu c?a ngày theo t?t c? c?a hàng
            for (var d = startDate; startDate <= endDate; d.AddDays(1))
            {
                int totalDateQuantity = 0;
                double totalDateAmount = 0;
                double finalDateAmount = 0;
                double discountDateFee = 0;
                foreach (var store in storeList)
                {
                    var listDateProduct3 = listDateProduct2.Where(a => a.StoreID == store.ID).ToList();
                    if (startDate == dateNow.GetStartOfDate())
                    {
                        var listOrder = listOrderdetail.Where(a => a.StoreId == store.ID && a.Product.CatID == selecteditem);
                        var quantity = listOrder.Sum(a => a.Quantity);
                        var totalAmount = listOrder.Sum(a => a.TotalAmount);
                        var finalAmount = listOrder.Sum(a => a.FinalAmount);
                        var discountFee = listOrder.Sum(a => a.Discount);
                        totalDateAmount += totalAmount;
                        finalDateAmount += finalAmount;
                        discountDateFee += discountFee;
                        totalDateQuantity += quantity;
                    }
                    else
                    {
                        var listdate = listDateProduct3.Where(a => a.Date.Date == startDate.Date).ToList();
                        var quantity = listdate.Sum(a => a.Quantity);
                        var totalAmount = listdate.Sum(a => a.TotalAmount);
                        var finalAmount = listdate.Sum(a => a.FinalAmount);
                        var discountFee = listdate.Sum(a => a.Discount);
                        totalDateAmount += totalAmount;
                        finalDateAmount += finalAmount;
                        discountDateFee += discountFee;
                        totalDateQuantity += quantity;
                    }
                }
                list.Add(new ProductLineReportModalViewModel()
                {
                    StartTime = startDate.ToString("dd/MM/yyyy"),
                    ProductId = selecteditem,
                    Quantity = totalDateQuantity,
                    TotalOrder = totalDateAmount,
                    Discount = discountDateFee,
                    TotalPrice = finalDateAmount,
                });
                startDate = startDate.AddDays(1);
            }
            return list;
        }
        public ActionResult ReportProductLineStoreGroupExportExcelEPPlus(int brandId, string startTime, string endTime, int storeGroupIdReport, int selecteditem, string checkvalue)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo diễn tiến sản phẩm");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var list = new List<dynamic>();
                if (checkvalue == "product")
                {
                    list = ExportProductLineStoreGroupTableToExcel(brandId, startTime, endTime, storeGroupIdReport, selecteditem);
                }
                else
                {
                    list = ExportCategoryProductLineStoreGroupTableToExcel(brandId, startTime, endTime, storeGroupIdReport, selecteditem);
                }
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu trước giảm giá";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giảm giá";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Doanh thu";

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
                foreach (var data in list)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.StartTime;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrder;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Discount;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalPrice;
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
                startTime = startTime.Replace('/', '-');

                endTime = endTime.Replace('/', '-');
                var fileDownloadName = "Báo cáo diễn tiến sản phẩm từ ngày " + startTime + " - " + endTime + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }

        public ActionResult ProductLineReportAllStore(int brandId, string startTime, string endTime, int selecteditem)
        {
            var dateproductApi = new DateProductApi();
            var orderDetailApi = new OrderDetailApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listTotalOrder = new List<Double[]>();
            var listTotalPrice = new List<Double[]>();
            var productApi = new ProductApi();
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                //Get list OrderDetai by DateNow
                var listOrderdetail = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //Get list DateProduct by AllStore
                var listDateProduct = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.ProductId == selecteditem).ToList();
                for (var d = startDate; startDate <= endDate; d.AddDays(1))
                {
                    if (startDate == dateNow.GetStartOfDate())
                    {
                        var listOrder = listOrderdetail.Where(a => a.ProductID == selecteditem);
                        var quantity = listOrder.Sum(a => a.Quantity);
                        var totalAmount = listOrder.Sum(a => a.TotalAmount);
                        var finalAmount = listOrder.Sum(a => a.FinalAmount);
                        var discountFee = listOrder.Sum(a => a.Discount);
                        ProductReport.Add(new ProductLineReportModalViewModel()
                        {
                            StartTime = startDate.ToString("dd/MM/yyyy"),
                            ProductId = selecteditem,
                            Quantity = quantity,
                            TotalOrder = totalAmount,
                            Discount = discountFee,
                            TotalPrice = finalAmount,
                        });

                    }
                    else
                    {
                        var listdate = listDateProduct.Where(a => a.Date.GetStartOfDate() == startDate.Date);
                        var quantity = listdate.Sum(a => a.Quantity);
                        var totalAmount = listdate.Sum(a => a.TotalAmount);
                        var finalAmount = listdate.Sum(a => a.FinalAmount);
                        var discountFee = listdate.Sum(a => a.Discount);
                        ProductReport.Add(new ProductLineReportModalViewModel()
                        {
                            StartTime = startDate.ToString("dd/MM/yyyy"),
                            ProductId = selecteditem,
                            Quantity = quantity,
                            TotalOrder = totalAmount,
                            Discount = discountFee,
                            TotalPrice = finalAmount,
                        });
                    }
                    startDate = startDate.AddDays(1);
                }
                listTotalOrder.Add(ProductReport.Select(a => a.TotalOrder).ToArray());
                listTotalPrice.Add(ProductReport.Select(a => a.TotalPrice).ToArray());
                //}
                #region Xuat Json
                var list = ProductReport.Select(a => new IConvertible[]
            {
                        a.StartTime,
                        a.ProductId,
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Quantity),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalOrder),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice),
                        a.ProductId,
            }).ToList();
                var _ProductName = productApi.GetProductById(selecteditem).ProductName.ToString();
                var _Day = ProductReport.Select(a => a.StartTime).ToArray();
                return Json(new
                {
                    datatable = list,
                    dataChart = new
                    {
                        _ProductName,
                        _Day,
                        listTotalOrder,
                        listTotalPrice,
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            catch (Exception ex)
            {
                return Json(new
                { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CategoryProductLineReportAllStore(int brandId, string startTime, string endTime, int selecteditem)
        {
            var dateproductApi = new DateProductApi();
            var storeApi = new StoreApi();
            var orderDetailApi = new OrderDetailApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listTotalOrder = new List<Double[]>();
            var listTotalPrice = new List<Double[]>();
            var productCategoryApi = new ProductCategoryApi();
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var table1 = storeApi.GetStoreByBrandId(brandId);
            var table2 = dateproductApi.GetDateProductAllStoreByTimeRange(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate());
            var tablejoin = (from t1 in table1
                            join t2 in table2
                            on t1.ID equals t2.StoreID
                            where t2.CategoryId_ == selecteditem
                             select new
                            {
                                t2.Date,
                                t2.CategoryId_,
                                t2.Quantity,
                                t2.TotalAmount,
                                t2.Discount,
                                t2.FinalAmount,
                            }).ToList();
            try
            {
                var dateNow = Utils.GetCurrentDateTime();
                //Get list OrderDetai by DateNow
                var listOrderdetail = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
                //Get list DateProduct by AllStore
                //var listDateProduct = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.CategoryId_ == selecteditem).ToList();
                for (var d = startDate; startDate <= endDate; d.AddDays(1))
                {
                    if (startDate == dateNow.GetStartOfDate())
                    {
                        var listOrder = listOrderdetail.Where(a => a.Product.CatID == selecteditem);
                        var quantity = listOrder.Sum(a => a.Quantity);
                        var totalAmount = listOrder.Sum(a => a.TotalAmount);
                        var finalAmount = listOrder.Sum(a => a.FinalAmount);
                        var discountFee = listOrder.Sum(a => a.Discount);
                        ProductReport.Add(new ProductLineReportModalViewModel()
                        {
                            StartTime = startDate.ToString("dd/MM/yyyy"),
                            ProductId = selecteditem,
                            Quantity = quantity,
                            TotalOrder = totalAmount,
                            Discount = discountFee,
                            TotalPrice = finalAmount,
                        });

                    }
                    else
                    {
                        var listdate = tablejoin.Where(a => a.Date.GetStartOfDate() == startDate.Date).ToList();
                        var quantity = listdate.Sum(a => a.Quantity);
                        var totalAmount = listdate.Sum(a => a.TotalAmount);
                        var finalAmount = listdate.Sum(a => a.FinalAmount);
                        var discountFee = listdate.Sum(a => a.Discount);
                        ProductReport.Add(new ProductLineReportModalViewModel()
                        {
                            StartTime = startDate.ToString("dd/MM/yyyy"),
                            CateId = selecteditem,
                            Quantity = quantity,
                            TotalOrder = totalAmount,
                            Discount = discountFee,
                            TotalPrice = finalAmount,
                        });
                    }

                    startDate = startDate.AddDays(1);
                }
                listTotalOrder.Add(ProductReport.Select(a => a.TotalOrder).ToArray());
                listTotalPrice.Add(ProductReport.Select(a => a.TotalPrice).ToArray());
                #region Xuat Json
                var list = ProductReport.Select(a => new IConvertible[]
            {
                        a.StartTime,
                        a.CateId,
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Quantity),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalOrder),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.Discount),
                        string.Format(CultureInfo.InvariantCulture,
                        "{0:0,0}", a.TotalPrice),
                        a.ProductId,
            }).ToList();
                var _ProductName = productCategoryApi.GetProductCategoryEntityById(selecteditem).CateName.ToString();
                var _Day = ProductReport.Select(a => a.StartTime).ToArray();

                return Json(new
                {
                    datatable = list,
                    dataChart = new
                    {
                        _ProductName,
                        _Day,
                        listTotalOrder,
                        listTotalPrice,
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            catch (Exception ex)
            {
                return Json(new
                { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public List<dynamic> ExportProductLineAllStoreTableToExcel(int brandId, string startTime, string endTime, int selecteditem)
        {
            List<dynamic> list = new List<dynamic>();
            var dateproductApi = new DateProductApi();
            var orderDetailApi = new OrderDetailApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listTotalOrder = new List<Double[]>();
            var listTotalPrice = new List<Double[]>();
            var productApi = new ProductApi();
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var dateNow = Utils.GetCurrentDateTime();
            var listOrderdetail = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
            var listDateProduct = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).Where(a => a.ProductId == selecteditem).ToList();
            for (var d = startDate; startDate <= endDate; d.AddDays(1))
            {

                if (startDate == dateNow.GetStartOfDate())
                {
                    var listOrder = listOrderdetail.Where(a => a.ProductID == selecteditem);
                    var quantity = listOrder.Sum(a => a.Quantity);
                    var totalAmount = listOrder.Sum(a => a.TotalAmount);
                    var finalAmount = listOrder.Sum(a => a.FinalAmount);
                    var discountFee = listOrder.Sum(a => a.Discount);
                    ProductReport.Add(new ProductLineReportModalViewModel()
                    {
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        ProductId = selecteditem,
                        Quantity = quantity,
                        TotalOrder = totalAmount,
                        Discount = discountFee,
                        TotalPrice = finalAmount,
                    });

                }
                else
                {
                    var listdate = listDateProduct.Where(a => a.Date.GetStartOfDate() == startDate.Date);
                    var quantity = listdate.Sum(a => a.Quantity);
                    var totalAmount = listdate.Sum(a => a.TotalAmount);
                    var finalAmount = listdate.Sum(a => a.FinalAmount);
                    var discountFee = listdate.Sum(a => a.Discount);
                    list.Add(new ProductLineReportModalViewModel()
                    {
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        ProductId = selecteditem,
                        Quantity = quantity,
                        TotalOrder = totalAmount,
                        Discount = discountFee,
                        TotalPrice = finalAmount,
                    });
                }
                startDate = startDate.AddDays(1);
            }
            return list;
        }
        public List<dynamic> ExportCategoryProductLineAllStoreTableToExcel(int brandId, string startTime, string endTime, int selecteditem)
        {
            List<dynamic> list = new List<dynamic>();
            var dateproductApi = new DateProductApi();
            var orderDetailApi = new OrderDetailApi();
            var storeApi = new StoreApi();
            var ProductReport = new List<ProductLineReportModalViewModel>();
            var listTotalOrder = new List<Double[]>();
            var listTotalPrice = new List<Double[]>();
            var productCategoryApi = new ProductCategoryApi();
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var dateNow = Utils.GetCurrentDateTime();
            var table1 = storeApi.GetStoreByBrandId(brandId);
            var table2 = dateproductApi.GetDateProductAllStoreByTimeRange(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate());
            var tablejoin = (from t1 in table1
                             join t2 in table2
                             on t1.ID equals t2.StoreID
                             where t2.CategoryId_ == selecteditem
                             select new
                             {
                                 t2.Date,
                                 t2.CategoryId_,
                                 t2.Quantity,
                                 t2.TotalAmount,
                                 t2.Discount,
                                 t2.FinalAmount,
                             }).ToList();
            var listOrderdetail = orderDetailApi.GetAllOrderDetailsByTimeRange(brandId, dateNow.GetStartOfDate(), dateNow.GetEndOfDate()).ToList();
            //var listDateProduct = dateproductApi.GetDateProductByTimeRangeAndBrand(startTime.ToDateTime().GetStartOfDate(), endTime.ToDateTime().GetEndOfDate(), brandId).ToList();
            //var listDateProductPro = listDateProduct.Where(a => a.CategoryId_ == selecteditem).ToList();
            for (var d = startDate; startDate <= endDate; d.AddDays(1))
            {

                if (startDate == dateNow.GetStartOfDate())
                {
                    var listOrder = listOrderdetail.Where(a => a.Product.CatID == selecteditem);
                    var quantity = listOrder.Sum(a => a.Quantity);
                    var totalAmount = listOrder.Sum(a => a.TotalAmount);
                    var finalAmount = listOrder.Sum(a => a.FinalAmount);
                    var discountFee = listOrder.Sum(a => a.Discount);
                    ProductReport.Add(new ProductLineReportModalViewModel()
                    {
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        ProductId = selecteditem,
                        Quantity = quantity,
                        TotalOrder = totalAmount,
                        Discount = discountFee,
                        TotalPrice = finalAmount,
                    });

                }
                else
                {
                    var listdate = tablejoin.Where(a => a.Date.GetStartOfDate() == startDate.Date).ToList();
                    var quantity = listdate.Sum(a => a.Quantity);
                    var totalAmount = listdate.Sum(a => a.TotalAmount);
                    var finalAmount = listdate.Sum(a => a.FinalAmount);
                    var discountFee = listdate.Sum(a => a.Discount);
                    list.Add(new ProductLineReportModalViewModel()
                    {
                        StartTime = startDate.ToString("dd/MM/yyyy"),
                        CateId = selecteditem,
                        Quantity = quantity,
                        TotalOrder = totalAmount,
                        Discount = discountFee,
                        TotalPrice = finalAmount,
                    });
                }
                startDate = startDate.AddDays(1);
            }
            return list;
        }
        public ActionResult ReportProductLineAllStoreExportExcelEPPlus(int brandId, string startTime, string endTime, int selecteditem, string checkvalue)
        {
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo diễn tiến sản phẩm");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                var list = new List<dynamic>();
                if (checkvalue == "product")
                {
                    list = ExportProductLineAllStoreTableToExcel(brandId, startTime, endTime, selecteditem);
                }
                else
                {
                    list = ExportCategoryProductLineAllStoreTableToExcel(brandId, startTime, endTime, selecteditem);
                }
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Ngày";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Doanh thu trước giảm giá";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giảm giá";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Doanh thu";

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
                foreach (var data in list)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.StartTime;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Quantity;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalOrder;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.Discount;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.TotalPrice;
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
                startTime = startTime.Replace('/', '-');

                endTime = endTime.Replace('/', '-');
                var fileDownloadName = "Báo cáo diễn tiến sản phẩm từ ngày " + startTime + " - " + endTime + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileDownloadName);
            }
        }
        #endregion
        public JsonResult LoadStoreList(int brandId)
        {
            var storeapi = new StoreApi();
            var stores = storeapi.GetActiveStoreByBrandId(brandId).ToArray();
            return Json(new
            {
                store = stores,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadStoreGroupList(int brandId)
        {
            var storeGroupapi = new StoreGroupApi();
            var storesGroup = storeGroupapi.GetStoreGroupByBrandId(brandId).ToArray();
            return Json(new
            {
                storeGroup = storesGroup,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadProductList(int brandId)
        {
            var productApi = new ProductApi();
            var listPro1 = productApi.GetActiveProductsEntitybyBrandId(brandId).ToArray();
            //listPro1.Count();
            List<object> listProduct = new List<object>();
            foreach (var i in listPro1)
            {
                listProduct.Add(new
                {
                    ProductName = i.ProductName,
                    ProductId = i.ProductID,
                });
            }
            var listPro2 = listProduct.ToArray();

            //List<ProductViewModel> fillterList = new List<ProductViewModel>();
            //var productCategoryApi = new ProductCategoryApi();
            ////var orderDetailApi = new OrderDetailApi();
            ////Get category in DB
            //var listCategory = productCategoryApi.GetActiveProductCategoriesByBrandId(brandId).Where(a => a.Type == 1);
            //foreach (var itemCat in listCategory)
            //{
            //    var listProduct = itemCat.Products.Where(a => a.Active);
            //    foreach (var itemP in listProduct)
            //    {
            //        var productItem = fillterList.FirstOrDefault(a => a.ProductID == itemP.ProductID);
            //        fillterList.Remove(productItem);
            //        fillterList.Add(new ProductViewModel
            //        {
            //            ProductID = itemP.ProductID,
            //            ProductName = itemP.ProductName,
            //        });
            //    }
            //}
            //var listPro = fillterList.Select(a => new IConvertible[]
            //{
            //    a.ProductID,
            //    a.ProductName,
            //}).ToArray();

            return Json(new
            {
                listPro = listPro2,
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadProductCateList(int brandId)
        {
            List<ProductViewModel> fillterList = new List<ProductViewModel>();
            var productCategoryApi = new ProductCategoryApi();
            //var orderDetailApi = new OrderDetailApi();
            //Get category in DB
            var listCategory = productCategoryApi.GetActiveProductCategoriesByBrandId(brandId).Where(a => a.Type == 1);
            foreach (var itemCat in listCategory)
            {
                fillterList.Add(new ProductViewModel
                {
                    CatID = itemCat.CateID,
                    CateName = itemCat.CateName,
                });
            }
            var listCate = fillterList.Select(a => new IConvertible[]
            {
                a.CatID,
                a.CateName,
            }).ToArray();

            return Json(new
            {
                listCate = listCate,
            }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetAllProducts(int brandId, string searchTokens, int page)
        {
            try
            {
                var productApi = new ProductApi();
                var result = await productApi.GetAllBrandActiveProductsForReport(brandId);
                if (!string.IsNullOrWhiteSpace(searchTokens))
                {
                    result = result.Where(q => Utils.CustomContains(q.ProductName, searchTokens) || Utils.CustomContains(q.CateName.ToLower(), searchTokens));
                }

                var list = result
                    .OrderBy(q => q.CateName)
                    .Skip(page * 50)
                    .Take(50)
                    .GroupBy(q => q.CateName)
                    .Select(p => new
                    {
                        text = p.Key,
                        children = p.Select(a => new
                        {
                            id = a.ProductID,
                            text = a.ProductName
                        })
                    });

                var total = result.Count();

                return Json(new { success = true, list = list, total = total }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult GetAllProductCategories(int brandId, string searchTokens, int page)
        {
            try
            {
                var productCateApi = new ProductCategoryApi();

                var result = productCateApi.GetProductCategoriesForReport(brandId);
                if (!string.IsNullOrWhiteSpace(searchTokens))
                {
                    result = result.Where(q => Utils.CustomContains(q.CateName, searchTokens));
                }

                var list = result
                    .OrderBy(q => q.CateName)
                    .Skip(page * 20)
                    .Take(20)
                    .Select(p => new
                    {
                        id = p.CateID,
                        text = p.CateName
                    });

                var total = result.Count();
                return Json(new { success = true, list = list, total = total }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }
        }

        public class ProductLineReportModalViewModel
        {
            public string StartTime { get; set; }
            public int ProductId { get; set; }
            public int CateId { get; set; }
            public string CateName { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public int QuantityAtStore { get; set; }
            public int QuantityTakeAway { get; set; }
            public int QuantityDelivery { get; set; }
            public double TotalPrice { get; set; }

            public double Percent { get; set; }
            public double Discount { get; set; }

            public double TotalOrder { get; set; }
        }
        public class TempSystemProductDateReportItem
        {
            public string StartTime { get; set; }
            public int Quantity { get; set; }
            public double TotalAmount { get; set; }
            public double FinalAmount { get; set; }
            public double TotalDiscountFee { get; set; }
        }
        public class ReportViewModel
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }
        public class ExcellistModelView
        {
            public string Name { get; set; }
            public double Percent { get; set; }
            public double TotalOrder { get; set; }
            public double Discount { get; set; }
            public double TotalPrice { get; set; }

        }
    }
}