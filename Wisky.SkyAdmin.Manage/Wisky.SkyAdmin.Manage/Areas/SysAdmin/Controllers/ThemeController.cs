using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ThemeController : DomainBasedController
    {

        

        // GET: SysAdmin/Theme
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            var model = new ThemeViewModel();
            return View(model);
        }

       
        [HttpPost]
        public async Task<ActionResult> Create(ThemeViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var user = System.Web.HttpContext.Current.User;
                if (user != null)
                {
                    var currentDateTime = Utils.GetCurrentDateTime();
                    model.CreatedBy = user.Identity.Name;
                    model.CreatedDate = currentDateTime;
                    model.LastModifiedBy = user.Identity.Name;
                    model.LastModifiedDate = currentDateTime;
                    model.Active = true;

                    var themeApi = new ThemeApi();
                    await themeApi.CreateThemeAsync(model);
                    return RedirectToAction("Index");
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        public async Task<ActionResult> Edit(int Id)
        {
            try
            {
                var themeApi = new ThemeApi();
                var model = await themeApi.GetAsync(Id);

                if (model == null || !model.Active)
                {
                    return HttpNotFound();
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }


        [HttpPost]
        public async Task<ActionResult> Edit(ThemeViewModel model)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    return View(model);
                }

                var user = System.Web.HttpContext.Current.User;
                if (user != null)
                {
                    model.LastModifiedBy = user.Identity.Name;
                    model.LastModifiedDate = Utils.GetCurrentDateTime();

                    var themeApi = new ThemeApi();
                    await themeApi.EditAsync(model.ThemeId, model);
                    return this.RedirectToAction("Index");
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int themeId)
        {
            var themeApi = new ThemeApi();
            try
            {
                await themeApi.DeleteAsync(themeId);
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
        }

        public async Task<JsonResult> GetListThemes(JQueryDataTableParamModel param)
        {
            var themeApi = new ThemeApi();
            var themes = themeApi.GetActiveThemes();
            var count = param.iDisplayStart + 1;

            try
            {
                var rs = (await themes.Where(q => string.IsNullOrEmpty(param.sSearch) ||
                            (!string.IsNullOrEmpty(param.sSearch)
                            && q.ThemeName.ToLower().Contains(param.sSearch.ToLower())))
                            .OrderByDescending(q => q.ThemeName)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength)
                            .ToListAsync())
                            .Select(theme => new IConvertible[] {
                                count++,
                                theme.ThemeName,
                                theme.ThemeFolderUrl,
                                theme.ThemeStyle,
                                theme.ImageUrl,
                                theme.CreatedDate.ToShortDateString(),
                                theme.Description,
                                theme.ThemeId
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
            catch (Exception e)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}