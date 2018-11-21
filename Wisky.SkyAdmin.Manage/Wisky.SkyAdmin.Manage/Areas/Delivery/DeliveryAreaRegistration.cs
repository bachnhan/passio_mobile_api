using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.Delivery
{
    public class DeliveryAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Delivery";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Delivery_default",
                "{brandId}/Delivery/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Wisky.SkyAdmin.Manage.Areas.Delivery.Controllers", }
            );
        }
    }
}