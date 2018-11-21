using HmsService.Sdk;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using HmsService.ViewModels;
using Wisky.SkyUp.Website.Models;
using Wisky.SkyAdmin.Manage.Models;
using HmsService.Models;
using System.Data.Entity;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    [Authorize(Roles = "Administrator")]
    //[Authorize(Roles = Utils.SysAdminAuthorizeRoles)]
    public class StoreController : BaseController
    {
        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult IndexList(BootgridRequestViewModel request)
        {
            var result = new StoreApi().GetAdminWithFilterAsync(
                request.searchPhrase, request.current, request.rowCount, request.FirstSortTerm);

            var model = new BootgridResponseViewModel<StoreViewModel>(result);
            return this.Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAllStoreLocation(int brandId)
        {
            var storeApi = new StoreApi();
            var stores = storeApi.GetStoreByBrandId(brandId);
            var rs = (await stores.ToListAsync()).Select(q => new
            {
                storeId = q.ID,
                latitude = q.Lat,
                longitude = q.Lon,
            });
            return Json(rs, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetListStoresByBrandId(JQueryDataTableParamModel param, int id)
        {
            var storeApi = new StoreApi();
            var stores = storeApi.GetAllStore(id);
            var count = param.iDisplayStart + 1;
            try
            {
                var rs = (await stores.Where(q => string.IsNullOrEmpty(param.sSearch) ||
                            (!string.IsNullOrEmpty(param.sSearch)
                            && q.Name.ToLower().Contains(param.sSearch.ToLower())))
                            .OrderByDescending(q => q.Name)
                            .ToListAsync())
                            .Select(q => new IConvertible[] {
                                count++,
                                q.Name,
                                q.Phone,
                                q.OpenTime?.ToShortTimeString(),
                                q.CloseTime?.ToShortTimeString(),
                                (q.isAvailable==true ?"<i class=\"glyphicon glyphicon-ok\"></i>":"<i class=\"glyphicon glyphicon-remove\"></i>"),
                                q.ID,
                            });
                var totalRecords = await stores.CountAsync();

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
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        //        public ActionResult Create()
        //        {
        //            var model = new StoreEditViewModel();

        //            this.PrepareCreate(model);
        //            return this.View(model);
        //        }

        //        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        //        public async Task<ActionResult> Create(StoreEditViewModel model)
        //        {
        //            if (!this.ModelState.IsValid)
        //            {
        //                this.PrepareCreate(model);
        //                return this.View(model);
        //            }

        ////            await new StoreApi().CreateAsync(model, nameof(model.ID));
        //            await new StoreApi().CreateAsync(model, "ID");

        //            this.SetMessage("Store Created!");
        //            return this.RedirectToAction("Edit", new { id = model.ID, });
        //        }

        private void PrepareCreate(StoreEditViewModel model)
        {
            model.StoreTypeEnum = (StoreTypeEnum)model.Type;
            model.SelectedMenu = GetSelectedMenu(model.StoreFeatureFilter);
        }

        //        public async Task<ActionResult> Edit(int? id)
        //        {
        //            // Validate
        //            var info = await new StoreApi().GetAsync(id);

        //            if (info == null || !info.isAvailable.GetValueOrDefault())
        //            {
        //                return this.IdNotFound();
        //            }

        //            var model = new StoreEditViewModel(info, this.Mapper);

        //            this.PrepareEdit(model);
        //            return this.View(model);
        //        }

        //        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        //        public async Task<ActionResult> Edit(StoreEditViewModel model)
        //        {
        //            var api = new StoreApi();
        //            // Validate
        //            var info = await api.GetAsync(model.ID);

        //            if (info == null || !info.isAvailable.GetValueOrDefault())
        //            {
        //                return this.IdNotFound();
        //            }

        //            if (!this.ModelState.IsValid)
        //            {
        //                this.PrepareEdit(model);
        //                return this.View(model);
        //            }

        //            model.isAvailable = true;

        //            await api.EditAsync(model.ID, model);

        //            return this.RedirectToAction("Index");
        //        }

        private void PrepareEdit(StoreEditViewModel model)
        {
            model.StoreTypeEnum = (StoreTypeEnum)model.Type;
            model.SelectedMenu = GetSelectedMenu(model.StoreFeatureFilter);
        }

        IEnumerable<SelectedMenuItem> GetSelectedMenu(string featureFilter)
        {
            var menuApi = new MenuApi();
            IEnumerable<SelectedMenuItem> results;
            if (featureFilter == null)
            {
                results = menuApi.GetMenuViewModelsByFilter((int)MenuTypeEnum.StoreMenu, "")
                     .Select(q => new SelectedMenuItem(q));
            }
            else
            {
                results = menuApi.GetMenuViewModelsByFilter((int)MenuTypeEnum.StoreMenu, featureFilter)
                    .Select(q => new SelectedMenuItem(q));
            }
            return results;
        }

        public ActionResult Create(int Id)
        {
            var model = new StoreEditViewModel();
            model.BrandId = Id;
            PrepareCreate(model);
            return View(model);
        }

        public ActionResult Create2(int brandId)
        {
            var model = new StoreEditViewModel();
            model.BrandId = brandId;
            PrepareCreate(model);

            return this.View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Create(StoreEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var storeApi = new StoreApi();
                    var productApi = new ProductApi();
                    model.Type = (int)model.StoreTypeEnum;
                    var listProduct = productApi.GetAllProducts().Where(q => q.Active && q.ProductCategory.BrandId == model.BrandId.Value);
                    await storeApi.CreateStoreAsync(model, listProduct);
                    return Json(new { success = true, message = "Tạo cửa hàng thành công" });
                }
                catch
                {
                    return Json(new { success = false, message = "Tạo cửa hàng thất bại" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Tạo cửa hàng thất bại" });
            }
        }

        public async Task<ActionResult> Edit(int Id)
        {
            var storeApi = new StoreApi();
            var info = await storeApi.GetAsync(Id);
            if (info == null || !info.isAvailable.GetValueOrDefault())
            {
                return this.IdNotFound();
            }

            var model = new StoreEditViewModel(info, this.Mapper);
            PrepareEdit(model);
            return View(model);
        }

        public async Task<ActionResult> Edit2(int Id)
        {
            var storeApi = new StoreApi();
            var model = new StoreEditViewModel(await storeApi.GetAsync(Id), this.Mapper);
            PrepareEdit(model);
            return View(model);
        }

        public JsonResult GetStores(int brandId)
        {
            var storeApi = new StoreApi();
            var stores = storeApi.GetActiveStoreByBrandId(brandId).Select(q => new { Id = q.ID, StoreName = q.Name });
            return Json(new { success = true, stores = stores }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(StoreEditViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    var storeApi = new StoreApi();
                    model.Type = (int)model.StoreTypeEnum;
                    await storeApi.EditStoreAsync(model);
                    return Json(new { success = true, message = "Sửa thông tin cửa hàng thành công" });
                }
                catch
                {
                    return Json(new { success = false, message = "Sửa thông tin cửa hàng thất bại" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Sửa thông tin cửa hàng thất bại" });
            }
        }

        public async Task<JsonResult> Delete(int? id)
        {
            try
            {
                var api = new StoreApi();
                var info = await api.GetAsync(id);

                if (info == null)
                {
                    return Json(new { success = false, message = "Không tồn tại cửa hàng này" });
                }

                await api.DeactivateAsync(id.Value);
                return Json(new { success = true, message = "Xóa cửa hàng thành công" });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Xóa cửa hàng thất bại" });
            }
        }

    }
}