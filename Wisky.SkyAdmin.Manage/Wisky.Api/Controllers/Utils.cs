using Jose;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        #region Jose token
        private static string privateKey = System.Configuration.ConfigurationManager.AppSettings["config.privatekey"];
        private static byte[] secretKey = Encoding.UTF8.GetBytes(privateKey);
        public static string GenerateToken(string customerID)
        {
            var payload = customerID + ":" + DateTime.Now.Ticks.ToString();

            return JWT.Encode(payload, secretKey, JwsAlgorithm.HS256);
        }

        public static bool IsTokenValid(string token)
        {
            bool result = false;
            try
            {
                string key = JWT.Decode(token, secretKey);

                result = true;
            }
            catch
            {
            }
            return result;
        }

        public static string getIdFromToken(string token)
        {
            string key = JWT.Decode(token, secretKey);
            string[] parts = key.Split(new char[] { ':' });
            return parts[0];
        }
        #endregion
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