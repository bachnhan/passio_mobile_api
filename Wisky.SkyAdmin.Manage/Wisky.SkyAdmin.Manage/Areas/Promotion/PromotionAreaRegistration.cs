using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.Promotion
{
    public class PromotionAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Promotion";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Promotion_default",
                "{brandId}/Promotion/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}