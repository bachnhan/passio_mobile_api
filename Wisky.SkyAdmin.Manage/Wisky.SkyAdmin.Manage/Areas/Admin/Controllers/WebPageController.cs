using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using Wisky.SkyAdmin.Manage.Models;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{

    //[Authorize(Roles = Utils.AdminAuthorizeRoles)]
    public class WebPageController : DomainBasedController
    {
        // GET: Admin/WebPage
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult IndexList(BootgridRequestViewModel request)
        {
            var result = new WebPageApi().GetAdminWithFilterAsync(
                this.CurrentStore.ID, request.searchPhrase,
                request.current, request.rowCount, request.FirstSortTerm);

            var model = new BootgridResponseViewModel<WebPageViewModel>(result);
            return this.Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ChangeStatus(string webpageId)
        {

            try
            {
                int id = Int32.Parse(webpageId);
                var webPage = new WebPageApi().Get(id);
                var status = webPage.IsActive;
                if (status)
                {
                    webPage.IsActive = false;
                    new WebPageApi().Edit(webPage.Id, webPage);
                }
                else
                {
                    webPage.IsActive = true;
                    new WebPageApi().Edit(webPage.Id, webPage);
                }
                return Json(new
                {
                    success = true
                });
            }
            catch
            {
                return Json(new
                {
                    success = false
                });
            }
        }

        public async Task<ActionResult> Edit(int? id)
        {
            var info = await new WebPageApi()
                .GetByStoreIdAsync(id.GetValueOrDefault(), this.CurrentStore.ID);

            if (info == null)
            {
                return this.IdNotFound();
            }

            var model = info;

            this.PrepareEdit(model);
            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public async Task<ActionResult> Edit(WebPageViewModel model)
        {
            var api = new WebPageApi();
            // Validate
            var info = await api
                .GetByStoreIdAsync(model.Id, this.CurrentStore.ID);

            if (info == null)
            {
                return this.IdNotFound();
            }

            if (!this.ModelState.IsValid)
            {
                this.PrepareEdit(model);
                return this.View(model);
            }

            model.StoreId = this.CurrentStore.ID;
            model.IsActive = true;
            await api.EditAsync(model.Id, model);

            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }

        public void PrepareEdit(WebPageViewModel model)
        {
        }

    }
}