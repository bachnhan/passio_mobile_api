using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    public class MenuConfigController : DomainBasedController
    {
        // GET: SysAdmin/MenuConfig
        public ActionResult Index(int brandId)
        {
            var brandApi = new BrandApi();
            var model = new BrandEditViewModel(brandApi.Get(brandId), this.Mapper);
            PrepareModel(model);
            return View("Index",model);
        }

        public ActionResult GetMenuParent()
        {
            var menuApi = new MenuApi();
            var listResult = menuApi.Get().Where(q=>q.MenuTypeCode == (int)MenuTypeEnum.BrandMenu || q.MenuTypeCode == (int)MenuTypeEnum.All);
            return Json(new { listParrent  = listResult}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMenuParentStore()
        {
            var menuApi = new MenuApi();
            var listResult = menuApi.Get().Where(q => q.MenuTypeCode == (int)MenuTypeEnum.StoreMenu || q.MenuTypeCode == (int)MenuTypeEnum.All);
            return Json(new { listParrent = listResult }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateMenuItem(string menutext, string area, string controller, string action, string iconCss, string displayOrder, string parrentId, int currentMenuId)
        {
            var menuApi = new MenuApi();
            try
            {
                var currentMenu = menuApi.Get(currentMenuId);
                currentMenu.Action = action;
                currentMenu.Area = area;
                currentMenu.MenuText = menutext;
                currentMenu.Controller = controller;
                currentMenu.IconCss = iconCss;
                currentMenu.DisplayOrder = int.Parse(displayOrder);
                if (!String.IsNullOrEmpty(parrentId) && parrentId != "0")
                {
                    var menuId = int.Parse(parrentId);
                    var currentParrentLevel = menuApi.GetCurrentMenuLevel(menuId);
                    currentMenu.ParentMenuId = menuId;
                    currentMenu.MenuLevel = currentParrentLevel + 1;
                }
                menuApi.Edit(currentMenu.Id, currentMenu);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            
        }

        [HttpPost]
        public ActionResult CreateMenuItem(string menutext, string area, string controller, string action, string iconCss, string displayOrder, string parrentId)
        {
            var menuApi = new MenuApi();
            try
            {
                var currentMenu = new Menu();
                currentMenu.Action = action;
                currentMenu.Area = area;
                currentMenu.MenuText = menutext;
                currentMenu.Controller = controller;
                currentMenu.IconCss = iconCss;
                currentMenu.DisplayOrder = int.Parse(displayOrder);
                if (!String.IsNullOrEmpty(parrentId) && parrentId != "0")
                {
                    var menuId = int.Parse(parrentId);
                    var currentParrentLevel = menuApi.GetCurrentMenuLevel(menuId);
                    currentMenu.ParentMenuId = menuId;
                    currentMenu.MenuLevel = currentParrentLevel + 1;
                }
                var keyfree = menuApi.GetKeyFreeBrand();
                currentMenu.FeatureCode = keyfree;
                currentMenu.MenuTypeCode = (int)MenuTypeEnum.BrandMenu;
                menuApi.BaseService.Create(currentMenu);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult CreateMenuItemStore(string menutext, string area, string controller, string action, string iconCss, string displayOrder, string parrentId)
        {
            var menuApi = new MenuApi();
            try
            {
                var currentMenu = new Menu();
                currentMenu.Action = action;
                currentMenu.Area = area;
                currentMenu.MenuText = menutext;
                currentMenu.Controller = controller;
                currentMenu.IconCss = iconCss;
                currentMenu.DisplayOrder = int.Parse(displayOrder);
                if (!String.IsNullOrEmpty(parrentId) && parrentId != "0")
                {
                    var menuId = int.Parse(parrentId);
                    var currentParrentLevel = menuApi.GetCurrentMenuLevel(menuId);
                    currentMenu.ParentMenuId = menuId;
                    currentMenu.MenuLevel = currentParrentLevel + 1;
                }
                var keyfree = menuApi.GetKeyFreeStore();
                currentMenu.FeatureCode = keyfree;
                currentMenu.MenuTypeCode = (int)MenuTypeEnum.StoreMenu;
                menuApi.BaseService.Create(currentMenu);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult DeleteMenuConfig(int idmenu)
        {
            var menuApi = new MenuApi();
            try
            {
                menuApi.Delete(idmenu);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ConfigMenuBrand()
        {
            var configmenuViewmodel = new ConfigMenuViewmodel();
            configmenuViewmodel.SelectedMenu = GetSelectedMenu("");
            return View("ConfigMenu", configmenuViewmodel);
        }

        public ActionResult ConfigMenuStore()
        {
            var configmenuViewmodel = new ConfigMenuViewmodel();
            configmenuViewmodel.SelectedMenu = GetSelectedMenuStore("");
            return View("ConfigMenuStore", configmenuViewmodel);
        }




        public ActionResult GetDetailMenu(int menuId)
        {
            var menuApi = new MenuApi();
            var menu = menuApi.Get(menuId);
            return Json(new { menu = menu }, JsonRequestBehavior.AllowGet);
        }


        private void PrepareModel(BrandEditViewModel model)
        {
            model.SelectedMenu = GetSelectedMenu(model.BrandFeatureFilter);
        }

        IEnumerable<SelectedMenuItem> GetSelectedMenu(string featureFilter)
        {
            var menuApi = new MenuApi();
            IEnumerable<SelectedMenuItem> results;
            if (featureFilter == null)
            {
                results = menuApi.GetMenuViewModelsByFilter((int)MenuTypeEnum.BrandMenu, "")
                    .Select(q => new SelectedMenuItem(q));
            }
            else
            {
                results = menuApi.GetMenuViewModelsByFilter((int)MenuTypeEnum.BrandMenu, featureFilter)
                    .Select(q => new SelectedMenuItem(q));
            }
            return results;
        }

        IEnumerable<SelectedMenuItem> GetSelectedMenuStore(string featureFilter)
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


        public ActionResult EditFillterMenu(BrandEditViewModel brandfillter)
        {
            var brandApi = new BrandApi();
            var inforBrand = brandApi.BaseService.Get(brandfillter.Id);
            inforBrand.BrandFeatureFilter = brandfillter.BrandFeatureFilter;
            brandApi.BaseService.Update(inforBrand);
             return Json(new { success = true, message = "Cấu hình thành công" });
        }

        //Config Menu ForStore

        public ActionResult ConfigMenuAllStore(int brandId)
        {
            var configMenuStore = new ConfigMenuStoreViewModel();
            configMenuStore.BrandId = brandId;
            var menuApi = new MenuApi();
            var menus = menuApi.GetMenuViewModelsByFilter((int)MenuTypeEnum.StoreMenu, "")
                    .Select(q => new SelectedMenuItem(q));
            configMenuStore.SelectedMenu = menus;
            return View("ConfigMenuAllStore", configMenuStore);

        }

        [HttpPost]
        public ActionResult EditFillterMenuAllStore(ConfigMenuStoreViewModel model)
        {
            var storeApi = new StoreApi();
            try
            {
                foreach (var item in model.storeArray)
                {
                    var store = storeApi.Get(item);
                    if (store != null)
                    {
                        store.StoreFeatureFilter = model.FilterString;
                        storeApi.Edit(store.ID, store);
                    }
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult LoadApplyStore(int brandId)
        {
            var storeApi = new StoreApi();
            var listStore = storeApi.GetStoreByBrandId(brandId);
            var applyStore = listStore.
                Select(a => new { storeId = a.ID, storeName = a.Name });
            return Json(new
            {
                applyStore = applyStore
            }, JsonRequestBehavior.AllowGet);
        }
    }
}