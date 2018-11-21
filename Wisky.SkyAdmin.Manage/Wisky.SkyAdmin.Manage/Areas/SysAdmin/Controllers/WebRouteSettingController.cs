using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Sdk;
using HmsService.Models;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    public class WebRouteSettingController : Controller
    {
        // GET: SysAdmin/WebRouteSetting
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetListStoreTheme()
        {
            StoreApi storeApi = new StoreApi();
            BrandApi brandApi = new BrandApi();
            var listStoreWeb = storeApi.GetActive().Where(s => s.Type == (int)StoreTypeEnum.Website);
            var listBrand = brandApi.GetActive();
            return Json(new { listStore = listStoreWeb, listBrand = listBrand }, JsonRequestBehavior.AllowGet);
        }
    }
}