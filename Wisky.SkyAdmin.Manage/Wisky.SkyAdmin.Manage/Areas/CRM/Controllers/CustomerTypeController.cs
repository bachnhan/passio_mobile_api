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
using System.Data.Entity;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.CRM.Controllers
{
    public class CustomerTypeController : DomainBasedController
    {
        // GET: CRM/CustomerType
        public ActionResult Index()
        {
            return View();
        }
        //change return type -> async type
        public JsonResult LoadCustomerType(JQueryDataTableParamModel param, int brandID)
        {
            int count = 0;
            var customerTypeApi = new CustomerTypeApi();
            var customerTypes = customerTypeApi.GetAllCustomerTypes(brandID).ToList();
            IEnumerable<IConvertible[]> rs = null;
            int totalRecords;
            int totalDisplay;
            try
            {

                var searchList = customerTypes;
                    //.Where(a => string.IsNullOrEmpty(param.sSearch) ||
                    //(!string.IsNullOrEmpty(param.sSearch) && a.CustomerType1.ToLower().Contains(param.sSearch.ToLower())));

                rs = (searchList
                    //.OrderBy(t => t.ID)
                    //.Skip(param.iDisplayStart) //implicit called skip() required orderby()
                    //.Take(param.iDisplayLength)
                    .ToList())
                    .Select(a => new IConvertible[]
                        {
                                a.ID,
                                string.IsNullOrEmpty(a.CustomerType1) ? "Không xác định" : a.CustomerType1
                        });


                totalDisplay = searchList.Count();
                totalRecords = customerTypes.Count();




            }
            catch
            {
                return Json(new { success = false, message = "Error" });
            }


            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalDisplay,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }


        #region Create

        public async Task<ActionResult> CreateCustomerType()
        {
            var model = new CustomerTypeEditViewModel();
            await PrepareCreateCustomerType(model);

            return this.View(model);
        }

        [HttpPost]
        public async Task<ActionResult> CreateCustomerType(CustomerTypeEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareCreateCustomerType(model);
                return this.View(model);
            }
            var api = new CustomerTypeApi();

            await api.CreateAsync(model);
            return this.RedirectToAction("Index", "CustomerType");

        }

        private async Task PrepareCreateCustomerType(CustomerTypeEditViewModel model)
        {
            model.AvailableCustomerType = (new CustomerTypeApi()
                .Get()
                .ToSelectList(q => q.CustomerType1, q => q.CustomerType1, q => false));
        }

        #endregion

        #region Edit
        public async Task<ActionResult> EditCustomerType(int id)
        {
            var api = new CustomerTypeApi();
            var customerType = api.GetCustomerTypeById(id);
            var model = new CustomerTypeEditViewModel(customerType, this.Mapper);
            await PrepareCreateCustomerType(model);
            return View(model);
        }


        [HttpPost]
        public async Task<ActionResult> EditCustomerType(CustomerTypeEditViewModel model)
        {
            if (!ModelState.IsValid)
            {

                await PrepareEditCustomerType(model);
                return this.View(model);
            }
            var api = new CustomerTypeApi();

            await api.EditAsync(model.ID, model);

            return this.RedirectToAction("Index", "CustomerType");
        }

        private async Task PrepareEditCustomerType(CustomerTypeEditViewModel model)
        {
            // truyen id = 1 de xem ket qua
            int id = 1;
            model.AvailableCustomerType = (new CustomerTypeApi()
               .GetAllCustomerTypes(1))
               .ToSelectList(q => q.CustomerType1, q => q.ID.ToString(), q => model.ID == q.ID);
        }

        #endregion


        [HttpPost]
        public string ValidateCustomerType(string name)
        {
            var api = new CustomerTypeApi();
            var customerType = api.GetCustomerTypeByName(name);
            string result = "";
            if (customerType != null)
            {
                result = "duplicated";
            }
            return result;
        }
    }
}