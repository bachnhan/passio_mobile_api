using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    public class ProvinceController : DomainBasedController
    {
        // GET: Admin/Province
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetAllArea()
        {
            var areaApi = new AreaDeliveryApi();

            var listArea = areaApi.GetAllAreaProvince();
            var finalResult = listArea.Select(a => new
            {
                AreaId = a.Id,
                AreaName = a.AreaName,
                BasePrice = a.PriceDelivery
            });
            return Json(new
            {
                listArea = finalResult
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult UpdateProvince(int detailId, decimal baseprice, int areaId)
        {
            var provinceApi = new ProvinceApi();
            try
            {
                var currentProvince = provinceApi.BaseService.Get(detailId);
                if (currentProvince != null)
                {
                    currentProvince.BasePriceDelivery = baseprice;
                    currentProvince.AreaProvinceId = areaId;
                    provinceApi.BaseService.Update(currentProvince);
                    return Json(new { success = true, message = "Chỉnh sửa thông tin thành công" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Dữ liệu không phù hợp, xin vui lòng thử lại" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra trong quá trình xử lý, vui lòng thử lại" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult IndexList(HmsService.Models.JQueryDataTableParamModel param, int areaId)
        {
            var provinceApi = new ProvinceApi();
            var areaApi = new AreaDeliveryApi();
            var provinces = provinceApi.GetProvincesBaseOnArea(areaId);
            var totalRecord = provinces.Count();
            if (!String.IsNullOrEmpty(param.sSearch))
            {
                provinces = provinces.Where(q => q.ProvinceName.Contains(param.sSearch));
            }
            var totalResult = provinces.Count();
            var count = param.iDisplayStart + 1;
            var result = provinces.OrderBy(q => q.ProvinceCode).Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList().Select(q => new IConvertible[] {
                count++,
                q.ProvinceName,
                q.AreaDelivery != null ? q.AreaDelivery.AreaName : "N/A",
                q.BasePriceDelivery,
                q.ProvinceCode,
                q.AreaProvinceId
            });
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecord,
                iTotalDisplayRecords = totalResult,
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }
    }
}