
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using Wisky.SkyAdmin.Manage.Models;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{

    //[Authorize(Roles = Utils.AdminAuthorizeRoles)]
    public class WebSettingsController : DomainBasedController
    {

        public ActionResult Index(int storeId)
        {
            var model =  new StoreWebSettingApi()
                .GetActiveByStore(storeId);

            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken(), ValidateInput(false)]
        public async Task<ActionResult> Index(StoreWebSettingsEditViewModel model, int storeId)
        {
            if (model != null)
            {
                var settings = model.Pairs.Select(q => new KeyValuePair<int, string>(q.Id, q.Value));

                try
                {
                    await new StoreWebSettingApi().MassUpdate(settings, storeId);
                }
                catch (UnauthorizedAccessException)
                {
                    // This exception is thrown when something is not right:
                    // this store is trying to modify value of other store.
                    return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                }

            }

            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }

    }
}