using AutoMapper;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Wisky.SkyAdmin.Manage.Automation;

namespace Wisky.SkyAdmin.Manage
{
    public class MvcApplication : System.Web.HttpApplication
    {


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            HmsService.ApiEndpoint.Entry(this.AdditionalMapperConfig);
            //this.Session_Start();
            //Bootstrapper.Run();

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings =
          new JsonSerializerSettings
          {
              DateFormatHandling = DateFormatHandling.IsoDateFormat,
              DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
              Culture = CultureInfo.GetCultureInfo("vi-VN")
          };

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("vi-VN");

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            this.Session_Start();
        }



        private static bool autoReportLoaded = false;
        public void Session_Start()
        {

            if (!autoReportLoaded)
            {
                autoReportLoaded = true;

                var isAuto = ConfigurationManager.AppSettings["AutoDateReport"];
                if (isAuto.Equals("true"))
                {


                    var url = new UrlHelper(HttpContext.Current.Request.RequestContext, RouteTable.Routes)
                                 .Action("AutoReport",
                                         "Home",
                                         new { token = "bdiuwqBIUWBIQ(98120912NDW" });
                    var urlHelper = new UrlHelper(this.Request.RequestContext);
                    //UrlHelper helper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), RouteTable.Routes.GetRouteData(new HttpContextWrapper(HttpContext.Current))));


                    var dateReport = new DateReportExecuter(urlHelper.Action("AutoReport", "Home", new { token = "bdiuwqBIUWBIQ(98120912NDW" }, this.Request.Url.Scheme));
                    dateReport.Start();
                    //var t = new Thread(dateReport.Start);
                    //t.Start();
                }
            }


        }

        public void AdditionalMapperConfig(IMapperConfiguration config)
        {
            config.CreateMap<ProductViewModel, ProductViewModel>();
            config.CreateMap<ProductDetailsViewModel, ProductDetailsViewModel>();
            config.CreateMap<ProductViewModel, ProductDetailsViewModel>();
            config.CreateMap<ProductEditViewModel, ProductViewModel>();
            //config.CreateMap<ProductEditViewModel, ProductViewModel>();
            config.CreateMap<ProductImageCollectionDetailsViewModel, ProductImageCollectionDetailsViewModel>();
            config.CreateMap<ImageCollectionViewModel, ImageCollectionViewModel>();
            config.CreateMap<OrderViewModel, OrderViewModel>();

            config.CreateMap<BlogPostViewModel, BlogPostViewModel>();

            config.CreateMap<StoreUser, StoreUserViewModel>();
            config.CreateMap<StoreUserViewModel, StoreUser>();

            //config.CreateMap<BlogPostEditViewModel, BlogPostViewModel>();

            //config.CreateMap<AspNetUserDetailsViewModel, AspNetUserEditViewModel>();
            config.CreateMap<ProductCategory, HmsService.ViewModels.ProductCategoryEditViewModel>()
                .ForMember(q => q.Products, opt => opt.MapFrom(q => q.Products));

            
        }
    }
}
