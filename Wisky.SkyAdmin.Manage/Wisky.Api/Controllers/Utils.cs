using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Wisky.Api.Controllers
{
    public static class Utils
    {
        private static string _accessToken = System.Configuration.ConfigurationManager.AppSettings["config.order.token"];
        private static string _enableToken = System.Configuration.ConfigurationManager.AppSettings["config.order.EnableToken"];
        public static void CheckToken(string token)
        {
            if (token != _accessToken)
            {
                throw new Exception("Invalid token!!1");
            }
        }

        public static TResult Service<TResult>(this ApiController controller)
        {
            return DependencyResolver.Current.GetService<TResult>();
        }
    }

    public class JsonContent : HttpContent
    {
        private readonly MemoryStream _Stream = new MemoryStream();
        public JsonContent(object value)
        {

            Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var jw = new JsonTextWriter(new StreamWriter(_Stream));
            jw.Formatting = Formatting.Indented;
            var serializer = new JsonSerializer();
            serializer.Serialize(jw, value);
            jw.Flush();
            _Stream.Position = 0;

        }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return _Stream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _Stream.Length;
            return true;
        }
    }
}