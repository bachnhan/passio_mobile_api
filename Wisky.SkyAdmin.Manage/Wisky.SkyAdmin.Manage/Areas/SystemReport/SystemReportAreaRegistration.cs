using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.SystemReport
{
    public class SystemReportAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "SystemReport";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "SystemReport_default",
                "{brandId}/SystemReport/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}