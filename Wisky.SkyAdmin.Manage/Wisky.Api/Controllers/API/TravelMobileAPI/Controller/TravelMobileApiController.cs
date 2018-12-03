using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Wisky.Api.Models;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Web.Http.Results;
using HmsService.Models;
using System.Web.Http;
using HmsService.Sdk;
using System.Web.Helpers;

namespace Wisky.Api.Controllers.API.TravelMobileAPI.Controller
{
    public class TravelMobileApiController : ApiController
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    _userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                }
                return _userManager;
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpPost]
        public HttpResponseMessage RegisterByEmail(RegisterViewModel model)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            //Check confirm password match password
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                return new HttpResponseMessage
                {
                    Content = new JsonContent(new
                    {
                        status = false,
                        message = ConstantManager.MES_CONFIRM_PASSWORD_NOT_MATCH,
                        data = new { }
                    }
                    )
                };
            }

            //Create user
            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email, FullName = model.FullName, BrandId = model.BrandId };

            var result = this.UserManager.Create(user, model.Password);

            //Check result
            if (!result.Succeeded)
            {
                if (this.UserManager.FindByNameAsync(model.Email) != null)
                {
                    return new HttpResponseMessage
                    {
                        Content = new JsonContent(new
                        {
                            status = false,
                            message = ConstantManager.MES_USER_EXISTED,
                            data = new { }
                        }
                    )
                    };
                }
                if (this.UserManager.FindByEmailAsync(model.Email) != null)
                {
                    return new HttpResponseMessage
                    {
                        Content = new JsonContent(new
                        {
                            status = false,
                            message = ConstantManager.MES_EMAIL_EXISTED,
                            data = new { }
                        }
                    )
                    };
                }
            }

            var newToken = Utils.GenerateToken(user.Id.ToString());
            return new HttpResponseMessage
            {
                Content = new JsonContent(new
                {
                    status = false,
                    message = ConstantManager.MES_SUCCESS,
                    data = new
                    {
                        access_token = newToken,
                        user_id = user.Id,
                        user_name = user.UserName,
                        full_name = user.FullName
                    }
                })
            };
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="fbAccessToken"></param>
        /// <param name="brandID"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage Login(string email, string password, int brandId)
        {
            try
            {
                var userApi = new AspNetUserApi();
                var user = userApi.GetUserByUsername(email);
                if (!Crypto.VerifyHashedPassword(user.PasswordHash, password))
                {
                    return new HttpResponseMessage
                    {
                        Content = new JsonContent(new
                        {
                            success = false,
                            message = ConstantManager.MES_WRONG_USERNAME_OR_PASSWORD,
                            data = new { }
                        })
                    };
                }

                var newToken = Utils.GenerateToken(user.Id.ToString());
                return new HttpResponseMessage
                {
                    Content = new JsonContent(new
                    {
                        success = true,
                        message = ConstantManager.MES_LOGIN_SUCCESS,
                        data = new
                        {
                            access_token = newToken,
                            user_id = user.Id,
                            user_name = user.UserName,
                            full_name = user.FullName
                        }
                    })
                };
            }
            catch (Exception e)
            {
                return new HttpResponseMessage
                {
                    Content = new JsonContent(new
                    {
                        success = false,
                        message = ConstantManager.MES_LOGIN_FAIL,
                        data = new { }
                    })
                };
            }
        }

        public HttpResponseMessage UploadImage(string email, string password, int brandId) {

            return new HttpResponseMessage
            {
                Content = new JsonContent(new
                {

                })
            };
        }
    }
}
