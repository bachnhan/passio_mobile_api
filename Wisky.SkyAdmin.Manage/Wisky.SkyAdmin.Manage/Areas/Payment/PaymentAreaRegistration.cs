using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.Payment
{
    public class PaymentAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Payment";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Payment_default",
                "{brandId}/Payment/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Wisky.SkyAdmin.Manage.Areas.Payment.Controllers", }
            );
        }
    }
}