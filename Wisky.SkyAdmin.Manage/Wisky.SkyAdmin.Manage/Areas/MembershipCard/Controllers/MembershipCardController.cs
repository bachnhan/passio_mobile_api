using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Validation;
using System.Globalization;
using Wisky.SkyAdmin.Manage.Controllers;
using LinqToExcel;
using CsvHelper;
using Newtonsoft.Json;
using Wisky.SkyAdmin.Manage.Areas.Delivery.Controllers;
using Wisky.SkyAdmin.Manage.Models.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Wisky.SkyAdmin.Manage.Areas.MembershipCard.Controllers
{
    public class MembershipCardController : DomainBasedController
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        #region CreateOrder MembershipCard
        public async Task<JsonResult> CreateOrderBuyMembershipCardAsync(OrderViewModel order, int storeId, int brandId, string membershipCardCode)
        {

            DateTime time = Utils.GetCurrentDateTime();
            if (order.CustomerID == 0 && order.Customer == null)
            {
                return Json(new
                {
                    success = false,
                    msg = "Không tìm thấy khách hàng"
                });
            }
            if (order.DeliveryAddress == null)
            {
                return Json(new
                {
                    success = false,
                    msg = "Không tìm thấy địa chỉ"
                });
            }
            if (order.StoreID == null)
            {
                return Json(new
                {
                    success = false,
                    msg = "Không tìm thấy cửa hàng"
                });
            }
            double tempTotalAmount = 0;
            double tempFinalAmount = 0;
            double discountOrderDetail = 0;
            foreach (var item in order.OrderDetails)
            {
                tempFinalAmount += item.FinalAmount;
                tempTotalAmount += item.TotalAmount;
                discountOrderDetail += item.Discount;
                item.OrderDate = time;
            }
            order.Payments = new List<PaymentViewModel>();
            order.Payments.Add(new PaymentViewModel
            {
                Amount = tempTotalAmount,
                CurrencyCode = "VND",
                Status = (int)PaymentStatusEnum.New,
                Type = (int)PaymentTypeEnum.Cash,
                FCAmount = (decimal)tempFinalAmount,
                PayTime = time,
                //Username = User.Identity.Name

            });


            order.CheckInDate = time;
            order.CheckInPerson = User.Identity.Name;
            order.TotalAmount = tempTotalAmount;
            //rent.FinalAmount = tempFinalAmount;       Final Amount = ToTal Amount - VATAmount - Discount
            //Calculator VAT amount
            //var vatAmount = (tempFinalAmount * 10 / 100); //VAT 10%
            var vatAmount = 0; //VAT 10%
            order.FinalAmount = tempFinalAmount - vatAmount;
            order.DiscountOrderDetail = discountOrderDetail;
            order.DeliveryStatus = (int)DeliveryStatus.Finish;
            order.OrderType = (int)OrderTypeEnum.OrderCard;
            order.OrderStatus = (int)OrderStatusEnum.Finish;
            order.InvoiceID = Utils.GetCurrentDateTime().Ticks.ToString() + "-" + order.StoreID;
            order.SourceType = (int)SourceTypeEnum.CallCenter; // Tam thoi de bang 0
            order.SourceID = storeId;
            order.GroupPaymentStatus = 0; //Tạm thời chưa xài đến
            if (order.Customer != null)
            {
                order.Customer.BrandId = brandId;
            }
            var orderApi = new OrderApi();
            var storeApi = new StoreApi();
            OrderCustomEntityViewModel orderEntity = new OrderCustomEntityViewModel()
            {
                Order = order,
                OrderDetails = order.OrderDetails,
                Customer = order.Customer,
            };

            //Create Tracansaction
            try
            {
                var transactionApi = new TransactionApi();
                var membershipCardApi = new MembershipCardApi();
                var currentCard = membershipCardApi.GetMembershipCardByCode(membershipCardCode);
                var accountApi = new AccountApi();
                var account = accountApi.GetAccountsByMembershipCardId(currentCard.Id).FirstOrDefault();
                var customer = currentCard.Customer;
                account.Balance += (decimal)order.FinalAmount;
                var message = (new BrandApi().Get(account.BrandId).BrandName) + ". "
                    + DateTime.Now.ToString("dd/MM/yyyy") + ";" + DateTime.Now.ToString("HH:mm") + ".  "
                    + "TK: " + "xxxxx" + currentCard.MembershipCardCode.Substring(5, currentCard.MembershipCardCode.Length - 5) + ". "
                    + "Nạp tiền: " + Utils.ToMoney(order.FinalAmount) + "VNĐ" + ". "
                    + "Số dư tài khoản: " + Utils.ToMoney((double)account.Balance) + "VNĐ";

                transactionApi.Create(new TransactionViewModel
                {
                    AccountId = account.AccountID,
                    Amount = (decimal)order.FinalAmount,
                    StoreId = storeId,
                    BrandId = brandId,
                    Date = DateTime.Now,
                    IsIncreaseTransaction = true,
                    TransactionType = (int)TransactionTypeEnum.Default,
                    Notes = order.Notes,
                    Status = (int)TransactionStatus.Approve,
                    UserId = User.Identity.Name
                });
                accountApi.Edit(account.AccountID, account);
                if (customer != null && customer.Phone != null)
                {
                    try
                    {
                        Utils.SendSMS(customer.Phone, message, brandId);
                    }
                    catch (Exception)
                    {

                    }
                  
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Tạo giao dịch không thành công!" });
            }


            var rs = 0;
            rs = orderApi.CreateOrderDelivery(orderEntity);
            //NotifyMessage sent Queue, and Pos
            var msg = new NotifyOrder()
            {
                StoreId = (int)order.StoreID,
                //StoreName = store.Name,
                NotifyType = (int)NotifyMessageType.OrderChange,
                Content = "Có đơn hàng mới",
                OrderId = rs,

            };
            await Utils.RequestOrderWebApi(msg);

            if (rs == 0)
            {
                return Json(new
                {
                    success = false,
                    msg = "Tạo đơn hàng không thành công"
                });
            }
            return Json(new
            {
                success = true,
                msg = "Tạo đơn hàng thành công"
            });
        }

        #endregion

        #region CreateMembershipCardStore
        public ActionResult CreateMembershipCardStore()
        {
            var brandId = int.Parse(RouteData.Values["brandId"]?.ToString());
            var storeId = int.Parse(RouteData.Values["storeId"]?.ToString());
            var customerTypeApi = new CustomerTypeApi();
            var model = new MembershipCardEditViewModel();
            model.Customer = new CustomerEditViewModel();
            model.Customer.AvailableType = customerTypeApi.GetAllCustomerTypes(brandId).Select(
                q => new SelectListItem
                {
                    Value = q.ID.ToString(),
                    Text = q.CustomerType1,
                    Selected = false
                });

            model.Customer.AvailableGender = new List<SelectListItem>();
            model.Customer.AvailableGender.Add(new SelectListItem()
            {
                Text = "Nam",
                Value = "true",
            });
            model.Customer.AvailableGender.Add(new SelectListItem()
            {
                Text = "Nữ",
                Value = "false",
            });
            ViewBag.storeId = storeId;
            ViewBag.brandId = brandId;
            return View(model);
        }
        public JsonResult CheckMembershipCard(string membershipCardCode)
        {
            MembershipCardApi cardApi = new MembershipCardApi();
            CustomerApi cusApi = new CustomerApi();
            bool accept = false;
            var card = cardApi.GetMembershipCardByCode(membershipCardCode);

            if (card != null)
            {
                if (card.CustomerId != null)
                {
                    var cus = cusApi.GetCustomerById((int)card.CustomerId);
                    accept = true;
                    if (card.Status == (int)MembershipStatusEnum.Suspensed)
                    {
                        return Json(new
                        {
                            checkCard = accept,
                            customerId = card.CustomerId,
                            customerName = cus.Name,
                            message = "Thẻ đã bị khóa"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new
                    {
                        checkCard = accept,
                        customerId = card.CustomerId,
                        customerName = cus.Name,
                        message = "Thẻ đã có chủ sở hữu"
                    }, JsonRequestBehavior.AllowGet);
                }
                else if (card.Active == false)
                {
                    accept = true;
                    return Json(new
                    {
                        customerId = -1,
                        customerName = "",
                        checkCard = accept,
                        message = "Thẻ đã bị hủy ! xin vui lòng liên hệ admin"
                    }, JsonRequestBehavior.AllowGet);
                }
                else if (card.Status == (int)MembershipStatusEnum.Suspensed)
                {
                    accept = true;
                    return Json(new
                    {
                        customerId = -1,
                        customerName = "",
                        checkCard = accept,
                        message = "Thẻ đã bị khóa"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                accept = false;
            }
            return Json(new
            {
                checkCard = accept,
                message = "Thẻ Trống ! Có thể sử dụng"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetListCardSample(int brandId)
        {
            var membershipCardApi = new MembershipCardApi();
            var listCard = membershipCardApi.GetMembershipCardSample(brandId);
            var result = listCard.Select(q => new
            {
                Id = q.Id,
                Code = q.MembershipCardCode
            });
            return Json(new
            {
                listCard = result,
                success = true
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadAccountSample(string cardId)
        {
            try
            {
                var cardSampleId = int.Parse(cardId);
                var accountApi = new AccountApi();
                var listAccountSample = accountApi.GetAccountByMembershipId(cardSampleId).AsEnumerable();
                var result = listAccountSample.Select(q => new IConvertible[] {
                    q.Type,
                    q.Balance,
                    q.ProductCode
                });

                return Json(new
                {
                    success = true,
                    listAccount = result
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi trong quá trình xử lý, vui lòng liên hệ Admin"
                }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult CheckCustomer(int brandId, string keyData)
        {
            try
            {
                var customerApi = new CustomerApi();
                var customer = customerApi.GetCustomerByEmailOrPhone(keyData, brandId);
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
                            id = customer.CustomerID,
                            Name = customer.Name,
                            Gender = gender,
                            Type = type,
                            Address = customer.Address,
                            Phone = customer.Phone,
                            Email = customer.Email ?? "N/A",
                            Date = date,
                            CMND = cmnd,
                            District = disctrict,
                            City = city,
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin khách hàng"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = "Có lỗi trong quá trình xử lý ! xin vui lòng liên hệ admin"
                }, JsonRequestBehavior.AllowGet);
            }


        }

        public async Task<ActionResult> CreateMembershipCardAtStore(int storeId, string cardCode, string customerId, string cardSampleId, string userName)
        {
            try
            {
                var membershipCardApi = new MembershipCardApi();
                var customerApi = new CustomerApi();
                var accountApi = new AccountApi();
                bool success = true;
                string message = "Tạo thẻ thành công ! Liên hệ Admin hệ thống để kích hoạt thẻ";
                var membershipCard = membershipCardApi.GetMembershipCardByCode(cardCode);
                if (membershipCard != null)
                {
                    success = false;
                    message = "Thẻ đã tồn tại trong hệ thống";
                }
                else
                {
                    if (Utils.IsDigitsOnly(customerId))
                    {
                        var customer = customerApi.GetCustomerByID((int.Parse(customerId)));
                        if (customer != null)
                        {
                            if (Utils.IsDigitsOnly(cardSampleId))
                            {
                                var cardSample = membershipCardApi.GetMembershipCardBaseOnID(int.Parse(cardSampleId));
                                if (cardSample != null)
                                {
                                    if (cardSample.ProductCode != null)
                                    {
                                        cardSample.CreateBy = userName;
                                        int cardNewId = await membershipCardApi.AddMembershipCardBaseOnCardSample(cardCode, cardSample, int.Parse(customerId), storeId);

                                        if (cardNewId > 0)
                                        {
                                            var listAccountSample = accountApi.GetAccountsByCardId(int.Parse(cardSampleId));
                                            foreach (var item in listAccountSample)
                                            {
                                                var accountNew = new AccountViewModel()
                                                {
                                                    AccountCode = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                                    AccountName = customer.Name,
                                                    BrandId = cardSample.BrandId,
                                                    StartDate = Utils.GetCurrentDateTime(),
                                                    Active = true,
                                                    MembershipCardId = cardNewId,
                                                    Balance = item.Balance,
                                                    ProductCode = item.ProductCode,
                                                    Type = item.Type,
                                                    FinishDate = item.FinishDate,
                                                    Level_ = item.Level_
                                                };

                                                await accountApi.CreateAsync(accountNew);

                                                //Create Order

                                            }
                                            try
                                            {
                                                await CreateOrderAsync(cardSample.BrandId, storeId,
                                                         cardSample.ProductCode, customer.CustomerID);
                                            }
                                            catch (Exception ex)
                                            {
                                                // Rollback
                                                foreach (var item in listAccountSample)
                                                {
                                                    accountApi.Delete(item.AccountID);
                                                }

                                                membershipCardApi.Delete(cardNewId);
                                                success = false;
                                                message = "Tạo đơn hàng cho thẻ thành công ! Liên hệ Admin để được xử lý";
                                            }

                                        }
                                        else
                                        {
                                            success = false;
                                            message = "Tạo thẻ không thành công ! Liên hệ Admin để được xử lý";
                                        }

                                    }
                                    else
                                    {
                                        success = false;
                                        message = "Thao tác không thành công! Thẻ mẫu chưa có sản phẩm";
                                    }
                                }
                                else
                                {
                                    success = false;
                                    message = "Thao tác không thành công! Không tìm thấy thẻ mẫu";
                                }

                            }
                            else
                            {
                                success = false;
                                message = "Thao tác không thành công! ID thẻ mẫu không hợp lệ";
                            }
                        }
                        else
                        {
                            success = false;
                            message = "Thao tác không thành công! Không tìm thấy khách hàng";
                        }

                    }
                }
                return Json(new
                {
                    success = success,
                    message = message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi trong quá trình xử lý ! vui lòng liên hệ admin để được giải quyết"
                }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult CreateCustomer(int brandId, CustomerViewModel model)
        {
            var customerApi = new CustomerApi();
            int id = 0;
            if (String.IsNullOrEmpty(model.Phone))
            {
                return Json(new { success = false, message = "Số điện thoại không được để trống" });
            }
            else
            {
                if (customerApi.GetCustomerByEmailOrPhone(model.Phone, brandId) != null)
                {
                    return Json(new { success = false, message = "SĐT đã tồn tại" });
                }
            }
            if (!String.IsNullOrEmpty(model.Email))
            {
                if (customerApi.GetCustomerByEmail(model.Email, brandId) != null)
                {
                    return Json(new { success = false, message = "Email đã tồn tại" });
                }
            }
            try
            {
                model.BrandId = brandId;
                if (model.birthDayString != null)
                {
                    model.BirthDay = model.birthDayString.ToDateTime();
                }
                id = customerApi.AddCustomer(model);
            }
            catch (Exception e)
            {

                return Json(new { success = false, message = Resources.Message_VI_Resources.CreateCustomerFailed });
            }

            return Json(new
            {
                success = true,
                message = Resources.Message_VI_Resources.CreateCustomerSuccessfully,
                customer = new
                {
                    id = id,
                    name = model.Name,
                    phone = model.Phone
                }
            });
        }
        #endregion
        // get: membershipcard/membershipcard
        public ActionResult index()
        {
            return View();
        }
        #region Load Datatable
        //TODO: MembershipTypeId chưa sửa
        [HttpGet]
        public JsonResult LoadDeactiveMembershipCard(JQueryDataTableParamModel param, int membershipTypeId, int brandId, int storeFilterId)
        {
            int count = 0;
            int totalRecords = 0;
            int totalDisplayRecords = 0;

            var cusApi = new CustomerApi();
            var membershipCardApi = new MembershipCardApi();
            count = param.iDisplayStart + 1;
            var rs = new object();

            if (membershipTypeId != -1)
            {
                var storeApi = new StoreApi();
                var storeList = storeApi.GetStoreById(storeFilterId);

                var card = membershipCardApi.GetDeactiveListByBrandAndType(brandId, membershipTypeId)
                                            .Where(a => a.IsSample != true);
                var searchList = card.Where(a => string.IsNullOrEmpty(param.sSearch) || (!string.IsNullOrEmpty(param.sSearch) && a.MembershipCardCode.ToLower()
                           .Contains(param.sSearch.ToLower())));
                if (storeFilterId != 0)
                {
                    searchList = searchList.Where(a => a.StoreId == storeFilterId);
                }
                rs = searchList.OrderByDescending(q => q.CreatedTime).Skip(param.iDisplayStart)
                        .Take(param.iDisplayLength)
                        .ToList()
                        .Select(a => new object[]
                        {
                                       count++,
                                       string.IsNullOrEmpty(a.MembershipCardCode) ? "---" : a.MembershipCardCode,
                                       a.CreatedTime.ToString("dd/MM/yyyy"),
                                       a.MembershipCardType == null ? "---" : a.MembershipCardType.TypeName,
                                       (a.CustomerId == null || a.CustomerId <= 0 || string.IsNullOrEmpty(a.Customer.Name)) ? "---" : a.Customer.Name,
                                       a.CustomerId,
                                       a.Id,
                                       (a.StoreId == null || a.StoreId.Value == 0 ) ? "Hệ Thống" : storeApi.Get(a.StoreId.Value).Name,
                        });
                totalRecords = searchList.Count();
                totalDisplayRecords = searchList.Count();
            }
            else
            {
                var storeApi = new StoreApi();
                var card = membershipCardApi.GetDeactiveListByBrandId(brandId).Where(a => a.IsSample != true);
                var searchList = card.Where(a => string.IsNullOrEmpty(param.sSearch) || (!string.IsNullOrEmpty(param.sSearch) && a.MembershipCardCode.ToLower()
                            .Contains(param.sSearch.ToLower())));
                if (storeFilterId != 0)
                {
                    searchList = searchList.Where(a => a.StoreId == storeFilterId);
                }
                rs = searchList.OrderByDescending(q => q.CreatedTime)
                        .Skip(param.iDisplayStart)
                        .Take(param.iDisplayLength)
                        .ToList()
                        .Select(a => new object[]
                        {
                                       count++,
                                       string.IsNullOrEmpty(a.MembershipCardCode) ? "---" : a.MembershipCardCode,
                                       a.CreatedTime.ToString("dd/MM/yyyy"),
                                       a.MembershipCardType == null ? "---" : a.MembershipCardType.TypeName,
                                       (a.CustomerId == null || a.CustomerId <= 0 || string.IsNullOrEmpty(a.Customer.Name)) ? "---" : a.Customer.Name,
                                       a.CustomerId,
                                       a.Id,
                                       (a.StoreId == null || a.StoreId.Value == 0 ) ? "Hệ Thống" : storeApi.Get(a.StoreId.Value).Name,

                        });
                totalRecords = searchList.Count();
                totalDisplayRecords = searchList.Count();
            }

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult LoadActiveMembershipCard(JQueryDataTableParamModel param, int membershipTypeId, int brandId, int storeFilterId)
        {
            int count = 0;
            int totalRecords = 0;
            int totalDisplayRecords = 0;

            var membershipCardApi = new MembershipCardApi();

            count = param.iDisplayStart + 1;
            var rs = new object();
            var storeApi = new StoreApi();
            if (membershipTypeId != -1)
            {
                var card = membershipCardApi.GetActiveListByBrandAndType(brandId, membershipTypeId).Where(a => a.IsSample != true);
                var searchList = card.Where(a => string.IsNullOrEmpty(param.sSearch)
                        || (!string.IsNullOrEmpty(param.sSearch) && a.MembershipCardCode.ToLower().Contains(param.sSearch.ToLower()))
                        || (!string.IsNullOrEmpty(param.sSearch) && a.Customer.Name.ToLower().Contains(param.sSearch.ToLower())))
                        .OrderByDescending(q => q.CreatedTime);
                if (storeFilterId != 0)
                {
                    searchList = searchList.Where(a => a.StoreId == storeFilterId).OrderByDescending(q => q.CreatedTime);
                }
                rs = searchList.Skip(param.iDisplayStart)
                           .Take(param.iDisplayLength)
                           .ToList().Select(a => new object[]
                           {
                                       count++,
                                       string.IsNullOrEmpty(a.MembershipCardCode) ? "---" : a.MembershipCardCode,
                                       (a.CustomerId == null || a.CustomerId <= 0 || string.IsNullOrEmpty(a.Customer.Name)) ? "---" : a.Customer.Name,
                                       a.CreatedTime.ToString("dd/MM/yyyy"),
                                       a.MembershipCardType == null ? "---" : a.MembershipCardType.TypeName,
                                       a.Id,
                                       a.CustomerId,
                                       a.Status,
                                       (a.StoreId == null || a.StoreId.Value == 0 ) ? "Hệ Thống" : storeApi.Get(a.StoreId.Value).Name,
                           });
                totalRecords = card.Count();
                totalDisplayRecords = searchList.Count();
            }
            else
            {
                var card = membershipCardApi.GetActiveListByBrandId(brandId).Where(a => a.IsSample != true);
                var searchList = card.Where(a => string.IsNullOrEmpty(param.sSearch)
                           || (!string.IsNullOrEmpty(param.sSearch) && a.MembershipCardCode.ToLower().Contains(param.sSearch.ToLower()))
                           || (!string.IsNullOrEmpty(param.sSearch) && a.Customer.Name.ToLower().Contains(param.sSearch.ToLower())));
                if (storeFilterId != 0)
                {
                    searchList = searchList.Where(a => a.StoreId == storeFilterId);
                }
                rs = searchList.OrderByDescending(q => q.CreatedTime)
                           .Skip(param.iDisplayStart)
                           .Take(param.iDisplayLength)
                           .ToList().Select(a => new object[]
                           {
                                       count++,
                                       string.IsNullOrEmpty(a.MembershipCardCode) ? "---" : a.MembershipCardCode,
                                       (a.CustomerId == null || a.CustomerId <= 0 || string.IsNullOrEmpty(a.Customer.Name)) ? "---" : a.Customer.Name,
                                       a.CreatedTime.ToString("dd/MM/yyyy"),
                                       a.MembershipCardType == null ? "---" : a.MembershipCardType.TypeName,
                                       a.Id,
                                       a.CustomerId,
                                       a.Status,
                                       (a.StoreId == null || a.StoreId.Value == 0 ) ? "Hệ Thống" : storeApi.Get(a.StoreId.Value).Name,

                            });
                totalRecords = card.Count();
                totalDisplayRecords = searchList.Count();
            }

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalDisplayRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult LoadCloseMembershipCard(JQueryDataTableParamModel param, int membershipTypeId, int brandId, int storeFilterId)
        {
            int count = 0;
            int totalRecords = 0;
            int totalDisplayRecords = 0;

            var membershipCardApi = new MembershipCardApi();

            count = param.iDisplayStart + 1;
            var rs = new object();
            var storeApi = new StoreApi();
            if (membershipTypeId != -1)
            {
                var card = membershipCardApi.GetCloseListByBrandAndType(brandId, membershipTypeId).Where(a => a.IsSample != true);
                var searchList = card.Where(a => string.IsNullOrEmpty(param.sSearch)
                           || (!string.IsNullOrEmpty(param.sSearch) && a.MembershipCardCode.ToLower().Contains(param.sSearch.ToLower()))
                           || (!string.IsNullOrEmpty(param.sSearch) && a.Customer.Name.ToLower().Contains(param.sSearch.ToLower())));
                if (storeFilterId != 0)
                {
                    searchList = searchList.Where(a => a.StoreId == storeFilterId);
                }
                rs = searchList.OrderByDescending(q => q.CreatedTime)
                           .Skip(param.iDisplayStart)
                           .Take(param.iDisplayLength)
                           .ToList().Select(a => new object[]
                  {
                                       count++,
                                       string.IsNullOrEmpty(a.MembershipCardCode) ? "---" : a.MembershipCardCode,
                                       (a.CustomerId == null || a.CustomerId <= 0 || string.IsNullOrEmpty(a.Customer.Name)) ? "---" : a.Customer.Name,
                                       a.CreatedTime.ToString("dd/MM/yyyy"),
                                       a.MembershipCardType == null ? "---" : a.MembershipCardType.TypeName,
                                       a.Id,
                                       a.CustomerId,
                                       a.Status,
                                       (a.StoreId == null || a.StoreId.Value == 0 ) ? "Hệ Thống" : storeApi.Get(a.StoreId.Value).Name,

                  });
                totalRecords = card.Count();
                totalDisplayRecords = searchList.Count();
            }
            else
            {
                var card = membershipCardApi.GetCloseListByBrandId(brandId).Where(a => a.IsSample != true);
                var searchList = card.Where(a => string.IsNullOrEmpty(param.sSearch)
                || (!string.IsNullOrEmpty(param.sSearch) && a.MembershipCardCode.ToLower().Contains(param.sSearch.ToLower()))
                || (!string.IsNullOrEmpty(param.sSearch) && a.Customer.Name.ToLower().Contains(param.sSearch.ToLower())));
                if (storeFilterId != 0)
                {
                    searchList = searchList.Where(a => a.StoreId == storeFilterId);
                }
                rs = searchList.OrderByDescending(q => q.CreatedTime)
                           .Skip(param.iDisplayStart)
                           .Take(param.iDisplayLength)
                           .ToList().Select(a => new object[]
                  {
                                       count++,
                                       string.IsNullOrEmpty(a.MembershipCardCode) ? "---" : a.MembershipCardCode,
                                       (a.CustomerId == null || a.CustomerId <= 0 || string.IsNullOrEmpty(a.Customer.Name)) ? "---" : a.Customer.Name,
                                       a.CreatedTime.ToString("dd/MM/yyyy"),
                                       a.MembershipCardType == null ? "---" : a.MembershipCardType.TypeName,
                                       a.Id,
                                       a.CustomerId,
                                       a.Status,
                                       (a.StoreId == null || a.StoreId.Value == 0 ) ? "Hệ Thống" : storeApi.Get(a.StoreId.Value).Name,
                  });
                totalRecords = card.Count();
                totalDisplayRecords = searchList.Count();
            }

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalDisplayRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        //        #region Create
        public class productListAndMembershipTypeList
        {
            public IEnumerable<Product> productList;
            public IEnumerable<MembershipCardTypeViewModel> membershipTypeList;
            public productListAndMembershipTypeList()
            {
                productList = null;
                membershipTypeList = null;
            }
        }
        public ActionResult CreateMembershipCard(int brandId)
        {
            var productApi = new ProductApi();
            var membershipType = new MembershipCardTypeApi();
            var productList = productApi.GetAllProductsByBrand(brandId);
            var membershipTypeList = membershipType.GetMembershipCardTypeByBrand(brandId);
            var model = new productListAndMembershipTypeList();
            model.productList = productList;
            model.membershipTypeList = membershipTypeList;
            return View(model);
        }
        //        [HttpGet]
        //        public ActionResult Create(MembershipCardEditViewModel model, int brandId)
        //        {
        //            var customerApi = new CustomerApi();
        //            return PartialView("Create", model);

        //            await membershipCardTypeMappingApi.CreateAsync(MembershipCardTypeMapping);
        //        }
        //    }
        //    #endregion

        //    #region createAccount
        //    //Tài khoản thanh toán
        //    if (creditAccountName != null)
        //    {
        //        var Account = new AccountViewModel();
        //        Account.AccountCode = DateTime.Now.ToString("yyyyMMddHHmmssfff"); //Get AccountCode theo thời gian
        //        Account.AccountName = creditAccountName.Trim();
        //        Account.Level_ = 0; //Tải khoản không có level
        //        Account.StartDate = DateTime.Now;
        //        Account.FinishDate = creditFinishDate.ToDateTime();
        //        Account.Balance = creditBalance; //Số tiền trong tài khoản
        //        Account.MembershipCardId = membershipCardApi.GetMembershipCardByCode(membershipCardId).Id;
        //        Account.BrandId = brandId;
        //        Account.AccountTypeId = 1; //Tài khoản thanh toán
        //        Account.Active = true;

        //        await accountApi.CreateAsync(Account);
        //    }
        //    //Tài khoản sản phẩm
        //    if (giftAccountName != null)
        //    {
        //        var Account = new AccountViewModel();
        //        var productApi = new ProductApi();
        //        Account.AccountCode = DateTime.Now.ToString("yyyyMMddHHmmssfff");//Get AccountCode theo thời gian
        //        Account.AccountName = giftAccountName.Trim();
        //        Account.Level_ = 0; //Tải khoản không có level
        //        Account.StartDate = DateTime.Now;
        //        Account.FinishDate = giftFinishDate.ToDateTime();
        //        Account.Balance = giftBalance; // Số sản phẩm trong tài khoản
        //        var productcode = productApi.GetProductById((int)giftProductId).Code;
        //        Account.ProductCode = productcode; // Get ProductCode
        //        Account.BrandId = brandId;
        //        Account.AccountTypeId = 2; //Tài khoản sản phẩm
        //        Account.MembershipCardId = membershipCardApi.GetMembershipCardByCode(membershipCardId).Id;
        //        Account.Active = true;

        //        await accountApi.CreateAsync(Account);
        //    }
        //    //Tài khoản tích điểm
        //    if (memberAccountName != null)
        //    {
        //        var Account = new AccountViewModel();
        //        Account.AccountCode = DateTime.Now.ToString("yyyyMMddHHmmssfff");//Get AccountCode theo thời gian
        //        Account.AccountName = memberAccountName.Trim();
        //        Account.StartDate = DateTime.Now;
        //        Account.FinishDate = memberFinishDate.ToDateTime();
        //        Account.Balance = memberBalance; // Số điểm trong tài khoản
        //        Account.Level_ = (short)memberLevel;
        //        Account.BrandId = brandId;
        //        Account.AccountTypeId = 3; //Tài khoản tích điểm
        //        Account.MembershipCardId = membershipCardApi.GetMembershipCardByCode(membershipCardId).Id;
        //        Account.Active = true;

        //        await accountApi.CreateAsync(Account);
        //    }
        //    #endregion
        //    return RedirectToAction("Index");

        //}

        //private void PrepareCreate(MembershipCardEditViewModel model, int brandId)
        //{
        //    var customerApi = new CustomerApi();
        //    var membershipTypeApi = new MembershipTypeApi();
        //    var listMembershipType = membershipTypeApi.GetMembershipTypeByBrand(brandId);

        //    model.ListTypeMembership = listMembershipType.Select(q => new SelectListItem()
        //    {
        //        Selected = model.MembershipTypeId.Equals(q.Id),
        //        Text = q.TypeName,
        //        Value = q.Id.ToString(),
        //    });

        //    //var customerList = customerApi.GetActive();
        //    var customerList = customerApi.GetCustomersByBrand(brandId);
        //    model.CustomerList = customerList.Select(q => new SelectListItem()
        //    {
        //        Selected = model.CustomerId.Equals(q.CustomerID),
        //        Text = q.Name,
        //        Value = q.CustomerID.ToString(),
        //    });
        //}
        #region Edit
        public ActionResult Edit(int id, int brandId)
        {
            var membershipCardApi = new MembershipCardApi();
            var entity = membershipCardApi.Get(id);
            var model = this.Mapper.Map<MembershipCardEditViewModel>(entity);
            PrepareEdit(model, brandId);
            return PartialView("Edit", model);

        }

        [HttpPost]
        public ActionResult Edit(MembershipCardEditViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View(model);
            }
            var membershipCardApi = new MembershipCardApi();
            membershipCardApi.UpdateMembershipCard(model);
            return RedirectToAction("Index");

        }
        private void PrepareEdit(MembershipCardEditViewModel model, int brandId)
        {
            var customerApi = new CustomerApi();
            var membershipTypeApi = new MembershipCardTypeApi();

            var listMembershipType = membershipTypeApi.GetMembershipCardTypeByBrand(brandId);

            model.ListTypeMembership = listMembershipType.Select(q => new SelectListItem()
            {
                Selected = model.MembershipTypeId.Equals(q.Id),
                Text = q.TypeName,
                Value = q.Id.ToString(),
            });

            var customerList = customerApi.GetActive();
            model.CustomerList = customerList.Select(q => new SelectListItem()
            {
                Selected = model.CustomerId.Equals(q.CustomerID),
                Text = q.Name,
                Value = q.CustomerID.ToString(),
            });
        }

        #endregion
        #region Activate Card
        public ActionResult ActivateCard(int id, int brandId)
        {
            var membershipCardApi = new MembershipCardApi();
            var customerTypeApi = new CustomerTypeApi();
            var entity = membershipCardApi.Get(id);
            var model = this.Mapper.Map<MembershipCardEditViewModel>(entity);
            model.Customer = new CustomerEditViewModel();
            model.Customer.AvailableType = customerTypeApi.GetAllCustomerTypes(brandId).Select(
                q => new SelectListItem
                {
                    Value = q.ID.ToString(),
                    Text = q.CustomerType1,
                    Selected = false
                });

            model.Customer.AvailableGender = new List<SelectListItem>();
            model.Customer.AvailableGender.Add(new SelectListItem()
            {
                Text = "Nam",
                Value = "true",
            });
            model.Customer.AvailableGender.Add(new SelectListItem()
            {
                Text = "Nữ",
                Value = "false",
            });

            //PrepareActivateCard(model, brandId, id);
            return PartialView("ActivateCard", model);
        }

        //private void PrepareActivateCard(MembershipCardEditViewModel model, int brandId, int cardId)
        //{
        //    var customerApi = new CustomerApi();
        //    var membershipTypeApi = new MembershipTypeApi();
        //    //var typeId = new MembershipCardTypeMappingApi().Get().Where(a => a.MembershipCardId == cardId).;
        //    //var typeId = new MembershipCardTypeMappingApi().Get(cardId).MembershipTypeId;
        //    var membershipCardService = this.Service<IMembershipCardTypeMappingService>();
        //    var typeList = membershipCardService.Get().Where(a => a.MembershipCardId == cardId).Select(b => new { Name = b.MembershipType.TypeName }).ToArray();
        //    List<string> nameList = new List<string>();
        //    foreach (var item in typeList)
        //    {
        //        nameList.Add(item.Name);
        //    }
        //    var customerList = customerApi.GetCustomersByBrand(brandId);
        //    int result = customerList.Count();
        //    model.CustomerList = customerList.Select(q => new SelectListItem()
        //    {
        //        Selected = model.CustomerId.Equals(q.CustomerID),
        //        Text = q.Name,
        //        Value = q.CustomerID.ToString(),
        //    });

        //    model.ListType = nameList;

        //}

        [HttpPost]
        public async Task<JsonResult> ActiveMembershipCard(string cardCode, int customerId)
        {
            bool check;
            try
            {
                CustomerApi cApi = new CustomerApi();
                MembershipCardApi mcApi = new MembershipCardApi();
                var card = mcApi.GetMembershipCardByCode(cardCode);
                var customer = cApi.GetCustomerById(customerId);
                card.CustomerId = customerId;
                //card.Customer.Name = customer.Name;
                card.Status = (int)MembershipStatusEnum.Active;// Kích hoạt
                MembershipCardViewModel model = new MembershipCardViewModel(card);
                await mcApi.EditAsync(model.Id, model);

                /// Create Transaction
                var accountApi = new AccountApi();
                var listAccount = accountApi.GetAccountsByCardId(model.Id);
                if (listAccount.Count > 0)
                {
                    var account = listAccount.Where(q => q.Active == true && q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault();
                    if (account != null)
                    {
                        if (account.Balance >= 0)
                        {
                            var transactionApi = new TransactionApi();
                            var historyTransaction = transactionApi.GetTransactionByCardId(model.Id).Count();
                            if (historyTransaction <= 0)
                            {
                                var transaction = new TransactionViewModel()
                                {
                                    AccountId = account.AccountID,
                                    Date = Utils.GetCurrentDateTime(),
                                    StoreId = model.StoreId == null ? 0 : model.StoreId.Value,
                                    BrandId = model.BrandId,
                                    IsIncreaseTransaction = true,
                                    Notes = "ActiveCard",
                                    Status = (int)TransactionStatus.Approve,
                                    Amount = account.Balance == null ? 0 : account.Balance.Value,
                                    TransactionType = (int)TransactionTypeEnum.ActiveCard,
                                    UserId = model.CreateBy
                                };
                                await transactionApi.CreateAsync(transaction);
                            }
                            var message = (new BrandApi().Get(model.BrandId)).BrandName + ". "
                                            + DateTime.Now.ToString("dd/MM/yyyy") + ";" + DateTime.Now.ToString("HH:mm") + ".  "
                                            + (new BrandApi().Get(model.BrandId)).BrandName+ " xin chao: " + customer.Name + "; "
                                            + "He thong "+ (new BrandApi().Get(model.BrandId)).BrandName + " da kich hoat TK:" + model.MembershipCardCode + "; "
                                            + "So du tai khoan: " + Utils.ToMoney((double)account.Balance) + "VNĐ";
                            try
                            {
                                Utils.SendSMS(customer.Phone, message, model.BrandId);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }

                check = true;
            }
            catch (Exception)
            {
                check = false;
            }

            return Json(new
            {
                success = check,

            });
        }

        public async Task<bool> unActiveMembershipCard(string cardCode)
        {
            bool check;
            try
            {
                MembershipCardApi mcApi = new MembershipCardApi();
                var card = mcApi.GetMembershipCardByCode(cardCode);
                card.Status = 0;// Tắt kích hoạt
                MembershipCardViewModel model = new MembershipCardViewModel(card);
                await mcApi.EditAsync(model.Id, model);
                check = true;
            }
            catch (Exception)
            {
                check = false;
            }

            return check;
        }
        #endregion

        #region Delete
        public async Task<JsonResult> Delete(int id)
        {
            var MembershipCardApi = new MembershipCardApi();
            var accountApi = new AccountApi();
            var transactionApi = new TransactionApi();
            var model = MembershipCardApi.Get(id);
            if (model == null)
            {
                return Json(new { success = false });
            }
            try
            {
                accountApi.DeactiveAccountByMembershipCardID(id);
                transactionApi.CancelAllTransaction(id);
                await MembershipCardApi.DeleteMembershipCardAsync(model);
            }
            catch (System.Exception e)
            {
                throw e;

            }
            return Json(new { success = true });
        }
        #endregion
        #region Change Status

        public async Task<ActionResult> ChangeStatus(int id, int status)
        {
            var api = new MembershipCardApi();
            var model = await api.GetAsync(id);
            switch (status)
            {
                case 0:
                    status = (int)MembershipStatusEnum.Active;
                    /// Create Transaction
                    var accountApi = new AccountApi();
                    var listAccount = accountApi.GetAccountsByCardId(model.Id);
                    if (listAccount.Count > 0)
                    {
                        var account = listAccount.Where(q => q.Active == true && q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault();
                        if (account != null)
                        {
                            if (account.Balance >= 0)
                            {
                                var transactionApi = new TransactionApi();
                                var historyTransaction = transactionApi.GetTransactionByCardId(model.Id).AsEnumerable();
                                if (historyTransaction.Count() <= 0)
                                {
                                    var transaction = new TransactionViewModel()
                                    {
                                        AccountId = account.AccountID,
                                        Date = Utils.GetCurrentDateTime(),
                                        StoreId = model.StoreId == null ? 0 : model.StoreId.Value,
                                        BrandId = model.BrandId,
                                        IsIncreaseTransaction = true,
                                        Notes = "ActiveCard",
                                        Status = (int)TransactionStatus.Approve,
                                        Amount = account.Balance == null ? 0 : account.Balance.Value,
                                        TransactionType = (int)TransactionTypeEnum.ActiveCard,
                                        UserId = model.CreateBy
                                    };
                                    await transactionApi.CreateAsync(transaction);
                                }
                                else
                                {
                                    decimal valueTrans = historyTransaction.Sum(q => q.Amount);
                                    if (account.Balance != null)
                                    {
                                        if (valueTrans - account.Balance <= 0)
                                        {
                                            var transaction = new TransactionViewModel()
                                            {
                                                AccountId = account.AccountID,
                                                Date = Utils.GetCurrentDateTime(),
                                                StoreId = model.StoreId == null ? 0 : model.StoreId.Value,
                                                BrandId = model.BrandId,
                                                IsIncreaseTransaction = true,
                                                Notes = "ActiveCard",
                                                Status = (int)TransactionStatus.Approve,
                                                Amount = account.Balance == null ? 0 : account.Balance.Value - valueTrans,
                                                TransactionType = (int)TransactionTypeEnum.ActiveCard,
                                                UserId = model.CreateBy
                                            };
                                            await transactionApi.CreateAsync(transaction);
                                        }
                                    }

                                }
                                var customerApi = new CustomerApi();
                                var customer = customerApi.Get(model.CustomerId);
                                var message = (new BrandApi().Get(model.BrandId)).BrandName + ". "
                                              + DateTime.Now.ToString("dd/MM/yyyy") + ";" + DateTime.Now.ToString("HH:mm") + ".  "
                                              + (new BrandApi().Get(model.BrandId)).BrandName+ " xin chao: " + customer.Name + "; "
                                              + "He thong "+ (new BrandApi().Get(model.BrandId)).BrandName + " da kich hoat TK:" + model.MembershipCardCode + "; "
                                              + "So du tai khoan: " + Utils.ToMoney((double)account.Balance) + "VNĐ";
                                try
                                {
                                    Utils.SendSMS(customer.Phone, message, model.BrandId);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                    break;
                case 1:
                    status = (int)MembershipStatusEnum.Suspensed;
                    break;
                default:
                    status = (int)MembershipStatusEnum.Active;
                    break;
            }
            model.Status = status;
            await api.EditAsync(model.Id, model);
            return this.RedirectToAction("Index");

        }
        #endregion

        #region CreateMembershipCard

        //public JsonResult GetGroupCard(int brandId)
        //{
        //    try
        //    {
        //        var cardGroupApi = new GroupMembershipCardApi();
        //        var cardGroup = cardGroupApi.GetAllActiveGroupByBrandId(brandId).ToList();
        //        List<object> listGroup = new List<object>();
        //        foreach (var item in cardGroup)
        //        {
        //            listGroup.Add(new
        //            {
        //                Name = item.GroupName,
        //                ID = item.GroudId,
        //            });
        //        }

        //        return Json(new { success = true, listGroup = listGroup }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch(Exception e)
        //    {
        //        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public JsonResult GetSampleMembershipCard(int brandId, int membershipTypeId)
        {
            try
            {
                var membershipCardApi = new MembershipCardApi();

                var memCards = membershipCardApi.GetActiveListByBrandAndType(brandId, membershipTypeId)
                            .Where(q => q.IsSample == true).ToList()
                            .Select(q => new SelectListItem
                            {
                                Text = q.MembershipCardCode,
                                Value = q.Id.ToString()
                            });

                return Json(new { success = true, memCards = memCards }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult LoadAllCustomer(int brandId)
        {
            var customerApi = new CustomerApi();
            var customers = customerApi.GetCustomersByBrand(brandId);
            return Json(new
            {
                success = true,
                data = customers.Select(a => new
                {
                    id = a.CustomerID,
                    text = a.Name,
                    phone = a.Phone
                })
            });
        }

        public JsonResult GetSampleAccounts(int membershipCardId)
        {
            try
            {
                var accountApi = new AccountApi();
                var productApi = new ProductApi();

                int count = 0;
                var accounts = accountApi.GetAllAccountsByMembershipId(membershipCardId)
                                .Where(q => q.Active == true).ToList()
                                .Select(q => new IConvertible[]
                                {
                                     ++count,
                                     q.AccountName,
                                     q.Type,
                                     q.Balance,
                                     q.ProductCode != null ? productApi.GetProductByCode(q.ProductCode).ProductName : "N/A",
                                     q.StartDate != null ? q.StartDate.Value.ToString("dd/MM/yyyy") : "N/A",
                                     q.Active,
                                     q.FinishDate != null ? q.FinishDate.Value.ToString("dd/MM/yyyy") : "",
                                     q.ProductCode,
                                     q.Level_
                                });

                return Json(new { success = true, accounts = accounts }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> CreateMembershipCardWithAccounts(int storeId, int brandId, string memCardCode, string createTime, int memTypeId, string listAccounts, string userName)
        {
            try
            {
                int memCardId;
                #region Create Membership Card
                var membershipCardApi = new MembershipCardApi();
                var accountApi = new AccountApi();
                var accounts = JsonConvert.DeserializeObject<List<dynamic>>(listAccounts);
                MembershipCardViewModel cardModel = new MembershipCardViewModel();

                var memCard = membershipCardApi.GetMembershipCardByCode(memCardCode);
                if (memCard == null)
                {
                    cardModel.MembershipCardCode = memCardCode;
                    cardModel.MembershipTypeId = memTypeId;
                    cardModel.CreatedTime = createTime.ToDateTime();
                    cardModel.BrandId = brandId;
                    cardModel.IsSample = false;
                    cardModel.StoreId = storeId;
                    cardModel.CreateBy = userName;
                    memCardId = await membershipCardApi.CreateMembershipCardAsync(cardModel);
                }
                else
                {
                    return Json(new { success = false, message = "Mã thẻ đã tồn tại, xin nhập mã thẻ khác." });
                }

                #endregion

                #region Create Accounts

                foreach (var data in accounts)
                {
                    AccountViewModel newAccount = new AccountViewModel();
                    newAccount.AccountCode = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    newAccount.AccountName = data[1];
                    newAccount.Type = data[2];

                    string accStartDate = data[5];
                    if (!string.IsNullOrEmpty(accStartDate) && !accStartDate.Equals("N/A"))
                    {
                        newAccount.StartDate = accStartDate.ToDateTime();
                    }

                    string accFinishDate = data[7];
                    if (!string.IsNullOrEmpty(accFinishDate))
                    {
                        newAccount.FinishDate = accFinishDate.ToDateTime().GetEndOfDate();
                    }

                    newAccount.Balance = data[3];

                    string productCode = data[8];
                    if (data[2] == (int)AccountTypeEnum.GiftAccount && !string.IsNullOrWhiteSpace(productCode))
                    {
                        newAccount.ProductCode = productCode;
                    }


                    string level = data[9];
                    if (data[2] == (int)AccountTypeEnum.PointAccount && !string.IsNullOrWhiteSpace(level))
                    {
                        newAccount.Level_ = data[9];
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
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public JsonResult GetAllCustomers(int brandId, string searchTokens, int page)
        {
            try
            {
                var customerApi = new CustomerApi();
                var result = customerApi.GetCustomersByBrandId(brandId);
                if (!string.IsNullOrWhiteSpace(searchTokens))
                {
                    result = result.Where(q => Utils.CustomContains(q.Name, searchTokens));
                }

                var list = result
                    .OrderBy(q => q.Name)
                    .Skip(page * 20)
                    .Take(20)
                    .Select(q => new
                    {
                        id = q.CustomerID,
                        text = q.Name
                    });

                var total = result.Count();

                return Json(new { success = true, list = list, total = total }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult GetCustomerById(int id)
        {
            try
            {
                var customerApi = new CustomerApi();
                var customer = customerApi.GetCustomerByID(id);
                var typeApi = new CustomerTypeApi();
                var type = "";
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
                if (customer != null)
                {
                    return Json(new
                    {
                        success = true,
                        customer = new
                        {
                            Name = customer.Name,
                            Gender = gender,
                            Type = type,
                            Address = customer.Address,
                            Phone = customer.Phone,
                            Email = customer.Email ?? "N/A",
                            Date = date,
                            CMND = cmnd,
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
                        message = "Không tồn tại khách hàng"
                    });
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra"
                });
            }
        }
        #region lấy thông tin membership card
        public JsonResult GetInfoMembershipCard(int cardId)
        {
            try
            {
                var membershipCardApi = new MembershipCardApi();
                var list = membershipCardApi.GetMembershipCardById(cardId);
                if (list != null)
                {
                    return Json(new
                    {
                        success = true,
                        list = list,
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, message = "Thẻ không tồn tại" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetAllMembershipCardTypes(int brandId)
        {
            try
            {
                var membershipCardTypeApi = new MembershipCardTypeApi();
                var typeList = membershipCardTypeApi.GetMembershipCardTypeByBrand(brandId).ToArray();
                if (typeList != null)
                {
                    return Json(new
                    {
                        success = true,
                        list = typeList
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Danh sách loại thẻ thành viên không tồn tại"
                    });
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra",
                    exception = e
                });
            }
        }
        //public JsonResult LoadMembershipCardType(int membershipCardId)
        //{
        //    var accountApi = new AccountApi();
        //    List<int> listType = new List<int>();
        //    int credit = 0, gift = 0, member = 0;
        //    var accountType = accountApi.GetAccountByMembershipId(membershipCardId).Where(a => a.Active == true).Select(q => q.AccountTypeId).ToList();
        //    foreach (var item in accountType)
        //    {
        //        if (item == 1)
        //        {
        //            credit++;
        //        }
        //        if (item == 2)
        //        {
        //            gift++;
        //        }
        //        if (item == 3)
        //        {
        //            member++;
        //        }
        //    }
        //    listType.Add(credit);
        //    listType.Add(gift);
        //    listType.Add(member);

        //    return Json(new
        //    {
        //        listType = listType.ToArray(),
        //    }, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public async Task<JsonResult> EditMembershipCardType(int brandId)
        {
            try
            {
                var accountData = Request["listAccount"];
                var membershipCardId = int.Parse(Request["membershipCardCode"]);
                var membershipCardTypeId = int.Parse(Request["membershipCardTypeId"]);
                var productCode = Request["productCode"];

                dynamic accountList = JsonConvert.DeserializeObject<dynamic>(accountData);
                dynamic deletedCodeList = JsonConvert.DeserializeObject<dynamic>(accountData);
                var accountApi = new AccountApi();
                var membershipCardApi = new MembershipCardApi();
                var sampleCard = membershipCardApi.GetMembershipCardSampleActiveByBrandId(brandId).FirstOrDefault(s => s.ProductCode.Equals(productCode));
                //var membershipCard = membershipCardApi.GetMembershipCardById(membershipCardId);
                var membershipCard = membershipCardApi.Get(membershipCardId);

                if (membershipCard != null)
                {
                    membershipCard.ProductCode = productCode;
                    membershipCard.InitialValue = 0;
                    membershipCard.MembershipTypeId = membershipCardTypeId;
                    await membershipCardApi.UpdateMembershipCard(membershipCard);
                }

                //bool flagCredit = false;
                //bool flagGift = false;
                //bool flagMember = false;
                foreach (var item in accountList)
                {
                    //var typeIdtmp = item[8];
                    //var active = item[6];
                    //if (active == true && typeIdtmp == (int)AccountTypeEnum.CreditAccount)
                    //{
                    //    flagCredit = true;
                    //}

                    //if (active == true && typeIdtmp == (int)AccountTypeEnum.GiftAccount)
                    //{
                    //    flagGift = true;
                    //}

                    //if (active == true && typeIdtmp == (int)AccountTypeEnum.PointAccount)
                    //{
                    //    flagMember = true;
                    //}
                    string datatmp = item[1];
                    var currentAccount = await accountApi.GetAccountByAccountCode(datatmp);
                    if (currentAccount != null)
                    {
                        currentAccount.Active = item[7];
                        await accountApi.UpdateAccountAsync(currentAccount);
                    }
                    else
                    {
                        currentAccount = new AccountViewModel();
                        currentAccount.Active = item[7];
                        currentAccount.AccountCode = item[1];
                        currentAccount.AccountName = item[2];
                        currentAccount.Type = item[3];
                        currentAccount.Balance = item[4];
                        currentAccount.MembershipCardId = membershipCardId;
                        string productCode2 = item[9];
                        if (!string.IsNullOrWhiteSpace(productCode2))
                        {
                            currentAccount.ProductCode = productCode2;
                        }
                        string startDate = item[6];
                        string finishDate = item[8];
                        currentAccount.StartDate = startDate.ToDateTime().GetStartOfDate();
                        currentAccount.FinishDate = finishDate.ToDateTime().GetEndOfDate();
                        currentAccount.BrandId = int.Parse(RouteData.Values["brandId"].ToString());
                        string level = item[10];
                        if (!string.IsNullOrWhiteSpace(level))
                        {
                            currentAccount.Level_ = item[10];
                        }
                        await accountApi.CreateAsync(currentAccount);

                        //var mapping = membershiptypeMapping.Get().Where(a => a.MembershipCardId == membershipId && a.MembershipTypeId == typeMember && a.Active == true);//dang chenh nhau 3 dv
                        //if (mapping == null || mapping.Count() <= 0)
                        //{

                        //    currentAccount.MembershipCardId = membershipId;
                        //    currentAccount.Balance = item[4];
                        //    currentAccount.ProductCode = item[5];
                        //    currentAccount.Active = item[7];
                        //    string datetime = item[9];
                        //    currentAccount.FinishDate = datetime.ToDateTime().GetEndOfDate();
                        //    currentAccount.BrandId = int.Parse(RouteData.Values["brandId"].ToString());
                        //    await accountApi.CreateAsync(currentAccount);
                        //    MembershipCardTypeMappingViewModel memberCardMapping = new MembershipCardTypeMappingViewModel();
                        //    memberCardMapping.MembershipCardId = membershipId;
                        //    //đang code cứng dưới DB
                        //    if (typeId == 1)
                        //    {
                        //        memberCardMapping.MembershipTypeId = 4; // TODO;
                        //    }
                        //    if (typeId == 2)
                        //    {
                        //        memberCardMapping.MembershipTypeId = 5; // TODO;
                        //    }
                        //    if (typeId == 3)
                        //    {
                        //        memberCardMapping.MembershipTypeId = 6; // TODO;
                        //    }
                        //    memberCardMapping.Active = true;
                        //    await membershiptypeMapping.CreateAsync(memberCardMapping);
                        //}
                    }
                }


                //var mappingtmp = membershiptypeMapping.Get().Where(a => a.MembershipCardId == membershipCardId);

                //foreach (var item in mappingtmp)
                //{
                //    if (item.MembershipTypeId == 4)
                //    {
                //        if (item.Active != flagCredit)
                //        {
                //            item.Active = flagCredit;
                //            await membershiptypeMapping.EditAsync(item.Id, item);
                //        }
                //    }
                //    if (item.MembershipTypeId == 5)
                //    {
                //        if (item.Active != flagGift)
                //        {
                //            item.Active = flagGift;
                //            await membershiptypeMapping.EditAsync(item.Id, item);
                //        }
                //    }
                //    if (item.MembershipTypeId == 6)
                //    {
                //        if (item.Active != flagMember)
                //        {
                //            item.Active = flagMember;
                //            await membershiptypeMapping.EditAsync(item.Id, item);
                //        }
                //    }
                //}
                //return null;

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại." });
            }
        }

        #endregion
        #region lấy thông tin account thuộc membership card
        //public JsonResult GetAccountMembershipCard(int accountId)
        //{
        //    try
        //    {
        //        var accountApi = new AccountApi();
        //        var listAccount = accountApi.GetAccountByMembershipId(accountId);
        //        if (listAccount != null)
        //        {
        //            int count = 1;
        //            var list = listAccount.ToList().Select(a => new IConvertible[] {
        //                count++,
        //                a.AccountCode,
        //                a.AccountName,
        //                //a.AccountType.Name,
        //                a.Balance,
        //                string.IsNullOrEmpty(a.ProductCode) ? "N/A" : a.ProductCode,
        //                a.Active,
        //                a.AccountID,
        //                //a.AccountTypeId,
        //                a.FinishDate,
        //            });
        //            return Json(new
        //            {
        //                success = true,
        //                list = list
        //            }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { success = true, message = "Thẻ chưa có tài khoản" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch
        //    {
        //        return Json(new { success = false, message = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        #endregion
        #region update active, deactive account
        [HttpPost]
        public async Task<JsonResult> ActiveAccount(int accountId, bool checkActive)
        {
            bool check;
            try
            {
                AccountApi accountApi = new AccountApi();
                var account = accountApi.GetAccountEntityById(accountId);
                account.AccountID = accountId;
                if (checkActive)
                {
                    account.Active = true;// Kích hoạt
                }
                else
                {
                    account.Active = false;// KHóa
                }
                AccountViewModel model = new AccountViewModel(account);
                await accountApi.EditAsync(model.AccountID, model);
                check = true;
            }
            catch (Exception)
            {
                check = false;
            }

            return Json(new
            {
                success = check,
            });
        }
        #endregion
        #region danh sách sản phẩm
        public JsonResult LoadProductList(int brandId)
        {
            var productApi = new ProductApi();
            var listPro1 = productApi.GetActiveProductsEntitybyBrandId(brandId).ToArray();
            List<object> listProduct = new List<object>();
            foreach (var i in listPro1)
            {
                listProduct.Add(new
                {
                    ProductName = i.ProductName,
                    ProductId = i.Code,
                });
            }
            var listPro2 = listProduct.ToArray();

            return Json(new
            {
                list = listPro2,
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region tạo account

        #endregion
        #region tạo thêm tài khoản membershipcard
        //[HttpPost]
        //public async Task<JsonResult> CreateNewTypeCard(string input_name, int input_AccountType, int input_ProductCode, int input_blance, string input_finishDate, int brandId, int input_codeID)
        //{
        //    bool check;
        //    var accountApi = new AccountApi();
        //    try
        //    {
        //        var Account = new AccountViewModel();
        //        Account.AccountCode = DateTime.Now.ToString("yyyyMMddHHmmssfff");//Get AccountCode theo thời gian
        //        Account.AccountName = input_name.Trim();
        //        Account.StartDate = DateTime.Now;
        //        Account.FinishDate = input_finishDate.ToDateTime();
        //        Account.Balance = input_blance; // Số điểm trong tài khoản
        //        Account.BrandId = brandId;
        //        Account.AccountTypeId = 3; //Tài khoản tích điểm
        //        Account.MembershipCardId = input_codeID;
        //        Account.Active = true;

        //        await accountApi.CreateAsync(Account);
        //        check = true;
        //    }
        //    catch (Exception)
        //    {
        //        check = false;
        //    }

        //    return Json(new
        //    {
        //        success = check,
        //    });
        //}
        #endregion
        public JsonResult CheckCode(string code, int brandId)
        {
            var membershipCardApi = new MembershipCardApi();
            var list = membershipCardApi.GetMembershipCardByCode(code);

            if (list != null)
            {
                return Json(new { success = false });
            }
            else
            {
                return Json(new { success = true });
            }
        }
        [HttpPost]
        public JsonResult LoadAllMembershipType(int brandID)
        {
            var api = new MembershipCardTypeApi();
            //get customer types

            //set brain id = 1 to view page
            var typeList = api.GetMembershipCardTypeByBrand(brandID).Select(a => new
            {
                MembershipType = a.Id,
                Name = a.TypeName
            }).ToList();
            return Json(typeList);
        }
        public ActionResult GetListStore()
        {
            var brandId = int.Parse(RouteData.Values["brandId"].ToString());
            var storeApi = new StoreApi();
            var listStore = storeApi.GetAllActiveStore(brandId).ToList();
            var result = listStore.Select(q => new
            {
                storeId = q.ID,
                storeName = q.Name
            });
            return Json(new { listresult = result }, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult LoadAllCustomer(int brandId)
        //{
        //    var customerApi = new CustomerApi();
        //    var customers = customerApi.GetCustomersByBrand(brandId);
        //    int result = customers.Count();
        //    var list = customers.Select(a => new
        //    {
        //        id = a.CustomerID,
        //        name = a.Name
        //    });
        //    return Json(list);
        //    //return Json(new
        //    //{
        //    //    success = true,
        //    //    data = customers.Select(a => new
        //    //    {
        //    //        id = a.CustomerID,
        //    //        text = a.Name
        //    //        //phone = a.Phone
        //    //    })
        //    //});
        //}
        //TODO: sửa lại membershipcardtypeID

        #region Upload excel
        [HttpPost]
        public async Task<JsonResult> UploadExcel(HttpPostedFileBase FileUpload, int brandId)
        {
            List<string> data = new List<string>();
            if (FileUpload != null)
            {
                // tdata.ExecuteCommand("truncate table OtherCompanyAssets");  
                if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string pathToExcelFile = "";
                    List<string> redundantCardCode = new List<string>();
                    try
                    {
                        string filename = FileUpload.FileName;
                        string targetpath = Server.MapPath("~/Doc/");
                        FileUpload.SaveAs(targetpath + filename);
                        pathToExcelFile = targetpath + filename;
                        var connectionString = "";
                        if (filename.EndsWith(".xls"))
                        {
                            connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                        }
                        else if (filename.EndsWith(".xlsx"))
                        {
                            connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                        }

                        var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
                        var ds = new DataSet();

                        adapter.Fill(ds, "ExcelTable");

                        DataTable dtable = ds.Tables["ExcelTable"];

                        string sheetName = "Sheet1";
                        var membershipCardApi = new MembershipCardApi();
                        var memberTypeApi = new MembershipCardTypeApi();
                        //var membershipMappingApi = new MembershipCardTypeMappingApi();
                        //MembershipCardTypeMappingViewModel newMapping = new MembershipCardTypeMappingViewModel();
                        var accountApi = new AccountApi();

                        var excelFile = new ExcelQueryFactory(pathToExcelFile);
                        var artistAlbums = from a in excelFile.Worksheet<MembershipCardEditViewModel>(sheetName) select a;


                        foreach (var a in artistAlbums)
                        {
                            try
                            {
                                if ((a.MembershipCardCode != "" && a.MembershipCardCode != null) && (a.MembershipCardTypeName != "" && a.MembershipCardTypeName != null))
                                {
                                    MembershipCardViewModel newCard = new MembershipCardViewModel();
                                    var existMembershipCard = membershipCardApi.GetMembershipCardByCode(a.MembershipCardCode);
                                    if (existMembershipCard == null)
                                    {
                                        var memType = memberTypeApi.GetMembershipCardTypeByNameSync(a.MembershipCardTypeName);
                                        int memTypeId = 0;
                                        if (memType != null)
                                        {
                                            memTypeId = memType.Id;
                                            int memCardId = membershipCardApi.CreateMembershipCard(a.MembershipCardCode, memTypeId, brandId);
                                            if (a.AccountType != null)
                                            {
                                                try
                                                {
                                                    int accTypeId = int.Parse(a.AccountType);
                                                    if (accTypeId == (int)AccountTypeEnum.PointAccount || accTypeId == (int)AccountTypeEnum.CreditAccount)
                                                    {
                                                        accountApi.CreateAccountByMemCard(a.MembershipCardCode, a.DefaultBalance, brandId, memCardId, accTypeId);
                                                    }
                                                    else if (accTypeId == (int)AccountTypeEnum.GiftAccount && a.ProductCode != null && a.ProductCode != "")
                                                    {
                                                        var productApi = new ProductApi();
                                                        var product = productApi.GetProductByCode(a.ProductCode);
                                                        if (product != null)
                                                        {
                                                            accountApi.CreateGiftAccountByMemCard(a.MembershipCardCode, a.DefaultBalance, brandId, memCardId, accTypeId, a.ProductCode);
                                                        }
                                                    }
                                                }
                                                catch (Exception e)
                                                {

                                                }
                                            }
                                        }
                                        else
                                        {
                                            redundantCardCode.Add(a.MembershipCardCode);
                                        }
                                    }
                                    else
                                    {
                                        //int reduntdantCount = 0;
                                        redundantCardCode.Add(a.MembershipCardCode);
                                    }
                                }
                                else
                                {
                                    //if (a.MembershipCardCode == "" || a.MembershipCardCode == null)
                                    //    data.Add("Cần có cột MembershipCardCode trong file!");
                                    //if (a.MembershipTypeName == "" || a.MembershipTypeName == null)
                                    //    data.Add("Cần có cột MembershipTypeId trong file");
                                    //if (a.DefaultBalance == null)
                                    //    data.Add("Cần có cột Default Balance trong file");

                                    //data.ToArray();
                                    //return Json(new { success = false, data = data }, JsonRequestBehavior.AllowGet);
                                    redundantCardCode.Add(a.MembershipCardCode);
                                }
                            }
                            catch (DbEntityValidationException ex)
                            {
                                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                                {

                                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                                    {

                                        //Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                                        return Json(new
                                        {
                                            success = false,
                                            data = "Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage
                                        }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                            //deleting excel file from folder  
                            if ((System.IO.File.Exists(pathToExcelFile)))
                            {
                                System.IO.File.Delete(pathToExcelFile);
                            }
                        }
                        return Json(new
                        {
                            success = true,
                            redundantCardCodeList = redundantCardCode,
                        }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {
                        return Json(new
                        {
                            success = false,
                            data = "Xin kiểm tra lại tên sheet, tên các cột trong file excel theo đúng format định sẵn",
                        });
                    }
                }
                else
                {
                    //alert message for invalid file format  
                    data.Add("Chỉ được upload file Excel!");
                    data.ToArray();
                    return Json(new
                    {
                        success = false,
                        data = data
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                data.Add("Xin chọn file excel");
                data.ToArray();
                return Json(new
                {
                    success = false,
                    data = data
                }, JsonRequestBehavior.AllowGet);
            }
        }
        //public ActionResult editcard()
        //{
        //    return View();
        //}
        [HttpGet]
        public ActionResult editcard(int brandId, int id)
        {
            //int Card_Id = int.Parse(Request["Card_Id"]);
            var membershipCardApi = new MembershipCardApi();
            var accountApi = new AccountApi();
            var productApi = new ProductApi();
            var card = membershipCardApi.GetMembershipCardById(id);
            List<Account> accounts = accountApi.GetAllAccountsByMembershipId(id).ToList();
            int count = 0;
            var dataTable = accounts.Select(q => new IConvertible[] {
                                     ++count,
                                     q.AccountCode,
                                     q.AccountName,
                                     q.Type,
                                     q.Balance,
                                     q.ProductCode != null ? productApi.GetProductByCode(q.ProductCode).ProductName : "N/A",
                                     q.StartDate != null ? q.StartDate.Value.ToString("dd/MM/yyyy") : "N/A",
                                     q.Active,
                                     q.FinishDate != null ? q.FinishDate.Value.ToString("dd/MM/yyyy") : "",
                                     q.ProductCode,
                                     q.Level_,
                                     true // bool để phân biệt account đã tạo và chưa tạo
                            });
            var model = new MembershipCardEditViewModel2
            {
                membershipCard = card,
                accountList = accounts,
                dataTable = dataTable.ToList()
            };
            //var membershipTypeApi = new MembershipTypeApi();
            //var accountTypeApi = new AccountTypeApi();
            //var types = membershipTypeApi.GetMembershipTypeByBrand(brandId);
            //var model = new membershipCardAndType();
            //model.membershipCard = card;
            //model.membershipTypeList = types;
            ViewBag.AccountTypes = accounts.Select(q => q.Type).ToList();
            var accountTypes = Enum.GetValues(typeof(AccountTypeEnum));
            var accountTypeList = new List<AccountTypeViewModel>();
            foreach (var type in accountTypes)
            {
                accountTypeList.Add(new AccountTypeViewModel
                {
                    Name = ((AccountTypeEnum)type).DisplayName(),
                    Value = (int)type
                });
            }
            ViewBag.AllAccountTypes = accountTypeList;
            return View("editcard", model);
        }
        public class MembershipCardEditViewModel2
        {
            public MembershipCardEditViewModel membershipCard { get; set; }
            public List<Account> accountList { get; set; }
            public List<IConvertible[]> dataTable { get; set; }
        }
        public class AccountTypeViewModel
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
        //public class membershipCardAndType
        //{
        //    public MembershipCardEditViewModel membershipCard;
        //    public IEnumerable<MembershipTypeViewModel> membershipTypeList;
        //    public membershipCardAndType()
        //    {
        //        membershipCard = null;
        //        membershipTypeList = null;
        //    }
        //}
        [HttpPost]
        public async Task<JsonResult> UploadExcelActiveAll(HttpPostedFileBase FileUpload, int FilterMembershipCard, int brandId)
        {
            List<string> data = new List<string>();
            if (FileUpload != null)
            {
                // tdata.ExecuteCommand("truncate table OtherCompanyAssets");  
                if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string pathToExcelFile = "";
                    List<string> redundantCardCode = new List<string>();
                    StreamReader fileReader = null;
                    try
                    {
                        string filename = FileUpload.FileName;
                        string targetpath = Server.MapPath("~/Doc/");
                        if (System.IO.File.Exists(targetpath + filename))
                            System.IO.File.Delete(targetpath + filename);
                        FileUpload.SaveAs(targetpath + filename);
                        pathToExcelFile = targetpath + filename;
                        fileReader = System.IO.File.OpenText(pathToExcelFile);
                        CsvReader csv = new CsvReader(fileReader);
                        CustomerApi cApi = new CustomerApi();
                        MembershipCardApi mcApi = new MembershipCardApi();
                        List<CustomerExcel> customerRecords = new List<CustomerExcel>();
                        while (csv.Read())
                        {
                            var name = csv.GetField<string>("Name");
                            var email = csv.GetField<string>("Email");
                            var address = csv.GetField<string>("Address");
                            var gender = csv.GetField<string>("Gender").Equals("Nam");
                            var phone = '0' + csv.GetField<string>("Phone");
                            CustomerExcel c = new CustomerExcel(name, email, address, gender, phone);
                            customerRecords.Add(c);
                        }
                        List<HmsService.Models.Entities.MembershipCard> listDeactiveCard = new List<HmsService.Models.Entities.MembershipCard>();
                        if (FilterMembershipCard != -1)
                        {
                            listDeactiveCard = mcApi.GetListDeactiveByFilter(FilterMembershipCard, brandId).ToList();
                        }
                        else
                        {
                            listDeactiveCard = mcApi.GetMembershipCardDeactiveByBranId(brandId);
                        }
                        if (customerRecords.Count() > listDeactiveCard.Count())
                        {
                            return Json(new
                            {
                                success = false,
                                data = "Số lượng khách hàng vượt quá số lượng thẻ chưa kích hoạt",
                            });
                        }

                        for (int i = 0; i < customerRecords.Count(); i++)
                        {
                            var customerExist = cApi.GetCustomerByEmail(customerRecords[i].Email, brandId);
                            if (customerExist == null)
                            {
                                CustomerViewModel customer = new CustomerViewModel();
                                customer.Name = customerRecords[i].Name;
                                customer.Email = customerRecords[i].Email;
                                customer.Address = customerRecords[i].Address;
                                customer.Gender = customerRecords[i].Gender;
                                customer.Phone = customerRecords[i].Phone;
                                customer.Type = 1; // Tạm thời mặc định Customer Type = 1
                                customer.BrandId = brandId;
                                var customerReturn = cApi.CreateCustomer2(customer);

                                listDeactiveCard[i].CustomerId = customerReturn.CustomerID;
                                //listDeactiveCard[i].Customer.Name = customerReturn.Name; Dòng này để làm gì??????
                                listDeactiveCard[i].Status = 1;// Kích hoạt
                                MembershipCardViewModel model = new MembershipCardViewModel(listDeactiveCard[i]);
                                await mcApi.EditAsync(model.Id, model);
                            }
                            else
                            {
                                redundantCardCode.Add(customerRecords[i].Name + " - " + customerRecords[i].Email);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        return Json(new
                        {
                            success = false,
                            data = "Xin kiểm tra lại tên sheet, tên các cột trong file excel theo đúng format định sẵn",
                        });
                    }
                    finally
                    {
                        fileReader.Dispose();
                        fileReader.Close();
                    }

                    //deleting excel file from folder  
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    return Json(new
                    {
                        success = true,
                        redundantCardCodeList = redundantCardCode,
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //alert message for invalid file format  
                    data.Add("Chỉ được upload file Excel!");
                    data.ToArray();
                    return Json(new
                    {
                        success = false,
                        data = data
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (FileUpload == null) data.Add("Xin chọn file excel");
                data.ToArray();
                return Json(new
                {
                    success = false,
                    data = data
                }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        public JsonResult SearchMemberShipCard(string membershipCardCode)
        {
            MembershipCardApi cardApi = new MembershipCardApi();
            CustomerApi cusApi = new CustomerApi();
            AccountApi accApi = new AccountApi();
            bool accept = false;
            var card = cardApi.GetMembershipCardByCode(membershipCardCode);

            var account = new Account();
            try
            {
                account = card.Accounts.Count(q => q.Type == (int)AccountTypeEnum.CreditAccount) > 0 ?
                card.Accounts.First(q => q.Type == (int)AccountTypeEnum.CreditAccount) : null;
            }
            catch(NullReferenceException e)
            {
                account = null;
            }

            if (card != null)
            {
                if (card.CustomerId != null)
                {
                    var cus = cusApi.GetCustomerById((int)card.CustomerId);
                    if (card.Status == (int)MembershipStatusEnum.Suspensed)
                    {
                        return Json(new
                        {
                            checkCard = accept,
                            customerId = card.CustomerId,
                            customerName = cus.Name,
                            phone = cus.Phone,
                            address = cus.Address,
                            email = cus.Email,
                            balance = account.Balance,
                            message = "Thẻ đã bị khóa"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    accept = true;
                    return Json(new
                    {
                        checkCard = accept,
                        customerId = card.CustomerId,
                        customerName = cus.Name,
                        phone = cus.Phone,
                        address = cus.Address,
                        email = cus.Email,
                        balance = account.Balance,
                        message = "Thẻ hợp lệ"
                    }, JsonRequestBehavior.AllowGet);
                }
                else if (card.Active == false)
                {
                    return Json(new
                    {
                        customerId = -1,
                        customerName = "",
                        checkCard = accept,
                        message = "Thẻ đã bị hủy ! xin vui lòng liên hệ admin"
                    }, JsonRequestBehavior.AllowGet);
                }
                else if (card.Status == (int)MembershipStatusEnum.Suspensed)
                {
                    return Json(new
                    {
                        customerId = -1,
                        customerName = "",
                        checkCard = accept,
                        message = "Thẻ đã bị khóa"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                checkCard = accept,
                message = "Không tìm thấy thẻ"
            }, JsonRequestBehavior.AllowGet);
        }

        #region edit MembershipCard Code
        public ActionResult EditMembershipCardCode()
        {
            return View();
        }
        public ActionResult CheckMembershipCardCode(int brandId, string membershipCardCode)
        {
            try
            {
                var membershipCardApi = new MembershipCardApi();
                var currentCard = membershipCardApi.GetMembershipCardByCode(membershipCardCode.Trim());
                if (currentCard != null && currentCard.BrandId == brandId)
                {
                    if (currentCard.Accounts != null && currentCard.Accounts.Count > 0)
                    {
                        foreach (var account in currentCard.Accounts)
                        {
                            if (account.Type == (int)AccountTypeEnum.CreditAccount)
                            {
                                var customer = currentCard.Customer;
                                return Json(new { success = true, AccountName = account.AccountName, Customer = new { Name = customer.Name, Phone = customer.Phone, Address = customer.Address } }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }

                    return Json(new { success = false, message = "Không tồn tại tài khoảng thanh toán!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Thẻ không tồn tại!" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liện hệ admin" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<ActionResult> UpdateMembershipCardCode(int brandId, string membershipCardCode, string membershipCardNew, string membershipCardAccount, string membershipCardPassword)
        {
            try
            {
                var membershipCardApi = new MembershipCardApi();
                var user = await UserManager.FindAsync(membershipCardAccount, membershipCardPassword);
                if (user == null)
                {
                    return Json(new { success = false, message = "Vui lòng xem lại thông tin" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (user.Roles.Any(q => q.RoleId == "12"))
                    {
                        var currentCard = membershipCardApi.GetMembershipCardByCode(membershipCardCode);
                        MembershipCardEditViewModel membershipCard = membershipCardApi.GetMembershipCardById(currentCard.Id);
                        membershipCard.MembershipCardCode = membershipCardNew;
                        membershipCardApi.UpdateMembershipCard(membershipCard);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Bạn không đủ quyền cập nhật mã thẻ" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liện hệ admin" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, message = "Cập nhật mã thẻ thành công" }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        public async Task<bool> CreateOrderAsync(int brandId, int storeId, string productCode, int customerId)
        {
            DateTime time = Utils.GetCurrentDateTime();
            var orderApi = new OrderApi();
            var productApi = new ProductApi();

            var product = productApi.GetProductByCode(productCode);
            var amount = product.Price;

             var orderDetail = new OrderDetail()
            {
                ProductID = product.ProductID,
                TotalAmount = amount,
                Quantity = 1,
                OrderDate = time,
                Status = 0,
                FinalAmount = amount,
                IsAddition = false,
                Discount = 0,
                UnitPrice = amount,
                StoreId = storeId,
            };

            var payment = new HmsService.Models.Entities.Payment()
            {
                Amount = amount,
                CurrencyCode = "VND",
                FCAmount = (decimal)amount,
                PayTime = time,
                Status = (int)PaymentStatusEnum.New,
                Type = (int)PaymentTypeEnum.Cash,
            };

            var order = new Order()
            {
                InvoiceID = Utils.GetCurrentDateTime().Ticks.ToString() + "-" + storeId,
                CheckInDate = time,
                TotalAmount = amount,
                Discount = 0,
                DiscountOrderDetail = 0,
                FinalAmount = amount,
                OrderStatus = (int)OrderStatusEnum.Finish,
                OrderType = (int)OrderTypeEnum.OrderCard,
                RentType = 0,
                CheckInPerson = User.Identity.Name,
                CustomerID = customerId,
                StoreID = storeId,
                SourceType = (int)SourceTypeEnum.CallCenter,
                OrderDetailsTotalQuantity = 1,
                CheckinHour = time.Hour,
                GroupPaymentStatus = 0,

                OrderDetails = new List<OrderDetail>() { orderDetail },
                Payments = new List<HmsService.Models.Entities.Payment>() { payment },
            };

            var rs = orderApi.CreateOrder(order);
            if (rs)
            {
                //NotifyMessage sent Queue, and Pos
                var msg = new NotifyOrder()
                {
                    StoreId = storeId,
                    //StoreName = store.Name,
                    NotifyType = (int)NotifyMessageType.OrderChange,
                    Content = "Có đơn hàng mới",
                    OrderId = order.RentID,
                };
                await Utils.RequestOrderWebApi(msg);

                return true;
            }
            return false;
        }
    }
    public class CustomerExcel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool Gender;

        public string Phone { get; set; }


        public CustomerExcel(string name, string email, string address, bool gender, string phone)
        {
            this.Name = name;
            this.Email = email;
            this.Address = address;
            this.Gender = gender;
            this.Phone = phone;
        }
    }


}