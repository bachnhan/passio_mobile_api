using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Cors;
using Wisky.SkyAdmin.Manage.Controllers;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Collections;
using System.Web.Helpers;
using System.Globalization;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Http;
using Wisky.Api.Connection;

namespace Wisky.Api.Controllers.API
{
    public class MembershipCardNewApiController : BaseController
    {
        private static readonly string _storeCode = System.Configuration.ConfigurationManager.AppSettings["config.store.code"];
        private readonly string _accessToken = System.Configuration.ConfigurationManager.AppSettings["config.order.token"];
        private readonly string _enableToken = System.Configuration.ConfigurationManager.AppSettings["config.order.EnableToken"];


        private readonly ICustomerService _customerService = DependencyUtils.Resolve<ICustomerService>();
        private readonly IProductService _productService = DependencyUtils.Resolve<IProductService>();
        private readonly IOrderService _orderService = DependencyUtils.Resolve<IOrderService>();

        [HttpPost]
        //[Route("MembershipCardNewApi/GetCustomerBalanceAndNameByCode/{token}/{terminalId}")]
        public JsonResult GetCustomerBalanceAndNameByCode(Card model)
        {
            int status = (int)MembershipStatusEnum.Inactive;
            double? balance;
            string message;
            string customerName = null;
            var storeApi = new StoreApi();
            var currentBrand = storeApi.GetStoreById(model.terminalId).BrandId;
            var accountApi = new AccountApi();
            var membershipCardApi = new MembershipCardApi();
            var membershipCard = membershipCardApi.GetMembershipCardByCode(model.membershipCardCode);
            if (membershipCard == null || membershipCard.BrandId != currentBrand)
            {
                message = "Không tìm thấy thẻ. Xin kiểm tra lại.";
                return Json(new
                {
                    Name = customerName,
                    Balance = 0,
                    Status = (int)MembershipStatusEnum.Inactive,
                    Message = message,
                });
            }
            if (!membershipCard.Active)//TODO: chua kiểm tra membershipType
            {
                message = "Thẻ đã bị vô hiệu hóa. Không sử dụng được.";
                return Json(new
                {
                    Name = customerName,
                    Balance = 0,
                    Status = (int)MembershipStatusEnum.Suspensed,
                    Message = message,
                });
            }
            else
            {
                status = (int)MembershipStatusEnum.Active;
            }
            var customer = membershipCard.Customer;
            customerName = customer == null ? "" : customer.Name;

            foreach (var account in membershipCard.Accounts)
            {
                if (account.Type == (int)AccountTypeEnum.CreditAccount)
                {
                    balance = (double?)account.Balance;
                    if (balance == null)
                    {
                        message = "Số dư của tài khoản mặc định rỗng";
                        balance = 0;
                        return Json(new
                        {
                            Name = customerName,
                            Balance = balance,
                            Status = status,
                            Message = message,
                        });
                    }
                    else
                    {
                        message = "Kiểm tra thành công!";
                        return Json(new
                        {
                            Name = customerName,
                            Balance = balance,
                            Status = status,
                            Message = message,
                        });
                    }
                }
            }

            message = "Tài khoản thanh toán không tồn tại. Xin tạo tài khoản cho khách hàng.";
            return Json(new
            {
                Name = customerName,
                Balance = 0,
                Status = status,
                Message = message,
            });
        }

        [HttpPost]
        public JsonResult GetGiftTalkCardDetail(Card model)
        {
            try
            {
                var storeApi = new StoreApi();
                int brandId = storeApi.GetStoreById(model.terminalId).BrandId.GetValueOrDefault();
                GiftTalkConnectApi giftTalkApi = new GiftTalkConnectApi();
                var checkGiftCard = giftTalkApi.ThirdPartyCardDetail(model.membershipCardCode, brandId, model.terminalId);
                return Json(new
                {
                    success = checkGiftCard.result,
                    message = checkGiftCard.message,
                    Enum = checkGiftCard.Enum,
                    result = checkGiftCard.result
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                });
            }
        }

