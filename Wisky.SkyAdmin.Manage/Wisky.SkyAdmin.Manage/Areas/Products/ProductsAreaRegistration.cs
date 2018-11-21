using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.Products
{
    public class ProductsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Products";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Products_default",
                "{brandId}/Products/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Wisky.SkyAdmin.Manage.Areas.Products.Controllers", }
            );
        }
    }
}