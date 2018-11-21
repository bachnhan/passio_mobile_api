using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    [Authorize(Roles = "BrandManager, Manager")]
    public class StoreController : DomainBasedController
    {
        // GET: Admin/Store
        public ActionResult Index(int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            return View();
        }

        public JsonResult LoadAllStoreInBrand(JQueryDataTableParamModel param, int brandId)
        {
            var storeApi = new StoreApi();
            var rawResult = storeApi.GetStoreByBrandId(brandId);
            int total = rawResult.Count();
            IEnumerable<StoreViewModel> filteredListItems;
            
            filteredListItems = rawResult.ToList();
            var count = param.iDisplayStart;

            var rp = filteredListItems.Select(a => new IConvertible[]
            {

                ++count,
                a.Name,
                a.Address,
                a.Phone,
                a.ID,
            });

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = filteredListItems.Count(),
                aaData = rp
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadStoreByStatusDatatables(JQueryDataTableParamModel param, int brandId)
        {
            var storeApi = new StoreApi();
            var rawResult = storeApi.GetStoreByBrandId(brandId);
            int total = rawResult.Count();
            IEnumerable<StoreViewModel> filteredListItems;
            //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            //var sortDirection = Request["sSortDir_0"];

            // Search.
            //if (!string.IsNullOrEmpty(param.sSearch))
            //{
            //    filteredListItems = rawResult.Where(
            //        d => (d.Name != null && d.Name.ToLower().Contains(param.sSearch.ToLower()))

            //    );
            //}
            //else
            //{
            //    filteredListItems = rawResult;
            //}

            //Func<StoreViewModel, object> sortBy = (s => s.ID);
            //switch (sortColumnIndex)
            //{
            //    case 2:
            //        sortBy = (s => s.Name);
            //        break;

            //}


            //filteredListItems = sortDirection == "asc"
            //    ? filteredListItems.OrderBy(sortBy)
            //        : filteredListItems.OrderByDescending(sortBy);
            //// Paging.
            //var displayedList = filteredListItems.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
            filteredListItems = rawResult.ToList();
            var count = param.iDisplayStart;

            var rp = filteredListItems.Select(a => new IConvertible[]
            {

                ++count,
                a.ID,
                a.Name,
                a.Address,
                a.Phone,
                ((StoreTypeEnum)a.Type).ToString(),
                "N/A",
                (  a.isAvailable==true ?"<i class=\"glyphicon glyphicon-ok\"></i>":"<i class=\"glyphicon glyphicon-remove\"></i>"),
                 "<a title=\"Chi tiết\" class=\"btn btn-primary btn-sm\" href=\"Store/Edit/"+a.ID+"\">" +
                                        "<i class=\"glyphicon glyphicon-eye-open\"></i></a> " +
                                        "<span class=\"btn btn-danger btn-sm\" style=\"display:none\"><i class=\"glyphicon glyphicon-trash\"></i></span>" +
                "<a title=\"Chỉnh sửa\" class=\"btn btn-success btn-assign btn-sm\" onclick=\"showAsignUser(" + a.ID + ", '" + a.Name + "')\">" +
                                        "<i class=\"glyphicon glyphicon-pencil\"></i></a>" +
                                        "<span class=\"btn btn-danger btn-sm\" style=\"display:none\"><i class=\"glyphicon glyphicon-trash\"></i></span>"
                //+"<a class=\"btn btn-danger btn-sm\" title=\"Hủy cửa hàng\" onclick=\"deactiveStore(" + a.ID + ")\"  style='margin-left: -3px;' >" +
                //                        "<i class=\"glyphicon glyphicon-trash\"></i></a>"
            });

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = filteredListItems.Count(),
                aaData = rp
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            var store = new StoreEditViewModel();
            //store.BrandId = brandId;
            //ViewBag.storeId = storeId;
            PrepareCreate(store);
            return View(store);
        }

        [HttpPost]
        public async Task<JsonResult> Create(StoreEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var storeApi = new StoreApi();
                    model.isAvailable = true;
                    var productApi = new ProductApi();
                    var listProduct = productApi.GetAllProducts().Where(q => q.Active && q.ProductCategory.BrandId == model.BrandId.Value);
                    await storeApi.CreateStoreAsync(model, listProduct);
                    return Json(new { success = true, message = "Tạo cửa hàng thành công" });
                }
                catch
                {
                    return Json(new { success = false, message = "Tạo cửa hàng thất bại" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Tạo cửa hàng thất bại" });
            }
        }

        public async Task<ActionResult> Edit(int Id, string storeId)
        {
            ViewBag.storeId = storeId;
            var storeApi = new StoreApi();
            var store = new StoreEditViewModel(await storeApi.GetStoreByID(Id), this.Mapper);
            if (store == null)
            {
                return HttpNotFound();
            }
            PrepareEdit(store);
            return View(store);
        }

        [HttpPost]
        public async Task<JsonResult> Edit(StoreViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var storeApi = new StoreApi();
                    await storeApi.EditStoreAsync(model);
                    return Json(new { success = true, message = "Sửa thông tin cửa hàng thành công" });
                }
                catch
                {
                    return Json(new { success = false, message = "Sửa thông tin cửa hàng thất bại" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Sửa thông tin cửa hàng thất bại" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeactiveStore(int id)
        {
            try
            {
                var storeApi = new StoreApi();
                await storeApi.DeactiveStore(id);
                return Json(new { success = true, message = "Hủy cửa hàng thành công" });
            }
            catch
            {
                return Json(new { success = false, message = "Hủy cửa hàng thất bại" });
            }
        }

        public ActionResult AsignUser()
        {
            return View();
        }

        private void PrepareCreate(StoreEditViewModel model)
        {
            model.StoreTypeEnum = (StoreTypeEnum)model.Type;
        }
        private void PrepareEdit(StoreEditViewModel model)
        {
            model.StoreTypeEnum = (StoreTypeEnum)model.Type;
        }

        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> ManagerStore(IEnumerable<SelectedUser> selectedUser, int? selectedStoreId)
        {
            var storeUserApi = new StoreUserApi();
            var storeApi = new StoreApi();
            string[] listStoreManager = (await storeApi.GetStoreUserByStoreIdAsync(selectedStoreId.Value)).Select(a => a.Username).ToArray();
            foreach (var user in selectedUser)
            {
                if (listStoreManager.Contains(user.username))
                {
                    if (!user.selected)
                    {
                        await storeUserApi.DeleteStoreUserAsync(user.username, selectedStoreId.Value);
                    }
                }
                else
                {
                    if (user.selected)
                    {
                        await storeUserApi.CreateAsync(new StoreUserViewModel
                        {
                            Username = user.username,
                            StoreId = selectedStoreId.Value,
                        });
                    }
                }
            }
            return Json(new {success = true }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> LoadManagerStore(int selectedStoreId, int brandId)
        {
            var aspNetUserAPi = new AspNetUserApi();
            var storeApi = new StoreApi();
            var rawResult = aspNetUserAPi.GetStoreManagersByBrandId(brandId);
            //IEnumerable<StoreUser> filteredListItems;


            // Paging
            //var displayedList = rawResult.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
            var count = 0;

            string[] listStoreManager = (await storeApi.GetStoreUserByStoreIdAsync(selectedStoreId)).Select(a => a.Username).ToArray();

            //var searchList = rawResult.Where(a => string.IsNullOrEmpty(param.sSearch)
            //            || a.UserName.ToLower().Contains(param.sSearch.ToLower()));

            var rs = rawResult.ToList() /*searchList.ToList()*/
                .Select(
                a => new IConvertible[]
                {
                    ++count,
                    a.UserName,
                    a.FullName,
                    listStoreManager.Contains(a.UserName),                  
                }
                );




            //return Json(new
            //{
            //   //sEcho = param.sEcho,
            //    //iTotalRecords = totalRecords,
            //   // iTotalDisplayRecords = totalDisplayRecords,
            //    aaData = rs.ToArray(),
            //}, JsonRequestBehavior.AllowGet);
             return Json(rs.ToArray());
        }
    }
    //    public async Task<JsonResult> LoadManagerStore(JQueryDataTableParamModel param, int selectedStoreId, int brandId)
    //    {
    //        var aspNetUserAPi = new AspNetUserApi();
    //        var storeApi = new StoreApi();
    //        var rawResult = aspNetUserAPi.GetStoreManagersByBrandId(brandId);
    //        //IEnumerable<StoreUser> filteredListItems;


    //        // Paging
    //        //var displayedList = rawResult.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

    //        var count = param.iDisplayStart;

    //        string[] listStoreManager = (await storeApi.GetStoreUserByStoreIdAsync(selectedStoreId)).Select(a => a.Username).ToArray();

    //        var searchList = rawResult.Where(a => string.IsNullOrEmpty(param.sSearch)
    //                    || a.UserName.ToLower().Contains(param.sSearch.ToLower()));

    //        var rs = searchList.ToList()
    //            .Skip(param.iDisplayStart)
    //            .Take(param.iDisplayLength).Select(
    //            a => new IConvertible[]
    //            {
    //                ++count,
    //                a.UserName,
    //                a.FullName,
    //                "<input type=\"checkbox\" name='managerchk' id='" + a.UserName + "' value='" + a.UserName + "' "+(listStoreManager.Contains(a.UserName)?"checked='true'":"")+"/>"
    //            }
    //            );


    //        int totalRecords = rawResult.Count();
    //        int totalDisplayRecords = searchList.Count();

    //        //return Json(new
    //        //{
    //        //    sEcho = param.sEcho,
    //        //    iTotalRecords = totalRecords,
    //        //    iTotalDisplayRecords = totalDisplayRecords,
    //        //    aaData = rs.ToArray(),
    //        //}, JsonRequestBehavior.AllowGet);
    //        return Json(rs.ToArray());
    //    }
    //}
}