        [HttpPost]
        //[Route("MembershipCardNewApi/GetCustomerBalanceAndNameByCode/{token}/{terminalId}")]
        public JsonResult GetMembershipCardDetail(Card model)
        {
            try
            {
                string customerName = null;
                int customerId = -1;
                var storeApi = new StoreApi();
                var currentBrand = storeApi.GetStoreById(model.terminalId).BrandId;
                var membershipApi = new MembershipCardApi();
                var membershipCard = membershipApi.GetMembershipCardByCode(model.membershipCardCode);
                if (membershipCard == null || membershipCard.BrandId != currentBrand)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy thẻ. Xin kiểm tra lại."
                    });
                }
                var status = ((MembershipStatusEnum)membershipCard.Status).ToString();
                customerName = membershipCard.Customer == null ? "" : membershipCard.Customer.Name;
                if (membershipCard.CustomerId != null)
                {
                    customerId = membershipCard.CustomerId.Value;
                }
                var accounts = membershipCard.Accounts.Where(q => q.Active.Value).Select(q => new AccountDetail
                {
                    AccountCode = q.AccountCode,
                    Level = q.Level_,
                    Balance = q.Balance,
                    Type = q.Type,
                    ProductCode = q.ProductCode,
                    BrandId = q.BrandId
                });

                var code = membershipCard.MembershipCardType.AppendCode == null ? "" : membershipCard.MembershipCardType.AppendCode;
                code += "_" + membershipCard.MembershipCardCode;

