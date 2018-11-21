using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System.Threading.Tasks;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using Wisky.SkyAdmin.Manage.Models;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    [Authorize(Roles = Utils.AdminAuthorizeRoles)]
    public class OrderController : DomainBasedController
    {
        // GET: Admin/Order
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult IndexList(BootgridRequestViewModel request, int storeId)
        {
            var result = new OrderApi().GetAdminWithFilterAsync(
                storeId, request.searchPhrase,
                request.current, request.rowCount, request.FirstSortTerm);

            var model = new BootgridResponseViewModel<OrderViewModel>(result);
            return this.Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult OrderDetail(int id, int storeId)
        {
            var result = new OrderApi().GetOrderById(storeId, id);
            if (result == null)
            {
                return Json(new
                {
                    success = false
                });
            }
            return Json(new
            {
                success = true,
                data = result
            });
        }
        public async Task<ActionResult> Delete(int? id, int storeId)
        {
            var orderApi = new OrderApi();

            var info = orderApi.GetOrderById(storeId, id.GetValueOrDefault());

            if (info == null)
            {
                return this.IdNotFound();
            }

            await orderApi.DeleteAsync(info.Order.RentID);
            //await orderApi.DeactivateAsync(info.Order.RentID);

            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }

        public ActionResult ChangeStatusOrder(int rentId, int orderStatus)
        {
            var orderApi = new OrderApi();
            var info = orderApi.Get(rentId);
            info.DeliveryStatus = orderStatus;
            orderApi.Edit(info.RentID, info);
            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }
    }
}