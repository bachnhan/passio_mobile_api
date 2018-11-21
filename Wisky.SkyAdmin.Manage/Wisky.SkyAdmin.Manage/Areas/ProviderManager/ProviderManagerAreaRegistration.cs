using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.ProviderManager
{
    public class ProviderManagerAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ProviderManager";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ProviderManager_default",
                "{brandId}/ProviderManager/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Wisky.SkyAdmin.Manage.Areas.ProviderManager.Controllers", }
            );
        }
    }
}