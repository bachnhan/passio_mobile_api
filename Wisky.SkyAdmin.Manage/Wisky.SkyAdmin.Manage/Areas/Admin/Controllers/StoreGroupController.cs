using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    [Authorize(Roles = "BrandManager")]
    public class StoreGroupController : DomainBasedController
    {
        // GET: Admin/StoreGroup
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult LoadStoresWithStatus(JQueryDataTableParamModel param, int brandId, int storeGroupId)
        {
            var storeGroupApi = new StoreGroupApi();
            var storeGroup = storeGroupApi.Get(storeGroupId);
            var storesInGroup = storeGroup.StoresInGroup;
            var storeApi = new StoreApi();
            var stores = storeApi.GetAllActiveStore(brandId);

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                stores = stores.Where(q => q.Address.Contains(param.sSearch));
            }
            int total = stores.Count();

            int count = param.iDisplayStart;

            var rs = stores
                .ToList()
                .Select(a => new IConvertible[]
                {
                    ++count,
                    a.Name,
                    a.Address,
                    storesInGroup != null ? storesInGroup.Any(r => r.ID == a.ID) : false
                });
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = stores.Count(),
                aaData = rs
            }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult LoadAllStoreGroup(JQueryDataTableParamModel param,int brandId)
        {
            var storegroupApi = new StoreGroupApi();
            var storeGroups = storegroupApi.GetStoreGroupByBrandId(brandId);
            int total = storeGroups.Count();
            var count = param.iDisplayStart;

            var rs = storeGroups.ToList()
                .Select(a => new IConvertible[]
                {
                    ++count,
                    a.GroupName,
                    a.Description,
                    a.GroupID,
                });

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = total,
                iTotalDisplayRecords = storeGroups.Count(),
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        #region Create New StoreGroup
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Create(StoreGroupEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var storegroupapi = new StoreGroupApi();
                    await storegroupapi.CreateStoreGroupAsync(model);
                    return Json(new { success = true, message = "Tạo nhóm cửa hàng thành công" });
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
        #endregion

        #region Edit StoreGroup

        public async Task<ActionResult> Edit(int storeGroupId)
        {
            var storegroupApi = new StoreGroupApi();
            StoreGroupEditViewModel storegroup = new StoreGroupEditViewModel(await storegroupApi.GetAsync(storeGroupId), Mapper );
            if (storegroup == null)
            {
                return HttpNotFound();
            }
            return View(storegroup);
        }

        [HttpPost]
        public async Task<JsonResult> Edit(StoreGroupEditViewModel model)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var storegroupapi = new StoreGroupApi();
                    await storegroupapi.UpdateStoreGroupAsync(model);
                    return Json(new { success = true, message = "Cập nhập nhóm cửa hàng thành công" });
                }
                catch (Exception)
                {
                    return Json(new { success = false, message = "Cập nhập nhóm cửa hàng thất bại" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Cập nhập nhóm cửa hàng thất bại" });
            }
        }

        #endregion
        public ActionResult AsignStore()
        {
            return View();
        }
        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> ManagerStore(IEnumerable<SelectedStore> selectedStore, int selectedStoreGroupId)
        {
            var storeGroupMappingApi = new StoreGroupMappingApi();
            var storeApi = new StoreApi();
            int[] listStoreInGroup = (storeApi.GetStoreByGroupId(selectedStoreGroupId)).Select(a => a.ID).ToArray();
            foreach (var store in selectedStore)
            {
                if (listStoreInGroup.Contains(store.ID))
                {
                    if (!store.selected)
                    {
                        await storeGroupMappingApi.DeleteByStoreIDAndStoreGroupIDAsync(store.ID, selectedStoreGroupId);
                    }
                }
                else
                {
                    if (store.selected)
                    {
                        await storeGroupMappingApi.CreateAsync(new StoreGroupMappingViewModel
                        {
                            StoreID = store.ID,
                            StoreGroupID = selectedStoreGroupId,
                        });
                    }
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult LoadStoreInGroup(int selectedGroupId, int brandId)
        {
            var StoreApi = new StoreApi();
            var StoreGroupApi = new StoreGroupApi();
            var rawResult = StoreApi.GetAllStore(brandId).Where(a=>a.isAvailable== true);
            //IEnumerable<StoreUser> filteredListItems;


            // Paging
            //var displayedList = rawResult.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

            //var count = param.iDisplayStart;
            var count = 0;

            string[] listStoreInGroup = (StoreApi.GetStoreByGroupId(selectedGroupId).Where(a=>a.isAvailable==true)).Select(a => a.Name).ToArray();

            //var searchList = rawResult.Where(a => string.IsNullOrEmpty(param.sSearch)
            //            || a.Name.ToLower().Contains(param.sSearch.ToLower()));

            //var rs = searchList.ToList()
            //    .Skip(param.iDisplayStart)
            //    .Take(param.iDisplayLength).Select(
            //    a => new IConvertible[]
            //    {
            //        ++count,
            //        a.Name,
            //        a.Address,
            //        a.Phone,
            //        a.Email,
            //        listStoreInGroup.Contains(a.Name),
            //        a.ID,
            //    }
            //    );


            //int totalRecords = rawResult.Count();
            //int totalDisplayRecords = searchList.Count();

            //return Json(new
            //{
            //    sEcho = param.sEcho,
            //    iTotalRecords = totalRecords,
            //    iTotalDisplayRecords = totalDisplayRecords,
            //    aaData = rs
            //}, JsonRequestBehavior.AllowGet);



            var rs = rawResult.ToList()
                .Select(a => new IConvertible[]
                {
                    ++count,
                    a.Name,
                    a.Address,
                    a.Phone,
                    a.Email,
                    listStoreInGroup.Contains(a.Name),
                    a.ID,
                }
                );

            return Json(rs.ToArray());
        }


        public async Task<JsonResult> Delete(int storeGroupID)
        {
            var storeGroupApi = new StoreGroupApi();

            try
            {
                var storeGroup = await storeGroupApi.GetAsync(storeGroupID);
                var storeGroupEdit = new StoreGroupEditViewModel(storeGroup, this.Mapper);

                await storeGroupApi.DeleteStoreGroupAsync(storeGroupEdit);

                return Json(new { success = true, message = "Xóa nhóm cửa hàng thành công." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra. Xin hãy thử lại!" });
            }
        }
    }
}