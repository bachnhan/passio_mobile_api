using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.Coupon
{
    public class CouponAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Coupon";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Coupon_default",
                "{brandId}/Coupon/{storeId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}