using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.StoreReport
{
    public class StoreReportAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "StoreReport";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "StoreReport_default",
                "{brandId}/StoreReport/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}