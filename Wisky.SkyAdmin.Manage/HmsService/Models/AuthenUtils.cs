using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Principal;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using HmsService.Models.Entities;
using Wisky.SkyAdmin.Manage.Models;

namespace HmsService.ViewModels
{
    public static class AuthenUtils
    {

        public static IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        public static ApplicationSignInManager SignInManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }
        }

        public static ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        public static ApplicationUser GetUser(TempDataDictionary pTempData, IIdentity pIdentity)
        {
            ApplicationUser user = pTempData["Account"] as ApplicationUser;

            if (user == null && pIdentity.IsAuthenticated)
            {
                user = UserManager.FindById(pIdentity.GetUserId());
                pTempData["Account"] = user;
            }

            return user;
        }

        public static AspNetUser GetUserAspNet(TempDataDictionary pTempData, IIdentity pIdentity, HmsEntities dc = null)
        {
            return GetUser(pTempData, pIdentity).ToAspNetUser(dc);
        }

        public static AspNetUser ToAspNetUser(this ApplicationUser pUser, HmsEntities dc = null)
        {
            if (dc == null)
            {
                dc = new HmsEntities();
            }

            return dc.AspNetUsers.Where(q => q.Id.Equals(pUser.Id)).FirstOrDefault();
        }

    }
}