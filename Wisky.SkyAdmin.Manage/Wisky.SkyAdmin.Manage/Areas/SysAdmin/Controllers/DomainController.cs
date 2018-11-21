using HmsService.Sdk;
using HmsService.ViewModels;
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

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{

    //[Authorize(Roles = Utils.SysAdminAuthorizeRoles)]
    public class DomainController : BaseController
    {

        public ActionResult Store(int? id)
        {
            if (id == null)
            {
                return this.IdNotFound();
            }

            return this.View(model: id.Value);
        }

        public ActionResult StoreList(int? id, BootgridRequestViewModel request)
        {
            if (id == null)
            {
                return this.IdNotFound();
            }

            var result = new StoreDomainApi().GetAdminWithFilterAsync(id.Value,
                request.searchPhrase, request.current, request.rowCount, request.FirstSortTerm);

            var model = new BootgridResponseViewModel<StoreDomainViewModel>(result);
            return this.Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int? id, string protocol, string hostName, int? port)
        {
            var storeApi = new StoreApi();
            var store = await storeApi.GetAsync(id);

            if (store == null || store.isAvailable != true || string.IsNullOrWhiteSpace(protocol) || string.IsNullOrWhiteSpace(hostName) || port == null)
            {
                return this.IdNotFound();
            }

            var storeDomainApi = new StoreDomainApi();
            await storeDomainApi.CreateAsync(new StoreDomainViewModel()
            {
                StoreId = id.Value,
                Protocol = protocol,
                HostName = hostName,
                Port = port.Value,
                Active = true,
            });

            return this.RedirectToAction("Store", new { id = id.Value, });
        }

        public async Task<ActionResult> Delete(int? id)
        {
            var api = new StoreDomainApi();
            var model = await api.GetAsync(id);

            if (model == null || !model.Active)
            {
                return this.IdNotFound();
            }

            await api.DeactivateAsync(model.Id);

            return this.RedirectToAction("Store", new { id = model.StoreId, });
        }

    }
}