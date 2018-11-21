using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    [Authorize(Roles = ("BrandManager, Manager, ProductManager"))]
    public class BrandController : DomainBasedController
    {
        // GET: Admin/Brand
        public ActionResult Index(string storeId)
        {
            ViewBag.storeId = storeId;
            return View();
        }
        
        //GetListBrandId
        public JsonResult IndexList(JQueryDataTableParamModel param)
        {
            var api = new BrandApi();
            var brandList = api.GetAllBrand();
            int count = 0;
            var rs = brandList
                //.Where(a => string.IsNullOrEmpty(param.sSearch) || a.BrandName.ToLower().Contains(param.sSearch.ToLower()))
                //.Skip(param.iDisplayStart)
                //.Take(param.iDisplayLength)
                .Select(a => new IConvertible[]
                {
                    ++count,
                    a.BrandName,
                    a.CompanyName,
                    a.ContactPerson,
                    a.PhoneNumber,
                    a.Fax != null ? a.Fax : "Chưa cập nhật",
                    a.Website != null ? a.Website: "Chưa cập nhật",
                    a.CreateDate.ToString("dd/MM/yyyy"),
                    a.Description,
                    a.Id
                }).ToList();
            int totalRecords = brandList.Count();
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        //CreateBrandIndex
        public ActionResult Create(string storeId)
        {
            BrandViewModel model = new BrandViewModel();
            ViewBag.storeId = storeId;
            return View(model);
        }

        //CreateBrand
        public async Task<JsonResult> CreateBrand(BrandViewModel model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var api = new BrandApi();
                    var brand = api.GetBrandByName(model.BrandName);
                    if(brand!=null)
                    {
                        return Json(new { success = false, message = "Thương hiệu đã tồn tại" });
                    }
                    else
                    {
                        await api.CreateBrandAsync(model);
                        return Json(new { success = true, message = "Tạo thương hiệu thành công" });
                    }                    
                }
                catch
                {
                    return Json(new { success = false, message = "Tạo thương hiệu thất bại" });
                }
            }
            else
            {
                {
                    return Json(new { success = false, message = "Tạo thương hiệu thất bại" });
                }
            }
        }

        //EditBrandIndex
        public ActionResult Edit(string storeId, int id)
        {
            var api = new BrandApi();
            var brand = api.GetBrandById(id);

            BrandViewModel model = new BrandViewModel();
            model = brand;

            ViewBag.storeId = storeId;
            return View(model);
        }

        //EditBrand
        public async Task<JsonResult> EditBrand(BrandViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var api = new BrandApi();
                    model.Active = true;
                    await api.EditBrandAsync(model);
                    return Json(new { success = true, message = "Chỉnh sửa thương hiệu thành công" });
                }
                catch
                {
                    return Json(new { success = false, message = "Chỉnh sửa thương hiệu thất bại" });
                }
            }
            else
            {
                {
                    return Json(new { success = false, message = "Chỉnh sửa thương hiệu thất bại" });
                }
            }
        }

        //Deactive Brand
        public async Task<JsonResult> DeactiveBrand(int id)
        {
            var api = new BrandApi();
            try
            {
                await api.DeactiveBrandAsync(id);
                return Json(new
                {
                    success = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new
                {
                    success = false,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        //Show assign store popup
        public ActionResult AssignStore(int id)
        {
            var api = new BrandApi();
            string name = api.GetBrandById(id).BrandName;
            ViewBag.BrandName = name;
            return View();
        }

        //Load list store by brandId
        public JsonResult LoadBrandStore(JQueryDataTableParamModel param, int id)
        {
            var api = new StoreApi();
            var storeList = api.GetListStoreByBrandAndNone(id);
            var count = 0;
            var rs = storeList
                .Skip(param.iDisplayStart).Take(param.iDisplayLength)
                //.Where(a => string.IsNullOrEmpty(param.sSearch) || a.Name.ToLower().Contains(param.sSearch.ToLower()))
                .Select(a => new IConvertible[] {
                    ++count,
                    a.Name,
                    "<input type=\"checkbox\" name='managerchk' id='" + a.ID + "' value='" + a.ID + "' "+(a.BrandId.Equals(id)?"checked='true'":"")+"/>"
                }).ToList();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = storeList.Count(),
                iTotalDisplayRecords = storeList.Count(),
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        //Update assigned store
        [HttpPost]
        public async Task<JsonResult> AssignStoreBrand(IEnumerable<SelectedStore> selectedStore, int selectedBrandId)
        {
            var storeUserApi = new StoreUserApi();
            var storeApi = new StoreApi();
            int[] listStore = (storeApi.GetListStoreByBrandAndNone(selectedBrandId)).Select(a => a.ID).ToArray();
            foreach (var store in selectedStore)
            {
                if (listStore.Contains(store.ID))
                {
                    if (!store.selected)
                    {
                        await storeApi.UnAssignStoreAsync(store.ID);
                    }
                    else
                    {
                        if (store.selected)
                        {
                            await storeApi.AssignStoreAsync(store.ID, selectedBrandId);
                        }
                    }
                }
                else
                {
                    if (store.selected)
                    {
                        await storeApi.AssignStoreAsync(store.ID, selectedBrandId);
                    }
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}