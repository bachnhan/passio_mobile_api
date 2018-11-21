using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.ViewModels;
using HmsService.Sdk;
using System.Threading.Tasks;
using System.Web.Helpers;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Inventory.Controllers
{
    [Authorize(Roles = "BrandManager, Inventory, StoreManager, Manager")]
    public class InventoryTemplateController : DomainBasedController
    {
        // GET: Inventory/InventoryTemplate
        public ActionResult Index()
        {
            return View();
        }

        #region Datatables
        public JsonResult LoadAllTemplates(int brandId)
        {
            var templateApi = new InventoryTemplateReportApi();
            var templates = templateApi.GetBrandActiveTemplate(brandId);
            int count = 0;
            var rs = templates.Select(q => new IConvertible[] {
                ++count,
                q.Name,
                q.Description,
                q.Id
            });
            return Json(new { success = true, data = rs }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Create (InventoryTemplateReportViewModel model)
        {
            var templateApi = new InventoryTemplateReportApi();
            try
            {
                await templateApi.CreateAsync(model);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return Json(new { success = false, message = "Tạo mẫu báo cáo thất bại!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, message = "Tạo mẫu báo cáo thành công!" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Deactive/Delete
        public async Task<JsonResult> Deactivate(int templateId)
        {
            var templateApi = new InventoryTemplateReportApi();
            try
            {
                await templateApi.DeactivateAsync(templateId);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return Json(new { success = false, message = "Xóa mẫu báo cáo thất bại!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, message = "Xóa mẫu báo cáo thành công!" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Update
        public ActionResult Edit(int templateId)
        {
            var templateApi = new InventoryTemplateReportApi();
            var model = templateApi.Get(templateId);
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> Edit(InventoryTemplateReportViewModel model)
        {
            var templateApi = new InventoryTemplateReportApi();
            try
            {
                var templateId = model.Id;
                model.Active = true;
                await templateApi.EditAsync(templateId, model);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return Json(new { success = false, message = "Cập nhật mẫu báo cáo thất bại!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, message = "Cập nhật mẫu báo cáo thành công!" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region AssignItems
        [HttpGet]
        public ActionResult AssignItems(int brandId, int templateId)
        {
            var itemCategoryApi = new ProductItemCategoryApi();
            var templateApi = new InventoryTemplateReportApi();
            var model = templateApi.GetTemplateEditViewModel(templateId);
            var itemCategories = itemCategoryApi.GetItemCategoryByBrand(brandId).ToList();
            ViewBag.ItemCates = itemCategories;
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateTemplateItems(InventoryTemplateReportEditViewModel model)
        {
            var templateItemMappingApi = new TemplateReportProductItemMappingApi();
            try
            {
                var templateItemsFromDB = templateItemMappingApi.GetAllTemplateItems(model.Id);
                var templateItemsFromView = model.TemplateReportProductItemMappings;
                foreach (var item in templateItemsFromDB)
                {
                    await templateItemMappingApi.DeleteAsync(item.Id);
                }
                foreach (var item in templateItemsFromView)
                {
                    item.InventoryTemplateReportId = model.Id;
                    await templateItemMappingApi.CreateAsync(item);
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return Json(new { success = false, message = "Cập nhật mẫu báo cáo thất bại!" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, message = "Cập nhật mẫu báo cáo thành công!" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}