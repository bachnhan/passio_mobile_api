using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Wisky.SkyAdmin.Manage.Models.Identity;
using HmsService.ViewModels;
using Wisky.SkyUp.Website.Models;
using Wisky.SkyAdmin.Manage.Helpers;
using System.Web.Configuration;
using HmsService.Sdk;
using Microsoft.AspNet.Identity.EntityFramework;
using HmsService.Models;

namespace Wisky.SkyAdmin.Manage.Controllers
{
    [Authorize]
    public class AccountController : DomainBasedController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private UserManager<ApplicationUser> userManager;

        public ActionResult Validate(int storeId, int brandId)
        {

            var brandIdGet = Utils.GetBrandId();
            if (brandId != brandIdGet || !Utils.GetStoreId(brandId).Contains(storeId) && storeId>0 )
            {              
                return Json(new { result = true, url = Url.Action("LogOff","Account") }, JsonRequestBehavior.AllowGet);
            }
            else            
            { return Json(new { result = false}, JsonRequestBehavior.AllowGet); }
        }
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
            if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(new LoginViewModel());
            }
            else
            {
                var user = System.Web.HttpContext.Current.User;
                var storeId = HmsService.Models.Utils.GetStore().ID;
                var brandId = HmsService.Models.Utils.GetBrandId();

                if (user.IsInRole("Administrator"))//Admin
                {
                    return this.Redirect("/SysAdmin/Brand");
                }
                else if (user.IsInRole("BrandManager"))//Brand Manager
                {
                    return this.Redirect("/" + brandId + "/DashBoard/" + storeId + "/HomeDashBoard");
                }
                else if (user.IsInRole("Inventory"))//Inventory
                {
                    return this.Redirect("/" + brandId + "/Inventory/" + storeId + "/Inventory");
                }
                else if (user.IsInRole("StoreManager"))//Store Manager
                {
                    return this.Redirect("/" + brandId + "/DashBoard/" + storeId + "/DateDashBoard");
                }
                else if (user.IsInRole("ProductManager"))//Product Manager
                {
                    return this.Redirect("/" + brandId + "/Admin/" + storeId + "/Product");
                }
                else if (user.IsInRole("Reception"))//Reception
                {
                    return this.Redirect("/" + brandId + "/MembershipCard/" + storeId + "/MembershipCard/CreateMembershipCardStore");
                }
                else if (user.IsInRole("StoreReportViewer"))
                {
                    //return this.Redirect("/" + brandId + "/PosReport/" + storeId + "/StoreReport/StoreRevenueReport");
                    return this.Redirect("/" + brandId + "/DashBoard/" + storeId + "/HomeDashBoard");
                }
                else if (user.IsInRole("Booking"))
                {
                    return this.Redirect("/" + brandId + "/DashBoard/" + storeId + "/HomeDashBoard");
                }
                else if (user.IsInRole("Employee"))
                {
                    var netUserApi = new AspNetUserApi();
                    var empId = netUserApi.BaseService.FirstOrDefault(q => q.UserName.Equals(user.Identity.Name)).EmployeeId;
                    return this.Redirect("/" + brandId + "/FingerScan/" + storeId + "/EmployeeRequest/?employeeId=" + empId);
                    //return this.Redirect("/" + brandId + "/FingerScan/" + storeId + "/EmployeeRequest");
                }
                return View();
            }
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            //Pass cũ theo hash cũ AGmB7FfA1o84zW/PRNg24r7+N2Ha99p+ir2UXxn5YktXgnBeSneiDKjVHUNFuqOtdw==
            //Pass mới theo hash mới 1E3CC07EBEE71CD8D1F9A753E098240A
            var signManager = this.SignInManager;
            if (!ModelState.IsValid)
            { 
                return View(model);
            }

            //UserManager.PasswordHasher = new MP5Hasher(FormsAuthPasswordFormat.MD5);

            var user = await UserManager.FindAsync(model.Username, model.Password);
            var storeUserApi = new StoreUserApi();
            var storeApi = new StoreApi();

            if (user != null)
            {
                var storeUsers = storeUserApi.GetStoresFromUser(user.UserName);

                if (!user.Roles.Any(q => q.RoleId == "4"))
                {
                    ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
                    return View(model);
                }
                else if (storeUsers.Count() == 0 && !(user.Roles.Any(q =>q.RoleId == "1") || user.Roles.Any(q => q.RoleId == "12") || user.Roles.Any(q => q.RoleId == "9") || user.Roles.Any(q => q.RoleId == "13")))
                {
                    ModelState.AddModelError("", "Tài khoản của bạn chưa thuộc bất kỳ cửa hàng nào. Xin vui lòng liên hệ Quản trị viên để được giúp đỡ");
                    return View(model);
                }
                var result = await signManager.PasswordSignInAsync(model.Username, model.Password, false, shouldLockout: false);
                Session["Username"] = user.UserName;
                Session["UserId"] = user.Id;
                //var storeUser = storeUserApi.GetAllStoreUser();
                StoreViewModel firstStore = new StoreViewModel();
                foreach (var item in storeUsers)
                {
                    var store = storeApi.Get(item.StoreId);
                    if ((store.Type == 5 || store.Type == 6 || store.Type == 7) && store.BrandId == user.BrandId)
                    {
                        firstStore = store;
                        break;
                    }
                }

                var storeId = firstStore.ID;
                //var brandId = firstStore.BrandId;
                var brandId = user.BrandId;
                //var brandId = HmsService.Models.Utils.GetBrandId();

                if (result == SignInStatus.Success)
                {
                    if (user.Roles.Any(q => q.RoleId == "1"))//Admin
                    {
                        return this.Redirect("/SysAdmin/Brand");
                    }
                    else if (user.Roles.Any(q => q.RoleId == "12"))//Brand Manager
                    {
                        return this.Redirect("/" + brandId + "/DashBoard/" + storeId + "/HomeDashBoard");
                    }
                    else if (user.Roles.Any(q => q.RoleId == "11"))//Inventory
                    {
                        return this.Redirect("/" + brandId + "/Inventory/" + storeId + "/Inventory");
                    }
                    else if (user.Roles.Any(q => q.RoleId == "5"))//Store Manager
                    {
                        return this.Redirect("/" + brandId + "/DashBoard/" + storeId + "/DateDashBoard");
                    }
                    else if (user.Roles.Any(q => q.RoleId == "9"))//Product Manager
                    {
                        return this.Redirect("/" + brandId + "/Admin/" + storeId + "/Product");
                    }
                    else if (user.Roles.Any(q => q.RoleId == "2"))//Reception
                    {
                        return this.Redirect("/" + brandId + "/MembershipCard/" + storeId + "/MembershipCard/CreateMembershipCardStore");
                       // return this.Redirect("/" + brandId + "/DashBoard/" + storeId + "/DateDashBoard");
                    }
                    else if (user.Roles.Any(q => q.RoleId == "7"))
                    {
                        //return this.Redirect("/" + brandId + "/PosReport/" + storeId + "/StoreReport/StoreRevenueReport");
                        return this.Redirect("/" + brandId + "/DashBoard/" + storeId + "/HomeDashBoard");
                    }
                    else if (user.Roles.Any(q => q.RoleId == "6"))
                    {
                        return this.Redirect("/" + brandId + "/DashBoard/" + storeId + "/HomeDashBoard");
                    }
                    else if (user.Roles.Any(q => q.RoleId == ((int)RoleTypeEnum.Employee).ToString()))
                    {
                        var netUserApi = new AspNetUserApi();
                        var empId = netUserApi.BaseService.FirstOrDefault(q => q.UserName.Equals(user.UserName)).EmployeeId;
                        return this.Redirect("/" + brandId + "/FingerScan/" + storeId + "/EmployeeRequest?employeeId=" + empId);
                    }
                }
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
            else
            {
                //return this.RedirectToAction("Index", "Home");
                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            //var result = await signManager.PasswordSignInAsync(model.Username, model.Password, false, shouldLockout: false);
            //switch (result)
            //{
            //    case SignInStatus.Success:
            //        return this.Redirect(Url.Action("Index", "AccountBrandManager"));
            //    case SignInStatus.LockedOut:
            //        return View("Lockout");
            //    case SignInStatus.RequiresVerification:
            //        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false, parameters = this.CurrentPageDomain.Directory });
            //    case SignInStatus.Failure:
            //    default:
            //        ModelState.AddModelError("", "Invalid login attempt.");
            //        return View(model);
            //}
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }



        public ActionResult LogOff()
        {
            Session.RemoveAll();
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //return this.RedirectToAction("Index", "Home", new { parameters = this.CurrentPageDomain.Directory });
            return this.Redirect("Login?ReturnUrl=%2FAdmin");
        }

        public ActionResult ChangePassword()
        {
            var model = new ChangePasswordModel();
            ViewBag.storeId = "0";
            return View(model);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> ChangePassword(ChangePasswordModel model)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            UserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(store);
            //UserManager.PasswordHasher = new MP5Hasher(FormsAuthPasswordFormat.MD5);
            var currAccount = AuthenUtils.GetUser(this.TempData, this.User.Identity);
            if (ModelState.IsValid)
            {
                var result = await UserManager.ChangePasswordAsync(currAccount.Id, model.OldPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    //return this.Redirect("/");
                    return Json(new { success = true, message = "Thay đổi Password thành công." });
                }
                else
                {
                    //return this.RedirectToAction(this.Url.Action("ChangePassword"));
                    return Json(new { success = false, message = "Password cũ không chính xác. Vui lòng thử lại" });
                }
            }
            //return this.RedirectToAction(this.Url.Action("ChangePassword"));
            return Json(new { success = false, message = "Thay đổi thất bại, xin thử lại." });
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult LoginTest(string returnUrl)
        {
            if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(new LoginViewModel());
            }
            else
            {
                var user = System.Web.HttpContext.Current.User;
                var storeId = HmsService.Models.Utils.GetStore().ID;
                var brandId = HmsService.Models.Utils.GetBrandId();
                if (user.IsInRole("Administrator"))//Admin
                {
                    return this.Redirect("/SysAdmin/Brand");
                }
                else if (user.IsInRole("BrandManager"))//Brand Manager
                {
                    return this.Redirect("/" + brandId + "/DashBoard/" + storeId + "/DateDashBoard");
                }
                else if (user.IsInRole("Inventory"))//Inventory
                {
                    return this.Redirect("/" + brandId + "/Inventory/" + storeId + "#");
                }
                else if (user.IsInRole("StoreManager"))//Store Manager
                {
                    return this.Redirect("/" + brandId + "/Admin/" + storeId + "/StoreManager");
                }
                else if (user.IsInRole("ProductManager"))//Product Manager
                {
                    return this.Redirect("/" + brandId + "/#/" + storeId + "/#");
                }
                else if (user.IsInRole("Reception"))
                {
                    return this.Redirect("/" + brandId + "/Delivery/" + storeId + "/Delivery/Create");
                }
                return View();
            }
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginTest(LoginViewModel model, string returnUrl)
        {
            //Pass cũ theo hash cũ AGmB7FfA1o84zW/PRNg24r7+N2Ha99p+ir2UXxn5YktXgnBeSneiDKjVHUNFuqOtdw==
            //Pass mới theo hash mới 1E3CC07EBEE71CD8D1F9A753E098240A
            var signManager = this.SignInManager;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //if (!model.RememberMe)
            //{
            //    Session.Timeout = 1;
            //    System.Web.HttpContext.Current.Session.Timeout = 1;
            //}
            //else
            //{
            //    Session.Timeout = 30 * 24 * 60;
            //    System.Web.HttpContext.Current.Session.Timeout = 30 * 24 * 60;
            //}
            //UserManager.PasswordHasher = new MP5Hasher(FormsAuthPasswordFormat.MD5);

            var user = await UserManager.FindAsync(model.Username, model.Password);

            var storeUserApi = new StoreUserApi();
            var storeApi = new StoreApi();

            if (user != null)
            {
                var result = await signManager.PasswordSignInAsync(model.Username, model.Password, false, shouldLockout: false);
                Session["Username"] = user.UserName;
                Session["UserId"] = user.Id;
                var storeUsers = storeUserApi.GetStoresFromUser(user.UserName);
                //var storeUser = storeUserApi.GetAllStoreUser();
                StoreViewModel firstStore = new StoreViewModel();
                foreach (var item in storeUsers)
                {
                    var store = storeApi.Get(item.StoreId);
                    if ((store.Type == 5 || store.Type == 7) && store.BrandId == user.BrandId)
                    {
                        firstStore = store;
                        break;
                    }
                }

                var storeId = firstStore.ID;
                var brandId = firstStore.BrandId;
                //var brandId = HmsService.Models.Utils.GetBrandI*();

                if (result == SignInStatus.Success)
                {
                    if (user.Roles.Any(q => q.RoleId == "1"))//Admin
                    {
                        return this.Redirect("/SysAdmin/Brand");
                    }
                    else if (user.Roles.Any(q => q.RoleId == "12"))//Brand Manager
                    {
                        return this.Redirect("/" + brandId + "/DashBoard/" + storeId + "/HomeDashBoard");
                    }
                    else if (user.Roles.Any(q => q.RoleId == "11"))//Inventory
                    {
                        return this.Redirect("/" + brandId + "/Inventory/" + storeId + "/Inventory");
                    }
                    else if (user.Roles.Any(q => q.RoleId == "5"))//Store Manager
                    {
                        return this.Redirect("/" + brandId + "/DashBoard/" + storeId + "/DateDashBoard");
                    }
                    else if (user.Roles.Any(q => q.RoleId == "9"))//Product Manager
                    {
                        return this.Redirect("/" + brandId + "/Admin/" + storeId + "/Product");
                    }
                    else if (user.Roles.Any(q => q.RoleId == "2"))
                    {
                        return this.Redirect("/" + brandId + "/Delivery/" + storeId + "/Delivery/Create");
                    }
                    else if (user.Roles.Any(q => q.RoleId == "7"))
                    {
                        return this.Redirect("/" + brandId + "/PosReport/0" + "/SystemReport/RevenueReport");
                    }
                }
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
            else
            {
                //return this.RedirectToAction("Index", "Home");
                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            //var result = await signManager.PasswordSignInAsync(model.Username, model.Password, false, shouldLockout: false);
            //switch (result)
            //{
            //    case SignInStatus.Success:
            //        return this.Redirect(Url.Action("Index", "AccountBrandManager"));
            //    case SignInStatus.LockedOut:
            //        return View("Lockout");
            //    case SignInStatus.RequiresVerification:
            //        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false, parameters = this.CurrentPageDomain.Directory });
            //    case SignInStatus.Failure:
            //    default:
            //        ModelState.AddModelError("", "Invalid login attempt.");
            //        return View(model);
            //}
        }

    }
}