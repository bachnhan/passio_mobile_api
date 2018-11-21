using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScanAtBrand
{
    public class FingerScanAtBrandAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "FingerScanAtBrand";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "FingerScanAtBrand_default",
                "{brandId}/FingerScanAtBrand/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}