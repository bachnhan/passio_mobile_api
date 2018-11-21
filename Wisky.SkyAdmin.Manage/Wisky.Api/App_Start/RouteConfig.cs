using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Wisky.Api
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index" , id = UrlParameter.Optional},
                namespaces: new[] { "Wisky.Api.Controllers" }   
            );

            //routes.MapRoute(
            //    "Default",
            //    "{controller}/{action}/{id}",
            //    new { controller = "Home", action = "Index", id = UrlParameter.Optional },
            //    namespaces: new[] { "Wisky.Api.Controllers" }
            //);

            //routes.MapRoute(
            //    "Api",
            //    "api/{controller}/{action}/{id}",
            //    new { controller = "Home", action = "Index", id = UrlParameter.Optional },
            //    namespaces: new[] { "Wisky.Api.Controllers.API" }
            //);
        }
    }
}
