using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class BrandController : DomainBasedController
    {
        // GET: SysAdmin/Brand
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> UpLoadFileJson(int brandID, HttpPostedFileBase file)
        {
            ViewBag.UpfileJson = "";
            if (file != null && file.ContentLength > 0)
            {
                // extract only the filename
                var fileName = Path.GetFileName(file.FileName);
                // store the file inside ~/App_Data/uploads folder
                var path = Path.Combine(Server.MapPath("~/Configuration"), fileName);
                file.SaveAs(path);

                //update database
                BrandApi brandApi = new BrandApi();
                var item = brandApi.GetBrandById(brandID);
                item.JsonConfigUrl = fileName;
                await brandApi.EditAsync(brandID, item);
            }

            return View("Index");
        }
        public async Task<JsonResult> GetListBrands(JQueryDataTableParamModel param)
        {
            var brandApi = new BrandApi();
            var brands = brandApi.GetAllActiveAndInactiveBrands();
            var count = param.iDisplayStart;
            try
            {
                var searchResult = brands.Where(q => string.IsNullOrEmpty(param.sSearch) ||
                            (!string.IsNullOrEmpty(param.sSearch)
                            && q.BrandName.ToLower().Contains(param.sSearch.ToLower())));

                var rs = (await searchResult
                            .OrderByDescending(q => q.BrandName)
                            .ToListAsync())
                            .Select(q => new IConvertible[] {
                                ++count,
                                q.BrandName,
                                q.Description,
                                q.CreateDate.ToShortDateString(),
                                q.CompanyName,
                                q.JsonConfigUrl,
                                q.PhoneNumber,
                                q.Active,
                                q.Id,
                            });

                var totalRecords = brands.Count();
                var totalDisplayRecords = searchResult.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> UpdateSMS(string id, string apiKey, string securityKey, string typeSMS, string brandName)
        {
            try
            {
                var brandApi = new BrandApi();
                var brand = brandApi.Get(int.Parse(id));
                if (brand != null)
                {
                    brand.ApiSMSKey = apiKey;
                    brand.SecurityApiSMSKey = securityKey;
                    brand.SMSType = int.Parse(typeSMS);
                    brand.BrandNameSMS = brandName;
                    await brandApi.EditAsync(int.Parse(id), brand);
                }
                return Json(new
                {
                    success = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false
                }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetSMS(string id)
        {
            try
            {
                var brandApi = new BrandApi();
                var brand = brandApi.Get(int.Parse(id));
                if (brand.ApiSMSKey != null)
                {
                    return Json(new
                    {
                        ApiKey = brand.ApiSMSKey,
                        SecurityKey = brand.SecurityApiSMSKey,
                        SMSType = brand.SMSType,
                        BrandName = brand.BrandNameSMS,
                        success = true
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        ApiKey = brand.ApiSMSKey,
                        SecurityKey = brand.SecurityApiSMSKey,
                        SMSType = brand.SMSType,
                        BrandName = brand.BrandNameSMS,
                        success = true
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = true
                }, JsonRequestBehavior.AllowGet);
            }

        }

        public async Task<ActionResult> Detail(int Id)
        {
            var brandApi = new BrandApi();
            var model = await brandApi.GetAsync(Id);
            //if (model == null || !model.Active)
            //{
            //    return HttpNotFound();
            //}
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        public ActionResult Create()
        {
            var model = new BrandViewModel();
            //return View(model);
            return PartialView("_Create", model);
        }

        //[HttpPost]
        //public async Task<ActionResult> Create(BrandViewModel model)
        //{
        //    if (!this.ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //    var brandApi = new BrandApi();
        //    await brandApi.CreateBrandAsync(model);
        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        public async Task<JsonResult> Create(BrandViewModel model)
        {
            var brandApi = new BrandApi();
            try
            {
                await brandApi.CreateBrandAsync(model);
                return Json(new { success = true, message = "Tạo nhãn hiệu thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, tạo nhãn hiệu thất bại." }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> Edit(int Id)
        {
            var brandApi = new BrandApi();
            var model = await brandApi.GetAsync(Id);
            //if (model == null || !model.Active)
            //{
            //    return HttpNotFound();
            //}
            //return View(model);
            if (model == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Edit", model);
        }

        //[HttpPost]
        //public async Task<ActionResult> Edit(BrandViewModel model)
        //{
        //    if (!this.ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //    var brandApi = new BrandApi();
        //    await brandApi.EditAsync(model.Id, model);
        //    return this.RedirectToAction("Index");
        //}

        [HttpPost]
        public async Task<JsonResult> Edit(BrandViewModel model)
        {
            var brandApi = new BrandApi();
            try
            {
                await brandApi.EditAsync(model.Id, model);
                return Json(new { success = true, message = "Cập nhật nhãn hiệu thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, cập nhật nhãn hiệu thất bại." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int Id)
        {
            var brandApi = new BrandApi();
            try
            {
                await brandApi.DeleteAsync(Id);
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ChangeBrandActivation(int brandId)
        {
            try
            {
                var brandApi = new BrandApi();
                var model = brandApi.GetBrandById(brandId);

                if (model.Active)
                {
                    return await Deactivate(model);
                }
                else
                {
                    return await Activate(model);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }

        public async Task<JsonResult> Activate(BrandViewModel model)
        {
            try
            {
                var brandApi = new BrandApi();
                if (model == null || model.Active)
                {
                    return Json(new { success = false, message = "Kích hoạt thương hiệu thất bại. Xin thử lại!" });
                }

                await brandApi.ChangeBrandActivationAsync(model.Id);
                return Json(new { success = true, message = "Kích hoạt thương hiệu thành công!" });
            }
            catch (Exception)
            {
                return Json(new { success = true, message = "Kích hoạt thương hiệu thất bại. Xin thử lại!" });
            }
        }

        public async Task<JsonResult> Deactivate(BrandViewModel model)
        {
            try
            {
                var brandApi = new BrandApi();
                if (model == null || !model.Active)
                {
                    return Json(new { success = false, message = "Vô hiệu hóa thương hiệu thất bại. Xin thử lại!" });
                }

                await brandApi.ChangeBrandActivationAsync(model.Id);
                return Json(new { success = true, message = "Vô hiệu hóa thương hiệu thành công!" });
            }
            catch (Exception)
            {
                return Json(new { success = true, message = "Vô hiệu hóa thương hiệu thất bại. Xin thử lại!" });
            }
        }

    }
}