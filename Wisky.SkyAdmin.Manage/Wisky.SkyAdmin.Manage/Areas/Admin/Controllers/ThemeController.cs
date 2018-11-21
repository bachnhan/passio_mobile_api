using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    [Authorize(Roles = "BrandManager, Manager")]
    public class ThemeController : DomainBasedController
    {
        // GET: Admin/Theme
        public ActionResult Index()
        {
            try
            {
                var storeId = RouteData.Values["storeId"].ToString();
                ViewBag.storeId = storeId;
                ViewBag.brandId = RouteData.Values["brandId"].ToString();
                ViewBag.ThemeOfCurrentStore = GetStoreThemeByCurrentStore(int.Parse(storeId));

                //var currentStoreTheme = GetCurrentUsingThemeByStoreId(int.Parse(storeId));
                //if (currentStoreTheme != null)
                //{
                //    var themeStyleOfCurrentTheme = GetListThemeStyleByThemeId(currentStoreTheme.ThemeId);
                //    ViewBag.ThemeStyleOfCurrentTheme = themeStyleOfCurrentTheme;
                //}
                return View();
            }
            catch (Exception) { return HttpNotFound(); }
        }


        public ActionResult GetTheme(string id)
        {
            var themeApi = new ThemeApi();
            var storeId = RouteData.Values["storeId"].ToString();
            var theme = themeApi.GetActive().Where(t => t.ThemeId == int.Parse(id)).FirstOrDefault();
            var storetheme = new StoreThemeApi();

            try
            {
                if (storetheme.GetActive().Where(st => st.StoreId == int.Parse(storeId) && st.ThemeId == int.Parse(id)).FirstOrDefault() != null)
                {
                    ViewBag.apply = 1;
                }
                else
                {
                    ViewBag.apply = 0;
                }
                ViewBag.storeId = storeId;
                ViewBag.themeId = id;
                ViewBag.themeselected = theme;
                return View();
            }
            catch (Exception) { return HttpNotFound(); }
        }


        public ActionResult Themes()
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            ViewBag.brandId = RouteData.Values["brandId"].ToString();
            return View();
        }

        public ActionResult MyThemes()
        {
            ViewBag.storeId = RouteData.Values["storeId"].ToString();
            ViewBag.brandId = RouteData.Values["brandId"].ToString();
            return View();
        }
        /// <summary>
        /// Open theme file
        /// </summary>
        /// <returns></returns>
        /// 
        public ActionResult openBrowser(string path)
        {
            System.Diagnostics.Process.Start(path);
            return null;
        }
        [HttpPost]
        public JsonResult GetAllThemes()
        {
            try
            {
                var themeApi = new ThemeApi();
                var themes = themeApi.GetActiveThemes().ToList();

                var result = JsonConvert.SerializeObject(themes);
                return Json(new { success = true, themes = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetThemeStyleByThemeId(string currentStoreThemeId)
        {
            try
            {
                var storeThemeApi = new StoreThemeApi();

                var storeTheme = storeThemeApi.Get(int.Parse(currentStoreThemeId));

                var themApi = new ThemeApi();

                var theme = themApi.Get(storeTheme.ThemeId);

                if (storeTheme != null)
                {
                    var listThemeStyle = GetListThemeStyleByThemeId(storeTheme.ThemeId);
                    return Json(new { success = true, listThemeStyle = listThemeStyle, currentStyle = storeTheme.CustomThemeStyle, themeObj = theme }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> UpdateChangeStoreTheme(string selectedStoreThemeId, string selectedStyle, int currentStoreThemeId)
        {
            try
            {
                var storeThemeApi = new StoreThemeApi();

                // Get current in use StoreTheme
                var storeTheme = storeThemeApi.GetActive().Where(st => st.StoreThemeId == int.Parse(selectedStoreThemeId)).FirstOrDefault();

                // Set current in use StoreTheme (IsUsing) to false
                var storeThemeNotUsing = storeThemeApi.GetActive().Where(st => st.StoreThemeId == currentStoreThemeId).FirstOrDefault();

                if (storeThemeNotUsing != null)
                {
                    storeThemeNotUsing.IsUsing = false;
                    await storeThemeApi.EditAsync(storeThemeNotUsing.StoreThemeId, storeThemeNotUsing);
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }

                // Save change StoreTheme
                if (storeTheme != null)
                {
                    storeTheme.IsUsing = true;
                    storeTheme.CustomThemeStyle = selectedStyle;
                    await storeThemeApi.EditAsync(storeTheme.StoreThemeId, storeTheme);
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Apply Theme for Store
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<JsonResult> ApplyThemeToStore(int themeId)
        {
            var user = System.Web.HttpContext.Current.User;
            var storeId = RouteData.Values["storeId"].ToString();
            var storeThemeApi = new StoreThemeApi();
            var themeApi = new ThemeApi();
            var theme = themeApi.GetActive().Where(t => t.ThemeId == themeId).FirstOrDefault();
            StoreThemeViewModel storeTheme = new StoreThemeViewModel();
            storeTheme.StoreId = int.Parse(storeId);
            storeTheme.ThemeName = theme.ThemeName;
            storeTheme.ThemeId = theme.ThemeId;
            storeTheme.StoreThemeName = "Apply " + theme.ThemeName;
            storeTheme.CreatedDate = theme.CreatedDate;
            storeTheme.IsUsing = false;
            storeTheme.LastModifiedDate = theme.LastModifiedDate.Value;
            storeTheme.LastModifiedBy = theme.LastModifiedBy;
            storeTheme.CreatedBy = theme.CreatedBy;
            storeTheme.Description = theme.Description;
            storeTheme.Active = true;
            try
            {
                await storeThemeApi.CreateStoreThemeAsync(storeTheme);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                throw (ex);
            }
        }

        public async Task<JsonResult> CancelApply(int themeId)
        {
            try
            {
                var storeId = RouteData.Values["storeId"].ToString();
                var storeThemeApi = new StoreThemeApi();
                var storeTheme = storeThemeApi.GetActive().Where(st => st.ThemeId == themeId && st.StoreId == int.Parse(storeId)).FirstOrDefault();
                bool flag = true;//kiểm tra phải Theme đang using không
                if (storeTheme.IsUsing == true)
                {
                    flag = false;
                }
                await storeThemeApi.DeleteAsync(storeTheme.StoreThemeId);
                if (!flag)
                {
                    var storethemeTmp = storeThemeApi.GetActive().Where(st => st.StoreId == int.Parse(storeId)).FirstOrDefault();
                    storethemeTmp.IsUsing = true;
                    await storeThemeApi.EditAsync(storethemeTmp.StoreThemeId, storethemeTmp);
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
           
            
        }
        public async Task<JsonResult> GetStoreThemeByStoreId(JQueryDataTableParamModel param)
        {
            try
            {
                var storeThemeApi = new StoreThemeApi();
                // Current storeId
                var storeId = RouteData.Values["storeId"].ToString();

                // Get StoreTheme by StoreId
                var storeThemes = storeThemeApi.GetActiveStoreThemeByStoreId(int.Parse(storeId));
                var count = param.iDisplayStart + 1;

                var rs = (await storeThemes.Where(q => string.IsNullOrEmpty(param.sSearch) ||
                            (!string.IsNullOrEmpty(param.sSearch)
                            && q.StoreThemeName.ToLower().Contains(param.sSearch.ToLower())))
                            .OrderByDescending(q => q.StoreThemeName)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength)
                            .ToListAsync())
                            .Select(theme => new IConvertible[] {
                                count++,
                                theme.StoreThemeName,
                                theme.CreatedBy,
                                GetThemeUrl(theme.ThemeId),
                                theme.CreatedDate.ToShortDateString(),
                                theme.IsUsing,
                                theme.StoreThemeId
                            });
                var totalRecords = rs.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<StoreThemeViewModel> GetStoreThemeByCurrentStore(int storeId)
        {
            try
            {
                var storeThemeApi = new StoreThemeApi();
                // Get StoreTheme by StoreId
                var storeThemes = storeThemeApi.GetActiveStoreThemeByStoreId(storeId).ToList();
                return storeThemes;
            }
            catch (Exception) { return null; }
        }

        private StoreThemeViewModel GetCurrentUsingThemeByStoreId(int storeId)
        {
            try
            {
                var storeThemeApi = new StoreThemeApi();
                var storeTheme = storeThemeApi.GetActiveStoreThemeByStoreId(storeId).FirstOrDefault();
                return storeTheme;
            }
            catch (Exception) { return null; }
        }
        private ThemeViewModel GetThemById(int themeId)
        {
            try
            {
                var themeApi = new ThemeApi();
                var theme = themeApi.Get(themeId);
                return theme;
            }
            catch (Exception) { return null; }
        }

        private string[] GetListThemeStyleByThemeId(int themeId)
        {
            try
            {
                var themeApi = new ThemeApi();
                var theme = themeApi.Get(themeId);
                if (theme != null)
                {
                    return theme.ThemeStyle.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception) { return null; }
        }

        private string GetThemeUrl(int themeId)
        {
            try
            {
                var themeApi = new ThemeApi();
                var theme = themeApi.Get(themeId);
                if (theme != null && (theme.ImageUrl != null))
                {
                    return theme.ImageUrl;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception) { return ""; }
        }
    }
}