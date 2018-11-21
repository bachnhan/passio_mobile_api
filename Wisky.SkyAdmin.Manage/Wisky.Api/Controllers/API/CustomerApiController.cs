using System;
using System.Collections.Generic;
using System.Linq;
//using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using Wisky.SkyAdmin.Manage.Controllers;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using HmsService.Models;
using SkyWeb.DatVM.Mvc;
using System.Web.Http;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Globalization;

namespace Wisky.Api.Controllers.API
{
    public class CustomerApiController : BaseController
    {

        private static readonly string _storeCode = System.Configuration.ConfigurationManager.AppSettings["config.store.code"];
        private readonly string _accessToken = System.Configuration.ConfigurationManager.AppSettings["config.order.token"];
        private readonly string _enableToken = System.Configuration.ConfigurationManager.AppSettings["config.order.EnableToken"];
        [System.Web.Http.HttpPost]

        public JsonResult CreateCustomer(CustomCustomerModel customer)
        {
            string message;
            int id = 0;
            var customerApi = new CustomerApi();
            var storeApi = new StoreApi();
            //   var accounts = listAccounts;
            MembershipCardViewModel cardModel = new MembershipCardViewModel();
            int? brandId = storeApi.GetStoreById(customer.TerminalId) != null ?
                       storeApi.GetStoreById(customer.TerminalId).BrandId : null;
            #region Create customer
            if (customerApi.GetCustomerByEmailOrPhone(customer.Phone, brandId.GetValueOrDefault()) != null)
            {

                message = "SĐT đã tồn tại";
                return Json(new
                {
                    Success = false,
                    Message = message
                });
            }


            else if (customer.Email!=null)
            {
                if (customerApi.GetCustomerByEmail(customer.Email, brandId.GetValueOrDefault()) != null)
                {
                    message = "Email đã tồn tại";
                    return Json(new
                    {
                        Success = false,
                        Message = message
                    });
                }
            }
          

            try
            {

                CustomerViewModel model = new CustomerViewModel()
                {
                    Name = customer.CustomerName,		
                    Address = customer.Address,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    IDCard = customer.IDCard,
                    Gender = (customer.Gender.Equals("Male")) ? true : false,
                    BirthDay = customer.BirthDay,
                    District = customer.District,
                    City = customer.City,
                    BrandId = brandId,
                    CustomerTypeId= customer.Type
                };
                id = customerApi.AddCustomer(model);
            }
            catch (Exception e)
            {


                message = "Chưa tạo được khách hàng  ";
                return Json(new
                {
                    Success = false,
                    Message = message
                });
            }
            #endregion

            message = " Thêm khách hàng thành công";
            return Json(new
            {
                Success = true,
                Message = message
            });
        }

        public JsonResult CheckCustomer(PhoneCustomerModel model)
        {
            try
            {
                var storeApi = new StoreApi();
                int brandId = storeApi.GetStoreById(model.TerminalID).BrandId.GetValueOrDefault();
                var customerApi = new CustomerApi();
                var customer = customerApi.GetCustomerByEmailOrPhone(model.Phone, brandId);
                if (customer != null)
                {
                    var type = "";
                    var typeApi = new CustomerTypeApi();
                    if (customer.CustomerTypeId != null)
                    {
                        type = typeApi.GetCustomerTypeById(customer.CustomerTypeId.Value).CustomerType1;
                    }
                    string gender = "Không xác định";
                    string date = "";
                    if (customer.Gender != null)
                    {
                        if (customer.Gender == true)
                            gender = "Nam";
                        else gender = "Nữ";
                    }
                    if (customer.BirthDay.HasValue)
                    {
                        date = customer.BirthDay.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        date = "N/A";
                    }
                    var cmnd = customer.IDCard;
                    var disctrict = customer.District;
                    var city = customer.City;
                    return Json(new
                    {
                        success = true,
                        customer = new
                        {
                            CustomerID = customer.CustomerID,
                            Type = customer.CustomerTypeId,
                            CustomerName = customer.Name,
                            Gender = gender,
                            Address = customer.Address,
                            Phone = customer.Phone,
                            Email = customer.Email ?? "N/A",
                            BirthDay = date,
                            IDCard = cmnd,
                            District = disctrict,
                            City = city,

                        }
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin khách hàng"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi trong quá trình xử lý ! xin vui lòng liên hệ admin"
                });
            }
        }

        public class SendCustomerModel
        {
            public int TerminalID { get; set; }
            public CustomCustomerModel Customer { get; set; }
            //public string MembershipCardCode { get; set; }
            //public string MembershipCardTypeName { get; set; }
        }
        public class CustomCustomerModel
        {
            public int CustomerID { get; set; }
            public string CustomerName { get; set; }
            public string Gender { get; set; }
            public virtual Nullable<int> Type { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string IDCard { get; set; }
            public Nullable<System.DateTime> BirthDay { get; set; }
            public string District { get; set; }
            public string City { get; set; }
            public int TerminalId { get; set; }
            public String StoreId { get; set; }
            public string MembershipCardCode { get; set; }
            public virtual System.DateTime CreatedTime { get; set; }
        }
        public class PhoneCustomerModel
        {
            public int TerminalID { get; set; }
            public string Phone { get; set; }
        }


    }
}

