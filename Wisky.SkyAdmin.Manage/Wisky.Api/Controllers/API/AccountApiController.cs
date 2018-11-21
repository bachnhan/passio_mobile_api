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

namespace Wisky.Api.Controllers.API
{
    public class AccountApiController : ApiController
    {
        private readonly string _accessToken = System.Configuration.ConfigurationManager.AppSettings["config.order.token"];
        private readonly string _enableToken = System.Configuration.ConfigurationManager.AppSettings["config.order.EnableToken"];

        [HttpGet]
        [Route("api/account/GetAccountList/{token}/{terminalId}")]
        //public List<AccountModel> GetAccountList(string token, int terminalId)
        //public IEnumerable<AccountApiViewModel> GetAccountList(string token, int terminalId)
        public HttpResponseMessage GetAccountList(string token, int terminalId)
        {
            // CheckToken(token);
            var aspNetUserApi = new AspNetUserApi();
            try
            {
                //aspNetUserApi = null;
                var listUser = aspNetUserApi.GetAccountApiList(terminalId, token);
                
                //return Json(listUser, JsonRequestBehavior.AllowGet);
                //return listUser;

                return new HttpResponseMessage()
                {
                    Content = new JsonContent(listUser)
                };
            }
            catch (Exception e)
            {
              
                return new HttpResponseMessage()
                {
                    Content = new JsonContent(e)
                };
            }

        }
        /// <summary>
        /// Test data:
        /// Method: POST
        /// URL: http://localhost:1366/AccountAPI/CheckLogin
        /// Request body params:
        /// {"model":{
        ///   "Username":"quanly",
        ///   "Password": "zaQ@123"
        /// }}
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/AccountAPI/CheckLogin/{username}/{password}")]
        public HttpResponseMessage CheckLogin(LoginAccountApiViewModel model)
        {
            //kiem tra xem co thieu parameter nao ko
            if (model == null || model.Username == null || model.Password == null)
            {
                //return Json(new
                //{
                //    Message = ConstantManager.MES_MISSING_PARAM,
                //    Status = ConstantManager.STT_MISSING_PARAM
                //}, JsonRequestBehavior.AllowGet);

                return new HttpResponseMessage()
                {
                    Content = new JsonContent(new
                    {
                        Message = ConstantManager.MES_MISSING_PARAM,
                        Status = ConstantManager.STT_MISSING_PARAM
                    })
                };
            }


            //chi 2 role nay duoc phep nhan token
            string managerRole = ConstantManager.ROLE_MANAGER,
                    adminRole = ConstantManager.ROLE_ADMIN;

            var aspNetUserApi = new AspNetUserApi();
            var storeApi = new StoreApi();
            var tokenApi = new TokenUserApi();
            var storeUserApi = new StoreUserApi();


            //var hashedPassword = MP5Hasher.GetMD5(model.Password);
            var user = aspNetUserApi.CheckLoginApi(model.Username, model.Password);
            //đăng nhập thành công, check tiep role roi moi tra ve token
            if (user != null)
            {
                //get user roles
                bool isAllowed = false;
                var roles = aspNetUserApi.GetUserRoles(user.UserName);
                //check roles
                if (roles.Contains(managerRole) || roles.Contains(adminRole))
                {
                    isAllowed = true;
                }

                // co 1 trong 2 role admin hay manager
                if (isAllowed)
                {
                    Guid id = Guid.NewGuid();
                    //cap nhat bang UserToken
                    tokenApi.UpdateUserToken(model.Username, id.ToString());
                    var listStore = storeApi.GetStoresByUserApi(user.UserName).ToList();
                    try
                    {
                        //return Json(new
                        //{
                        //    message = ConstantManager.MES_LOGIN_SUCCESS,
                        //    status = ConstantManager.STT_SUCCESS,
                        //    data = new
                        //    {
                        //        stores = listStore.Select(a => new
                        //        {
                        //            storeName = a.Name,
                        //            shortName = a.ShortName,
                        //            storeId = a.ID,
                        //            address = a.Address,
                        //        }),
                        //        token = id,
                        //        roles = roles
                        //    },
                        //}, JsonRequestBehavior.AllowGet);

                        return new HttpResponseMessage()
                        {
                            Content = new JsonContent(new
                            {
                                message = ConstantManager.MES_LOGIN_SUCCESS,
                                status = ConstantManager.STT_SUCCESS,
                                data = new
                                {
                                    stores = listStore.Select(a => new
                                    {
                                        storeName = a.Name,
                                        shortName = a.ShortName,
                                        storeId = a.ID,
                                        address = a.Address,
                                    }),
                                    token = id,
                                    roles = roles
                                },
                            })
                        };
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                else //ko co 1 trong 2 role admin hay manager
                {
                    //return Json(new
                    //{
                    //    message = ConstantManager.MES_ROLE_UNAUTHENTICATED,
                    //    status = ConstantManager.STT_FAIL
                    //}, JsonRequestBehavior.AllowGet);

                    return new HttpResponseMessage()
                    {
                        Content = new JsonContent(new
                        {
                            message = ConstantManager.MES_ROLE_UNAUTHENTICATED,
                            status = ConstantManager.STT_FAIL
                        })
                    };
                }


            }
            // đăng nhập không thành công
            else
            {
                //return Json(new
                //{
                //    message = ConstantManager.MES_LOGIN_FAIL,
                //    status = ConstantManager.STT_FAIL
                //}, JsonRequestBehavior.AllowGet);

                return new HttpResponseMessage()
                {
                    Content = new JsonContent(new
                    {
                        message = ConstantManager.MES_LOGIN_FAIL,
                        status = ConstantManager.STT_FAIL
                    })
                };
            }

        }

        public HttpResponseMessage UpdateAccount(AccountApiViewModel account)
        {
            CheckToken(account.Token);
            try
            {
                var aspNetUserApi = new AspNetUserApi();
                var user = aspNetUserApi.GetUserByUsername(account.AccountId);
                user.PasswordHash = account.AccountPassword;
                aspNetUserApi.ChangeAccountPassword(user);

                //return Json(user.PasswordHash);

                return new HttpResponseMessage()
                {
                    Content = new JsonContent(new
                    {
                        user.PasswordHash
                    })
                };
            }
            catch(Exception e)
            {
                return new HttpResponseMessage()
                {
                    Content = new JsonContent(new
                    {
                        e.Message,
                    })
                };
            }
        }

        private void CheckToken(string token)
        {
            if (token != _accessToken)
            {
                throw new Exception("Invalid token!!1");
            }
        }

        public string ComputeHash(string input, HashAlgorithm algorithm)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);

            return BitConverter.ToString(hashedBytes);
        }


    }
}

