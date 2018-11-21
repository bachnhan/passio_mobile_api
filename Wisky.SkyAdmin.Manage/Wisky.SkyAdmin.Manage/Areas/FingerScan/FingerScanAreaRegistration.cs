using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan
{
    public class FingerScanAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "FingerScan";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "FingerScan_default",
                "{brandId}/FingerScan/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}