                return Json(new
                {
                    success = true,
                    message = "Tìm thấy thẻ thành viên",
                    Enum = (int)PaymentTypeEnum.MemberPayment,
                    result = new
                    {
                        CustomerID = customerId,
                        CustomerName = customerName,
                        Accounts = accounts,
                        Code = code,
                        Status = status,
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                });
            }


        }
        public JsonResult GetCardDetail(Card model)
        {
            try
            {
                string customerName = null;
                int customerId = -1;
                var storeApi = new StoreApi();
                int brandId = storeApi.GetStoreById(model.terminalId).BrandId.GetValueOrDefault();
                var currentBrand = storeApi.GetStoreById(model.terminalId).BrandId;
                var membershipApi = new MembershipCardApi();
                var membershipCard = membershipApi.GetMembershipCardByCode(model.membershipCardCode);
                if (membershipCard == null || membershipCard.BrandId != currentBrand)
                {
                    GiftTalkConnectApi giftTalkApi = new GiftTalkConnectApi();
                    var checkGiftCard = giftTalkApi.ThirdPartyCardDetail(model.membershipCardCode, brandId, model.terminalId);
                    if (checkGiftCard.success == true)
                    {
                        return Json(new
                        {
                            success = true,
                            message = checkGiftCard.message,
                            Enum = checkGiftCard.Enum,
                            result = checkGiftCard.result
                        });
                    }
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy thẻ. Xin kiểm tra lại."
                    });
                }
                var status = ((MembershipStatusEnum)membershipCard.Status).ToString();
                var productApi = new ProductApi();
                customerName = membershipCard.Customer == null ? "" : membershipCard.Customer.Name;
                if (membershipCard.CustomerId != null)
                {
                    customerId = membershipCard.CustomerId.Value;
                }
                var accounts = membershipCard.Accounts.Where(q => q.Active.Value).Select(q => new AccountDetail
                {
                    AccountCode = q.AccountCode,
                    Level = q.Level_,
                    Balance = q.Balance,
                    Type = q.Type,
                    ProductCode = q.ProductCode,
                    BrandId = q.BrandId
                });

                var code = membershipCard.MembershipCardType.AppendCode == null ? "" : membershipCard.MembershipCardType.AppendCode;
                code += "_" + membershipCard.MembershipCardCode;

                return Json(new
                {
                    success = true,
                    message = "Tìm thấy thẻ thành viên",
                    Enum = (int) PaymentTypeEnum.MemberPayment,
                    result = new
                    {
                        CustomerID = customerId,
                        CustomerName = customerName,
                        Accounts = accounts,
                        Code = code,
                        Status = status,
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                });
            }


        }
        [HttpPost]
        //[Route("MembershipCardNewApi/CheckAvailableCard/{token}/{terminalId}")]
        public JsonResult CheckAvailableCard(Card model)
        {
            var message = "";
            var storeApi = new StoreApi();
            var membershipCardApi = new MembershipCardApi();
            var currentBrand = storeApi.GetStoreById(model.terminalId).BrandId;
            var membershipCard = membershipCardApi.GetMembershipCardByCode(model.membershipCardCode);
            if (membershipCard == null || membershipCard.BrandId != currentBrand)
            {
                message = "Thẻ có thể sử dụng";
                return Json(new
                {
                    Success = true,
                    Message = message
                });
            }
            if (membershipCard.Active)//TODO: chua kiểm tra membershipType
            {
                if (membershipCard.Status == (int)MembershipStatusEnum.Inactive)
                {
                    message = "Thẻ đã tạo nhưng chưa kích hoạt.";
                    return Json(new
                    {
                        Success = false,
                        Message = message
                    });
                }

                else if (membershipCard.Status == (int)MembershipStatusEnum.Suspensed)
                {
                    message = "Thẻ bị tạm ngưng.";
                    return Json(new
                    {
                        Success = false,
                        Message = message
                    });
                }
                else
                {
                    message = "Thẻ đang hoạt đọng.";
                    return Json(new
                    {
                        Success = false,
                        Message = message
                    });
                }

            }
            else
            {
                message = "Thẻ đã bị vô hiệu. ";
                return Json(new
                {
                    Success = false,
                    Message = message
                });
            }

        }
        [HttpPost]
        public async Task<JsonResult> CreateMemberShipCardWithAccounts(MembershipCustomerModel customer)
        {
            String message;
            try
            {
                #region CreateMemberShipCard
                int memCardId;
                int id = 0;
                var membershipCardApi = new MembershipCardApi();
                var accountApi = new AccountApi();
                var customerApi = new CustomerApi();
                var storeApi = new StoreApi();

                var currentBrand = storeApi.GetStoreById(customer.TerminalId).BrandId;
                //   var accounts = listAccounts;
                MembershipCardViewModel cardModel = new MembershipCardViewModel();
                int? brandId = storeApi.GetStoreById(customer.TerminalId) != null ?
                           storeApi.GetStoreById(customer.TerminalId).BrandId : null;
                var memCard = membershipCardApi.GetMembershipCardByCode(customer.MembershipCardCode);
                var sampleCard = membershipCardApi.GetAllMembershipCardSampleByBrandId(currentBrand.GetValueOrDefault()).FirstOrDefault(p => p.MembershipCardCode.Equals(customer.SampleShipCardCode));
                var cusModel = customerApi.GetCustomerByID(customer.CustomerID);
                var date = customer.CreatedTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                if (cusModel != null)
                {
                    if (memCard == null)
                    {
                        cardModel.MembershipCardCode = customer.MembershipCardCode;
                        cardModel.MembershipTypeId = sampleCard.MembershipTypeId;
                        cardModel.CreatedTime = date.ToDateTime();
                        cardModel.BrandId = brandId.GetValueOrDefault();
                        cardModel.IsSample = false;
                        cardModel.StoreId = int.Parse(customer.StoreId);
                        cardModel.CustomerId = customer.CustomerID;
                        cardModel.ProductCode = customer.ProductCode;
                        memCardId = await membershipCardApi.CreateMembershipCardAsync(cardModel);
                    }
                    else
                    {
                        message = "Mã thẻ đã tồn tại, xin nhập mã thẻ khác.";
                        return Json(new
                        {
                            Success = false,
                            Message = message
                        });
                    }
                }
                else
                {
                    message = "Khách hàng không tồn tại . ";
                    return Json(new
                    {
                        Success = false,
                        Message = message
                    });
                }

                #endregion
                #region Create Accounts

                foreach (var data in sampleCard.Accounts)
                {
                    AccountViewModel newAccount = new AccountViewModel();
                    newAccount.AccountCode = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    newAccount.AccountName = customer.CustomerName;
                    newAccount.Type = data.Type;

                    string accStartDate = date;
                    if (!string.IsNullOrEmpty(accStartDate) && !accStartDate.Equals("N/A"))
                    {
                        newAccount.StartDate = accStartDate.ToDateTime();
                    }

                    string accFinishDate = "08/08/2018";
                    if (!string.IsNullOrEmpty(accFinishDate))
                    {
                        newAccount.FinishDate = accFinishDate.ToDateTime().GetEndOfDate();
                    }

                    newAccount.Balance = data.Balance;

                    string productCode = data.ProductCode;
                    if (data.Type == (int)AccountTypeEnum.GiftAccount && !string.IsNullOrWhiteSpace(productCode))
                    {
                        newAccount.ProductCode = productCode;
                    }


                    string level = data.Level_.ToString();
                    if (data.Type == (int)AccountTypeEnum.PointAccount && !string.IsNullOrWhiteSpace(level))
                    {
                        newAccount.Level_ = data.Level_;
                    }
                    else
                    {
                        newAccount.Level_ = 0;
                    }

                    newAccount.MembershipCardId = memCardId;
                    newAccount.BrandId = brandId;
                    newAccount.Active = true;
                    await accountApi.CreateAsync(newAccount);
                }
                #endregion
            }
            catch (Exception e)
            {
                message = "Có lỗi xảy ra, vui lòng thử lại.";
                return Json(new
                {
                    Success = false,
                    Message = message
                });

            }
            return Json(new
            {
                Success = true
            });
        }

        [HttpPost]
        public JsonResult GetSampleMembershipCard(Card model)
        {
            try
            {
                int i = 0;
                var storeApi = new StoreApi();
                int brandId = storeApi.GetStoreById(model.terminalId).BrandId.GetValueOrDefault();
                var membershipCardApi = new MembershipCardApi();
                var account = new AccountApi();
                var customer = new CustomerApi();
                var customerTypeAPI = new CustomerTypeApi();
                var memCards = membershipCardApi.GetDeactiveListByBrandId(brandId)
                             .Where(q => q.IsSample == true).ToList();
                var typeList = customerTypeAPI.GetAllCustomerTypes(brandId).ToList();

                List<MembershipCardSampleModel> collection = new List<MembershipCardSampleModel>();


                foreach (var q in memCards)
                {
                    MembershipCardSampleModel s = new MembershipCardSampleModel();
                    s.MembershipCardCode = q.MembershipCardCode;
                    s.MembershipTypeId = q.MembershipTypeId;
                    s.ProductCode = q.ProductCode;
                    s.Account = new List<SampleAccount>();

                    foreach (var item in q.Accounts)
                    {
                        s.Account.Add(new SampleAccount(item.Balance, item.Type));
                    }
                    collection.Add(s);
                };
                string list2 = JsonConvert.SerializeObject(collection, Formatting.Indented);

                return Json(new
                {
                    list = list2
                });
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpPost]
        public JsonResult GetAllCustomerType(int terminalId)
        {
            var storeApi = new StoreApi();
            var customerTypeAPI = new CustomerTypeApi();
            int brandId = storeApi.GetStoreById(terminalId).BrandId.GetValueOrDefault();
            var typeList = customerTypeAPI.GetAllCustomerTypes(brandId).ToList();
            List<CustomCustomerType> listType = new List<CustomCustomerType>();
            foreach (var item in typeList)
            {
                listType.Add(new CustomCustomerType(item.ID, item.CustomerType1));
            }

            string json = JsonConvert.SerializeObject(listType, Formatting.Indented);
            List<CustomCustomerType> products = JsonConvert.DeserializeObject<List<CustomCustomerType>>(json);

            return Json(new
            {
                list = json
            });
        }


        public class CustomCustomerType
        {
            public CustomCustomerType(int ID, string CustomerType)
            {
                this.ID = ID;
                this.CustomerType = CustomerType;
            }
            public virtual int ID { get; set; }
            public virtual string CustomerType { get; set; }
        }


        public class MembershipCustomerModel
        {
            public int CustomerID { get; set; }
            public string CustomerName { get; set; }
            public int TerminalId { get; set; }
            public String StoreId { get; set; }
            public string MembershipCardCode { get; set; }
            public virtual System.DateTime CreatedTime { get; set; }
            public string SampleShipCardCode { get; set; }
            public string ProductCode { get; set; }
        }

        public class Card
        {
            public string token { get; set; }
            public int terminalId { get; set; }
            public string membershipCardCode { get; set; }
        }

        public class AccountDetail
        {
            public string AccountCode { get; set; }
            public decimal? Balance { get; set; }
            public string ProductCode { get; set; }
            public int? Type { get; set; }
            public short Level { get; set; }
            public int? BrandId { get; set; }
        }

        public class MembershipCardSampleModel
        {
            public string MembershipCardCode { get; set; }
            public Nullable<int> MembershipTypeId { get; set; }
            public List<SampleAccount> Account { get; set; }
            public string ProductCode { get; set; }
        }

        public class SampleAccount
        {
            public SampleAccount(decimal? balance, int? type)
            {
                this.Balance = balance;
                this.Type = type;
            }
            public Nullable<decimal> Balance { get; set; }
            public Nullable<int> Type { get; set; }

        }
    }
}
