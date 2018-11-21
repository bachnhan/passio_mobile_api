using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Facebook;
using HmsService.Sdk;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using Newtonsoft.Json.Linq;
using System.Text;
using Jose;
using Newtonsoft.Json;
using Wisky.Api.Models;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web.Http;
using System.Net;
using Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers;
using AutoMapper;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Wisky.Api.Controllers.API
{
    public class MobileApiController : Controller
    {
        private const string privateKey = ConstantManager.PRIVATE_KEY;
        private static byte[] secretKey = Encoding.UTF8.GetBytes(privateKey);
        private List<int> listMobileTypeId = ConstantManager.LIST_MOBILE_TYPE_ID;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="membershipcardId"></param>
        public bool createVoucherNewBie(MembershipCard membershipcard, int promotionId, PromotionDetail promotionDetail)
        {
            //call api to process
            string seed = DateTime.Now.Minute + "" + DateTime.Now.Second + "" + Math.Abs((int)DateTime.Now.Ticks);

            string voucherCode = seed.Substring(0, 10);

            MembershipCardApi mcApi = new MembershipCardApi();
            VoucherApi voucherApi = new VoucherApi();
            Voucher voucher = new Voucher();
            voucher.Active = true;
            voucher.isUsed = false;
            voucher.MembershipCardId = membershipcard.Id;
            voucher.MembershipCard = membershipcard;
            voucher.PromotionID = promotionId;
            voucher.PromotionDetailID = promotionDetail.PromotionDetailID;
            voucher.PromotionDetail = promotionDetail;
            voucher.Quantity = 1;
            voucher.VoucherCode = voucherCode;
            voucher.UsedQuantity = 0;
            voucher.IsGetted = true;

            bool check = voucherApi.CreateVoucher(voucher);
            return check;

        }
        /// <summary>
        /// hàm dùng để client đăng nhặp fb bằng sdk
        /// </summary>
        /// <param name="fbAccessToken">đây là access token của facebook trả về khi đăng nhập thành công</param>
        /// <param name="brandID">là mã brand của passio</param>
        /// <returns></returns>
        // GET: MobileApi
        public JsonResult LoginByFacebook(string fbAccessToken, int brandID)
        {
            try
            {
                #region call api to process
                var customerApi = new CustomerApi();
                AccountApi accountApi = new AccountApi();
                MembershipCardApi mcApi = new MembershipCardApi();
                MembershipCardTypeApi msctApi = new MembershipCardTypeApi();
                #endregion

                //Get Facebook client profile by accessToken
                //dùng facebook client để lây  account data ở fb 
                var fb = new FacebookClient();
                fb.AccessToken = fbAccessToken;
                dynamic me = fb.Get("me?fields=id,link,name,picture.width(200).height(200),email");
                string fbId = me.id;
                string fbEmail = me.email;
                string fbName = me.name;
                bool? fbGender = null;
                string fbpicUrl = "";
                var jsonPic = me.picture;
                //get picture
                fbpicUrl = (string)(((JsonObject)(((JsonObject)jsonPic)["data"]))["url"]);
                //Check Cusomter Facebook email exitested
                var customer = customerApi.GetCustomersByBrand(brandID).Where(q => q.FacebookId.Equals(fbId)).FirstOrDefault();
                //nếu customer có trong db
                if (customer != null)
                {
                    //Check customer membershipcard
                    checkMembershipCard(customer, brandID);
                    if (string.IsNullOrEmpty(customer.picURL))
                    {
                        customer.picURL = fbpicUrl;
                    }
                    customerApi.UpdateCustomer(customer);
                    var newToken = GenerateToken(customer.CustomerID.ToString());
                    // lấy thẻ  của customer 
                    var card = mcApi.GetMembershipCardActiveByCustomerId(customer.CustomerID)
                        .Where(q => q.MembershipTypeId != null && listMobileTypeId
                        .Contains((int)q.MembershipTypeId)).FirstOrDefault();
                    if (card == null)
                    {
                        return Json(new
                        {
                            status = new
                            {
                                success = false,
                                status = ConstantManager.STATUS_SUCCESS,
                                message = ConstantManager.MES_FAIL
                            },
                            data = new { }
                        }, JsonRequestBehavior.AllowGet);
                    }
                    if (card.MembershipCardType == null)
                    {
                        card.MembershipCardType = msctApi.GetMembershipCardTypeById((int)card.MembershipTypeId);
                    }
                    //get point and balance of customer
                    double balance = 0;
                    int point = 0;
                    // lấy điểm và số tiền trong account của custoemr
                    if (card.PhysicalCardCode == null)
                    {
                        balance = (double)card.Accounts.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault().Balance;
                        point = (int)card.Accounts.Where(q => q.Type == (int)AccountTypeEnum.PointAccount).FirstOrDefault().Balance;
                    }
                    else
                    {
                        var physicalCard = mcApi.GetMembershipCardByCode(card.PhysicalCardCode);
                        balance = (double)physicalCard.Accounts.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault().Balance;
                        point = (int)card.Accounts.Where(q => q.Type == (int)AccountTypeEnum.PointAccount).FirstOrDefault().Balance;
                    }
                    return Json(new
                    {
                        status = new
                        {
                            success = true,
                            message = ConstantManager.MES_LOGIN_SUCCESS,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new
                        {
                            data = new
                            {
                                access_token = newToken,
                                customer_id = customer.CustomerID,
                                is_first_login = string.IsNullOrEmpty(customer.AccountPhone) ? true : false,
                                is_phone_no_login = false,
                                name = customer.Name,
                                address = customer.Address,
                                phone = customer.AccountPhone,
                                email = customer.Email,
                                pic_url = customer.picURL,
                                membershicard = new
                                {
                                    code = card.PhysicalCardCode != null ? card.PhysicalCardCode : card.MembershipCardCode,
                                    type_name = card.MembershipCardType == null ? "" : card.MembershipCardType.TypeName,
                                    type_level = card.MembershipCardType == null ? 0 : card.MembershipCardType.TypeLevel,
                                    balance = balance,
                                    point = point
                                }
                            }
                        },
                    }, JsonRequestBehavior.AllowGet);
                }
                else   //nếu customer không có trong db
                {
                    //Create customer by facebook profile data
                    CustomerViewModel newCustomer = new CustomerViewModel();
                    newCustomer.Name = fbName;
                    newCustomer.Gender = fbGender;
                    newCustomer.FacebookId = fbId;
                    newCustomer.Email = fbEmail;
                    newCustomer.BrandId = brandID;
                    newCustomer.picURL = fbpicUrl;
                    int resId = -1;
                    resId = customerApi.AddCustomer(newCustomer);
                    if (resId != -1)
                    {
                        var createdCustomer = customerApi.GetCustomerEntityById(resId);
                        //create membershipcard and account , 1 membershipcard 2 account: payment and pointment
                        creataNewMembershipCard(brandID, createdCustomer.ToViewModel<Customer, CustomerViewModel>());
                        //Apply first login gift
                        applyFirstLoginGift(createdCustomer.CustomerID, brandID);
                        //generate new token
                        var newToken = GenerateToken(createdCustomer.CustomerID.ToString());
                        //lấy thẻ của customer
                        var card = mcApi.GetMembershipCardActiveByCustomerId(createdCustomer.CustomerID)
                            .Where(q => q.MembershipTypeId != null && listMobileTypeId.Contains((int)q.MembershipTypeId)).FirstOrDefault();
                        if (card == null)
                        {
                            return Json(new
                            {
                                status = new
                                {
                                    success = false,
                                    status = ConstantManager.STATUS_SUCCESS,
                                    message = ConstantManager.MES_FAIL
                                },
                                data = new { }
                            }, JsonRequestBehavior.AllowGet);
                        }
                        card.MembershipCardType = msctApi.GetMembershipCardTypeById((int)card.MembershipTypeId);
                        //lấy tiền và điểm account của customer đó
                        double balance = 0;
                        int point = 0;
                        if (card.PhysicalCardCode == null)
                        {
                            balance = (double)card.Accounts.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault().Balance;
                            point = (int)card.Accounts.Where(q => q.Type == (int)AccountTypeEnum.PointAccount).FirstOrDefault().Balance;
                        }
                        else
                        {
                            var physicalCard = mcApi.GetMembershipCardByCode(card.PhysicalCardCode);
                            balance = (double)physicalCard.Accounts.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault().Balance;
                            point = (int)card.Accounts.Where(q => q.Type == (int)AccountTypeEnum.PointAccount).FirstOrDefault().Balance;
                        }
                        return Json(new
                        {
                            status = new
                            {
                                success = true,
                                message = ConstantManager.MES_LOGIN_SUCCESS,
                                status = ConstantManager.STATUS_SUCCESS
                            },
                            data = new
                            {
                                data = new
                                {
                                    access_token = newToken,
                                    customer_id = createdCustomer.CustomerID,
                                    is_first_login = true,
                                    is_phone_no_login = false,
                                    name = createdCustomer.Name,
                                    pic_url = createdCustomer.picURL,
                                    phone = "",
                                    email = createdCustomer.Email,
                                    balance = balance,
                                    point = point,
                                    membershicard = new
                                    {
                                        code = card.PhysicalCardCode != null ? card.PhysicalCardCode : card.MembershipCardCode,
                                        type_name = card.MembershipCardType == null ? "" : card.MembershipCardType.TypeName,
                                        type_level = card.MembershipCardType == null ? 0 : card.MembershipCardType.TypeLevel,
                                        balance = balance,
                                        point = point
                                    }
                                }
                            },
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_LOGIN_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                status = new
                {
                    success = false,
                    message = ConstantManager.MES_LOGIN_FAIL,
                    status = ConstantManager.STATUS_SUCCESS
                },
                data = new { }
            }, JsonRequestBehavior.AllowGet);

        }
        ///// <summary>
        ///// là hàm dùng để kết nối fb khi từ account kit mún chuyển đổi kết nối bằng fb
        ///// </summary>
        ///// <param name="AccessToken">là access token của server passio</param>
        ///// <param name="FBAccessToken">là access token của server fb </param>
        ///// <returns></returns>
        //public JsonResult ConnectToFacebookAccount(string AccessToken, string FBAccessToken)
        //{
        //    //call api to process 
        //    CustomerApi customerApi = new CustomerApi();
        //    //lấy customerId từ access token 
        //    int customerId = Int32.Parse(getCustomerIdFromToken(AccessToken));
        //    //lấy customer theo customerId
        //    var customer = customerApi.GetCustomerByID(customerId);
        //    //check customer có bằng null ko ???
        //    //nếu có thì trả về false 
        //    if (customer == null)
        //    {
        //        return Json(new
        //        {
        //            status = new
        //            {
        //                success = false,
        //                message = ConstantManager.MES_LOGIN_FAIL,
        //                status = ConstantManager.STATUS_SUCCESS
        //            },
        //            data = new { }
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    else//nếu customer tồn tại 
        //    {
        //        //gọi account từ fb thông qua fb client
        //        var fb = new FacebookClient();
        //        fb.AccessToken = FBAccessToken;
        //        dynamic me = fb.Get("me?fields=id,link,name,picture.width(200).height(200),email");
        //        string fbId = me.id;
        //        string fbEmail = me.email;
        //        string fbName = me.name;
        //        string fbpicUrl = "";
        //        var jsonPic = me.picture;
        //        fbpicUrl = (string)(((JsonObject)(((JsonObject)jsonPic)["data"]))["url"]);
        //        customer.FacebookId = fbId;
        //        customerApi.UpdateCustomer(customer);
        //        var customerNew = customerApi.GetCustomerByID(customer.CustomerID);
        //        var newToken = GenerateToken(customerId.ToString());
        //        return Json(new
        //        {
        //            status = new
        //            {
        //                success = true,
        //                message = ConstantManager.MES_LOGIN_FAIL,
        //                status = ConstantManager.STATUS_SUCCESS
        //            },
        //            data = new
        //            {
        //                data = new {
        //                    access_token = newToken,
        //                    name = customerNew.Name,
        //                    email = customerNew.Email,
        //                    is_first_login = false,
        //                    is_phone_no_login = false,
        //                }
        //            }
        //        }, JsonRequestBehavior.AllowGet);
        //    }

        //}
        public string ConvertFormart(DateTime time)
        {
            string resultFormat = "";
            DateTime now = DateTime.Now;
            //check if now is today
            if (time.Day == now.Day && time.Month == now.Month && time.Year == now.Year)
            {
                if (time.Day == now.Day && time.Hour <= now.Hour && time.Minute == now.Minute)
                {
                    return "vài giây trước";
                }
                if (time.Day == now.Day && time.Hour == now.Hour)
                {
                    return "vài phút trước";
                }
                resultFormat = "Hôm nay, " + time.ToString(ConstantManager.FORMART_DATETIME_2);
                return resultFormat;
            }
            else
            {
                int checkYesterDay = now.Day - time.Day;
                if (checkYesterDay == 1)
                {
                    resultFormat = "Hôm qua, " + time.ToString(ConstantManager.FORMART_DATETIME_2);
                    return resultFormat;
                }
                int dayOfWeek = (int)time.DayOfWeek;
                switch (dayOfWeek)
                {
                    case 0:
                        resultFormat = "Chủ nhật, " + time.ToString(ConstantManager.FORMART_DATETIME_2);
                        break;
                    case 1:
                        resultFormat = "Thứ hai, " + time.ToString(ConstantManager.FORMART_DATETIME_2);
                        break;
                    case 2:
                        resultFormat = "Thứ ba, " + time.ToString(ConstantManager.FORMART_DATETIME_2);
                        break;
                    case 3:
                        resultFormat = "Thứ tư, " + time.ToString(ConstantManager.FORMART_DATETIME_2);
                        break;
                    case 4:
                        resultFormat = "Thứ năm, " + time.ToString(ConstantManager.FORMART_DATETIME_2);
                        break;
                    case 5:
                        resultFormat = "Thứ sáu, " + time.ToString(ConstantManager.FORMART_DATETIME_2);
                        break;
                    case 6:
                        resultFormat = "Thứ bảy, " + time.ToString(ConstantManager.FORMART_DATETIME_2);
                        break;
                }
                return resultFormat;
            }
        }
        /// <summary>
        /// get notification from server
        /// </summary>
        /// <returns></returns>
        public JsonResult GetNotification(string accessToken, int pageCurrent, int pageLimit)
        {
            //call Api to process 
            NotificationApi notiApi = new NotificationApi();
            //fill data 
            int customerId = Int32.Parse(getCustomerIdFromToken(accessToken));
            if (string.IsNullOrEmpty(accessToken))
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_CONNECTFB_VALID,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var notications = notiApi.GetNotificationByCustomerIdAndGeneral(customerId);
                var notication = notications.Skip((pageCurrent - 1) * pageLimit).Take(pageLimit).ToList();
                if (notication.Count == 0)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            message = ConstantManager.MES_NOTIFICATION_FAIL,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }

                var result = from n in notication
                             select new
                             {
                                 id = n.Id,
                                 title = n.Title,
                                 title_en = n.Title_En,
                                 description = n.Description,
                                 description_en = n.Description_En,
                                 type = n.Type,
                                 create_date = n.CreateDate == null ? "" : ConvertFormart(n.CreateDate.Value),
                                 update_date = n.UpdateDate == null ? "" : ConvertFormart(n.UpdateDate.Value),
                                 pic_url = n.PicUrl,
                                 short_description = n.Opening,
                                 content = n.Content,

                             };
                return Json(new
                {
                    status = new
                    {
                        success = true,
                        message = ConstantManager.MES_NOTIFICATION_SUCCESS,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new
                    {
                        data = result
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_NOTIFICATION_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// là hàm dùng để kết nối fb khi từ account kit mún chuyển đổi kết nối bằng fb
        /// </summary>
        /// <param name="AccessToken">là access token của server passio</param>
        /// <param name="FBAccessToken">là access token của server fb </param>
        /// <returns></returns>
        public JsonResult ConnectToFaceBook(string accessToken, string fbAccessTokenNew, int brandId)
        {
            // call Api to process
            CustomerApi customerApi = new CustomerApi();
            //check access token and fbAccessToken 
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(fbAccessTokenNew))
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_CONNECTFB_VALID,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
            int customerId = Int32.Parse(getCustomerIdFromToken(accessToken));
            try
            {
                var customer = customerApi.GetCustomerEntityById(customerId);
                if (customer == null)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            message = ConstantManager.MES_EDITACOUNTPHONE_CUSTOMER_FAIL,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
                var fb = new FacebookClient();
                fb.AccessToken = fbAccessTokenNew;
                dynamic me = fb.Get("me?fields=id,link,name,picture.width(200).height(200),email");
                string fbId = me.id;
                string fbEmail = me.email;
                string fbName = me.name;
                bool? fbGender = null;
                string fbpicUrl = "";
                var jsonPic = me.picture;
                //get picture
                fbpicUrl = (string)(((JsonObject)(((JsonObject)jsonPic)["data"]))["url"]);
                //get picture
                bool checkCustomer = customerApi.IsCustomerByFacbookId(fbId, brandId);
                if (checkCustomer)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            message = ConstantManager.MES_CONNECTFB_ISTHERE,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
                //Check Cusomter Facebook email exitested
                customer.FacebookId = fbId;
                if (string.IsNullOrEmpty(customer.Name))
                {
                    customer.Name = fbName;
                }
                if (string.IsNullOrEmpty(customer.Email))
                {
                    customer.Email = fbEmail;
                }
                if (string.IsNullOrEmpty(customer.picURL))
                {
                    customer.picURL = fbpicUrl;
                }
                customerApi.UpdateEntityCustomer(customer);
                var customerNew = customerApi.GetCustomerEntityById(customer.CustomerID);

                if (customerNew == null)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            message = ConstantManager.MES_CONNECTFB_FAIL,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
                var newToken = GenerateToken(customerNew.CustomerID.ToString());
                return Json(new
                {
                    status = new
                    {
                        success = true,
                        message = ConstantManager.MES_EDITACOUNTPHONE_SUCCESS,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new
                    {
                        data = new
                        {
                            access_token = newToken,
                            name = customerNew.Name,
                            pic_url = customerNew.picURL,
                            email = customerNew.Email,
                            is_first_login = false,
                            is_phone_no_login = false,
                            is_connected = string.IsNullOrEmpty(customerNew.FacebookId) ? false : true

                        }
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_CONNECTFB_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }


        }
        /// <summary>
        /// sửa đổi số đt đã đăng nhập , mún đổi thành số đt khác
        /// </summary>
        /// <param name="accessToken">access token từ server</param>
        /// <param name="fbAccessTokenNew">access token mới của fb từ sđt</param>
        /// <returns></returns>
        public JsonResult EditAccountPhone(string accessToken, string fbAccessTokenNew, int brandId)
        {
            // call Api to process
            CustomerApi customerApi = new CustomerApi();

            //get customer from access token
            try
            {//check access token and fbAccessToken 
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(fbAccessTokenNew))
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            message = ConstantManager.MES_CONNECTFB_VALID,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
                //lấy customerId thông qua access token
                int customerId = Int32.Parse(getCustomerIdFromToken(accessToken));
                //lấy customer từ customer Id 
                var customer = customerApi.GetCustomerEntityById(customerId);
                //if không tồn tại customer từ server => trả false
                if (customer == null)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            message = ConstantManager.MES_EDITACOUNTPHONE_FAIL,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
                //gọi fb client để lấy tài khoản từ server fb
                var fb = new FacebookClient();
                fb.AccessToken = fbAccessTokenNew;
                //dynamic result = fb.Get("me");
                var result = getPhone(fbAccessTokenNew);
                if (string.IsNullOrEmpty(result))
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            message = ConstantManager.MES_EDITACOUNTPHONE_FAIL,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new
                        {
                            data = new
                            {
                            }
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                var parJson = JObject.Parse(result);
                var phone = parJson["phone"]["number"].ToString();
                var checkCustomer = customerApi.GetCustomersByAccountPhone(phone, brandId).FirstOrDefault();
                if (checkCustomer != null)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            message = ConstantManager.MES_CONNECTACCOUTNKIT_ISTHERE,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new
                        {
                            data = new
                            {
                            }
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                customer.AccountPhone = phone;
                customer.Phone = phone;
                customerApi.UpdateEntityCustomer(customer);// update customer
                var newCustomer = customerApi.GetCustomerById(customer.CustomerID);
                if (newCustomer == null)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            message = ConstantManager.MES_EDITACOUNTPHONE_SUCCESS,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new
                        {

                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                string newToken = GenerateToken(newCustomer.CustomerID.ToString());
                return Json(new
                {
                    status = new
                    {
                        success = true,
                        message = ConstantManager.MES_EDITACOUNTPHONE_SUCCESS,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new
                    {
                        data = new
                        {
                            phone = newCustomer.AccountPhone,
                        }
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_EDITACOUNTPHONE_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }

        }
        private static void checkMembershipCard(CustomerViewModel customer, int brandID)
        {
            //Check MemberShipCard exist
            var membershipCardApi = new MembershipCardApi();
            var accountApi = new AccountApi();
            if (customer.MembershipCards.Count() > 0)
            {
                var MembershipCard = customer.MembershipCards.Where(q => q.Active).FirstOrDefault();

                //Check Acount exist
                var accountList = MembershipCard.Accounts.ToList();
                if (accountList.Count() > 0)
                {
                    //Create CreditAccount
                    if (accountList.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount) == null)
                    {
                        accountApi.CreateAccountByMemCard(MembershipCard.MembershipCardCode, 0, 1, MembershipCard.Id, (int)AccountTypeEnum.CreditAccount);
                    }
                    //Create Pointaccount
                    if (accountList.Where(q => q.Type == (int)AccountTypeEnum.PointAccount) == null)
                    {
                        accountApi.CreateAccountByMemCard(MembershipCard.MembershipCardCode, 0, 1, MembershipCard.Id, (int)AccountTypeEnum.PointAccount);
                    }
                }
                else
                {
                    accountApi.CreateAccountByMemCard(MembershipCard.MembershipCardCode, 0, 1, MembershipCard.Id, (int)AccountTypeEnum.CreditAccount);
                    accountApi.CreateAccountByMemCard(MembershipCard.MembershipCardCode, 0, 1, MembershipCard.Id, (int)AccountTypeEnum.PointAccount);
                }
            }
            else
            {
                //Create new MembershipCard Account
                creataNewMembershipCard(brandID, customer);
            }
        }
        private static void creataNewMembershipCard(int brandID, CustomerViewModel customer)
        {
            var membershipCardApi = new MembershipCardApi();
            var accountApi = new AccountApi();
            string newCardCode = generateNewMemberShipCardCode(brandID);
            while (membershipCardApi.GetMembershipCardByCodeByBrandId(newCardCode, brandID) != null)
            {
                newCardCode = generateNewMemberShipCardCode(brandID);
            }
            var newMembershipCard = membershipCardApi.AddMembershipCard(newCardCode, brandID, customer.CustomerID, (int)MembershipCardTypeEnum.Green);
            accountApi.CreateAccountByMemCard(newMembershipCard.MembershipCardCode, 0, 1, newMembershipCard.Id, (int)AccountTypeEnum.CreditAccount);
            accountApi.CreateAccountByMemCard(newMembershipCard.MembershipCardCode, 0, 1, newMembershipCard.Id, (int)AccountTypeEnum.PointAccount);
        }
        private static string generateNewMemberShipCardCode(int brandID)
        {
            var random = new Random();
            switch (brandID)
            {
                case (int)BrandEnum.Passio:
                    string chars = "0123456789";
                    int length = 10;
                    return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                default:
                    return String.Empty;
            }
        }
        public string getPhone(String accessToken)
        {
            string url = "https://graph.accountkit.com/v1.3/me/?access_token=" + accessToken;
            string parameter = "access_token=" + accessToken;
            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json; charset=utf-8";
            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                string jsonText;
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    jsonText = sr.ReadToEnd();
                }
                return jsonText;
            }
            catch
            {
                return "";
            }

        }
        public JsonResult LoginByPhoneNumber(string fbAccessToken, int brandID)
        {
            AccountApi accountApi = new AccountApi();
            MembershipCardApi mcApi = new MembershipCardApi();
            MembershipCardTypeApi msctApi = new MembershipCardTypeApi();
            var fb = new FacebookClient();
            fb.AccessToken = fbAccessToken;
            //dynamic result = fb.Get("me");
            var result = getPhone(fbAccessToken);
            if (string.IsNullOrEmpty(result))
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_LOGIN_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new
                    {
                        data = new
                        {
                        }
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            var parJson = JObject.Parse(result);
            var phone = parJson["phone"]["number"].ToString();
            CustomerApi customerApi = new CustomerApi();
            var customer = customerApi.GetCustomersByAccountPhone(phone, brandID).FirstOrDefault();/*.ToViewModel<Customer, CustomerViewModel>();*/
            if (customer == null)
            {
                //Create customer
                CustomerViewModel newCustomer = new CustomerViewModel();
                newCustomer.AccountPhone = phone;
                newCustomer.Phone = phone;
                newCustomer.BrandId = brandID;
                int resId = -1;
                resId = customerApi.AddCustomer(newCustomer);
                if (resId != -1)
                {
                    var createdCustomer = customerApi.GetCustomerEntityById(resId);
                    //Create new MembershipCard Account
                    creataNewMembershipCard(brandID, createdCustomer.ToViewModel<Customer, CustomerViewModel>());
                    //apply first login gift
                    applyFirstLoginGift(createdCustomer.CustomerID, brandID);
                    //generate new token
                    var newToken = GenerateToken(createdCustomer.CustomerID.ToString());
                    //lấy thẻ của customer
                    var card = mcApi.GetMembershipCardActiveByCustomerId(createdCustomer.CustomerID)
                        .Where(q => q.MembershipTypeId != null && listMobileTypeId.Contains((int)q.MembershipTypeId)).FirstOrDefault();
                    if (card == null)
                    {
                        return Json(new
                        {
                            status = new
                            {
                                success = false,
                                status = ConstantManager.STATUS_SUCCESS,
                                message = ConstantManager.MES_FAIL
                            },
                            data = new { }
                        }, JsonRequestBehavior.AllowGet);
                    }
                    card.MembershipCardType = msctApi.GetMembershipCardTypeById((int)card.MembershipTypeId);
                    //lấy tiền và điểm account của customer đó
                    double balance = 0;
                    int point = 0;
                    if (card.PhysicalCardCode == null)
                    {
                        balance = (double)card.Accounts.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault().Balance;
                        point = (int)card.Accounts.Where(q => q.Type == (int)AccountTypeEnum.PointAccount).FirstOrDefault().Balance;
                    }
                    else
                    {
                        var physicalCard = mcApi.GetMembershipCardByCode(card.PhysicalCardCode);
                        balance = (double)physicalCard.Accounts.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault().Balance;
                        point = (int)card.Accounts.Where(q => q.Type == (int)AccountTypeEnum.PointAccount).FirstOrDefault().Balance;
                    }
                    return Json(new
                    {
                        status = new
                        {
                            success = true,
                            message = ConstantManager.MES_LOGIN_SUCCESS,
                            status = ConstantManager.STATUS_SUCCESS
                        },
                        data = new
                        {
                            data = new
                            {
                                access_token = newToken,
                                customer_id = createdCustomer.CustomerID,
                                is_first_login = true,
                                is_phone_no_login = true,
                                phone = createdCustomer.AccountPhone,
                                pic_url = createdCustomer.picURL,
                                email = createdCustomer.Email,
                                membershicard = new
                                {
                                    code = card.MembershipCardCode,
                                    type_name = card.MembershipCardType == null ? "" : card.MembershipCardType.TypeName,
                                    type_level = card.MembershipCardType == null ? 0 : card.MembershipCardType.TypeLevel,
                                    balance = balance,
                                    point = point
                                }
                            }
                        },
                    }, JsonRequestBehavior.AllowGet);
                }
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_LOGIN_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
            var customerViewModel = customer.ToViewModel<Customer, CustomerViewModel>();
            if (customer != null && !String.IsNullOrEmpty(customer.Name) && !String.IsNullOrEmpty(customer.Email))
            {
                //Check customer membershipcard
                checkMembershipCard(customerViewModel, brandID);
                var newToken = GenerateToken(customer.CustomerID.ToString());
                //get point and balance
                double balance = 0;
                int point = 0;
                var card = mcApi.GetMembershipCardActiveByCustomerId(customer.CustomerID)
                       .Where(q => q.MembershipTypeId != null && listMobileTypeId
                       .Contains((int)q.MembershipTypeId)).FirstOrDefault();
                if (card == null)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_FAIL
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
                card.MembershipCardType = msctApi.GetMembershipCardTypeById((int)card.MembershipTypeId);
                if (card.PhysicalCardCode == null)
                {
                    balance = (double)card.Accounts.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault().Balance;
                    point = (int)card.Accounts.Where(q => q.Type == (int)AccountTypeEnum.PointAccount).FirstOrDefault().Balance;
                }
                else
                {
                    var physicalCard = mcApi.GetMembershipCardByCode(card.PhysicalCardCode);
                    balance = (double)physicalCard.Accounts.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault().Balance;
                    point = (int)card.Accounts.Where(q => q.Type == (int)AccountTypeEnum.PointAccount).FirstOrDefault().Balance;
                }
                return Json(new
                {
                    status = new
                    {
                        success = true,
                        message = ConstantManager.MES_LOGIN_SUCCESS,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new
                    {
                        data = new
                        {
                            access_token = newToken,
                            customer_id = customer.CustomerID,
                            is_first_login = false,
                            is_phone_no_login = true,
                            name = customer.Name,
                            address = customer.Address == null ? "" : customer.Address,
                            phone = customer.AccountPhone,
                            email = customer.Email,
                            pic_url = customer.picURL,
                            membershicard = new
                            {
                                code = card.MembershipCardCode,
                                type_name = card.MembershipCardType == null ? "" : card.MembershipCardType.TypeName,
                                type_level = card.MembershipCardType == null ? 0 : card.MembershipCardType.TypeLevel,
                                balance = balance,
                                point = point
                            }
                            //  birthday = customer.BirthDay.Value == null ? "": customer.BirthDay.Value.ToShortDateString()
                        }
                    },
                }, JsonRequestBehavior.AllowGet);
            }
            else if (customer != null && (String.IsNullOrEmpty(customer.Name) || String.IsNullOrEmpty(customer.Email)))
            {
                checkMembershipCard(customerViewModel, brandID);
                var newToken = GenerateToken(customer.CustomerID.ToString());

                return Json(new
                {
                    status = new
                    {
                        success = true,
                        message = ConstantManager.MES_LOGIN_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new
                    {
                        data = new
                        {
                            access_token = newToken,
                            customer_id = customer.CustomerID,
                            is_first_login = true,
                            is_phone_no_login = true,
                            phone = phone,
                            email = customer.Email,
                            pic_url = customer.picURL
                        }
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                status = new
                {
                    success = false,
                    message = ConstantManager.MES_LOGIN_FAIL,
                    status = ConstantManager.STATUS_SUCCESS
                },
                data = new
                {
                    data = new
                    {
                        is_first_login = true,
                        is_phone_no_login = true,
                        phone = phone,
                        pic_url = customer.picURL
                    }
                }
            }, JsonRequestBehavior.AllowGet);
        }

        private void applyFirstLoginGift(int customerId, int brandId)
        {
            var mcApi = new MembershipCardApi();
            var msctApi = new MembershipCardTypeApi();
            var promotionDetailApi = new PromotionDetailApi();
            var promotionApi = new PromotionApi();
            var notifiApi = new NotificationApi();
            var accountApi = new AccountApi();
            var time = DateTime.Now;
            //voucher code newbie
            var membershipcard = mcApi.GetMembershipCardActiveByCustomerIdByBrandId(customerId, brandId).Where(q => listMobileTypeId.Contains((int)q.MembershipTypeId)).FirstOrDefault();
            var membershipcardType = msctApi.GetMembershipCardTypeById(membershipcard.MembershipTypeId.Value);
            for (int i = 0; i < 4; i++)
            {
                createVoucherNewBie(membershipcard, 110, promotionDetailApi.GetDetailByPromotionDetailCode("786NK-10%001"));
            }
            createVoucherNewBie(membershipcard, 109, promotionDetailApi.GetDetailByPromotionDetailCode("786NK-30%001"));
            //Create notification
            NotificationViewModel notification = new NotificationViewModel()
            {
                Active = true,
                CreateDate = time,
                UpdateDate = time,
                Title = "Quà tặng",
                Opening = "Quý khách được tặng 5 voucher giảm giá cho lần đăng nhập đầu tiên. Áp dụng tại cửa hàng 786 Nguyễn Kiệm.",
                CustomerId = customerId,
                Type = (int)NotificationsTypeEnum.Notify
            };
            notifiApi.Create(notification);
            for (int i = 0; i < 2; i++)
            {
                createVoucherNewBie(membershipcard, 117, promotionDetailApi.GetDetailByPromotionDetailCode("Latte-50%001"));
            }
            //Create notification
            NotificationViewModel notification3 = new NotificationViewModel()
            {
                Active = true,
                CreateDate = time,
                UpdateDate = time,
                Title = "Quà tặng",
                Opening = "Quý khách được tặng 2 voucher giảm giá Latte 50% cho lần đăng nhập đầu tiên.",
                CustomerId = customerId,
                Type = (int)NotificationsTypeEnum.Notify
            };
            notifiApi.Create(notification3);
            //plus 5 memberpoint for first login
            var PointAccount = membershipcard.Accounts.Where(q => q.Type == (int)AccountTypeEnum.PointAccount).FirstOrDefault();
            PointAccount.Balance += 5;
            accountApi.BaseService.Update(PointAccount);
            //Create notification for plus 5 memberpoint
            NotificationViewModel notification2 = new NotificationViewModel()
            {
                Active = true,
                CreateDate = time,
                UpdateDate = time,
                Title = "Tích điểm",
                Opening = "Quý khách được tặng 5 điểm tích lũy cho lần đăng nhập đầu tiên.",
                CustomerId = customerId,
                Type = (int)NotificationsTypeEnum.Notify
            };
            notifiApi.Create(notification2);
        }
        [System.Web.Mvc.HttpPost]
        public JsonResult updateProfileUser(string accessToken, string fullName, string phoneNumber, string email, string birthDay)
        {
            //Get image from request
            byte[] avatarImage = null;
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                int fileSizeInBytes = file.ContentLength;

                using (var br = new BinaryReader(file.InputStream))
                {
                    avatarImage = br.ReadBytes(fileSizeInBytes);
                }
            }

            //api to proccess
            CustomerApi customerApi = new CustomerApi();
            AccountApi accountApi = new AccountApi();
            MembershipCardApi mcApi = new MembershipCardApi();
            int customerId = Int32.Parse(getCustomerIdFromToken(accessToken));
            var customer = customerApi.GetCustomerEntityById(customerId);
            if (!string.IsNullOrEmpty(accessToken) || customer != null)
            {
                try
                {
                    var newToken = GenerateToken(customer.CustomerID.ToString());
                    // byte[] avatarImage = Encoding.ASCII.GetBytes(avatarImage);
                    if (avatarImage != null)
                    {
                        //update is false if customer is first update
                        bool update = false;
                        if (customer.picURL != null)
                        {
                            update = true;
                        }
                        var picUrl = SaveImageToServer(avatarImage, customerId, update);
                        customer.picURL = picUrl;
                    }
                    customer.Name = fullName;
                    customer.AccountPhone = phoneNumber;
                    customer.Email = email;

                    //Update birthday 
                    var birthDayFormatted = new DateTime();
                    if (DateTime.TryParseExact(birthDay, ConstantManager.FORMART_DATETIME_3, CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDayFormatted) && customer.BirthDay == null){
                        customer.BirthDay = birthDayFormatted;
                    }
                    
                    //Update Customer
                    customerApi.UpdateEntityCustomer(customer);
                    decimal balance = 0;
                    decimal point = 0;
                    var account = accountApi.GetAccountByCustomerId(customer.CustomerID);
                    foreach (var item in account)
                    {
                        if (item.Type == (int)AccountTypeEnum.CreditAccount)
                        {
                            balance = item.Balance == null ? 0 : item.Balance.Value;
                        }
                        if (item.Type == (int)AccountTypeEnum.PointAccount)
                        {
                            point = item.Balance == null ? 0 : item.Balance.Value;
                        }
                    }
                    var membershipcard = mcApi.GetMembershipCardActiveByCustomerId(customer.CustomerID).FirstOrDefault();
                    return Json(new
                    {
                        status = new
                        {
                            success = true,
                            message = ConstantManager.MES_UPDATE_SUCCESS,
                            status = ConstantManager.STATUS_SUCCESS,
                        },
                        data = new
                        {
                            data = new
                            {
                                access_token = newToken,
                                is_first_login = false,
                                name = customer.Name,
                                email = customer.Email,
                                phone = customer.AccountPhone,
                                pic_url = customer.picURL,
                                birth_day = (customer.BirthDay != null) ? ((DateTime)customer.BirthDay).ToString(ConstantManager.FORMART_DATETIME_3) : "",
                                point = point,
                                balance = balance,
                                card_code = membershipcard.MembershipCardCode
                            }
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            message = ConstantManager.MES_UPDATE_FAIL,
                            status = ConstantManager.STATUS_SUCCESS,
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        message = ConstantManager.MES_UPDATE_FAIL,
                        status = ConstantManager.STATUS_SUCCESS
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private string SaveImageToServer(byte[] avatarImage, int CustomerId, bool update)
        {
            var ms = new MemoryStream(avatarImage);
            var image = Image.FromStream(ms);
            string imageServerPath = ConstantManager.IMAGE_SERVER_PATH + "\\" + CustomerId.ToString();
            if (Directory.Exists(imageServerPath) == false)
            {
                Directory.CreateDirectory(imageServerPath);
            }
            if (update)
            {

                string fileName = CustomerId.ToString() + "_" + ConstantManager.IMAGE_FORMAT_EXTENSION;
                string filePath = imageServerPath + "\\" + fileName;

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                image.Save(filePath, ImageFormat.Png);
                return ConstantManager.IMAGE_SERVER_URL + "/" + CustomerId.ToString() + "/" + fileName;
            }
            else
            {
                string fileName = CustomerId.ToString() + "_" + DateTime.Now.Ticks.ToString() + ConstantManager.IMAGE_FORMAT_EXTENSION;
                string filePath = imageServerPath + "\\" + fileName;
                image.Save(filePath, ImageFormat.Png);
                return ConstantManager.IMAGE_SERVER_URL + "/" + CustomerId.ToString() + "/" + fileName;
            }

        }

        public JsonResult GetAccountInfo(int CustomerID, int brandID)
        {
            return Json(new
            {
                status = new
                {
                    success = true,
                    message = ConstantManager.MES_LOGIN_SUCCESS,
                    status = ConstantManager.STATUS_SUCCESS,
                },
                data = new { }
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// get history order by customerID, brandId 
        /// </summary>
        /// <param name="CustomerID">customerId of user</param>
        /// <param name="brandID">brandId </param>
        /// <param name="pageCurrent">current of page</param>
        /// <param name="pageLimit">what do you want to show how many number of record </param>
        /// <returns></returns>
        public JsonResult getHistoryOrder(string accessToken, int brandId, int pageCurrent, int pageLimit)
        {
            //test access token 

            int customerId = Int32.Parse(getCustomerIdFromToken(accessToken));
            #region call api to process 
            OrderApi orderApi = new OrderApi();
            StoreApi storeApi = new StoreApi();
            #endregion
            //get order by customerId and brandId

            var orders = orderApi.GetOrderByCustomerIdandBrandId(customerId, brandId).Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish || q.OrderStatus == (int)OrderStatusEnum.New).OrderByDescending(q => q.CheckInDate).ToList();
            int countOrder = orders.Count();
            var order = orders.Skip((pageCurrent - 1) * pageLimit).Take(pageLimit).ToList();
            //check list order 
            if (!order.Any())
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_FAIL
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
            Store store = new Store();
            store = storeApi.GetStoreById(orders.FirstOrDefault().StoreID.Value);//get store
            var mobileStore = storeApi.GetStoresByBrandIdAndType(brandId, (int)StoreTypeEnum.MobileApp).FirstOrDefault();
            var history_order = from o in order
                                select new
                                {
                                    invoice_id = o.InvoiceID,
                                    check_in_date = o.CheckInDate.Value.ToString(ConstantManager.FORMART_DATETIME),
                                    store_address = (o.Store != null) ? (o.Store.Name.Equals(mobileStore.Name) ? ConstantManager.MESS_ORDER_NEW : o.Store.Address) : "",
                                    final_amount = o.FinalAmount,
                                    member_point = o.MemberPoint,
                                    order_id = o.RentID
                                };
            return Json(new
            {
                status = new
                {
                    success = true,
                    status = ConstantManager.STATUS_SUCCESS,
                    message = ConstantManager.MES_SUCCESS,
                },
                data = new
                {
                    data = history_order,
                    totalOrderHistory = countOrder,
                }
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// get history order detail
        /// </summary>
        /// <param name="OrderID"></param>
        /// <param name="brandID"></param>
        /// <returns></returns>
        public JsonResult getHistoryOrderDetail(int orderID, int brandID)
        {
            try
            {

                #region call api order, orderdetail to process
                OrderApi orderApi = new OrderApi();
                OrderDetailApi orderDetailApi = new OrderDetailApi();
                StoreApi storeApi = new StoreApi();
                AccountApi accountApi = new AccountApi();
                PaymentApi paymentApi = new PaymentApi();
                ProductApi productApi = new ProductApi();
                ProductCategoryApi productCategoryApi = new ProductCategoryApi();
                #endregion
                //inital order to get value
                var orders = orderApi.GetOrderByIdAndBrandId(brandID, orderID);

                if (orders == null)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_FAIL,
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);

                }
                //get orderDetail by rentId
                var orderDetailParent = orderDetailApi.GetOrderDetailsByRentId(orderID).Where(q => q.ParentId == null && !q.Product.ProductName.Equals(ConstantManager.DELIVERY)).ToList();
                double deliveryFee = 0;
                var orderDetailDelivery = orderDetailApi.GetOrderDetailsByRentId(orderID).Where(q => q.ParentId == null && q.Product.ProductName.Equals(ConstantManager.DELIVERY)).ToList();
                if (orderDetailDelivery.Count() != 0)
                {
                    deliveryFee = orderDetailDelivery.FirstOrDefault().UnitPrice;
                }
                if (orderDetailParent.Count() == 0)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_FAIL,
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
                Store store = new Store();
                if (orders.StoreID != null)
                {
                    store = storeApi.GetStoreById(orders.StoreID.Value);//get store
                    if (store == null)
                    {
                        return Json(new
                        {
                            status = new
                            {
                                success = false,
                                status = ConstantManager.STATUS_SUCCESS,
                                message = ConstantManager.MES_FAIL,
                            },
                            data = new { }
                        }, JsonRequestBehavior.AllowGet);
                    }
                }

                //load extra
                var productExtra = from p in productCategoryApi.GetProductCategorieExtra()
                                   select new
                                   {
                                       catId = p.CateID,
                                       catName = p.CateName,
                                   };

                //////get size, productName, quanity, and unitPrice in OrderDetail
                ////var historyOrderDetail = (from orderDetail in orderDetails
                ////                          select new
                ////                          {
                ////                              orderDetailId = orderDetail.OrderDetailID,
                ////                              size = orderDetail.Product.Att1,
                ////                              productName = orderDetail.Product.ProductName,
                ////                              quantity = orderDetail.Quantity,
                ////                              productImage = orderDetail.Product.PicURL,
                ////                              unitPrice = orderDetail.UnitPrice,
                ////                              cateId = orderDetail.Product.CatID,
                ////                              parentId = orderDetail.ParentId
                ////                          }).ToList();
                ////get value of product, and product extra

                List<OrderDetail> list = new List<OrderDetail>();
                List<OrderDetail> listExtra = new List<OrderDetail>();
                List<OrderDetailAppModel> childOrderDetail = new List<OrderDetailAppModel>();
                Dictionary<OrderDetail, List<OrderDetailAppModel>> result = new Dictionary<OrderDetail, List<OrderDetailAppModel>>();
                foreach (var item in orderDetailParent)
                {
                    var childOdt = (from odt in orderDetailApi.GetOrderDetailIsExtra(item.OrderDetailID)
                                    select new OrderDetailAppModel
                                    {
                                        parent_id = odt.ParentId,
                                        cat_id = odt.Product.CatID,
                                        order_detail_id = odt.OrderDetailID,
                                        product_name = odt.Product.ProductName,
                                        product_image = odt.Product.PicURL,
                                        quantity = odt.Quantity,
                                        size = odt.Product.Att1,
                                        unit_price = odt.UnitPrice
                                    }).ToList();

                    result.Add(item, childOdt);
                }



                //get delivery fee
                //get order


                var data = from r in result

                           select new
                           {
                               order_detail_id = r.Key.OrderDetailID,
                               size = r.Key.Product.Att1,
                               product_name = r.Key.Product.ProductName,
                               quantity = r.Key.Quantity,
                               product_image = r.Key.Product.PicURL,
                               unit_price = r.Key.UnitPrice,
                               cat_id = r.Key.Product.CatID,
                               parent_id = r.Key.ParentId == null ? 0 : r.Key.ParentId,
                               product_extra = r.Value
                           };

                //fill data product
                var payment = paymentApi.GetPaymentByOrder(orderID).ToList();
                string paymentString = "";
                int paymentType = 0;
                if (payment.Count() > 0)
                {
                    //get enum of payment
                    if (payment.FirstOrDefault().Type == (int)PaymentTypeEnum.Cash)
                    {
                        paymentType = (int)PaymentTypeEnum.Cash;
                        paymentString = ConstantManager.MES_PAYMENT_CASH;
                    }
                    else if (payment.FirstOrDefault().Type == (int)PaymentTypeEnum.MoMo)
                    {
                        paymentType = (int)PaymentTypeEnum.MoMo;
                        paymentString = ConstantManager.MES_PAYMENT_MOMO;
                    }
                    else if (payment.FirstOrDefault().Type == (int)PaymentTypeEnum.MemberPayment)
                    {
                        paymentType = (int)PaymentTypeEnum.MemberPayment;
                        paymentString = ConstantManager.MES_PAYMENT_MEMBER_POINT;
                    }
                }
                //get store call center to check is confirm order ???
                var mobileStore = storeApi.GetStoresByBrandIdAndType(brandID, (int)StoreTypeEnum.MobileApp).FirstOrDefault();
                string storeName = "";
                string storeAddress = "";
                if (store != null)
                {
                    if (store.Name.Equals(mobileStore.Name))
                    {
                        storeName = ConstantManager.MESS_ORDER_NEW;
                    }
                    else
                    {
                        storeName = store.Name;
                        storeAddress = store.Address;
                    }
                }
                return Json(new
                {
                    status = new
                    {
                        success = true,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_SUCCESS,
                    },
                    data = new
                    {
                        data = new
                        {
                            delivery_fee = deliveryFee,
                            store_name = storeName,
                            store_address = storeAddress,
                            check_in_date = orders.CheckInDate.Value.ToString(ConstantManager.FORMART_DATETIME),
                            history_order_detail_list = data,
                            delivery_address = orders.DeliveryAddress == null ? "" : orders.DeliveryAddress,
                            total_amount = orders.TotalAmount - deliveryFee,
                            final_amount = orders.FinalAmount,
                            discount = orders.Discount + orders.DiscountOrderDetail,
                            // category_extra = productExtra.ToList(),
                            //    discount_order_detail = orders.DiscountOrderDetail,
                            customer_name = orders.Customer == null ? "" : orders.Receiver,
                            phone_number = orders.DeliveryPhone == null ? "" : orders.DeliveryPhone,
                            payment_type = paymentType,
                            payment_by = paymentString
                        }
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_FAIL,
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// return list product of passio
        /// </summary>
        /// <returns></returns>
        public JsonResult getListProduct(int brandId)
        {
            #region call api to process
            ProductApi productApi = new ProductApi();
            StoreApi storeApi = new StoreApi();
            ProductDetailMappingApi pdmApi = new ProductDetailMappingApi();
            ProductCategoryApi productCategoryApi = new ProductCategoryApi();
            #endregion
            var mobileStore = storeApi.GetStoresByBrandIdAndType(brandId, (int)StoreTypeEnum.MobileApp).FirstOrDefault();
            var product = pdmApi.GetProductByStoreId(mobileStore.ID).ToList();
            //get product if isDefaultChild = 1 (sản phẩm mặc định)
            //var product = productApi.GetAllActiveProductByBrand(brandId)
            //                        .Where(q => q.GeneralProductId == null
            //                                      && q.Active == true
            //                                      && q.ProductCategory.IsExtra == false
            //                                      && q.ProductCategory.IsDisplayed
            //                               ).ToList();

            if (product.Count() == 0)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_FAIL
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
            List<List<Product>> childProduct = new List<List<Product>>();
            Dictionary<Product, List<ProductAppModel>> result = new Dictionary<Product, List<ProductAppModel>>();
            try
            {
                foreach (var item in product)
                {
                    var tmpChildProduct = (from tmp in productApi.GetProductGeneralByProductId(item.Product.ProductID)
                                           select new ProductAppModel
                                           {
                                               product_id = tmp.ProductID,
                                               product_name = tmp.ProductName,
                                               price = tmp.Price,
                                               description = tmp.Description,
                                               pic_url = tmp.PicURL,
                                               cat_id = tmp.CatID,
                                               size = tmp.Att1,
                                               att2 = tmp.Att2,
                                               has_extra = tmp.HasExtra
                                           }).ToList();

                    if (tmpChildProduct == null)
                    {
                        // tmpChildProduct = new List<object>();
                    }

                    result.Add(item.Product, tmpChildProduct);
                }
                var data = (from r in result
                            select new
                            {
                                product_id = r.Key.ProductID,
                                product_name = r.Key.ProductName,
                                price = r.Key.Price,
                                description = r.Key.Description,
                                cat_id = r.Key.CatID,
                                has_extra = r.Key.HasExtra,
                                pic_url = r.Key.PicURL,
                                child_list = r.Value
                            }).ToList();
                //var data = from p in result
                //             select new
                //             {
                //                 productId = p.Key,
                //                 productName = p.ProductName,
                //                 picUrl = p.PicURL,
                //                 unitPrice = p.Price,
                //                 isExtra = p.HasExtra,
                //                 catId = p.CatID
                //             };
                //get product category
                var productCategory = productCategoryApi
                    .GetByBrandId(brandId)
                    .Where(q => q.Type != (int)ProductCategoryType.CardPayment
                        && q.CateName != ConstantManager.PRODUCT_CATEGORY_EVENT
                        && q.CateName != ConstantManager.PRODUCT_CATEGORY_OTHER
                        && q.CateName != ConstantManager.PRODUCT_CATEGORY_ITALI
                        && q.CateName != ConstantManager.PRODUCT_CATEGORY_FRESH_JUICE
                        && q.CateName != ConstantManager.PRODUCT_CATEGORY_CAKE
                        && q.CateName != ConstantManager.PRODUCT_CATEGORY_COMBO
                        && q.CateName != ConstantManager.PRODUCT_CATEGORY_SPARKLING
                        && q.CateName != ConstantManager.PRODUCT_CATEGORY_FRESH_AND_EASY
                        ).OrderBy(q => q.DisplayOrder)
                        .ToList();
                //to get Passio Coffee in first record
                //List<ProductCategoryAppModel> categoryList = new List<ProductCategoryAppModel>();
                //ProductCategoryAppModel passioCoffee = new ProductCategoryAppModel();
                //int i = 1;
                //foreach (var item in productCategory)
                //{
                //    ProductCategoryAppModel tmpCategory = new ProductCategoryAppModel();
                //    if(!item.CateName.Equals(ConstantManager.PRODUCT_CATEGORY_PASSIO_COFFEE))
                //    {
                //        tmpCategory.id = i;
                //        tmpCategory.cat_id = item.CateID;
                //        tmpCategory.cat_name = item.CateName;
                //        categoryList.Add(tmpCategory);
                //    }else
                //    {
                //        passioCoffee.cat_id = item.CateID;
                //        passioCoffee.cat_name = item.CateName;
                //    }
                //    i++;
                //}
                ////to category in first record
                //passioCoffee.id = i;
                //categoryList.Add(passioCoffee);

                //get cateId and cateName
                var category = from pc in productCategory

                               select new
                               {
                                   cat_id = pc.CateID,
                                   cat_name = pc.CateName
                               };
                var deliveryFee = productApi.GetAllActiveProductByBrand(brandId).Where(q => q.ProductName.Equals(ConstantManager.DELIVERY)).FirstOrDefault();

                return Json(new
                {
                    status = new
                    {
                        success = true,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_SUCCESS,
                    },
                    data = new
                    {
                        data = new
                        {
                            product_list = data,//chỗ này chứa data
                            product_category_list = category.ToList(),
                            delivery_fee = deliveryFee != null ? deliveryFee.Price : 0
                        }
                    }

                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_FAIL
                    },
                    data = new { }

                }, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// get product detail with extra, infomation, price
        /// </summary>
        /// <param name="productId">id of product</param>
        /// <returns></returns>
        public JsonResult getProductDetail(int productId)
        {
            #region call api to process
            ProductApi productApi = new ProductApi();
            CategoryExtraMappingApi categoryExtraMapping = new CategoryExtraMappingApi();
            #endregion
            var product = productApi.GetProductByIdEntity(productId);
            if (product == null)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_FAIL
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);

            }
            //get categoryExtraMapping by product CatID
            var categoryExrta = categoryExtraMapping.GetByPrimaryCategoryId(product.CatID);
            //get product extra by eachProduct have CatId in catergoryExtraMapping
            var productExtra = from p in productApi.GetAllProducts()
                               join c in categoryExrta
                               on p.CatID equals c.ExtraCategoryId
                               where p.Active == true && !p.ProductName.Equals(ConstantManager.DELIVERY)
                               select new
                               {
                                   product_name = p.ProductName,
                                   product_id = p.ProductID,
                                   price = p.Price,
                               };

            return Json(new
            {
                status = new
                {
                    success = true,
                    status = ConstantManager.STATUS_SUCCESS,
                    message = ConstantManager.MES_SUCCESS
                },
                data = new
                {
                    data = new
                    {
                        product_name = product.ProductName,
                        product_id = product.ProductID,
                        description = product.Description,
                        price = product.Price,
                        size = product.Att1,
                        product_extra = productExtra.ToList()
                    }
                }

            }, JsonRequestBehavior.AllowGet);

        }
        public PromotionDetail getPromotionDetail(string code)
        {
            PromotionDetail result = new PromotionDetail();
            #region call api to process
            PromotionDetailApi promotionDetailApi = new PromotionDetailApi();
            PromotionApi promotionApi = new PromotionApi();
            #endregion
            //get date to get promotion is experied ??
            var promotion = promotionApi.GetPromotionByDateAndCode(code);
            if (promotion == null)
            {
                result.DiscountAmount = 0;
                result.DiscountRate = 0;
                return result;
            }
            var promotionDetail = promotionDetailApi.GetDetailByPromotionDetailCode(promotion.PromotionCode);
            if (promotionDetail == null)
            {
                result.DiscountAmount = 0;
                result.DiscountRate = 0;
                return result;
            }
            return promotionDetail;

        }
        public PromotionDetail checkPromotion(string code, string accessToken, int brandId, OrderViewModel order)
        {
            #region call api to process
            PromotionDetailApi promotionDetailApi = new PromotionDetailApi();
            MembershipCardApi mscApi = new MembershipCardApi();
            PromotionApi promotionApi = new PromotionApi();
            ProductApi productApi = new ProductApi();
            VoucherApi voucherApi = new VoucherApi();
            #endregion
            #region Membershipcard
            int customerId = Int32.Parse(getCustomerIdFromToken(accessToken));
            var card = mscApi.GetMembershipCardActiveByCustomerIdByBrandId(customerId, brandId).Where(q => listMobileTypeId.Contains((int)q.MembershipTypeId)).FirstOrDefault();
            if (card == null)
            {
                return null;
            }
            #endregion
            //TODO get voucher có isUsed = false, db chưa thêm isUsed
            var voucher = voucherApi.GetVoucherIsNotUsedAndCode(code);
            if (voucher != null && voucher.MembershipCardId != null && voucher.MembershipCardId != card.Id)
            {
                return null;
            }
            //var voucher = voucherAp
            //TODO after have voucher
            #region Voucher

            if (voucher == null)
            {
                return null;
            }

            #endregion
            //get date to get promotion is experied ??
            #region Promotion 
            DateTime now = DateTime.Now;
            var promotion = promotionApi.GetPromotionByDateAndId(voucher.PromotionID);
            if (promotion == null)
            {
                return null;
            }
            else if (!(promotion.ApplyFromTime <= now.Hour && now.Hour <= promotion.ApplyToTime))
            {
                return null;
            }

            #endregion
            #region PromtionDetail
            #region PromtionDetail
            // loop to get total amount, final amount to get check in promotiondetail
            double finalAmount = 0;

            foreach (var item in order.OrderDetails)
            {
                finalAmount += (productApi.GetProductById(item.ProductID).Price * item.Quantity);
            }

            var promotionDetail = promotionDetailApi.GetDetailByPromotionDetailCode(voucher.PromotionDetail.PromotionDetailCode);
            if (promotionDetail == null)
            {
                return null;
            }
            //check promotion detail is have min, max order != null ???
            if (promotionDetail.MinOrderAmount != null || promotionDetail.MaxOrderAmount != null)
            {
                if (finalAmount < promotionDetail.MinOrderAmount)
                {

                    return null;
                }
                else
                {
                    if (promotionDetail.DiscountRate == null)
                    {
                        promotionDetail.DiscountRate = 0;
                    }
                    else if (promotionDetail.DiscountAmount == null)
                    {
                        promotionDetail.DiscountAmount = 0;
                    }
                    return promotionDetail;

                    #endregion
                }
            }
            return promotionDetail;
            //var promotionDetail = promotionDetailApi.GetDetailByCode(promotion.PromotionCode).FirstOrDefault();
            //if (promotionDetail == null)
            //{
            //    return null;
            //}
            //if (promotionDetail.DiscountRate == null)
            //{
            //    promotionDetail.DiscountRate = 0;
            //}
            //else if (promotionDetail.DiscountAmount == null)
            //{
            //    promotionDetail.DiscountAmount = 0;
            //}
            //return promotionDetail;

            #endregion
        }

        /// <summary>
        /// CheckPromotion
        /// </summary>
        /// <param name="PromotionCode">Mã giảm giá</param>
        /// <returns></returns>
        public JsonResult checkVoucher(string code, string accessToken, int brandId, OrderViewModel order)
        {
            #region call api to process
            PromotionDetailApi promotionDetailApi = new PromotionDetailApi();
            MembershipCardApi mscApi = new MembershipCardApi();
            PromotionApi promotionApi = new PromotionApi();
            ProductApi productApi = new ProductApi();
            VoucherApi voucherApi = new VoucherApi();
            StoreApi storeApi = new StoreApi();
            #endregion
            #region Membershipcard
            bool isFreeShip = false;//variable to check free shipping??? 
            int customerId = Int32.Parse(getCustomerIdFromToken(accessToken));
            var card = mscApi.GetMembershipCardActiveByCustomerIdByBrandId(customerId, brandId).Where(q => listMobileTypeId.Contains((int)q.MembershipTypeId)).FirstOrDefault();
            if (card == null)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_PROMOTION_FAIL,
                    },
                    data = new
                    {
                    }
                });
            }

            #endregion
            //TODO get voucher có isUsed = false, db chưa thêm isUsed
            var voucher = voucherApi.GetVoucherIsNotUsedAndCode(code);
            if (voucher != null && voucher.MembershipCardId != null && voucher.MembershipCardId != card.Id)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_PROMOTION_FAIL,
                    },
                    data = new
                    {
                    }
                });
            }
            //var voucher = voucherAp
            //TODO after have voucher
            #region Voucher

            if (voucher == null)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_PROMOTION_FAIL,
                    },
                    data = new
                    {
                    }
                });
            }



            #endregion
            //get date to get promotion is experied ??
            #region Promotion 
            var promotion = promotionApi.GetPromotionByDateAndId(voucher.PromotionID);
            DateTime now = DateTime.Now;
            if (promotion == null)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_PROMOTION_FAIL,
                    },
                    data = new
                    {
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else if (!(promotion.ApplyFromTime <= now.Hour && now.Hour <= promotion.ApplyToTime))
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_PROMOTION_DATE_FAIL,
                    },
                    data = new
                    {
                    }
                }, JsonRequestBehavior.AllowGet);
            }

            #endregion

            #region check store promotion mapping
            //get mobile storeid
            var mobileStore = storeApi.GetStoresByBrandIdAndType(brandId, (int)StoreTypeEnum.MobileApp).FirstOrDefault();
            if (mobileStore == null || !promotion.PromotionStoreMappings.Where(p => p.Active == true).Select(q => q.StoreId).Contains(mobileStore.ID))
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_PROMOTION_FAIL,
                    },
                    data = new
                    {
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            #region PromtionDetail rule 1 min , max order???
            // loop to get total amount, final amount to get check in promotiondetail
            double finalAmount = 0;

            foreach (var item in order.OrderDetails)
            {
                finalAmount += (productApi.GetProductById(item.ProductID).Price * item.Quantity);
            }

            var promotionDetail = promotionDetailApi.GetDetailByPromotionDetailCode(voucher.PromotionDetail.PromotionDetailCode);
            if (promotionDetail == null)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_PROMOTION_FAIL,
                    },
                    data = new
                    {
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            //biên trả về cho client
            double result = 0;
            //check promotion detail is have min, max order != null ???
            CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");   // format money in Viet nam
            if (promotionDetail.MinOrderAmount != null || promotionDetail.MaxOrderAmount != null)
            {
                if (finalAmount < promotionDetail.MinOrderAmount)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_CHECK_VOUCHER_RULE_1 + (promotionDetail.MinOrderAmount == null ? "" : promotionDetail.MinOrderAmount.Value.ToString("#,###", cul.NumberFormat)) + " VNĐ",
                        },
                        data = new
                        {
                            promotion_detail_id = promotionDetail.PromotionDetailID,
                            promotion_detail_code = code,
                            discount_amount = 0,
                            is_free_ship = isFreeShip
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    double tmpAmount = promotionDetail.DiscountAmount == null ? 0 : System.Convert.ToDouble(promotionDetail.DiscountAmount.Value);
                    double tmpRate = promotionDetail.DiscountRate == null ? 0 : promotionDetail.DiscountRate.Value;
                    if (tmpRate > 0)
                    {
                        result = (finalAmount * promotionDetail.DiscountRate.Value) / 100;
                    }
                    else if (tmpAmount > 0)
                    {
                        result = System.Convert.ToDouble(promotionDetail.DiscountAmount.Value);
                    }
                    return Json(new
                    {
                        status = new
                        {
                            success = true,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_PROMOTION_SUCCESS,
                        },
                        data = new
                        {
                            data = new
                            {
                                promotion_detail_id = promotionDetail.PromotionDetailID,
                                promotion_detail_code = code,
                                discount_amount = result,
                                //      discount_rate = promotionDetail.DiscountRate,
                                is_free_ship = isFreeShip
                            }
                        }
                    }, JsonRequestBehavior.AllowGet);


                }
            }
            #endregion
            //else if min, max order is null and product code is not null,
            #region RULE 2 buy min, max quantity of each product 

            else if (promotionDetail.BuyProductCode != null)
            {
                //list product discount 
                List<ProductDiscount> listProductDiscount = new List<ProductDiscount>();

                string mesMinBuyProduct = "";
                bool checkCount = false;//check quanitty min order buy
                var pmDetail = promotionDetailApi.GetDetailViewModelById(promotionDetail.PromotionDetailID);
                int countProduct = 0;
                foreach (var item in order.OrderDetails)
                {
                    var tmpProductOrder = productApi.GetProductEntityById(item.ProductID);
                    //if order product is child product, use parent product code instead
                    string tmpParentProductCode = "";
                    if (tmpProductOrder.GeneralProductId != null)
                    {
                        tmpParentProductCode = productApi.GetProductEntityById(tmpProductOrder.GeneralProductId.Value).Code;
                    }
                    //get product name by code in promotion detail
                    var tmpProductDiscount = productApi.GetProductByCode(pmDetail.BuyProductCode);
                    if (tmpProductOrder.Code == pmDetail.BuyProductCode || tmpParentProductCode == pmDetail.BuyProductCode)
                    {
                        countProduct += item.Quantity;
                        double tmpAmount = pmDetail.DiscountAmount == null ? 0 : System.Convert.ToDouble(pmDetail.DiscountAmount.Value);
                        double tmpRate = pmDetail.DiscountRate == null ? 0 : pmDetail.DiscountRate.Value;
                        if (tmpAmount > 0)
                        {
                            result += System.Convert.ToDouble(pmDetail.DiscountAmount.Value) * item.Quantity;
                        }
                        else if (tmpRate > 0)
                        {
                            result += (tmpProductOrder.Price * pmDetail.DiscountRate.Value * item.Quantity) / 100;
                        }
                        if (countProduct < pmDetail.MinBuyQuantity)
                        {
                            mesMinBuyProduct = pmDetail.MinBuyQuantity + " " + (tmpProductDiscount == null ? "" : tmpProductDiscount.ProductName);
                            checkCount = false;
                        }
                        else
                        {
                            checkCount = true;
                        }
                    }
                }
                if (!checkCount)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_CHECK_VOUCHER_RULE_2 + mesMinBuyProduct,
                        },
                        data = new
                        {
                            data = new
                            {
                            }
                        }
                    }, JsonRequestBehavior.AllowGet);

                }
                return Json(new
                {
                    status = new
                    {
                        success = true,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_PROMOTION_SUCCESS,
                    },
                    data = new
                    {
                        data = new
                        {
                            promotion_detail_id = promotionDetail.PromotionDetailID,
                            promotion_detail_code = code,
                            discount_amount = result,
                            //      discount_rate = promotionDetail.DiscountRate,
                            is_free_ship = isFreeShip
                            //product_discount = listProductDiscount,
                            //is_free_ship = isFreeShip
                        }
                    }
                }, JsonRequestBehavior.AllowGet);


            }
            return Json(new
            {
                status = new
                {
                    success = false,
                    status = ConstantManager.STATUS_SUCCESS,
                    message = ConstantManager.MES_PROMOTION_FAIL,
                },
                data = new
                {
                    data = new
                    {
                        promotion_detail_id = promotionDetail.PromotionDetailID,
                        promotion_detail_code = code,
                        discount_amount = promotionDetail.DiscountAmount,
                        discount_rate = promotionDetail.DiscountRate,
                        is_free_ship = isFreeShip

                    }
                }
            }, JsonRequestBehavior.AllowGet);
            #endregion

        }
        public JsonResult getListStore(int brandId)
        {
            #region call api to process
            StoreApi storeApi = new StoreApi();
            #endregion
            var stores = storeApi.GetAllActiveStore(brandId).ToList();
            //get corresponding infomation of store
            var result = (from store in stores.ToList()
                          select new
                          {
                              id = store.ID,
                              address = store.Address,
                              lat = store.Lat,
                              lon = store.Lon,
                              phone_number = store.Phone,
                              open_time = store.OpenTime.Value.ToString(ConstantManager.FORMART_DATETIME),
                              closed_time = store.CloseTime.Value.ToString(ConstantManager.FORMART_DATETIME),
                              logo_URL = store.LogoUrl
                          }).ToList();
            if (stores.Count() == 0)//check store is null
            {
                //if null return fail
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_STORE_FAIL
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
            //if not null, return success
            return Json(new
            {
                status = new
                {
                    success = true,
                    status = ConstantManager.STATUS_SUCCESS,
                    message = ConstantManager.MES_STORE_SUCCESS,
                },
                data = new
                {
                    data = result
                }
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// return list store by district and city of passio
        /// </summary>
        /// <param name="brandId"></param>
        /// <returns></returns>
        public JsonResult getListStoreByGroup(int brandId)
        {
            #region call api to process
            StoreApi storeApi = new StoreApi();
            #endregion
            var stores = storeApi.GetAllActiveStore(brandId).ToList();
            //get corresponding infomation of store

            //var result = (from store in stores.ToList()
            //              group store.Province  by store.Address 
            //              into storeGroup
            //              select new
            //              {
            //                  id = store.ID,
            //                  address = store.Address,
            //                  lat = store.Lat,
            //                  lon = store.Lon,
            //                  phone_number = store.Phone,
            //                  open_time = store.OpenTime.Value.ToString(ConstantManager.FORMART_DATETIME),
            //                  closed_time = store.CloseTime.Value.ToString(ConstantManager.FORMART_DATETIME)
            //              }).ToList();
            var provinces = from province in stores.ToList()
                            group province.Province by province.Province
                            into provinceGroup
                            select new
                            {
                                name = provinceGroup.Key,

                            };
            List<StoreViewModel> listStore = new List<StoreViewModel>();
            var districts = from district in stores.ToList()
                            group district.District by new
                            {
                                district.District,
                                district.Province
                            }
                             into districtGroup
                            select new
                            {
                                areas = districtGroup.Key,
                                name = districtGroup.Key.District
                            };
            Dictionary<string, List<string>> group = new Dictionary<string, List<string>>();

            foreach (var item in provinces)
            {
                List<string> districtStr = new List<string>();
                foreach (var item2 in districts)
                {
                    if (item2.areas.Province != null && item2.areas.Province.Equals(item.name))
                    {
                        districtStr.Add(item2.name);
                    }
                }
                if (item.name != null)
                {
                    group.Add(item.name, districtStr);
                }
            }


            Dictionary<string, StoreDistrictModel> result = new Dictionary<string, StoreDistrictModel>();

            foreach (var item in group)
            {
                List<string> district = new List<string>();
                district = item.Value;
                StoreDistrictModel groupDistrict = new StoreDistrictModel();
                Dictionary<string, List<StoreModel>> groupStore = new Dictionary<string, List<StoreModel>>();
                foreach (var item3 in district)
                {

                    List<StoreModel> storeView = new List<StoreModel>();
                    foreach (var item2 in stores)
                    {
                        StoreModel storeModel = new StoreModel();
                        storeModel.id = item2.ID;
                        storeModel.address = item2.Address;
                        storeModel.lat = item2.Lat;
                        storeModel.lon = item2.Lon;
                        storeModel.open_time = item2.OpenTime.Value.ToString(ConstantManager.FORMART_DATETIME);
                        storeModel.closed_time = item2.CloseTime.Value.ToString(ConstantManager.FORMART_DATETIME);
                        if (item2.District != null && item2.District.Equals(item3))
                        {

                            storeView.Add(storeModel);

                        }

                    }

                    groupStore.Add(item3, storeView);
                    var areas = (from g in groupStore
                                 select new Areas
                                 {
                                     name = g.Key,
                                     stores = g.Value
                                 }).ToList();
                    groupDistrict.areas = areas;
                }
                result.Add(item.Key, groupDistrict);
            }

            var cities = from r in result
                         select new
                         {

                             name = r.Key,
                             areas = r.Value.areas,

                         };

            //var result = (from store in stores.ToList()
            //              group store.Province by new
            //              {
            //                  store.OpenTime.Value,
            //                  store.CloseTime,
            //                  store.Address,
            //                  store.Phone,
            //                  store.Lat,
            //                  store.Lon
            //              }
            //             into storeGroup
            //              select new
            //              {
            //                  id = storeGroup.Key,
            //                  store = storeGroup.ToList()
            //              }).ToList();
            if (stores.Count() == 0)//check store is null
            {
                //if null return fail
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_STORE_FAIL
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
            //if not null, return success
            return Json(new
            {
                status = new
                {
                    success = true,
                    status = ConstantManager.STATUS_SUCCESS,
                    message = ConstantManager.MES_STORE_SUCCESS,
                },
                data = new
                {
                    data = new { cities = cities.ToList() }
                }
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// get blogpost
        /// </summary>
        /// <returns></returns>
        public JsonResult getBlogPost()
        {
            #region call api to process
            BlogPostApi blogPostApi = new BlogPostApi();
            #endregion
            try
            {
                int customerId = 0;
                var accessToken = Request.Headers.GetValues("accessToken");
                if (accessToken != null && accessToken.Count() > 0)
                {
                    customerId = Int32.Parse(getCustomerIdFromToken(accessToken.FirstOrDefault()));
                }
                //fill data
                var blogPostNews = blogPostApi.GetBlogPostOrderUpdateTimeAndType((int)BlogTypeEnum.News);
                var blogPostVote = blogPostApi.GetBlogPostOrderUpdateTimeAndType((int)BlogTypeEnum.Vote);
                var blogPost = blogPostNews.Union(blogPostVote);
                var blogPostForYou = blogPostApi.GetBlogPostOrderUpdateTimeAndType((int)BlogTypeEnum.ForYou);
                //string domainName = "http://" + HttpContext.Request.Url.Authority;
                string domainName = ConstantManager.IMAGE_SERVER_URL;
                if (blogPost.Count() == 0) //check blogpost is empty
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_FAIL
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
                else//if not return value
                {
                    var result = (from b in blogPost
                                  select new
                                  {
                                      id = b.Id,
                                      title = b.Title,
                                      title_en = b.Title_En,
                                      description = b.BlogContent == null ? "" : b.BlogContent,
                                      description_en = b.BlogContent_En == null ? "" : b.BlogContent_En,
                                      image = domainName + b.Image,
                                      short_description = b.Opening,
                                      url = (String.IsNullOrEmpty(b.URL)) ? "" :
                                     ((b.BlogType == (int)BlogTypeEnum.Vote) ?
                                     b.URL + "?cusId=" + customerId.ToString() : b.URL),
                                      position = b.Position
                                  }).OrderBy(q => q.position);
                    var resultForYou = (from b in blogPostForYou
                                        select new
                                        {
                                            id = b.Id,
                                            title = b.Title,
                                            title_en = b.Title_En,
                                            description = b.BlogContent == null ? "" : b.BlogContent,
                                            description_en = b.BlogContent_En == null ? "" : b.BlogContent_En,
                                            image = domainName + b.Image,
                                            short_description = b.Opening,
                                            url = b.URL == null ? "" : b.URL,
                                            position = b.Position
                                        }).OrderBy(q => q.position);
                    return Json(new
                    {
                        status = new
                        {
                            success = true,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_SUCCESS,
                        },
                        data = new
                        {
                            data = new
                            {
                                data = result,
                                for_you = resultForYou
                            }
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_FAIL
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private static string GenerateToken(string customerID)
        {
            var payload = customerID + ":" + DateTime.Now.Ticks.ToString();

            return JWT.Encode(payload, secretKey, JwsAlgorithm.HS256);
        }

        public static bool IsTokenValid(string token)
        {
            bool result = false;
            try
            {
                string key = JWT.Decode(token, secretKey);

                result = true;
            }
            catch
            {
            }
            return result;
        }

        private string getCustomerIdFromToken(string token)
        {
            string key = JWT.Decode(token, secretKey);
            string[] parts = key.Split(new char[] { ':' });
            return parts[0];
        }
        public JsonResult getDeliveryInfo(string accessToken)
        {

            int customerId = Int32.Parse(getCustomerIdFromToken(accessToken));
            DeliveryInfoApi deliveryInfoApi = new DeliveryInfoApi();
            CustomerApi customerApi = new CustomerApi();
            var customer = customerApi.GetCustomerByID(customerId);
            var delivery = deliveryInfoApi.Get().Where(q => q.CustomerId == customerId && q.Active == true).ToList();
            var delivery_info = from d in delivery
                                select new
                                {
                                    id = d.Id,
                                    //customer_id = d.CustomerId,
                                    customer_name = d.CustomerName,
                                    address = d.Address,
                                    phone = d.Phone,
                                    type = d.Type,
                                    active = d.Active,
                                    is_default_delivery_info = d.isDefaultDeliveryInfo
                                };
            if (delivery.Count() == 0)
            {
                return Json(new
                {
                    status = new
                    {
                        success = true,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_FAIL,
                    },
                    data = new
                    {
                        data = delivery
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            //var result = from s in delivery
            //             select new
            //             {
            //                 id = s.Id,
            //                 customerName = s.CustomerName,
            //                 address = 
            //             }
            return Json(new
            {
                status = new
                {
                    success = true,
                    status = ConstantManager.STATUS_SUCCESS,
                    message = ConstantManager.MES_SUCCESS,
                },
                data = new
                {
                    data = delivery_info.ToList()
                }
            }, JsonRequestBehavior.AllowGet);
        }
        [System.Web.Mvc.HttpPost]
        public JsonResult getCustomerInfomation(string accessToken, int brandId)
        {
            #region call api to proccess
            CustomerApi customerApi = new CustomerApi();
            MembershipCardApi mscApi = new MembershipCardApi();
            AccountApi accountApi = new AccountApi();
            MembershipCardTypeApi msctApi = new MembershipCardTypeApi();
            PromotionApi promotionApi = new PromotionApi();
            PromotionDetailApi pdApi = new PromotionDetailApi();
            #endregion
            //get customerid by accessToken
            try
            {
                var customerId = Int32.Parse(getCustomerIdFromToken(accessToken));
                //fill data to model
                var customer = customerApi.GetCustomerByID(customerId);

                if (customer == null)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_CHECK_CUSTOMERID_FAIL
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    double balance = 0;
                    int point = 0;
                    //get membershipcard + accounts
                    var membershipCard = mscApi.GetMembershipCardActiveByCustomerId(customerId)
                        .Where(q => q.MembershipTypeId != null && listMobileTypeId
                        .Contains((int)q.MembershipTypeId)).FirstOrDefault();
                    if (membershipCard == null)
                    {
                        return Json(new
                        {
                            status = new
                            {
                                success = false,
                                status = ConstantManager.STATUS_SUCCESS,
                                message = ConstantManager.MES_FAIL
                            },
                            data = new { }
                        }, JsonRequestBehavior.AllowGet);
                    }
                    if (membershipCard.MembershipCardType == null)
                    {
                        membershipCard.MembershipCardType = msctApi.GetMembershipCardTypeById((int)membershipCard.MembershipTypeId);
                    }
                    if (membershipCard.PhysicalCardCode == null)
                    {
                        balance = (double)membershipCard.Accounts.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault().Balance;
                        point = (int)membershipCard.Accounts.Where(q => q.Type == (int)AccountTypeEnum.PointAccount).FirstOrDefault().Balance;
                    }
                    else
                    {
                        var physicalCard = mscApi.GetMembershipCardByCode(membershipCard.PhysicalCardCode);
                        balance = (double)physicalCard.Accounts.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault().Balance;
                        point = (int)membershipCard.Accounts.Where(q => q.Type == (int)AccountTypeEnum.PointAccount).FirstOrDefault().Balance;
                    }
                    var newToken = GenerateToken(customer.CustomerID.ToString());
                    //get membershipcard
                    //TODO: lấy account type point, lấy balance so sánh trong membershipReward trong db nêu mức nào thì trả về type đó
                    //var listMemberShipCardTypeForMobile = msctApi.Get().Where(q => q.);

                    var data = new
                    {
                        access_token = newToken,
                        pic_url = customer.picURL,
                        name = customer.Name,
                        phone = customer.AccountPhone,
                        email = customer.Email,
                        is_connected = string.IsNullOrEmpty(customer.FacebookId) ? false : true
                    };

                    //get list mobile membershipCard type + promotion
                    //var listMBSCType = msctApi.GetAllMembershipCardTypeByBrand(brandId).Where(q => q.IsMobile == true).OrderBy(q => q.TypeLevel).ToList();
                    //foreach (var item in listMBSCType)
                    //{
                    //    item.promotions = getAvailablePromotionByMSCTypeAndBrandId(item, brandId);
                    //    foreach (var promotion in item.promotions)
                    //    {
                    //        promotion.FromDate.ToString(ConstantManager.FORMART_DATETIME);
                    //        promotion.ToDate.ToString(ConstantManager.FORMART_DATETIME);

                    //    }
                    //}
                    return Json(new
                    {
                        status = new
                        {
                            success = true,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_SUCCESS,
                        },
                        data = new
                        {
                            data = new
                            {
                                information = data,
                                membershipcard = new
                                {
                                    code = membershipCard.PhysicalCardCode != null ? membershipCard.PhysicalCardCode : membershipCard.MembershipCardCode,
                                    type_name = membershipCard.MembershipCardType != null ? membershipCard.MembershipCardType.TypeName : "",
                                    type_level = membershipCard.MembershipCardType != null ? membershipCard.MembershipCardType.TypeLevel : null,
                                    balance = balance,
                                    point = point
                                },

                            }
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_FAIL
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get available promotion by membershipcard type and brand Id
        /// </summary>
        /// <returns></returns>
        private List<PromotionViewModel> getAvailablePromotionByMSCTypeAndBrandId(MembershipCardTypeViewModel type, int brandId)
        {
            PromotionApi promotionApi = new PromotionApi();
            PromotionDetailApi pdApi = new PromotionDetailApi();
            var listPDPromotionCode = pdApi.GetPromotionDetailByAppendCode(type.AppendCode).Select(q => q.PromotionCode).ToList();
            var promotionAvailable = promotionApi.GetPromotionVMByBrandId(brandId).Where(q => listPDPromotionCode.Count() != 0 && listPDPromotionCode.Contains(q.PromotionCode)).ToList();
            return promotionAvailable;
        }

        public JsonResult getAvailableVoucher(string accessToken, int brandId)
        {
            int customerId = Int32.Parse(getCustomerIdFromToken(accessToken));
            VoucherApi voucherApi = new VoucherApi();
            MembershipCardApi mscApi = new MembershipCardApi();
            PromotionDetailApi pdApi = new PromotionDetailApi();
            PromotionApi promotionApi = new PromotionApi();
            //get membership card
            var membershipCard = mscApi.GetMembershipCardActiveByCustomerIdByBrandId(customerId, brandId).Where(q => listMobileTypeId.Contains((int)q.MembershipTypeId)).FirstOrDefault();
            if (membershipCard == null)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_FAIL,
                    },
                    data = new
                    {
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            var vouchers = voucherApi.GetVoucherByMembershipcardId(membershipCard.Id).OrderByDescending(q => q.VoucherID).ToList();
            if (vouchers == null)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_FAIL,
                    },
                    data = new
                    {
                    }
                }, JsonRequestBehavior.AllowGet);
            }

            List<VoucherAppModel> listVoucherModel = new List<VoucherAppModel>();
            foreach (var item in vouchers)
            {
                VoucherAppModel voucherModel = new VoucherAppModel();
                voucherModel.id = item.VoucherID;
                voucherModel.code = item.VoucherCode;
                voucherModel.is_used = item.isUsed;
                var promotion = promotionApi.Get(item.PromotionID);
                voucherModel.is_expired = (DateTime.Now > promotion.ToDate);
                voucherModel.is_infinity = ((promotion.ToDate.Year - promotion.FromDate.Year) > 10);
                PromotionAppModel promotionModel = new PromotionAppModel();
                if (promotion != null)
                {
                    promotionModel.apply_from_time = promotion.ApplyFromTime;
                    promotionModel.apply_to_time = promotion.ApplyToTime;
                    promotionModel.apply_level = promotion.ApplyLevel;
                    promotionModel.short_description = promotion.ShortDescription;
                    promotionModel.description = promotion.Description;
                    promotionModel.from_date = promotion.FromDate.ToString(ConstantManager.FORMART_DATETIME);
                    promotionModel.to_date = promotion.ToDate.ToString(ConstantManager.FORMART_DATETIME);
                    promotionModel.promotion_name = promotion.PromotionName;
                    promotionModel.promotion_id = promotion.PromotionID;
                    promotionModel.image_url = promotion.ImageUrl;
                }
                voucherModel.promotion = promotionModel;
                listVoucherModel.Add(voucherModel);
            }
            return Json(new
            {
                status = new
                {
                    success = true,
                    status = ConstantManager.STATUS_SUCCESS,
                    message = ConstantManager.MES_SUCCESS,
                },
                data = new
                {
                    data = listVoucherModel
                }
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// update delivery info 
        /// </summary>
        /// <param name="entity">delivery info entity</param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public JsonResult createDeliveryInfo(DeliveryInfo deliveryInfo, string accessToken)
        {

            //call Api to process 
            DeliveryInfoApi deliveryInfoApi = new DeliveryInfoApi();
            //parse from accessToken to get customerId
            var customerId = Int32.Parse(getCustomerIdFromToken(accessToken));
            //fill customerId into deliveryInfo.CustomerIds 
            deliveryInfo.CustomerId = customerId;

            //call api to create

            if (deliveryInfo != null)
            {
                var deliveryInfoOld = deliveryInfoApi.getDeliveryByCustomerId(customerId);
                if (deliveryInfoOld.Count() == (int)ConstantManager.MAX_DELIVERYINFO)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_FULL_DELIVERY
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                }
                if (deliveryInfoOld.Count() == 0)
                {
                    deliveryInfo.isDefaultDeliveryInfo = true;
                }
                if (deliveryInfo.isDefaultDeliveryInfo == true)
                {
                    foreach (var item in deliveryInfoOld)
                    {
                        if (item.isDefaultDeliveryInfo == true)
                        {
                            item.isDefaultDeliveryInfo = false;
                            deliveryInfoApi.updateDeliveryInfo(item);
                        }
                    }
                }
                deliveryInfoApi.createDeliveryInfo(deliveryInfo);
            }
            else
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_FAIL
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }


            return Json(new
            {
                status = new
                {
                    success = true,
                    status = ConstantManager.STATUS_SUCCESS,
                    message = ConstantManager.MES_SUCCESS,
                },
                data = new { }
            }, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// update active = false
        /// </summary>
        /// <param name="deliveryInfoId">id deliveryInfo</param>
        /// <param name="accessToken">accessToken</param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public JsonResult deleteDeliveryInfo(int deliveryInfoId, string accessToken)
        {
            //call Api to process 
            DeliveryInfoApi deliveryInfoApi = new DeliveryInfoApi();

            //parse from accessToken to get customerId
            var customerId = Int32.Parse(getCustomerIdFromToken(accessToken));
            var deliveryInfo = deliveryInfoApi.getDeliveryById(deliveryInfoId);
            //fill customerId into deliveryInfo.CustomerIds 
            deliveryInfo.CustomerId = customerId;
            //call api to create
            if (deliveryInfo != null)
            {
                if (deliveryInfo.isDefaultDeliveryInfo == true)
                {
                    var deliveryInfoOld = deliveryInfoApi.getDeliveryByCustomerId(customerId).Where(q => q.isDefaultDeliveryInfo == false).FirstOrDefault();
                    if (deliveryInfoOld != null)
                    {
                        deliveryInfoOld.isDefaultDeliveryInfo = true;
                        deliveryInfoApi.updateDeliveryInfo(deliveryInfoOld);
                    }

                }
                deliveryInfoApi.deleteDeliveryInfo(deliveryInfo);
            }
            else
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_FAIL
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                status = new
                {
                    success = true,
                    status = ConstantManager.STATUS_SUCCESS,
                    message = ConstantManager.MES_SUCCESS,
                },
                data = new { }
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// update deliveryInfo
        /// </summary>
        /// <param name="deliveryInfo">entity</param>
        /// <param name="accessToken">accessTOken</param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public JsonResult updateDeliveryInfo(DeliveryInfo deliveryInfo, string accessToken)
        {

            int customerId = Int32.Parse(getCustomerIdFromToken(accessToken));
            //call api 
            DeliveryInfoApi deliveryInfoApi = new DeliveryInfoApi();
            CustomerApi customerApi = new CustomerApi();
            int deliveryInfoId = 0;

            if (deliveryInfo.isDefaultDeliveryInfo.Value)
            {
                var listDelivery = deliveryInfoApi.getDeliveryByCustomerId(customerId);
                foreach (var item in listDelivery)
                {
                    if (item.isDefaultDeliveryInfo == true)
                    {
                        if (item.Id != deliveryInfo.Id)
                        {
                            deliveryInfoId = item.Id;
                        }
                    }
                }
            }
            else
            {
                var listDelivery = deliveryInfoApi.getDeliveryByCustomerId(customerId);
                //nếu địa chỉ chỉ có thì ko đc đổi lại là false 
                if (listDelivery.Count() == 1)
                {
                    //if (listDelivery.FirstOrDefault().isDefaultDeliveryInfo == true)
                    //{
                    // deliveryInfo.isDefaultDeliveryInfo = true;
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_UPDATE_DELIVERYINFO,
                        },
                        data = new { }
                    }, JsonRequestBehavior.AllowGet);
                    //   }
                }
                else
                {
                    bool checkIsDefault = true;//deliveryInfo existed 1 is true
                    int idDeliveryInfo = 0;
                    foreach (var item in listDelivery)
                    {
                        if (item.isDefaultDeliveryInfo.Value)
                        {
                            idDeliveryInfo = item.Id;
                        }
                        //if (item.Id == deliveryInfo.Id && item.isDefaultDeliveryInfo != true)
                        //{
                        //    if (item.isDefaultDeliveryInfo == true)
                        //    {
                        //        checkIsDefault = true;
                        //    }
                        //    else
                        //    {

                        //        checkIsDefault = false;
                        //    }
                        //}
                        //if (!checkIsDefault)
                        //{
                        //    return Json(new
                        //    {
                        //        status = new
                        //        {
                        //            success = false,
                        //            status = ConstantManager.STATUS_SUCCESS,
                        //            message = ConstantManager.MES_UPDATE_DELIVERYINFO,
                        //        },
                        //        data = new { }
                        //    }, JsonRequestBehavior.AllowGet);
                        //}
                        //        //if(item.Id == deliveryInfo.Id && deliveryInfo.isDefaultDeliveryInfo == false)
                        //        //{
                        //        //    return Json(new
                        //        //    {
                        //        //        status = new
                        //        //        {
                        //        //            success = false,
                        //        //            status = ConstantManager.STATUS_SUCCESS,
                        //        //            message = ConstantManager.MES_UPDATE_DELIVERYINFO,
                        //        //        },
                        //        //        data = new { }
                        //        //    }, JsonRequestBehavior.AllowGet);
                        //        //}
                        //    }
                        //}

                    }
                    if (idDeliveryInfo == deliveryInfo.Id)
                    {
                        return Json(new
                        {
                            status = new
                            {
                                success = false,
                                status = ConstantManager.STATUS_SUCCESS,
                                message = ConstantManager.MES_UPDATE_DELIVERYINFO,
                            },
                            data = new { }
                        }, JsonRequestBehavior.AllowGet);

                    }
                }
            }
            //if set default other deliveryInfo
            if (deliveryInfoId > 0)
            {
                var deliveryInfoOld = deliveryInfoApi.getDeliveryById(deliveryInfoId);
                deliveryInfoOld.isDefaultDeliveryInfo = false;
                deliveryInfoApi.updateDeliveryInfo(deliveryInfoOld);
            }
            //fill data 
            var entity = deliveryInfoApi.getDeliveryById(deliveryInfo.Id);
            entity.CustomerId = customerId;
            entity.CustomerName = deliveryInfo.CustomerName;
            entity.Address = deliveryInfo.Address;
            entity.Active = deliveryInfo.Active;
            entity.Phone = deliveryInfo.Phone;
            entity.Id = deliveryInfo.Id;
            entity.Type = deliveryInfo.Type;
            entity.isDefaultDeliveryInfo = deliveryInfo.isDefaultDeliveryInfo;
            try
            {
                //nếu địa chỉ này là mặc định
                if (deliveryInfo.isDefaultDeliveryInfo.Value)
                {
                    //lấy deliveryInfoDefault của customer để gán giá trị lại là id(deliveryInfo)
                    var customer = customerApi.GetCustomerEntityById(customerId);
                    customer.deliveryInfoDefault = deliveryInfo.Id;
                    customerApi.UpdateEntityCustomer(customer);
                }

                deliveryInfoApi.updateDeliveryInfo(entity);
                //get deliveryInfo đã update
                var entityNew = deliveryInfoApi.getDeliveryById(deliveryInfo.Id);
                entityNew.Customer = null;
                DeliveryInfo result = new DeliveryInfo();
                //fill data delveryInfo
                result.Id = entityNew.Id;
                result.CustomerName = entityNew.CustomerName;
                result.Address = entityNew.Address;
                result.Active = entityNew.Active;
                result.isDefaultDeliveryInfo = entityNew.isDefaultDeliveryInfo;
                result.Phone = entityNew.Phone;
                result.Type = entityNew.Type;

                return Json(new
                {
                    status = new
                    {
                        success = true,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_SUCCESS,
                    },
                    data = new
                    {
                        data = new
                        {
                            id = result.Id,
                            customer_name = result.CustomerName,
                            address = result.Address,
                            active = result.Active,
                            is_default_delivery_info = result.isDefaultDeliveryInfo,
                            phone = result.Phone,
                            type = result.Type
                        }
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_FAIL,
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [System.Web.Mvc.HttpPost]
        public JsonResult getMemberShipcardType(string accessToken, int brandId)
        {
            #region call api to proceess
            var msctApi = new MembershipCardTypeApi();
            MembershipCardApi mcApi = new MembershipCardApi();
            VoucherApi voucherApi = new VoucherApi();
            #endregion
            //get list mobile membershipCard type + promotion
            var listMBSCType = msctApi.GetAllMembershipCardTypeByBrand(brandId).Where(q => q.IsMobile == true).OrderBy(q => q.TypeLevel).ToList();
            List<MembershipAppTypeModel> listMembershipTypeModel = new List<MembershipAppTypeModel>();
            foreach (var item in listMBSCType)
            {

                item.promotions = getAvailablePromotionByMSCTypeAndBrandId(item, brandId);
                MembershipAppTypeModel membershipTypeModel = new MembershipAppTypeModel();
                PromotionAppModel promotionModel = new PromotionAppModel();
                List<PromotionAppModel> listPromotionModel = new List<PromotionAppModel>();
                foreach (var promotion in item.promotions)
                {
                    // promotionModel.active = promotion.Active;
                    promotionModel.apply_from_time = promotion.ApplyFromTime;
                    promotionModel.apply_to_time = promotion.ApplyToTime;
                    promotionModel.apply_level = promotion.ApplyLevel;
                    //   promotionModel.brand_id = promotion.BrandId;
                    promotionModel.short_description = promotion.ShortDescription;
                    promotionModel.description = promotion.Description;
                    promotionModel.from_date = promotion.FromDate.ToString(ConstantManager.FORMART_DATETIME);
                    promotionModel.to_date = promotion.ToDate.ToString(ConstantManager.FORMART_DATETIME);
                    //   promotionModel.promotion_type = promotion.PromotionType;
                    promotionModel.promotion_name = promotion.PromotionCode;
                    promotionModel.promotion_id = promotion.PromotionID;
                    //  promotionModel.gift_type = promotion.GiftType;
                    //   promotionModel.is_for_member = promotion.IsForMember;
                    //   promotionModel.promotion_class_name = promotion.PromotionClassName;
                    promotionModel.image_url = promotion.ImageUrl;
                    listPromotionModel.Add(promotionModel);
                }
                membershipTypeModel.id = item.Id;
                membershipTypeModel.type_name = item.TypeName;
                membershipTypeModel.type_level = item.TypeLevel;
                membershipTypeModel.point = item.TypePoint;
                membershipTypeModel.promotions = listPromotionModel;
                listMembershipTypeModel.Add(membershipTypeModel);
            }
            return Json(new
            {
                status = new
                {
                    success = true,
                    status = ConstantManager.STATUS_SUCCESS,
                    message = ConstantManager.MES_SUCCESS
                },
                data = new
                {
                    data = new
                    {
                        membershipcard_type = listMembershipTypeModel
                    }
                }
            });
        }
        public PromotionRule checkPromotionRule(OrderViewModel order, string code, int productId, int quantity, OrderDetailViewModel od, int checkCountProduct, int brandId)
        {
            #region call api to proccess 
            VoucherApi voucherApi = new VoucherApi();
            PromotionApi promotionApi = new PromotionApi();
            PromotionDetailApi promotionDetailApi = new PromotionDetailApi();
            ProductApi productApi = new ProductApi();
            StoreApi storeApi = new StoreApi();
            #endregion
            PromotionRule result = new PromotionRule();
            //var voucher = voucherApi
            //TODO after have voucher
            #region Voucher
            var voucher = voucherApi.GetVoucherIsNotUsedAndCode(code);
            if (voucher == null)
            {
                result.rule = 0;
                result.discountAmount = 0;
                return result;
            }
            #endregion
            //get date to get promotion is experied ??
            #region Promotion 
            var promotion = promotionApi.GetPromotionByDateAndId(voucher.PromotionID);
            DateTime now = DateTime.Now;

            if (promotion == null)
            {
                result.rule = 0;
                result.discountAmount = 0;
                return result;
            }
            else if (!(promotion.ApplyFromTime <= now.Hour && now.Hour <= promotion.ApplyToTime))
            {
                result.rule = 0;
                result.discountAmount = 0;
                return result;
            }

            #endregion

            #region check store promotion mapping
            //get mobile storeid
            var mobileStore = storeApi.GetStoresByBrandIdAndType(brandId, (int)StoreTypeEnum.MobileApp).FirstOrDefault();
            if (mobileStore == null || !promotion.PromotionStoreMappings.Where(p => p.Active == true).Select(q => q.StoreId).Contains(mobileStore.ID))
            {
                result.rule = 0;
                result.discountAmount = 0;
                return result;
            }
            #endregion

            #region PromtionDetail rule 1 min , max order???
            // loop to get total amount, final amount to get check in promotiondetail
            double finalAmount = 0;

            foreach (var item in order.OrderDetails)
            {
                finalAmount += (productApi.GetProductById(item.ProductID).Price * item.Quantity);
            }

            var promotionDetail = promotionDetailApi.GetDetailByPromotionDetailCode(voucher.PromotionDetail.PromotionDetailCode);
            if (promotionDetail == null)
            {
                result.rule = 0;
                result.discountAmount = 0;
                return result;
            }
            //check promotion detail is have min, max order != null ???
            if (promotionDetail.MinOrderAmount != null || promotionDetail.MaxOrderAmount != null)
            {
                if (finalAmount < promotionDetail.MinOrderAmount)
                {
                    result.rule = 0;
                    result.discountAmount = 0;
                    return result;
                }
                else
                {
                    try
                    {
                        double discountAmount = 0;//amount return  
                        if (promotionDetail.DiscountRate != null && promotionDetail.DiscountRate > 0)
                        {
                            //nhân với phần trăm giảm giá và trả về số tiền lun
                            discountAmount = (productApi.GetProductById(productId).Price * promotionDetail.DiscountRate.Value) / 100;
                        }
                        else if (promotionDetail.DiscountAmount != null && promotionDetail.DiscountAmount > 0)
                        {
                            //nếu giảm giá theo tiền mặt
                            discountAmount = Convert.ToDouble(promotionDetail.DiscountAmount.Value);
                        }
                        result.rule = ConstantManager.PROMOTION_RULE_1;
                        result.discountAmount = discountAmount;
                        return result;
                    }
                    catch
                    {
                        result.rule = 0;
                        result.discountAmount = 0;
                        return result;
                    }

                }
            }
            #endregion

            #region rule 2 buy min, max quantity of each product 
            else if (promotionDetail.BuyProductCode != null)
            {
                double discountAmount = 0;
                //check product code is in order ????
                bool checkProductCode = false;
                //list product discount 
                List<ProductDiscount> listProductDiscount = new List<ProductDiscount>();
                // var pmDetail = promotionDetailApi.GetDetailByCode(promotion.PromotionCode);
                int pDetailId = voucher.PromotionDetailID == null ? 0 : voucher.PromotionDetailID.Value;
                var pmDetail = promotionDetailApi.GetDetailById(pDetailId);
                decimal tmpDiscountAmount = 0;
                double tmpDiscountRate = 0;
                string mesMinBuyProduct = "";
                bool checkCount = true;//check quanitty min order buy
                var tmpProductOrder = productApi.GetProductEntityById(productId);

                bool checkCountProductQuantity = true;//false => đơn hàng gửi lên giống nhau nhưng ko đủ quantity min order,
                                                      //true => đơn hàng gửi lên giống nhau nhưng đủ quantity
                foreach (var item in order.OrderDetails)
                {
                    if (tmpProductOrder.Code == pmDetail.BuyProductCode)
                    {
                        checkCountProduct += item.Quantity;//cộng đồn quantity trong order
                        if (checkCountProduct < pmDetail.MinBuyQuantity)
                        {
                            checkCountProductQuantity = false;//false => đơn hàng gửi lên giống nhau nhưng ko đủ quantity min order,
                        }
                        else
                        {
                            checkCountProductQuantity = true;
                        }
                    }
                }
                if (pmDetail != null)
                {
                    if (tmpProductOrder.Code == pmDetail.BuyProductCode)
                    {
                        quantity += od.Quantity;
                        tmpDiscountAmount = pmDetail.DiscountAmount == null ? 0 : pmDetail.DiscountAmount.Value * od.Quantity;
                        tmpDiscountRate = pmDetail.DiscountRate == null ? 0 : pmDetail.DiscountRate.Value;
                        checkProductCode = true;
                        if (quantity < pmDetail.MinBuyQuantity)
                        {
                            mesMinBuyProduct = pmDetail.MinBuyQuantity + " " + tmpProductOrder.ProductName;
                            checkCount = false;
                        }
                    }
                    else
                    {

                        // checkCount = true;
                    }
                }

                try
                {

                    //if true => get list product discount
                    if (checkProductCode)
                    {
                        //check amount discount and rate, return value
                        if (tmpDiscountAmount > 0)
                        {
                            discountAmount = System.Convert.ToDouble(tmpDiscountAmount);
                        }
                        else if (tmpDiscountRate > 0)
                        {
                            discountAmount = (tmpProductOrder.Price * tmpDiscountRate * od.Quantity) / 100;
                        }

                    }
                    checkProductCode = false;
                    //return list product with discount amount, rate
                }
                catch
                {
                    result.rule = 0;
                    result.discountAmount = 0;
                    result.quantity = 0;
                    result.countProduct = false;
                    return result;
                }


                if (!checkCount && !checkCountProductQuantity)
                {
                    result.rule = 0;
                    result.discountAmount = 0;
                    result.quantity = quantity;
                    result.countProduct = checkCountProductQuantity;
                    return result;

                }
                result.rule = ConstantManager.PROMOTION_RULE_2;
                result.discountAmount = discountAmount;
                result.quantity = quantity;
                result.countProduct = checkCountProductQuantity;
                return result;


            }
            result.rule = 0;
            result.discountAmount = 0;
            result.quantity = 0;
            return result;
            #endregion
        }
        /// <summary>
        /// to set order db
        /// </summary>
        /// <param name="order">model order</param>
        /// <param name="brandId">1</param>
        /// <param name="paymentType">cash, momo</param>
        /// <param name="accessToken">customer accessToken</param>
        /// <param name="voucherCode">code of voucher</param>
        /// <param name="deliveryInfoId">id of delivery info</param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public JsonResult setOrder([FromBody]OrderViewModel order, int brandId,
            int paymentType, string accessToken, string voucherCode, int deliveryInfoId)
        {
            //Block
            if (paymentType == (int)PaymentTypeEnum.MemberPayment)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_BLOCK_ORDER_BY_MEMBER_PAYMENT
                    },
                    data = new { }
                });
            }
            try
            {
                order.CustomerID = Int32.Parse(getCustomerIdFromToken(accessToken));
                //order.CustomerID = 113;
                DateTime time = HmsService.Models.Utils.GetCurrentDateTime();
                StoreApi storeApi = new StoreApi();
                ProductApi productApi = new ProductApi();
                CustomerApi customerApi = new CustomerApi();
                AccountApi accountApi = new AccountApi();
                DeliveryInfoApi deliveryInfoApi = new DeliveryInfoApi();
                OrderDetailPromotionMappingApi odpmApi = new OrderDetailPromotionMappingApi();
                VoucherApi voucherApi = new VoucherApi();
                PromotionDetailApi promotionDtApi = new PromotionDetailApi();
                MembershipCardApi membershipcardApi = new MembershipCardApi();
                if (order.CustomerID == 0 || order.CustomerID == null)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_CHECK_CUSTOMERID_FAIL
                        },
                        data = new { }
                    });
                }
                var customer = customerApi.GetCustomerByID((int)order.CustomerID);
                if (customer == null)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_CHECK_CUSTOMERID_FAIL
                        },
                        data = new { }
                    });
                }
                #region OrderDetail

                double orderDetailTotalAmount = 0;
                double orderDetailFinalAmount = 0;
                //double discountOrderDetail = 0;
                bool checkQuantity = false;
                //biến giảm giá trên mỗi sản phẩm
                double discountEachProduct = 0;
                //biến giảm giá trên toàn hóa đơn
                double discount = 0;
                try
                {
                    bool checkDeliveryFee = false;
                    double finalAmount = 0;
                    double totalAmount = 0;

                    //foreach (var item in order.OrderDetails)
                    //{
                    //    totalAmount = productApi.GetProductById(item.ProductID).Price * item.Quantity;
                    //    finalAmount += totalAmount - item.Discount;
                    //    if (finalAmount >= ConstantManager.DELIVERY_FREE)
                    //    {
                    //        checkDeliveryFee = true;
                    //    }
                    //}

                    //if (!checkDeliveryFee)
                    //{
                    //    var tmpOrderDetail = order.OrderDetails.ToList();
                    //    var productDelivery = productApi.GetProductDeliveryFee();
                    //    OrderDetailViewModel deliveryOrderDt = new OrderDetailViewModel();
                    //    deliveryOrderDt.ProductID = productDelivery.ProductID;
                    //    deliveryOrderDt.Quantity = 1;
                    //    deliveryOrderDt.ProductOrderType = (int)ProductOrderType.Single;
                    //    tmpOrderDetail.Add(deliveryOrderDt);
                    //    order.OrderDetails = tmpOrderDetail;
                    //}
                    //add order detail have product is a delivery fee
                    //giảm giá trên từng sản phẩm, hóa đơn tùy theo rule ở hàm checkPromotionRUle
                    int quantity = 0;//check quantity trong order gửi 1 đơn hàng và có quantity trong đó
                    int checkCountProduct = 0;//dùng check trường hợp gửi 2 đơn hàng giống nhau nhưng mỗi cái có 1 quantity
                    foreach (var item in order.OrderDetails)
                    {
                        item.TotalAmount = productApi.GetProductById(item.ProductID).Price * item.Quantity;

                        orderDetailTotalAmount += item.TotalAmount;
                        item.UnitPrice = productApi.GetProductById(item.ProductID).Price;
                        //lấy giảm giá theo rule 
                        PromotionRule rule = new PromotionRule();
                        rule = checkPromotionRule(order, voucherCode, item.ProductID, quantity, item, checkCountProduct, brandId);
                        //check rule 1, 2 dc định nghĩ ở ConstantManager
                        if (rule.rule == ConstantManager.PROMOTION_RULE_2 || rule.countProduct)
                        {
                            item.Discount = rule.discountAmount;
                        }
                        else if (rule.rule == ConstantManager.PROMOTION_RULE_1)
                        {
                            discount = rule.discountAmount;
                        }

                        quantity = rule.quantity;
                        item.FinalAmount = item.TotalAmount - item.Discount;
                        orderDetailFinalAmount += item.FinalAmount;
                        discountEachProduct += item.Discount;
                        //discountOrderDetail += item.Discount;
                        item.OrderDate = time;
                        if (item.Quantity <= 0)
                        {
                            checkQuantity = true;
                        }
                        var product = productApi.GetProductEntityById(item.ProductID);
                        if (product == null)
                        {
                            checkQuantity = true;
                        }
                    }
                    foreach (var item in order.OrderDetails)
                    {
                        totalAmount = productApi.GetProductById(item.ProductID).Price * item.Quantity;
                        finalAmount += totalAmount - item.Discount;
                        if (finalAmount >= ConstantManager.DELIVERY_FREE)
                        {
                            checkDeliveryFee = true;
                        }
                    }

                    if (!checkDeliveryFee)
                    {
                        var tmpOrderDetail = order.OrderDetails.ToList();
                        var productDelivery = productApi.GetProductDeliveryFee();
                        OrderDetailViewModel deliveryOrderDt = new OrderDetailViewModel();
                        deliveryOrderDt.ProductID = productDelivery.ProductID;
                        deliveryOrderDt.Quantity = 1;
                        deliveryOrderDt.TotalAmount = productDelivery.Price;
                        deliveryOrderDt.FinalAmount = productDelivery.Price;
                        deliveryOrderDt.UnitPrice = productDelivery.Price;
                        orderDetailTotalAmount += productDelivery.Price;

                        deliveryOrderDt.ProductOrderType = (int)ProductOrderType.Single;
                        deliveryOrderDt.OrderDate = time;
                        tmpOrderDetail.Add(deliveryOrderDt);
                        order.OrderDetails = tmpOrderDetail;
                    }
                    if (checkQuantity == true)
                    {
                        return Json(new
                        {
                            status = new
                            {
                                success = false,
                                status = ConstantManager.STATUS_SUCCESS,
                                message = ConstantManager.MES_CREATE_ORDER_FAIL
                            },
                            data = new { }
                        });
                    }
                }
                catch (NullReferenceException e)
                {

                }
                #endregion
                #region Order

                //order.Payments = new List<PaymentViewModel>();
                order.CheckInDate = time;
                //order.CheckInPerson = User.Identity.Name;
                //discount => t?ng ti?n - t?ng ti?n * chi?t kh?u
                //Calculator VAT amount
                //var vatAmount = (tempFinalAmount * 10 / 100); //VAT 10%

                var vatAmount = 0; //VAT 10%
                #region edit promotion
                //Promotion Voucher Discount Calculate , 
                //Giảm giá trên toàn hóa đơn 
                //  var promotiondetail = checkPromotion(voucherCode, accessToken, brandId, order);

                //if (promotiondetail != null)
                //{
                //    if (promotiondetail.DiscountAmount != null && promotiondetail.DiscountRate == 0)
                //    {
                //        discount = (double)promotiondetail.DiscountAmount;
                //    }
                //    else if (promotiondetail.DiscountAmount == 0 && promotiondetail.DiscountRate != null)
                //    {
                //        discount = discount * (double)promotiondetail.DiscountRate;
                //    }
                //}


                #endregion

                //if (orderDetailTotalAmount < ConstantManager.DISCOUNT_VOUCHER)
                //{
                //    discount = 0;
                //    return Json(new
                //    {
                //        status = new
                //        {
                //            success = false,
                //            status = ConstantManager.STATUS_SUCCESS,
                //            message = ConstantManager.MES_DISCOUNT_VOUCHER_FAIL
                //        },
                //        data = new { }
                //    });
                //}
                order.TotalAmount = orderDetailTotalAmount;
                order.Discount = discount;
                order.DiscountOrderDetail = discountEachProduct;
                order.FinalAmount = orderDetailTotalAmount - vatAmount - discount - discountEachProduct;//l?y order detail sum l?i => ra du?c order thi?t c?a passio
                                                                                                        //order.DiscountOrderDetail = discountOrderDetail;
                                                                                                        //gán giản giá trên từng sản phẩm cho order

                //Get delivery info from database
                var deliveryInfo = deliveryInfoApi.getDeliveryById(deliveryInfoId);
                if (deliveryInfo != null)
                {
                    order.Receiver = deliveryInfo.CustomerName;
                    order.DeliveryAddress = deliveryInfo.Address;
                    order.DeliveryPhone = deliveryInfo.Phone;
                }
                else
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_CREATE_ORDER_FAIL
                        },
                        data = new { }
                    });
                }

                order.DeliveryStatus = (int)DeliveryStatus.New;
                order.OrderType = (int)OrderTypeEnum.Delivery;
                order.OrderStatus = (int)OrderStatusEnum.New;
                order.InvoiceID = ConstantManager.PREFIX_MOBILE + HmsService.Models.Utils.GetCurrentDateTime().Ticks.ToString() + "-" + brandId;// truy?n mã hóa don
                order.SourceType = (int)SourceTypeEnum.Mobile;
                order.isSync = false;

                //get mobile storeid
                var mobileStore = storeApi.GetStoresByBrandIdAndType(brandId, (int)StoreTypeEnum.MobileApp).FirstOrDefault();
                if (mobileStore != null)
                {
                    order.StoreID = mobileStore.ID;
                }

                order.GroupPaymentStatus = 0; //Tam thoi chua xài
                order.PaymentStatus = (int)OrderPaymentStatusEnum.Finish;

                if (customer != null)
                {
                    customer.BrandId = brandId;
                }
                var orderApi = new OrderApi();

                OrderCustomEntityViewModel orderEntity = new OrderCustomEntityViewModel()
                {
                    Order = order,
                    OrderDetails = order.OrderDetails,
                    //Customer = order.Customer,
                };
                #endregion
                //dua di lieu vào payment

                PaymentViewModel payment = new PaymentViewModel();

                double tmpAmountCard = order.FinalAmount;
                List<PaymentViewModel> paymentList = new List<PaymentViewModel>();
                #region PAYMENT
                ////l?y ti?n m?t c?a external truy?n vào 
                switch (paymentType)
                {
                    //thanh toán bằng tiền mặt
                    case (int)PaymentTypeEnum.Cash:
                        payment.Amount = tmpAmountCard;
                        payment.Type = (int)PaymentTypeEnum.Cash;
                        payment.PayTime = time;
                        payment.ToRentID = order.RentID;
                        paymentList.Add(payment);
                        break;
                    //thanh toán bằng thẻ
                    case (int)PaymentTypeEnum.MemberPayment:
                        payment = new PaymentViewModel();
                        payment.Amount = tmpAmountCard;
                        payment.Type = (int)PaymentTypeEnum.MemberPayment;
                        payment.PayTime = time;
                        payment.ToRentID = order.RentID;
                        paymentList.Add(payment);

                        //Update Member Account
                        //get mobile membership card by customerId
                        var card = membershipcardApi.GetMembershipCardActiveByCustomerId(customer.CustomerID)
                        .Where(q => q.MembershipTypeId != null && listMobileTypeId
                        .Contains((int)q.MembershipTypeId)).FirstOrDefault();

                        //get account follow by customerId
                        var tmpAccount = card.Accounts.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault();
                        //If mobile card has bind to a physical one, use physical card credit account instead
                        if (card.PhysicalCardCode != null)
                        {
                            var physicalCard = membershipcardApi.GetMembershipCardByCode(card.PhysicalCardCode);
                            tmpAccount = physicalCard.Accounts.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault();
                        }

                        //get tmpAccount to update

                        tmpAccount.Balance = tmpAccount.Balance - (decimal)tmpAmountCard;

                        AccountViewModel updateAccount = tmpAccount.ToViewModel<Account, AccountViewModel>();//get Frist value => 1 customer => 1 value => First()
                        var checkUpdate = 0;
                        //Check balance > 0
                        if (updateAccount.Balance < 0)
                        {
                            return Json(new
                            {
                                status = new
                                {
                                    success = false,
                                    status = ConstantManager.STATUS_SUCCESS,
                                    message = ConstantManager.MES_UPDATE_ACCOUNT_FAIL
                                },
                                data = new { }
                            });
                        }
                        checkUpdate = accountApi.UpdateAccount(updateAccount);//update
                        if (!(checkUpdate > 0))//update false checkUpdapte < 0
                        {
                            return Json(new
                            {
                                status = new
                                {
                                    success = false,
                                    status = ConstantManager.STATUS_SUCCESS,
                                    message = ConstantManager.MES_UPDATE_FAIL
                                },
                                data = new { }
                            });
                        }
                        break;
                    case (int)PaymentTypeEnum.MoMo:
                        order.OrderStatus = (int)OrderStatusEnum.Unpaid;
                        break;
                    default:
                        break;
                }
                //nếu payment type là momo thì ko tạo payment ngc lại thì tạo, chỉ tạo khi check payment với momo dc
                if (paymentType == (int)PaymentTypeEnum.MoMo)
                {

                }
                else
                {
                    //d? payment m?i vào
                    order.Payments = paymentList;
                }

                #endregion
                var orderTest = order.ToEntity();
                var rs = orderApi.BaseService.CreateOrderTransaction(orderTest);


                if (rs == false)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_CREATE_ORDER_FAIL
                        },
                        data = new { }
                    });
                }
                //Update voucher after used
                if (!string.IsNullOrEmpty(voucherCode))
                {
                    var voucher = voucherApi.GetVoucherIsNotUsedAndCode(voucherCode);
                    if (voucher != null)
                    {
                        if (voucher.MembershipCardId == null)
                        {
                            voucher.UsedQuantity++;
                            if (voucher.UsedQuantity >= voucher.Quantity)
                            {
                                voucher.isUsed = true;
                            }
                            voucherApi.Update(voucher);
                        }
                        else
                        {
                            voucher.isUsed = true;
                            voucherApi.Update(voucher);
                        }
                    }
                }
                return Json(new
                {
                    status = new
                    {
                        success = true,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_CREATE_ORDER_SUCCESS,
                    },
                    data = new
                    {
                        data = new
                        {
                            invoice_id = order.InvoiceID,
                            order_status = order.OrderStatus,
                        }
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    status = new
                    {
                        success = false,
                        status = ConstantManager.STATUS_SUCCESS,
                        message = ConstantManager.MES_CREATE_ORDER_FAIL
                    },
                    data = new { }
                }, JsonRequestBehavior.AllowGet);

            }
        }
        public async System.Threading.Tasks.Task<JsonResult> checkPaymentMomo(int orderId, string partnerRefId, string momoTransId, string description, string hash, int version)
        {
            //call api to proccess

            //var partnerMappingApi = new PartnerMappingApi();
            //  var listPartnerMapping = partnerMappingApi.GetActivePartnerMappingByBrandId(brandID);
            //  var partnerMapping = listPartnerMapping.FirstOrDefault(pm => pm.PartnerId == (int)PartnerEnum.Momo);
            //  var config = JsonConvert.DeserializeObject<MomoConfig>(partnerMapping.Config);

            //    var store = new StoreApi().GetActiveStoreById(storeID);

            //var confirmPaymentModel = new 
            //{
            //    partnerCode = config.PaymentPartnerCode,
            //    partnerRefId = orderVM.OrderCode,
            //    amount = (int)orderVM.FinalAmount,
            //    paymentCode = orderVM.PaymentCode,
            //    storeId = storeID.ToString(),
            //    storeName = store.Name
            //};

            // var encrypted = MomoRSAHelper.Encrypt(JsonConvert.SerializeObject(confirmPaymentModel), config.RSAPublicKey);

            string partnerCode = ConstantManager.PARTNERCODEMOMO;
            var request = new
            {
                partnerCode = partnerCode,
                partnerRefId = partnerRefId,
                description = "Thanh toán với Momo từ Order " + orderId,
                hash = hash,
                version = version
            };

            #region Send request for MoMo
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // declare apis
                    var orderApi = new OrderApi();
                    var paymentApi = new PaymentApi();
                    string url = "https://test-payment.momo.vn/pay/query-status";
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.PostAsJsonAsync("", request);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseObj = await response.Content.ReadAsAsync<ConfirmPaymentResponse>();
                        if (responseObj.status == (int)MomoStatusEnum.Success)
                        {
                            var transMomo = responseObj.data;
                            var orders = orderApi.GetOrderById(orderId);
                            orders.OrderStatus = (int)OrderStatusEnum.Finish;
                            orders.OrderType = (int)OrderTypeEnum.MobileDelivery;


                            // orders.CopyToEntity(orderEntity);
                            orderApi.EditOrder(orders);

                            //mới xong 9h23
                            Payment payment = new Payment();
                            payment.ToRentID = orders.RentID;
                            payment.Amount = transMomo.amount;
                            payment.Notes = transMomo.description;
                            payment.PayTime = DateTime.Now;
                            payment.Status = (int)PaymentStatusEnum.New;
                            payment.Type = (int)PaymentTypeEnum.MoMo;
                            payment.Order = orders;
                            int id = paymentApi.CreatePaymentReturnId(payment);
                            if (id > 0)
                            {
                                return Json(new
                                {
                                    status = new
                                    {
                                        success = true,
                                        status = ConstantManager.STATUS_SUCCESS,
                                        message = ConstantManager.MES_CREATE_ORDER_SUCCESS
                                    },
                                    data = new
                                    {
                                        payment = 1
                                    }
                                });
                            }
                            else
                            {
                                return Json(new
                                {
                                    status = new
                                    {
                                        success = false,
                                        status = ConstantManager.STATUS_SUCCESS,
                                        message = ConstantManager.MES_CREATE_PAYMENT_MOMO_FAIL
                                    },
                                    data = new { }
                                });

                            }
                        }

                    }

                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_CREATE_PAYMENT_MOMO_FAIL
                        },
                        data = new { }
                    });
                }
                catch (Exception e)
                {
                    return Json(new
                    {
                        status = new
                        {
                            success = false,
                            status = ConstantManager.STATUS_SUCCESS,
                            message = ConstantManager.MES_CHECK_CUSTOMERDEVICE_FAIL
                        },
                        data = new { }
                    });
                }
            }
            #endregion
        }

    }
    public class ConfirmPaymentResponse
    {
        public int status { get; set; }
        public string message { get; set; }
        public ConfirmPaymentResponseTran data { get; set; }
    }
    public class ConfirmPaymentResponseTran
    {
        public dynamic amount { get; set; }
        public string description { get; set; }
        public string phoneNumber { get; set; }
        public string transid { get; set; }
    }
    public class MomoConfig
    {
        [JsonProperty("rsaPubKey")]
        public string RSAPublicKey { get; set; }
        [JsonProperty("pgpPubKey")]
        public string PublicKey { get; set; }
        [JsonProperty("pgpPrivateKey")]
        public string PrivateKey { get; set; }
        [JsonProperty("pgpPassPhrase")]
        public string PassPhrase { get; set; }
        [JsonProperty("partnerCode")]
        public string PartnerCode { get; set; }
        [JsonProperty("paymentPartnerCode")]
        public string PaymentPartnerCode { get; set; }
        [JsonProperty("momo_checktrans_api")]
        public string MomoCheckTransApiUrl { get; set; }
        [JsonProperty("momo_confirmpayment_api")]
        public string MomoConfirmPaymentApiUrl { get; set; }
        [JsonProperty("version")]
        public int Version { get; set; }
        [JsonProperty("tran_type")]
        public int TranType { get; set; }
        [JsonProperty("show_popup")]
        public int ShowPopup { get; set; }
    }
    public class PromotionAppModel
    {
        public string from_date { get; set; }
        public string to_date { get; set; }
        public int promotion_id { get; set; }

        public string promotion_name { get; set; }
        //  public string promotion_class_name { get; set; }
        public string short_description { get; set; }
        public string description { get; set; }
        public int apply_level { get; set; }
        //  public int gift_type { get; set; }
        // public bool is_for_member { get; set; }
        //  public bool active { get; set; }
        public int? apply_from_time { get; set; }
        public int? apply_to_time { get; set; }
        //public int? brand_id { get; set; }
        public string image_url { get; set; }
        // public int promotion_type { get; set; }
    }
    public class OrderDetailAppModel
    {
        public int order_detail_id { get; set; }
        public string size { get; set; }
        public string product_name { get; set; }
        public int quantity { get; set; }
        public string product_image { get; set; }
        public double unit_price { get; set; }
        public int cat_id { get; set; }
        public int? parent_id { get; set; }
    }
    public class ProductAppModel
    {
        public int product_id { get; set; }
        public string product_name { get; set; }
        public string product_name_eng { get; set; }
        public double price { get; set; }
        public string pic_url { get; set; }
        public int cat_id { get; set; }
        public bool has_extra { get; set; }
        public string size { get; set; }
        public string att2 { get; set; }
        public string description { get; set; }
        public Nullable<int> general_product_id { get; set; }
    }
    public class MembershipAppTypeModel
    {
        public int id { get; set; }
        public string type_name { get; set; }
        public int? type_level { get; set; }
        public int? point { get; set; }
        public List<PromotionAppModel> promotions { get; set; }
    }
    public class PromotionRule
    {
        public int rule { get; set; }
        public double discountAmount { get; set; }
        public int quantity { get; set; }
        public bool countProduct { get; set; }
    }
    public class Areas
    {
        public string name { get; set; }
        //  public StoreDistrictModel stores { get; set; }
        public List<StoreModel> stores { get; set; }
    }
    public class ProductDiscount
    {
        public int product_id { get; set; }
        public double discount_rate { get; set; }
        public decimal discount_amount { get; set; }
    }
    public class StoreModel
    {
        public int id { get; set; }
        public string address { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string closed_time { get; set; }
        public string open_time { get; set; }
    }
    //gom store theo quận 
    public class StoreDistrictModel
    {
        // public Dictionary<string, List<StoreModel>> areas { get; set; }
        public List<Areas> areas { get; set; }
    }

    public class VoucherAppModel
    {
        public int id { get; set; }
        public string code { get; set; }
        public bool? is_used { get; set; }
        public bool? is_expired { get; set; }
        public bool? is_infinity { get; set; }
        public PromotionAppModel promotion { get; set; }
    }
    public class ProductCategoryAppModel
    {
        public int id { get; set; }
        public int cat_id { get; set; }
        public string cat_name { get; set; }
    }
}
