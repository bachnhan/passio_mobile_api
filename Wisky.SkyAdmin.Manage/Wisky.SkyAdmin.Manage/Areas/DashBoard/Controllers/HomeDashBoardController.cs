using HmsService.Models.Entities;
using HmsService.Sdk;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.DashBoard.Controllers
{
    public class HomeDashBoardController : DomainBasedController
    {
        // GET: DashBoard/HomeDashBoard
        public ActionResult Index(int storeId, int brandId)
        {
            var storeApi = new StoreApi();

            if(storeId != 0)
            {
                var store = storeApi.Get(storeId);
                ViewBag.storeId = store.ID.ToString();
                ViewBag.storeName = store.Name;
                ViewBag.brandId = brandId;

                return View();
            }

            ViewBag.storeId = "0";
            ViewBag.storeName = "Tổng quan hệ thống";
            ViewBag.brandId = brandId;

            return View();
        }
    }
}