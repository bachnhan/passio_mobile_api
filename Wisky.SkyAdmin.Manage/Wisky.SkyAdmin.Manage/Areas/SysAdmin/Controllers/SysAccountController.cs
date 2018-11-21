using HmsService.Sdk;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SkyWeb.DatVM.Mvc;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Wisky.SkyAdmin.Manage.Models.Identity;
using HmsService.Models;
using Wisky.SkyAdmin.Manage.Controllers;
using HmsService.ViewModels;
using System.Resources;
using Wisky.SkyAdmin.Manage.Helpers;
using System.Web.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    //[Authorize(Roles = "Administrator")]
    public class SysAccountController : DomainBasedController
    {
        private const string SysAdminRoleName = "Administrator";

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public SysAccountController()
        {
        }

        public SysAccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        [AllowAnonymous]
        public ActionResult Install(string token, bool? forceResetPassword)
        {
            if (token != ConfigurationManager.AppSettings["SysAdminInstallToken"])
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Unauthorized request source.");
            }

            var context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var accountNames = ConfigurationManager.AppSettings["SysAdminAccounts"].Split(';');
            var defaultPassword = ConfigurationManager.AppSettings["SysAdminDefaultPassword"];

            this.CreateRoleIfNotExist(SysAdminRoleName, roleManager);

            foreach (var accountName in accountNames)
            {
                this.CreateOrResetPassword(userManager, accountName, defaultPassword, forceResetPassword.GetValueOrDefault());
            }

            return this.Content("OK!");
        }

        public async Task<ActionResult> Index()
        {
            var model = (await new AspNetUserApi()
                .GetDetails());

            return this.View(model);
        }

        //public JsonResult LoadSysAccount()

        public ActionResult Create()
        {
            var model = new AspNetUserEditViewModel()
            {
                AspNetUser = new HmsService.ViewModels.AspNetUserViewModel(),
            };

            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(AspNetUserEditViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng liên hệ admin" });
            }

            //UserManager.PasswordHasher = new MP5Hasher(FormsAuthPasswordFormat.MD5);
            var userManager = this.UserManager;
            var result = await UserManager.CreateAsync(new ApplicationUser()
            {
                UserName = model.AspNetUser.UserName,
                Email = model.AspNetUser.Email,
            });
            if (!result.Succeeded)
            {
                if (await this.UserManager.FindByNameAsync(model.AspNetUser.UserName) != null)
                {
                    return Json(new { success = false, message = "Tài khoản " + model.AspNetUser.UserName + " đã tồn tại!" });
                }
                if (await this.UserManager.FindByEmailAsync(model.AspNetUser.Email) != null)
                {
                    return Json(new { success = false, message = "Email " + model.AspNetUser.Email + " đã tồn tại" });
                }
                return Json(new { success = false, message = "Tạo người dùng thất bại, vui lòng liên hệ admin!" });
            }
            this.ThrowIfNotSucceed(result);

            var user = await userManager.FindByNameAsync(model.AspNetUser.UserName);
            UserManager.AddToRole(user.Id, "ActiveUser");
            Utils.SetMessage(this, "Thành công!");
            return Json(new { success = true, message = "Tạo người dùng thành công!", userId = user.Id });
        }

        public async Task<JsonResult> DeleteAccount(string id)
        {
            var userApi = new AspNetUserApi();

            try
            {
                await userApi.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Xóa người dùng thất bại, xin vui lòng thử lại." });
            }

            return Json(new { success = false, message = "Xóa người dùng thành công !" });
        }

        public async Task<ActionResult> Edit(string id)
        {
            var details = await new AspNetUserApi().GetDetails(id);

            if (details == null)
            {
                return this.IdNotFound();
            }

            // Exchange to higher model
            var model = new AspNetUserEditViewModel(details, this.Mapper);
            await this.PrepareEdit(model);

            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AspNetUserEditViewModel model)
        {
            var api = new AspNetUserApi();
            var entity = await api.GetAsync(model?.AspNetUser?.Id);

            if (entity == null)
            {
                return this.IdNotFound();
            }

            if (!this.ModelState.IsValid)
            {
                await this.PrepareEdit(model);
                return this.View(model);
            }

            await api.EditAsync(model.AspNetUser, model.SelectedRoles ?? new string[0], model.AspNetUser.BrandId);

            Utils.SetMessage(this, "Edited!");
            return this.RedirectToAction("Index");
        }

        private async Task PrepareEdit(AspNetUserEditViewModel model)
        {
            //model.AvailableStores = (await new StoreApi().GetActiveAsync()).Select(q => new SelectListItem()
            //{
            //    Text = q.Name,
            //    Value = q.ID.ToString(),
            //    Selected = q.ID == model.AdminStoreId,
            //});
            ResourceManager rm = Resources.EnumLanguage.ResourceManager;
            model.AvailableBrands = (await new BrandApi().GetActiveAsync()).Select(q => new SelectListItem()
            {
                Text = q.BrandName,
                Value = q.Id.ToString(),
                Selected = q.Id == model.AspNetUser.BrandId,
            });
            var roleIds = model.Roles.Select(q => q.Id);
            model.AvailableRoles = (await new AspNetRoleApi().GetActiveAsync())
                .ToSelectList(q => rm.GetString(q.Name), q => q.Id.ToString(), q => roleIds.Contains(q.Id));
        }

        public async Task<JsonResult> SetPassword(string id, string password)
        {
            var userManager = this.UserManager;

            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tồn tại người dùng này!" });
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user.Id);
            var result = await userManager.ResetPasswordAsync(user.Id, token, password);
            if (!result.Succeeded)
            {
                return Json(new { success = false, message = "Mật khẩu không đúng định dạng, vui lòng nhập lại!" });
            }

            Utils.SetMessage(this, "Password set.!");
            return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
        }

        private void CreateOrResetPassword(UserManager<ApplicationUser> userManager, string username, string password, bool forceResetPassword)
        {
            var user = userManager.FindByName(username);

            if (user == null)
            {
                user = new ApplicationUser()
                {
                    UserName = username,
                    Email = "sysadmin@wisky.vn",
                };

                var result = userManager.Create(user, password);
                this.ThrowIfNotSucceed(result);
            }
            else
            {
                if (forceResetPassword)
                {
                    var token = userManager.GeneratePasswordResetToken(user.Id);
                    var result = userManager.ResetPassword(user.Id, token, password);
                    this.ThrowIfNotSucceed(result);
                }
            }

            if (!userManager.IsInRole(user.Id, SysAdminRoleName))
            {
                var result = userManager.AddToRole(user.Id, SysAdminRoleName);
                this.ThrowIfNotSucceed(result);
            }
        }

        private void CreateRoleIfNotExist(string role, RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExists(role))
            {
                roleManager.Create(new IdentityRole()
                {
                    Name = role,
                });
            }
        }

        private void ThrowIfNotSucceed(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                throw new OperationCanceledException(string.Join(Environment.NewLine, result.Errors));
            }
        }


        public JsonResult UpdateNewHash()
        {
            //UserManager.PasswordHasher = new MP5Hasher(FormsAuthPasswordFormat.MD5);
            var userManager = this.UserManager;
            var users = userManager.Users.ToList();
            foreach (var user in users)
            {
                var token = userManager.GeneratePasswordResetToken(user.Id);
                var result = userManager.ResetPassword(user.Id, token, "123456");
                this.ThrowIfNotSucceed(result);
            }
            return Json(new { success = true, message = "Reset thành công" }, JsonRequestBehavior.AllowGet);
        }
    }
}