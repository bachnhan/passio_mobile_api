//using HmsService.Models;
//using HmsService.Sdk;
//using HmsService.ViewModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;

//namespace Wisky.SkyAdmin.Manage.Areas.MembershipType.Controllers
//{
//    public class MembershipTypeController : Controller
//    {
//        // GET: MembershipType/MembershipType
//        public ActionResult Index(int storeId)
//        {
//            ViewBag.storeId = storeId.ToString();

//            return View();
//        }
//        public JsonResult LoadMembershipType(JQueryDataTableParamModel param, int brandId)
//        {
//            var membershipTypeApi = new MembershipTypeApi();
//            var membershipTypes = membershipTypeApi.GetMembershipTypeByBrand(brandId);
//            int count = 0;
//            var searchList = membershipTypes
//                   .Where(a => string.IsNullOrEmpty(param.sSearch) || a.TypeName.ToLower().Contains(param.sSearch.ToLower()));
//            count = param.iDisplayStart + 1;
//            var rs = searchList
//                .Skip(param.iDisplayStart)
//                .Take(param.iDisplayLength).ToList()
//                .Select(a => new IConvertible[]
//                    {
//                        count++,
//                        string.IsNullOrEmpty(a.TypeName) ? "Không xác định" : a.TypeName,
//                        a.Id
//                    });
//            var totalRecords = membershipTypes.Count();
//            var totalDisplayRecords = searchList.Count();
//            return Json(new
//            {
//                sEcho = param.sEcho,
//                iTotalRecords = totalRecords,
//                iTotalDisplayRecords = totalDisplayRecords,
//                aaData = rs
//            }, JsonRequestBehavior.AllowGet);
//        }
//        #region Create
//        [HttpGet]
//        public ActionResult Create(MembershipTypeViewModel model)
//        {
//            //var model = new MembershipTypeViewModel();
//            return PartialView("Create", model);

//        }

//        [HttpPost]
//        public async Task<ActionResult> Create(MembershipTypeViewModel model, int brandId)
//        {
//            if (!this.ModelState.IsValid)
//            {
//                return View(model);
//            }
//            var membershipTypeApi = new MembershipTypeApi();

//            //model.CustomerName = customerApi.GetCustomerByID(model.CustomerId).Name;
//            model.Active = true;
//            model.BrandId = brandId;
//            await membershipTypeApi.CreateMembershipTypeAsync(model);
//            return RedirectToAction("Index", "MembershipType");

//        }
//        #endregion
//        #region Edit

//        public ActionResult Edit(int id, int brandId)
//        {
//            var membershipTypeApi = new MembershipTypeApi();
//            var model = membershipTypeApi.Get(id);
//            return PartialView("Edit", model);

//        }

//        [HttpPost]
//        public ActionResult Edit(MembershipTypeViewModel model)
//        {
//            if (!this.ModelState.IsValid)
//            {
//                return View(model);
//            }
//            var membershipTypeApi = new MembershipTypeApi();
//            membershipTypeApi.UpdateMembershipType(model);
//            return RedirectToAction("Index", "MembershipType");

//        }
//        #endregion
//        public async Task<JsonResult> Delete(int id)
//        {
//            var membershipTypeApi = new MembershipTypeApi();
//            var model = membershipTypeApi.Get(id);
//            if (model == null)
//            {
//                return Json(new { success = false });
//            }
//            try
//            {
//                await membershipTypeApi.DeleteMembershipTypeAsync(model);
//            }
//            catch (System.Exception e)
//            {
//                throw e;

//            }
//            return Json(new { success = true });
//        }

//        [HttpPost]
//        public async Task<string> ValidateType(string name)
//        {
//            var membershipTypeApi = new MembershipTypeApi();
//            var type = await membershipTypeApi.GetMembershipTypeByName(name);
//            string result = "";
//            if (type != null)
//            {
//                result = "duplicated";
//            }
//            return result;
//        }
//    }
//}