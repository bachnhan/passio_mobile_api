using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.WebElement
{
    public class WebElementAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "WebElement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "WebElement_default",
                //"WebElement/{controller}/{action}/{id}",
                "{brandId}/WebElement/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }              
            );
        }
    }
}