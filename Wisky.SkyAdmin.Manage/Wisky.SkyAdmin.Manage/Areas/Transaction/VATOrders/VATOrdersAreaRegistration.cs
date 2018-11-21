using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.VATOrders
{
    public class VATOrdersAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "VATOrders";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "VATOrders_default",
                "{brandId}/VATOrders/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Wisky.SkyAdmin.Manage.Areas.VATOrders.Controllers", }
            );
        }
    }
}