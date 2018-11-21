using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.CostManager
{
    public class CostManagerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "CostManager";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "CostManager_default",
                "{brandId}/CostManager/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Wisky.SkyAdmin.Manage.Areas.CostManager.Controllers", }
            );
        }
    }
}