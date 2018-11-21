using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.PosReport
{
    public class PosReportAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PosReport";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            //context.MapRoute(
            //    "PosReport_default",
            //    "PosReport/{controller}/{action}/{id}",
            //    new { action = "Index", id = UrlParameter.Optional }

            context.MapRoute(
                "PosReport_default",
                "{brandId}/PosReport/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Wisky.SkyAdmin.Manage.Areas.PosReport.Controllers", }
            );
        }
    }
}