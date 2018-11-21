using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.ProviderManager.Controllers
{
    [Authorize(Roles = "BrandManager, Manager")]
    public class ProviderManageController : DomainBasedController
    {
        // GET: ProviderManager/ProviderManage
        public ActionResult IndexProvider(int storeId)
        {
            ViewBag.storeId = storeId.ToString();

            return View();
        }


        public JsonResult LoadProvider(JQueryDataTableParamModel param, int brandId)
        {
            var providerApi = new ProviderApi();
            var listProvider = providerApi.GetProvidersByBrand(brandId);
            int count = 0;
            try
            {
                var searchList = listProvider
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.ProviderName.ToLower().Contains(param.sSearch.ToLower()));

                count = param.iDisplayStart + 1;
                var rs = searchList
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength).ToList()
                    .Select(a => new IConvertible[]
                        {
                        count++,
                        string.IsNullOrEmpty(a.ProviderName) ? "Không xác định" : a.ProviderName,
                        a.Address,
                        a.Phone,
                        a.Id
                        });

                var totalRecords = listProvider.Count();
                var totalDisplayRecords = searchList.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalDisplayRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #region Create
        public ActionResult Create(string storeId)
        {
            ViewBag.storeId = storeId;
            var model = new ProviderViewModel();

            return PartialView("Create", model);
        }

        [HttpPost]
        public async Task<ActionResult> Create(ProviderViewModel model, int brandId)
        {
            if (!this.ModelState.IsValid)
            {
                return View(model);
            }
            var providerApi = new ProviderApi();
            model.BrandId = brandId;
            model.IsAvailable = true;
            await providerApi.CreateProviderAsync(model);
            return RedirectToAction("IndexProvider", "ProviderManage");
        }
        #endregion
        #region Edit
        public async Task<ActionResult> Edit(int id, string storeId)
        {
            ViewBag.storeId = storeId;
            var providerApi = new ProviderApi();
            var model = await providerApi.GetAsync(id);
            return PartialView("Edit", model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(ProviderViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View(model);
            }
            var providerApi = new ProviderApi();
            await providerApi.UpdateProviderAsync(model);
            return RedirectToAction("IndexProvider", "ProviderManage");

        }
        #endregion

        public async Task<JsonResult> Delete(int id)
        {
            var providerApi = new ProviderApi();
            var providerProductApi = new ProviderProductItemMappingApi();

            var providers = providerProductApi.GetActive().Where(q => q.ProviderID == id);
            var item = providers.Count();

            var model = providerApi.Get(id);


            if (model != null && item == 0)
            {

                await providerApi.DeleteAccountAsync(model);
                return Json(new { success = true, message = "Xóa nhà cung cấp thành công" });
            }
            else
            {
                return Json(new { success = false, message = "Không thể xóa. Tồn tại nguyên liệu chứa nhà cung cấp này!" });
            }
            //if (model == null)
            //{
            //    return Json(new { success = false });
            //}
            //try
            //{
            //    await providerApi.DeleteAccountAsync(model);
            //}
            //catch (System.Exception e)
            //{
            //    throw e;
            //    //return Json(new { success = false });
            //}
            //return Json(new { success = true });
        }
    }
}