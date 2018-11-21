using HmsService.Sdk;
using HmsService.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Controllers
{
    public abstract class AuthenticationBasedController : BaseController
    {
        private AspNetUserViewModel loggedInAccountField;
        private string loggedInRoleField;

        public AspNetUserViewModel LoggedInAccount
        {
            get
            {
                if (this.loggedInAccountField == null)
                {
                    this.GetUserInformation().Wait();
                }
                return this.loggedInAccountField;
            }
        }

        public string LoggedInRole
        {
            get
            {
                if (this.loggedInRoleField == null)
                {
                    this.GetUserInformation().Wait();
                }
                return this.loggedInRoleField;
            }
        }

        private async Task GetUserInformation()
        {
            var userManager = this.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if (this.User.Identity.IsAuthenticated)
            {
                var accountId = this.User.Identity.GetUserId();
                this.loggedInRoleField = userManager.GetRoles(accountId).First();

                var api = new AspNetUserApi();
                this.loggedInAccountField = await api.GetAsync(accountId);
            }
        }

        protected virtual void SaveUserInfoIntoViewBag()
        {
            this.ViewBag.LoggedInAccount = this.LoggedInAccount;
            this.ViewBag.LoggedInRole = this.LoggedInRole;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Do whatever here...
            
        }
    }
}