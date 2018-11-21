using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    public class DistrictController : Controller
    {
        // GET: Admin/District
        // GET: Admin/District
        [HttpGet]
        public ActionResult Index(int id)
        {
            var provinceApi = new ProvinceApi();

            var province = provinceApi.Get(id);
            ViewBag.ProvinceName = province.ProvinceName;
            ViewBag.ProvinceID = province.ProvinceCode;
            ViewBag.AreaId = province.AreaProvinceId;
            return View();
        }

        public ActionResult GetAllArea(int areaId)
        {
            var areaApi = new AreaDeliveryApi();

            var listArea = areaApi.GetAllAreaDistrictByAreaId(areaId);
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
        public ActionResult UpdateDistrict(int detailId, decimal baseprice, int areaId)
        {
            var districtApi = new DistrictApi();
            try
            {
                var currentDistrict = districtApi.BaseService.Get(detailId);
                if (currentDistrict != null)
                {
                    currentDistrict.PriceDelivery = baseprice;
                    currentDistrict.AreaDistrictId = areaId;
                    districtApi.BaseService.Update(currentDistrict);
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


        public ActionResult IndexList(HmsService.Models.JQueryDataTableParamModel param, int areaId, int provinceId)
        {
            var districtApi = new DistrictApi();
            var areaApi = new AreaDeliveryApi();
            var districts = districtApi.GetAllDistrictByProvinceIDAndAreaId(provinceId, areaId);
            var totalRecord = districts.Count();
            if (!String.IsNullOrEmpty(param.sSearch))
            {
                districts = districts.Where(q => q.DistrictName.Contains(param.sSearch));
            }
            var totalResult = districts.Count();
            var count = param.iDisplayStart + 1;
            var result = districts.OrderBy(q => q.DistrictCode).Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList().Select(q => new IConvertible[] {
                count++,
                q.DistrictName,
                q.AreaDelivery != null ? q.AreaDelivery.AreaName : "N/A",
                q.PriceDelivery,
                q.DistrictCode,
                q.AreaDistrictId
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