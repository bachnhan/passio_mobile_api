using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Models
{
    public static class Utils
    {

        public const string AdminAuthorizeRoles = "Administrator,SysAdmin";
        public const string SysAdminAuthorizeRoles = "SysAdmin";

        public static bool HasRequiredAttribute(this PropertyInfo property)
        {
            return property.IsDefined(typeof(RequiredAttribute), true);
        }
        public static string ResolveServerUrl(string serverUrl, bool forceHttps)
        {
            if (serverUrl.IndexOf("://") > -1)
                return serverUrl;

            string newUrl = serverUrl;
            Uri originalUri = System.Web.HttpContext.Current.Request.Url;
            newUrl = (forceHttps ? "https" : originalUri.Scheme) +
                "://" + originalUri.Authority + newUrl;
            return newUrl;
        }

        public static MvcHtmlString RenderHtmlAttributes(KeyValuePair<string, string>[] values)
        {
            if (values == null)
            {
                return null;
            }

            var result = new StringBuilder();

            foreach (var value in values)
            {
                result.AppendFormat("{0}=\"{1}\"", value.Key, value.Value);
            }

            return new MvcHtmlString(result.ToString());
        }

        public static MvcHtmlString RenderHtmlAttributes(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            var result = new StringBuilder();

            foreach (var property in properties)
            {
                result.AppendFormat("{0}=\"{1}\"", property.Name, property.GetValue(obj));
            }

            return new MvcHtmlString(result.ToString());
        }

        public static void SetMessage(this Controller controller, string message)
        {
            controller.ViewData["Message"] = message;
        }

    }

}