using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    public class AreaDeliveryController : DomainBasedController
    {
        // GET: Admin/AreaDelivery
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult DetailArea(int id)
        {
            var areaApi = new AreaDeliveryApi();
            var area = areaApi.Get(id);
            ViewBag.AreaId = area.Id;
            ViewBag.NameArea = area.AreaName;
            return View();
        }


        public ActionResult GetListChildArea(HmsService.Models.JQueryDataTableParamModel param, int areaId)
        {
            var areaApi = new AreaDeliveryApi();
            var listAreaProvince = areaApi.GetAllAreaDistrictByAreaId(areaId).ToList();
            var count = 0;
            var result = listAreaProvince.Select(q => new IConvertible[] {
                count++,
                q.AreaName,
                q.PriceDelivery,
                q.Id
            });
            var totalRecords = result.Count();
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IndexList(HmsService.Models.JQueryDataTableParamModel param)
        {
            var areaApi = new AreaDeliveryApi();
            var listAreaProvince = areaApi.GetAllAreaProvince().ToList();
            var count = 0;
            var result = listAreaProvince.Select(q => new IConvertible[] {
                count++,
                q.AreaName,
                q.AreaDelivery1.Count(),
                q.PriceDelivery,
                q.Id
            });
            var totalRecords = result.Count();
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateArea(int detailId, decimal baseprice, string areaName)
        {
            var areaApi = new AreaDeliveryApi();
            try
            {
                var currentArea = areaApi.BaseService.Get(detailId);
                if (currentArea != null)
                {
                    currentArea.PriceDelivery = (decimal)baseprice;
                    currentArea.AreaName = areaName;
                    areaApi.BaseService.Update(currentArea);
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

        [HttpPost]
        public ActionResult CreateArea(decimal baseprice, string areaName)
        {
            var areaApi = new AreaDeliveryApi();
            try
            {
                var currentArea = new AreaDelivery();
                if (currentArea != null)
                {
                    currentArea.PriceDelivery = (decimal)baseprice;
                    currentArea.AreaType = (int)AreaTypeEnum.Province;
                    currentArea.AreaName = areaName;
                    areaApi.BaseService.Create(currentArea);
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

        [HttpPost]
        public ActionResult CreateChildArea(decimal baseprice, string areaName, int parentArea)
        {
            var areaApi = new AreaDeliveryApi();
            try
            {
                var currentArea = new AreaDelivery();
                if (currentArea != null)
                {
                    currentArea.PriceDelivery = (decimal)baseprice;
                    currentArea.AreaType = (int)AreaTypeEnum.District;
                    currentArea.AreaName = areaName;
                    currentArea.AreaID = parentArea;
                    areaApi.BaseService.Create(currentArea);
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
    }
}