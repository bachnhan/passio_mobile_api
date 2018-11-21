using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.MembershipType
{
    public class MembershipTypeAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "MembershipType";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                 "MembershipType_default",
                 "{brandId}/MembershipType/{storeId}/{controller}/{action}/{id}",
                 new { action = "Index", id = UrlParameter.Optional },
                 namespaces: new string[] { "Wisky.SkyAdmin.Manage.Areas.MembershipType.Controllers", }
            );
        }
    }
}