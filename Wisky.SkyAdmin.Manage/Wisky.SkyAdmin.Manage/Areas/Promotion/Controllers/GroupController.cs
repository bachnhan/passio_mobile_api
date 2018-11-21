using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Promotion.Controllers
{
    public class GroupController : DomainBasedController
    {
        // GET: Promotion/Group
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult LoadGroup(JQueryDataTableParamModel param, int brandId)
        {
            var groupApi = new GroupApi();
            var listGroup = groupApi.GetGroupActive();
            int count = 0;
            try
            {
                count = param.iDisplayStart + 1;
                var rs = listGroup
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Name.ToLower().Contains(param.sSearch.ToLower()))
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList()
                    .Select(a => new IConvertible[]
                        {
                        count++,
                        string.IsNullOrEmpty(a.Name) ? "Không xác định" : a.Name,
                        a.Description,                       
                        a.GroupId
                        });
                var totalRecords = listGroup.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #region Create
        public ActionResult Create(string storeId)
        {
            ViewBag.storeId = storeId;
            var model = new GroupViewModel();

            return PartialView("Create", model);
        }

        [HttpPost]
        public async Task<ActionResult> Create(GroupViewModel model, int brandId)
        {
            if (!this.ModelState.IsValid)
            {
                return View(model);
            }
            var groupApi = new GroupApi();          
            await groupApi.CreateGroup(model);
            return RedirectToAction("Index", "Group");
        }
        #endregion

        #region Edit
        public async Task<ActionResult> Edit(int id, string storeId)
        {
            ViewBag.storeId = storeId;
            var groupApi = new GroupApi();
            var model = await groupApi.GetAsync(id);
            return PartialView("Edit", model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(GroupViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View(model);
            }
            var groupApi = new GroupApi();
            await groupApi.UpdateGroupAsync(model);
            return RedirectToAction("Index", "Group");

        }
        #endregion
        #region Delete
        public async Task<JsonResult> Delete(int id)
        {
            var groupApi = new GroupApi();
            var model = groupApi.Get(id);
            if (model == null)
            {
                return Json(new { success = false });
            }
            try
            {
                await groupApi.DeleteGroupAsync(model);
            }
            catch (System.Exception e)
            {
                throw e;
                //return Json(new { success = false });
            }
            return Json(new { success = true });
        }
        #endregion
    }
}