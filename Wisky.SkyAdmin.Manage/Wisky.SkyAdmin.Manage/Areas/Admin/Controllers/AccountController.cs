using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using Wisky.SkyUp.Website.Models;
using Wisky.SkyAdmin.Manage.Controllers;
using Wisky.SkyAdmin.Manage.Models.Identity;
using HmsService.ViewModels;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    public class AccountController : DomainBasedController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

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

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            var signManager = this.SignInManager;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await signManager.PasswordSignInAsync(model.Username, model.Password, false, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return this.Redirect(Url.Action("Index","AccountBrandManager"));
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false, parameters = this.CurrentPageDomain.Directory });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //return this.RedirectToAction("Index", "Home", new { parameters = this.CurrentPageDomain.Directory });
            return this.Redirect("Login?ReturnUrl=%2FAdmin");
        }

        public ActionResult ChangePassword()
        {
            return this.View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordModel model)
        {
            var userManager = this.UserManager;
            //#region Get and Change Account info
            var currAccount = AuthenUtils.GetUser(this.TempData, this.User.Identity);

            if (ModelState.IsValid)
            {
                var result = await userManager.ChangePasswordAsync(currAccount.Id, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return this.RedirectToAction("Index", "Default", new { parameters = this.CurrentPageDomain.Directory });
                }
                else
                {
                    return View();
                }
            }

            return View();
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
    }
}