using HmsService.Models;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Areas.FingerScan.Controllers;
using Wisky.SkyAdmin.Manage.Controllers;
using Wisky.SkyAdmin.Manage.Helpers;
using Wisky.SkyAdmin.Manage.Models;
using Wisky.SkyAdmin.Manage.Models.Identity;




namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    [Authorize(Roles = "BrandManager, Manager")]
    public class AccountBrandManagerController : DomainBasedController
    {
        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;
        private IAspNetUserService _aspNetUserService;

        public ApplicationUserManager UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    //_userManager.PasswordHasher = new MP5Hasher(FormsAuthPasswordFormat.MD5);
                }
                return _userManager;
            }
            private set
            {
                _userManager = value;
            }
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

        public ActionResult GetListAssignToStore(int empId)
        {
            var brandId = int.Parse(RouteData.Values["brandId"].ToString());
            var storeApi = new StoreApi();
            var listStore = storeApi.GetAllActiveStore(brandId).ToList();
            var employeeStoreApi = new EmployeeInStoreApi();
            var result = new List<storeEmployeeMapping>();
            foreach (var item in listStore)
            {
                var mapping  = new storeEmployeeMapping();
                var tmpMapping = employeeStoreApi.GetEmployeeByIdAndStoreID(empId, item.ID);
                if (tmpMapping != null)
                {
                    mapping.StoreId = item.ID;
                    mapping.Checked = true;
                    mapping.StoreName = item.Name;
                }
                else
                {
                    mapping.StoreId = item.ID;
                    mapping.Checked = false;
                    mapping.StoreName = item.Name;
                }
                result.Add(mapping);
            }
            return Json(new { listresult = result }, JsonRequestBehavior.AllowGet);
        }


        // GET: SysAdmin/AccountBrandManager
        #region Index page
        public ActionResult Index(int storeId)
        {
            var context = new ApplicationDbContext();
            var AspNetRolesApi = new AspNetRoleApi();
            //Không lấy role brandManager vs Administrator
            ResourceManager rm = Resources.EnumLanguage.ResourceManager;
            ViewBag.RoleList = context.Roles.Where(a => !a.Name.Equals("ActiveUser") && !a.Name.Equals("BrandManager") && !a.Name.Equals("Administrator") && !a.Name.Equals("SystemAdmin")).ToArray().Select(a => new { Id = a.Id, Name = rm.GetString(a.Name) }).ToArray();
            ViewBag.UserName = User.Identity.GetUserName();
            ViewBag.storeId = storeId.ToString();

            return View();
        }
        [Authorize(Roles = "BrandManager, ShiftCreator")]
        public ActionResult GetListAccountBrandManager(HmsService.Models.JQueryDataTableParamModel param, int brandId, int storeIdCode)
        {

            //var user = HttpContext.User;//để exclude thằng đang đăng nhập ra khỏi danh sách

            int count = 0;
            // IEnumerable<IConvertible[]> rs = null;
            int totalRecords;

            var customerUserApi = new AspNetUserApi();
            var storeApi = new StoreUserApi();

            var storeUser = storeApi.GetAllStoreUser();

            //Chỉ lấy activeUser
            //var accountBrandManagers = customerUserApi.GetAllAccountBrandMananger()
            //    .Where(q => q.AspNetUser.BrandId == brandId && q.Roles.Any(a => a.Name.Equals("ActiveUser")));

            var accountBrandManagers = customerUserApi.GetAllAccountActiveByBrandID(brandId);
            totalRecords = accountBrandManagers.Count();
            if (storeIdCode != 0)
            {
                accountBrandManagers = accountBrandManagers
               .Where(q => String.IsNullOrEmpty(param.sSearch) || q.UserName.ToLower().Contains(param.sSearch.ToLower())).ToList();
                count = param.iDisplayStart + 1;
                var GetListUser = storeUser.Where(a => a.StoreId == storeIdCode);

                accountBrandManagers = from t1 in accountBrandManagers
                           join t2 in GetListUser
                           on t1.UserName equals t2.Username
                           select t1;
                var rs = accountBrandManagers
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .Select(a => new object[]
                    {
                    count++,
                    string.IsNullOrEmpty(a.UserName) ? "Không xác định" : a.UserName,
                    string.IsNullOrEmpty(a.FullName) ? "Không xác định" : a.FullName,
                    //a.EmailConfirmed ? "True" : "False",
                    true,
                    a.AspNetRoles.Where(r => !r.Name.Equals("ActiveUser")).Select(r => new {Id = r.Id, Name = Utils.DisplayName((RoleTypeEnum)Enum.Parse(typeof(RoleTypeEnum), r.Name))}).ToArray(),
                    storeUser.Where(s=>s.Username.ToLower().Equals(a.UserName.ToLower())).Select(b=> new { Name = b.Store.Name }).ToArray(),
                    a.Id,
                    a.AspNetRoles.Any(r => r.Name.Equals("ActiveUser")),
                    }).ToList();

                var displayRecord = accountBrandManagers.Count();
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = displayRecord,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                accountBrandManagers = accountBrandManagers
                .Where(q => String.IsNullOrEmpty(param.sSearch) || q.UserName.ToLower().Contains(param.sSearch.ToLower())).ToList();

                count = param.iDisplayStart + 1;

                var rs = accountBrandManagers
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .Select(a => new object[]
                    {
                    count++,
                    string.IsNullOrEmpty(a.UserName) ? "Không xác định" : a.UserName,
                    string.IsNullOrEmpty(a.FullName) ? "Không xác định" : a.FullName,
                    a.EmailConfirmed ? "True" : "False",
                    a.AspNetRoles.Where(r => !r.Name.Equals("ActiveUser")).Select(r => new {Id = r.Id, Name = Utils.DisplayName((RoleTypeEnum)Enum.Parse(typeof(RoleTypeEnum), r.Name))}).ToArray(),
                    storeUser.Where(s=>s.Username.ToLower().Equals(a.UserName.ToLower())).Select(b=> new { Name = b.Store.Name }).ToArray(),
                    a.Id,
                    a.AspNetRoles.Any(r => r.Name.Equals("ActiveUser")),
                    }).ToList();

                var displayRecord = accountBrandManagers.Count();
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = displayRecord,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Cập nhật chức vụ
        [HttpPost]
        public async Task<JsonResult> AssignRole(string id, string[] newRoles, int brandId)
        {
            var storeApi = new StoreApi();
            var user = UserManager.Users.FirstOrDefault(a => a.Id.Equals(id));
            if (user == null)
            {
                return Json(new { success = false });
            }
            var currentRole = UserManager.GetRoles(id).Where(a => !a.Equals("ActiveUser"));
            var b = Task.Run(() => UserManager.RemoveFromRolesAsync(id, currentRole.ToArray())).Result;
            if (newRoles == null)
            {
                newRoles = new string[0];
            }
            var c = Task.Run(() => UserManager.AddToRolesAsync(id, newRoles)).Result;
            var storeList = storeApi.GetActiveStoreByBrandId(brandId);
            await Utils.PostNotiMessageToStores(storeList, (int)NotifyMessageType.AccountChange);
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> UpdateRole(string id, string[] newRoles, int brandId)
        {
            var storeApi = new StoreApi();
            var user = UserManager.Users.FirstOrDefault(a => a.Id.Equals(id));
            if (user == null)
            {
                return Json(new { success = false });
            }
            var currentRole = UserManager.GetRoles(id).Where(a => !a.Equals("ActiveUser") && !a.Equals("Administrator") && !a.Equals("BrandManager"));
            var b = Task.Run(() => UserManager.RemoveFromRolesAsync(id, currentRole.ToArray())).Result;
            if (newRoles == null)
            {
                newRoles = new string[0];
            }
            var c = Task.Run(() => UserManager.AddToRolesAsync(id, newRoles)).Result;
            var storeList = storeApi.GetActiveStoreByBrandId(brandId);
            await Utils.PostNotiMessageToStores(storeList, (int)NotifyMessageType.AccountChange);
            return Json(new { success = true, message = "Cập nhật thành công" });
        }
        #endregion

        #region Assign User cho store
        public async Task<JsonResult> AssignUser(string username, string[] stores, int brandId)
        {
            var storeUserApi = new StoreUserApi();
            var storeApi = new StoreApi();
            var deleteCheck = await storeUserApi.DeleteAllStoreUserByUsername(username);
            if (stores != null && deleteCheck)
            {
                foreach (var StoreID in stores)
                {
                    try
                    {
                        var storeId = int.Parse(StoreID);
                        storeUserApi.Create(new StoreUserViewModel()
                        {
                            Username = username,
                            StoreId = storeId,
                        });
                    }
                    catch (Exception e)
                    {
                        Console.Write(e);
                    }
                }
                var storeList = storeApi.GetActiveStoreByBrandId(brandId);
                await Utils.PostNotiMessageToStores(storeList, (int)NotifyMessageType.AccountChange);
                return Json(new { success = true, message = "Cập nhật thành công." }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Cập nhật thất bại." }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetAssignedUser(string id, int brandId)
        {
            var user = UserManager.Users.FirstOrDefault(a => a.Id.Equals(id));
            ViewBag.UserName = user.UserName;

            var storeApi = new StoreApi();

            var stores = storeApi.GetStores()
                .Where(q => (q.Type == (int)StoreTypeEnum.Store || q.Type == (int)StoreTypeEnum.CallCenter || q.Type == (int)StoreTypeEnum.Website) && q.isAvailable == true && q.BrandId == brandId)
                .Select(q => new AssignUserPartialModel
                {
                    StoreID = q.ID,
                    StoreName = q.ShortName,
                    IsChecked = false,
                }).ToList();

            var storeUserApi = new StoreUserApi();
            var CheckedStores = storeUserApi.GetAllStoreUser()
                .Where(q => q.Username.Equals(user.UserName));

            foreach (var storeUser in stores)
            {
                if (CheckedStores.FirstOrDefault(q => q.StoreId == storeUser.StoreID) != null)
                {
                    storeUser.IsChecked = true;
                }
            }

            return PartialView("_AssignUserPartial", stores);
        }

        #endregion

        #region Tạo người dùng mới
        public ActionResult Create(int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            RegisterViewModel model = new RegisterViewModel();
            return View(model);
        }

        //Gắn brandId
        [HttpPost]
        public async Task<JsonResult> Create(RegisterViewModel model, int brandId)
        {
            var storeApi = new StoreApi();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser() { UserName = model.Username, Email = model.Email, FullName = model.FullName, BrandId = brandId };

                var result = await this.UserManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    if (await this.UserManager.FindByNameAsync(model.Username) != null)
                    {
                        return Json(new { success = false, message = "Tài khoản " + model.Username + " đã tồn tại!" });
                    }
                    if (await this.UserManager.FindByEmailAsync(model.Email) != null)
                    {
                        return Json(new { success = false, message = "Email " + model.Email + " đã tồn tại" });
                    }
                    return Json(new { success = false, message = "Tạo người dùng thất bại, vui lòng liên hệ admin!" });
                }

                var rs = await UserManager.AddToRoleAsync(user.Id, "ActiveUser");

                if (!rs.Succeeded)
                {
                    return Json(new { success = false, message = rs.Errors });

                }
                var stores = storeApi.GetActiveStoreByBrandId(brandId).ToList();
                await Utils.PostNotiMessageToStores(stores, (int)NotifyMessageType.AccountChange);
                return Json(new { success = true, message = "Tạo người dùng thành công" });
            }
            return Json(new { success = false, message = "Tạo người dùng thất bại" });
        }

        #endregion


        [HttpPost]
        public async Task<JsonResult> Delete(string id, int brandId)
        {
            var user = UserManager.Users.FirstOrDefault(q => q.Id.Equals(id));

            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng này không tồn tại trong hệ thống, xin hãy thử lại." });
            }

            var currentRole = UserManager.GetRoles(id).Where(q => q.Equals("ActiveUser"));

            try
            {
                var b = await UserManager.RemoveFromRolesAsync(id, currentRole.ToArray());
                var stores = new StoreApi().GetActiveStoreByBrandId(brandId).ToList();
                await Utils.PostNotiMessageToStores(stores, (int)NotifyMessageType.AccountChange);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Đã có lỗi xảy ra, vui lòng thử lại." });
            }
            return Json(new { success = true, message = "Xóa người dùng thành công" });
        }

        #region Update
        public ActionResult Update(string id, int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            var user = UserManager.Users.FirstOrDefault(q => q.Id.Equals(id));
            if (user == null)
            {
                return Json(new { message = "Không tồn tại người dùng trong hệ thống!" });
            }
            var model = new RegisterViewModel()
            {
                Username = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Password = user.PasswordHash,
                CurrentEmail = user.Email,
                CurrentPassword = user.PasswordHash
            };

            return this.View(model);
        }

        [HttpPost]
        public async Task<JsonResult> Update(RegisterViewModel model, int brandId)
        {
            if (!ModelState.IsValid)
                return Json(
                    new
                    {
                        success = false,
                        validate = ModelState.Select(a => new
                        {
                            name = a.Key,
                            value = a.Value.Errors.Any()
                                ? a.Value.Errors.FirstOrDefault().ErrorMessage
                                : ""
                        }).ToArray()
                    });

            var user = UserManager.Users.FirstOrDefault(q => q.UserName.Equals(model.Username));
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại trong hệ thống, cập nhật thất bại." });
            }

            if (await this.UserManager.FindByEmailAsync(model.Email) != null && model.Email != model.CurrentEmail)
            {
                return Json(new { success = false, message = "Email " + model.Email + " đã tồn tại" });
            }
            user.FullName = model.FullName;
            user.Email = model.Email;

            if (model.Password != model.CurrentPassword)
            {
                user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
            }

            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Json(new { success = false, message = "Cập nhật thất bại, vui lòng thử lại." });
            }
            var stores = new StoreApi().GetActiveStoreByBrandId(brandId).ToList();
            await Utils.PostNotiMessageToStores(stores, (int)NotifyMessageType.AccountChange);
            return Json(new { success = true, message = "Cập nhật thành công" });
        }
        #endregion
    }

    public class storeEmployeeMapping
    {
        public int StoreId { get; set; }
        public bool Checked { get; set; }
        public string StoreName { get; set; }

    }
}