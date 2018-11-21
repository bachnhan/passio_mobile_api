using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.Orders
{
    public class OrdersAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Orders";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Orders_default",
                "{brandId}/Orders/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
                //namespaces: new string[] { "Wisky.SkyAdmin.Manage.Areas.Orders.Controllers", }
            );
        }
    }
}