using HmsService.Models;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using Newtonsoft.Json;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Automation;
using Wisky.SkyAdmin.Manage.Models.Identity;

namespace Wisky.SkyAdmin.Manage.Controllers
{
    public class HomeController : DomainBasedController
    {
        public ActionResult Index(int? storeId, int? brandId)
        {
            var storeService = this.Service<IStoreService>();
            var store = storeService.Get(storeId);
            ViewBag.storeId = store.ID.ToString();
            ViewBag.storeName = store.Name;
            ViewBag.brandId = brandId.Value;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        //Điều hướng
        public ActionResult RenderStoreNavigation(int brandId, string returnUrl = "")
        {
            ViewBag.brandId = brandId;
            var model = new List<StoreNavigationViewModel>();
            var api = new StoreApi();
            var userApi = new StoreUserApi();
            var storeUserApi = new StoreUserApi();
            var username = HttpContext.User.Identity.Name;
            var userStores = storeUserApi.GetActiveStoresFromUser(username).Select(q => q.StoreId);
            var stores = api.BaseService.GetActiveStoreByBrandId(brandId);
            var isManager = HttpContext.User.IsInRole("BrandManager");
            if (isManager)
            {
                foreach (var store in stores)
                {
                    var modelStore = new StoreNavigationViewModel()
                    {
                        Id = store.ID,
                        Address = store.Address,
                        StoreName = store.Name,
                        User = userApi.GetFirstStoreManager(store.ID) ?? "Chưa có nhân viên phụ trách",
                    };
                    model.Add(modelStore);
                }
            }
            else
            {
                foreach (var store in stores)
                {
                    if (userStores.Contains(store.ID))
                    {
                        var modelStore = new StoreNavigationViewModel()
                        {
                            Id = store.ID,
                            Address = store.Address,
                            StoreName = store.Name,
                            User = userApi.GetFirstStoreManager(store.ID) ?? "Chưa có nhân viên phụ trách",
                        };
                        model.Add(modelStore);
                    }
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_StoreNavigationPartial", model);
        }
        //Chọn cửa hàng
        public async Task<ActionResult> ChooseStore(string returnUrl, int storeId, int? brandId)
        {
            if (storeId == 0)
            {
                var user = System.Web.HttpContext.Current.User;
                if (user.IsInRole("BrandManager") || user.IsInRole("Administrator"))
                {
                    return Redirect("/" + brandId + "/DashBoard/0/HomeDashBoard/");
                }
                else if (user.IsInRole("ProductManager"))
                {
                    return Redirect("/" + brandId + "/Admin/0/Product");
                }
                else
                {
                    return Redirect("/Account/Login");
                }
            }
            var api = new StoreApi();
            var stores = await api.GetStoreByID(storeId);
            if (stores == null)
            {
                return View();
            }
            ViewBag.StoreName = stores != null
                    ? stores.Name
                    : "Title";
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else if (stores.Type == (int)StoreTypeEnum.Hotel)
            {
                return Redirect("/" + stores.BrandId + "/DashBoard/" + storeId + /*"/" + stores.ShortName +*/ "/HomeDashBoard/Index");
            }
            else
            {
                var user = System.Web.HttpContext.Current.User;
                //if (HttpContext.User.IsInRole("Administrator") || HttpContext.User.IsInRole("StoreManager") /*|| HttpContext.User.IsInRole("Manager")*/)
                //{
                //    return Redirect("/" + stores.BrandId + "/DashBoard/" + storeId + /*"/" + stores.ShortName +*/ "/HomeDashBoard/Index");
                //}
                //else if (HttpContext.User.IsInRole("Inventory") || HttpContext.User.IsInRole("Reception"))
                //{
                //    return this.Redirect("/" + stores.BrandId + "/Orders/" + storeId + "/Order/Index");
                //    //"/" + stores.ShortName + 
                //}
                //else if (HttpContext.User.IsInRole("ProductManager") || HttpContext.User.IsInRole("Reception"))
                //{
                //    return this.Redirect("/" + stores.BrandId + "/Admin/" + storeId + "/Product");
                //    //"/" + stores.ShortName +
                //}
                //return Redirect("/" + stores.BrandId + "/DashBoard/" + storeId + "/HomeDashBoard/Index");
                // "/" + stores.ShortName +


                if (user.IsInRole("Administrator") || user.IsInRole("StoreManager"))
                {
                    return Redirect("/" + stores.BrandId + "/DashBoard/" + storeId + "/HomeDashBoard/Index");
                }
                else if (user.IsInRole("Inventory"))
                {
                    return this.Redirect("/" + stores.BrandId + "/Orders/" + storeId + "/Order/Index");
                }
                else if (user.IsInRole("ProductManager"))
                {
                    return this.Redirect("/" + stores.BrandId + "/Admin/" + storeId + "/Product");
                }
                else if (user.IsInRole("Reception"))
                {
                    return this.Redirect("/" + stores.BrandId + "/MembershipCard/" + storeId + "/MembershipCard/CreateMembershipCardStore");
                }
                return Redirect("/" + stores.BrandId + "/DashBoard/" + storeId + "/HomeDashBoard/Index");
            }
        }

        public class menuItem
        {
            public String name;
            public bool enable;
        }
        public class ListMenuItemStore
        {
            public IEnumerable<menuItem> menuItemStore;
        }
        public class ListMenuItemBrand
        {
            public IEnumerable<menuItem> menuItemBrand;
        }
        public class ListMenuItemWebsite
        {
            public IEnumerable<menuItem> menuItemWebsite;
        }
        [Authorize]
        public ActionResult LeftMenu()
        {
            try
            {
                var context = new ApplicationDbContext();
                var user = context.Users.FirstOrDefault(a => a.UserName.Equals(User.Identity.Name));
                var userRole = user.Roles.Select(a => a.RoleId).ToArray();
                var role = context.Roles.Where(a => userRole.Contains(a.Id)).Select(a => a.Name).ToArray();

                if (role.Any(q => q == "Administrator"))
                {
                    return PartialView("_MenuForSysAdmin");
                }

                int currentStoreId;
                var menuApi = new MenuApi();
                int brandId = int.Parse(RouteData.Values["brandId"]?.ToString());
                BrandApi brandApi = new BrandApi();
                StoreApi storeApi = new StoreApi();
                var brand = brandApi.GetBrandById(brandId);
                int menuTypeCode = 0;
                string menuFilter = "";
                var area = "";
                var listArea = new List<string>();

                if (brand != null) //if brand exists
                {
                    if (RouteData.Values["storeId"] != null)
                    {
                        currentStoreId = Convert.ToInt32(RouteData.Values["storeId"].ToString());
                    }
                    else
                    {
                        currentStoreId = 0;
                    }

                    if (currentStoreId != 0) //Url indicates admin page for stores
                    {
                        var ValidStore = Utils.GetStoreId(brandId).Contains(currentStoreId);
                        if (!ValidStore) //store not found
                        {
                            return HttpNotFound();
                        }
                        else
                        {
                            var store = storeApi.GetStoreById(currentStoreId);
                            if (store != null)
                            {
                                menuFilter = store.StoreFeatureFilter;
                                menuTypeCode = (int)MenuTypeEnum.StoreMenu;
                            }
                        }
                        foreach (int brandEnum in Enum.GetValues(typeof(BrandEnum)))
                        {
                            if (brandId == brandEnum)
                            {
                                string brandName = Enum.GetName(typeof(BrandEnum), brandEnum) + "MenuAreaEnum";
                                var type = System.Reflection.Assembly.GetAssembly(typeof(MenuUrlCode)).GetTypes().Where(q => string.Equals(q.Namespace, "HmsService.Models", StringComparison.Ordinal) && q.Name.Equals(brandName)).FirstOrDefault();
                                foreach (var areaItem in Enum.GetValues(type))
                                {
                                    listArea.Add(areaItem.ToString());
                                }
                            }
                        }
                    }//end if current store != null
                    else //Url indicates admin page for brands 
                    {
                        var IsAuthorized = role.Any(q => q == "BrandManager");
                        if (!IsAuthorized) //if current User roles do not authorized for Brand manager
                        {
                            return new HttpUnauthorizedResult();
                        }
                        else
                        {
                            menuFilter = brand.BrandFeatureFilter;
                            menuTypeCode = (int)MenuTypeEnum.BrandMenu;
                        }

                        foreach (int brandEnum in Enum.GetValues(typeof(BrandEnum)))
                        {
                            if (brandId == brandEnum)
                            {
                                string brandName = Enum.GetName(typeof(BrandEnum), brandEnum) + "MenuAreaEnum";
                                var type = System.Reflection.Assembly.GetAssembly(typeof(MenuUrlCode)).GetTypes().Where(q => string.Equals(q.Namespace, "HmsService.Models", StringComparison.Ordinal) && q.Name.Equals(brandName)).FirstOrDefault();
                                foreach (var areaItem in Enum.GetValues(type))
                                {
                                    listArea.Add(areaItem.ToString());
                                }
                            }                            
                        }                        
                    }

                    ViewBag.StoreId = currentStoreId;
                    ViewBag.BrandId = brandId;
                    var menuList = menuApi.GetMenuViewModelsByFilterAndRoles(menuTypeCode, menuFilter, role, listArea);
                    return PartialView("_MenuForManagers", menuList);
                }
                else //if brand not exists
                {
                    return HttpNotFound();
                }


            }
            catch (Exception e)
            {
                return HttpNotFound();
            }
        }

        [AllowAnonymous]
        public ActionResult AutoReport(string token)
        {
            if (token == "bdiuwqBIUWBIQ(98120912NDW")
            {
                DateReportExecuter.PerformWork();
            }


            return null;
        }

    }
}