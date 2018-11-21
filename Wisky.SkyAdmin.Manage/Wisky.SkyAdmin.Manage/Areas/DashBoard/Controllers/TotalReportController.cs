using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.DashBoard.Controllers
{
    public class TotalReportController : Controller
    {
        // GET: DashBoard/TotalReport
        public ActionResult Index(int brandId, int storeId)
        {
            ViewBag.BrandId = brandId;
            ViewBag.StoreId = storeId;
            return View();
        }

        [HttpGet]
        public JsonResult LoadStoreList(int brandId)
        {
            var storeapi = new StoreApi();
            var stores = storeapi.GetActiveStoreByBrandId(brandId).ToArray();
            return Json(new
            {
                store = stores,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}