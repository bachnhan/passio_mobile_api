using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using Microsoft.Ajax.Utilities;
using OfficeOpenXml;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Inventory.Controllers
{
    public class ProviderController : DomainBasedController
    {
        #region Thống kê nhập hàng theo nhà cung cấp
        //View
        public ActionResult ImportReportList(int brandId, int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            ViewBag.ReviewDate = Utils.GetCurrentDateTime().ToString("dd/MM/yyyy");
            return View();
        }
        //Json load
        public JsonResult LoadImportReportList(JQueryDataTableParamModel param, string startTime, string endTime, int brandId)
        {
            var providerApi = new ProviderApi();
            var itemApi = new InventoryReceiptItemApi();
            var providerLst = providerApi.GetProviders().Where(q => q.BrandId == brandId);
            var startDate = (startTime.IsNullOrWhiteSpace()) ? Utils.GetCurrentDateTime().GetStartOfDate() : startTime.ToDateTime().GetStartOfDate();
            var endDate = (endTime.IsNullOrWhiteSpace()) ? Utils.GetCurrentDateTime().GetEndOfDate() : endTime.ToDateTime().GetEndOfDate();
            int count = 0;
            count = param.iDisplayStart;
            try
            {
                var rs = providerLst
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.ProviderName.ToLower().Contains(param.sSearch.ToLower()))
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList()
                    .Select(a => new IConvertible[]
                    {
                        ++count,
                        a.ProviderName,
                        string.Format(CultureInfo.InvariantCulture,"{0:0,0}",
                            itemApi.GetItemReceiptByProviderIdAndTimeRange(a.Id, startDate, endDate).Sum(q => q.Price*q.Quantity)),
                        //.ToString("0.##"),
                        a.Id
                    });                
                var totalRecords = providerLst.Count();
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Error" });
            }
        }
        //ItemDetail
        public ActionResult ImportReportItem(string startDate, string endDate)
        {
            //ViewBag.storeId = storeId.ToString();
            //var api = new ProviderApi();
            //var model = await api.GetProviderById(id);
            //ViewBag.ProviderName = model.ProviderName;
            ViewBag.ReviewDate = startDate + (startDate == endDate ? " - " + endDate : "");
            //ViewBag.ProviderId = id;
            return View();
        }
        //LoadItemDetail
        public JsonResult LoadImportReportItem(JQueryDataTableParamModel param, int providerId, string startTime, string endTime, int brandId)
        {
            var itemApi = new InventoryReceiptItemApi();
            var receiptApi = new InventoryReceiptApi();
            var startDate = (startTime.IsNullOrWhiteSpace()) ? Utils.GetCurrentDateTime().GetStartOfDate() : startTime.ToDateTime().GetStartOfDate();
            var endDate = (endTime.IsNullOrWhiteSpace()) ? Utils.GetCurrentDateTime() : endTime.ToDateTime().GetEndOfDate();
            var itemLst = itemApi.GetItemReceiptByProviderIdAndTimeRange(providerId, startDate, endDate);
            int count = 0;
            try
            {
                var rs = itemLst
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.ProductItem.ItemName.ToLower().Contains(param.sSearch.ToLower()))
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList()
                    .Select(a => new IConvertible[]
                    {
                        ++count,
                        a.ProductItem.ItemName,
                        receiptApi.GetChangeDateOfInventoryReceipt(a.ReceiptID),
                        a.ProductItem.Unit,
                        a.Quantity,
                        a.Price,                       
                    });
                var totalRecords = itemLst.Count();
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Error" });
            }
        }
        #endregion

        public JsonResult GetCurrentProviderItem(int itemId)
        {
            var providerProductItemMappingApi = new ProviderProductItemMappingApi();
            var count = 0;
            List<dynamic> listDt = new List<dynamic>();
            var providers = providerProductItemMappingApi.GetProviderProductItems()
                .Where(q => q.Active && q.ProductItemID == itemId)
                .Select(q => new
                {
                    No = ++count,
                    Name = q.Provider.ProviderName,
                    ProviderProductItemId = q.ProviderProductItemId,
                });
            return Json(new { data = providers.ToList() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> CreateProviderProductItem(ProviderProductItemMappingViewModel model)
        {
            var providerProductItemMappingApi = new ProviderProductItemMappingApi();
            var providerItem = await providerProductItemMappingApi.GetProviderProductItemById(model.ProviderID, model.ProductItemID);
            if (providerItem == null)
            {
                await providerProductItemMappingApi.CreateAsync(model);
                return Json(new { success = true, message = "Thêm thành công!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (providerItem.Active == true)
                {
                    return Json(new { success = false, message = "Đã tồn tại nhà cung cấp này!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    providerItem.Active = true;
                    await providerProductItemMappingApi.EditAsync(providerItem.ProviderProductItemId, providerItem);
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public JsonResult GetProviderProductItem(int productItemId)
        {
            var providerProductItemMappingApi = new ProviderProductItemMappingApi();
            var providerItem = providerProductItemMappingApi.GetProviderProductItemByItemId(productItemId);
            if (providerItem.Count() == 0)
            {
                return Json(new { success = false, message = "Chưa có nhà cung cấp" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true, message = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> RemoveProviderProductItem(int id)
        {
            var providerProductItemMappingApi = new ProviderProductItemMappingApi();
            var providerItem = await providerProductItemMappingApi.GetAsync(id);
            if (providerItem != null)
            {
                providerItem.Active = false;
                await providerProductItemMappingApi.EditAsync(providerItem.ProviderProductItemId, providerItem);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}