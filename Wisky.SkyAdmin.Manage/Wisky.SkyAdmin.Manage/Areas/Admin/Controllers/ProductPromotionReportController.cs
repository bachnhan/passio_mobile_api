using HmsService.Models;
using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    public class ProductPromotionReportController : DomainBasedController
    {
        // GET: Admin/ProductPromotionReport
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult getData(string startTime, string endTime, int storeId, int brandId)
        {
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var promotionApi = new PromotionApi(); 
            //var reportItem =
            return null;
        }
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