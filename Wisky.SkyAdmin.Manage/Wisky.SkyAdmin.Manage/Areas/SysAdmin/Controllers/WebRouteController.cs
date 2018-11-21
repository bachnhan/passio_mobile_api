using HmsService.Sdk;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyUp.Website.Models;
using Wisky.SkyAdmin.Manage.Models;
using HmsService.Models;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{ 
    //[Authorize(Roles = Utils.SysAdminAuthorizeRoles)]
    public class WebRouteController : DomainBasedController
    {

        public async Task<ActionResult> Store(int? id)
        {
            var storeApi = new StoreApi();
            var store = await storeApi.GetAsync(id);

            if (store == null || store.isAvailable != true)
            {
                return this.IdNotFound();
            }
            
            return this.View(model: id.Value);
        }

        [HttpPost]
        public async Task<ActionResult> StoreRoutes(int? id)
        {
            var storeApi = new StoreApi();
            var store = await storeApi.GetAsync(id);

            if (store == null || store.isAvailable != true)
            {
                return this.IdNotFound();
            }

            var webRouteApi = new StoreWebRouteApi();
            var model = await webRouteApi.GetStoreRoutesAsync(id.Value);

            return this.Json(model);
        }

    }
}