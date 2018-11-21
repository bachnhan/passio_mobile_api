using System.Threading.Tasks;
using System.Web.Mvc;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using Wisky.SkyAdmin.Manage.Models;
using Wisky.SkyAdmin.Manage.Controllers;
using HmsService.Models;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    [Authorize(Roles = Utils.AdminAuthorizeRoles)]
    public class CustomerFeedbackController : DomainBasedController
    {
        // GET: Admin/CustomerFeedback
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult IndexList(BootgridRequestViewModel request, int storeId)
        {
            var result = new WebCustomerFeedbackApi().GetAdminWithFilterAsync(
                storeId, request.searchPhrase,
                request.current, request.rowCount, request.FirstSortTerm);

            var model = new BootgridResponseViewModel<WebCustomerFeedbackViewModel>(result);
            return this.Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Detail(int? id, int storeId)
        {
            var customerFeedbackApi = new WebCustomerFeedbackApi();
            var info = await customerFeedbackApi
                .GetByStoreIdAsync(id.GetValueOrDefault(), storeId);

            if (info == null)
            {
                return this.IdNotFound();
            }
            
            return this.View("Detail", info);
        }

        public async Task<ActionResult> Delete(int? id, int storeId)
        {

            var customerFeedbackApi = new WebCustomerFeedbackApi();
            var info = await customerFeedbackApi
                .GetByStoreIdAsync(id.GetValueOrDefault(), storeId);

            if (info == null)
            {
                return this.IdNotFound();
            }

            await customerFeedbackApi.DeactivateAsync(id.Value);

            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }

    }
}