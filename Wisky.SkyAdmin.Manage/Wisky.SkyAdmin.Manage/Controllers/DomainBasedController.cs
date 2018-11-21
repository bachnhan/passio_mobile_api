using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Controllers
{
    public class DomainBasedController : AuthenticationBasedController
    {
        private const string DefaultStoreName = "System";

        private StoreDomainViewModel currentPageDomainField;
        private string actualParameters;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.actualParameters = "";
            if (filterContext.ActionParameters.ContainsKey("parameters"))
            {
                this.actualParameters = filterContext.ActionParameters["parameters"] as string;
            }
            if (this.CurrentPageDomain == null)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid Hostname");
                return;
            }
            filterContext.ActionParameters["parameters"] = this.actualParameters;
            ViewBag.CurrentStore = CurrentStore;
            ViewBag.CurrentDomain = CurrentPageDomain;
            var directory = "";
            if (!string.IsNullOrEmpty(CurrentPageDomain.Directory))
            {
                directory = CurrentPageDomain.Directory + "/";
            }
            ViewBag.Directory = "/" + directory;
            base.OnActionExecuting(filterContext);
        }

        public StoreViewModel CurrentStore
        {
            // Get StoreStatus in Web.config
            get
            {

                if (WebConfigurationManager.AppSettings["StoreStatus"] == "true")
                {
                    return this.CurrentPageDomain?.StoreInfo;
                }
                else
                {
                    if (this.RouteData.Values["storeId"] != null)
                    {
                        var storeId = int.Parse(this.RouteData.Values["storeId"]?.ToString());
                        if (storeId != 0)
                        {
                            return Utils.GetStore(storeId).ToViewModel<Store, StoreViewModel>();
                        }
                    }
                    return this.CurrentPageDomain?.StoreInfo;
                }
            }
        }

        public StoreDomainViewModel CurrentPageDomain
        {
            get
            {
                if (this.currentPageDomainField == null)
                {
                    var parameters = this.RouteData.Values["parameters"]?.ToString();
                    string directory = null;

                    if (parameters != null)
                    {
                        var split = parameters.Split('/');

                        if (split.Length > 0)
                        {
                            directory = split[0];
                        }
                    }

                    this.currentPageDomainField = new StoreDomainApi()
                        .Get(this.Request.Url.Scheme, this.Request.Url.Host, this.Request.Url.Port, directory);
                    if (directory != null
                        && this.currentPageDomainField.Directory?.ToLowerInvariant() == directory.ToLowerInvariant())
                    {
                        this.actualParameters = parameters.Substring(directory.Length);
                        if (this.actualParameters.Length > 0 && this.actualParameters[0] == '/')
                        {
                            this.actualParameters = this.actualParameters.Substring(1);
                        }
                        this.RouteData.Values["parameters"] = this.actualParameters;
                    }
                }

                return this.currentPageDomainField;
            }
        }

        public bool IsPageAdmin
        {
            get
            {
                var loggedInAccount = this.LoggedInAccount;

                return loggedInAccount != null;
                //return loggedInAccount != null &&
                //    loggedInAccount.Id.ToLower() == this.CurrentStore.AdminAccountId.ToLower();
            }
        }

    }
}