using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace HmsService.Filter
{
  public class StoreFilter : ActionFilterAttribute
    {
      //  public IStoreService StoreService { get; set; }
      
        public int StoreType { get; set; }

        public StoreFilter()
        {
            StoreType = -1;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)

        {
            var redirectTargetDictionary = new RouteValueDictionary();
            //var store = filterContext.HttpContext.Request.Cookies["storeId"];
            // int? store = (int?)filterContext.HttpContext.Session["storeId"];--PhuongLHK

            var a = filterContext.RouteData.Values["storeId"];

            //int store = (filterContext.RouteData.Values["storeId"]!= null)? int.Parse(filterContext.RouteData.Values["storeId"]):-1;
            int store = (filterContext.RouteData.Values["storeId"] != null) ? Convert.ToInt32(filterContext.RouteData.Values["storeId"]) : -1;


            // int.TryParse(, out store);



            var storeApi = new StoreApi();

            //if (store != "")
            if (store != -1)
            {
                if (StoreType != -1)
                {
                    var s = storeApi.GetStoreByIdSync(store);
                    if (s == null || s.Type != StoreType)
                    {
                        redirectTargetDictionary.Add("action", "ChooseStores");
                        redirectTargetDictionary.Add("controller", "Home");
                        filterContext.Result = new RedirectToRouteResult("Default", redirectTargetDictionary);
                        return;
                    }
                }
                base.OnActionExecuting(filterContext);
                return;
            }
            var stores = storeApi.GetStoresByUser(filterContext.HttpContext.User.Identity.Name).ToList();
            if (!stores.Any() || (StoreType != -1 && stores[0].Type != (int)StoreType))
            {
                redirectTargetDictionary.Add("action", "ChooseStores");
                redirectTargetDictionary.Add("controller", "Home");
                filterContext.Result = new RedirectToRouteResult("Default", redirectTargetDictionary);
                return;
            }
            else
            {
                //filterContext.HttpContext.Response
                //    .SetCookie(new HttpCookie("storeId", stores[0].ID.ToString(CultureInfo.InvariantCulture)));
                //filterContext.HttpContext.Response
                //    .SetCookie(new HttpCookie("storeName", stores[0].Name.ToString(CultureInfo.InvariantCulture)));
                filterContext.HttpContext.Session["storeId"] = stores[0].ID;
                filterContext.HttpContext.Session["storeType"] = stores[0].Type;
                filterContext.HttpContext.Session["storeName"] = stores[0].Name.ToString(CultureInfo.InvariantCulture);
                filterContext.HttpContext.Session["storeShortName"] = stores[0].ShortName.ToString();
                base.OnActionExecuting(filterContext);
            }
        }

    }
}

