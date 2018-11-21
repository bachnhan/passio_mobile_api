using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.MembershipCard
{
    public class MembershipCardAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "MembershipCard";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "MembershipCard_default",
                "{brandId}/MembershipCard/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Wisky.SkyAdmin.Manage.Areas.MembershipCard.Controllers", });
            
        }
    }
}