using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    public class StoreManagerController : DomainBasedController
    {
        // GET: Admin/StoreManager
        public ActionResult Index(int storeId)
        {
            ViewBag.StoreId = storeId.ToString();
            return View();
        }//End Index

        public JsonResult LoadEmployeeStore(JQueryDataTableParamModel param, int? brandId, int? storeId)
        {
            var storeUserApi = new StoreUserApi();
            var userApi = new AspNetUserApi();

            int count = 0;
            //IEnumerable<IConvertible[]> rs = null;
            int totalRecords;


            var storeUsers = storeUserApi.GetStoreUserWithStoreId(storeId.Value);
            var employees = userApi.GetListUserByStoreUsers(storeUsers);
            // remove role brandmanager and storemanager
            // Edit by HienLN 
            var listUser = new List<AspNetUser>();
            foreach (var employee in employees)
            {
                var flag = true;
                
                foreach (var item in employee.AspNetRoles)
                {
                    if (item.Name.Equals("StoreManager") || item.Name.Equals("BrandManager"))
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    listUser.Add(employee);
                }

            }
            count = param.iDisplayStart + 1;
            var rs = (listUser.Where(a => string.IsNullOrEmpty(param.sSearch) ||
                        (!string.IsNullOrEmpty(param.sSearch)
                        && a.UserName.ToLower().Contains(param.sSearch.ToLower())))
                        .OrderByDescending(a => a.UserName)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList())
                    .Select(a => new object[]
                        {
                        count++,
                        string.IsNullOrEmpty(a.UserName) ? "Không xác định" : a.UserName,
                        string.IsNullOrEmpty(a.FullName) ? "Không xác định" : a.FullName,
                        a.AspNetRoles.Where(r => !r.Name.Equals("ActiveUser")).Select(r => new {Id = r.Id, Name = Utils.DisplayName((RoleTypeEnum)Enum.Parse(typeof(RoleTypeEnum), r.Name))}).ToArray(),
                        a.Id
                        });

            totalRecords = listUser.Count();


            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);


        }//End LoadEmployeeStore

        public ActionResult AddEmployee(int? brandId, int? storeId)
        {
            ViewBag.StoreId = storeId.ToString();
            return View();
        }//End AddEmployee

        public JsonResult LoadBrandEmployee(JQueryDataTableParamModel param, int? brandId, int? storeId)
        {
            var storeUserService = this.Service<IStoreUserService>();
            var userService = this.Service<IAspNetUserService>();
            int count = 0;
            int totalRecords;


            var employees = userService.GetWorkedEmployeeByBrandId(brandId);

            var workedEmployees = userService.GetAllEmployee();
            count = param.iDisplayStart + 1;

            var rs = (employees.Where(a => string.IsNullOrEmpty(param.sSearch) ||
                         (!string.IsNullOrEmpty(param.sSearch)
                         && a.ToLower().Contains(param.sSearch.ToLower())))
                         .OrderByDescending(a => a)
                     .Skip(param.iDisplayStart)
                     .Take(param.iDisplayLength)
                     .ToList())
                     .Select(a => new object[]
                         {
                        count++,
                        string.IsNullOrEmpty(a) ? "Không xác định" : a,
                        string.IsNullOrEmpty(a) ? "Không xác định" : userService.GetEmployeeByUsername(a).FullName,
                        userService.GetEmployeeByUsername(a).Id
                         });

            totalRecords = employees.Count();


            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);


        }//End LoadBrandEmployee

        [HttpPost]
        public async Task<JsonResult> AddEmp(string id,int? storeId)
        {
            try
            {
                var storeUserService = this.Service<IStoreUserService>();
                var userService = this.Service<IAspNetUserService>();
                var newEmp = new StoreUser();
                var emp = userService.Get(id);
                newEmp.Username = emp.UserName;
                newEmp.StoreId = storeId.Value; 
                var storeApi = new StoreApi();
                var listStores = storeApi.GetStoreArrayByID(storeId.Value);
                await Utils.PostNotiMessageToStores(listStores, (int)NotifyMessageType.AccountChange);
                storeUserService.Create(newEmp);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }//End AddEmp

        [HttpPost]
        public async Task<JsonResult> DeleteEmp(int? storeId, string id)
        {
            try
            {
                var userService = this.Service<IAspNetUserService>();
                var storeUserService = this.Service<IStoreUserService>();
                var user = userService.Get(id);
                bool isDeleted = storeUserService.DeleteStoreUser(storeId, user.UserName);
                var storeApi = new StoreApi();
                var listStores = storeApi.GetStoreArrayByID(storeId.Value);
                await Utils.PostNotiMessageToStores(listStores, (int)NotifyMessageType.AccountChange);
               
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }//End Class
}//End Controller