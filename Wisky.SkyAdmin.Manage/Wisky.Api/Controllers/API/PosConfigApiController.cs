using HmsService.Sdk;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Wisky.Api.Controllers.API
{
    public class PosConfigApiController : ApiController
    {
        [HttpGet]
        [Route("api/posConfig/GetPosConfigFromFile/{token}/{terminalId}")]
        public HttpResponseMessage GetPosConfigFromFile(string token, int terminalId, string fileName, string version)
        {
            Utils.CheckToken(token);
            var posConfigFileApi = new PosFileApi();
            var posFile = posConfigFileApi.GetActivePosFileByNameAndStore(fileName, terminalId);
            if(posFile == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new JsonContent(new
                    {
                        Message = "PosFile is unavailable."
                    })
                };
            }
            if (version.CompareTo(posFile.Version) < 0) {
                var posConfigs = posFile.PosConfigs;
                return new HttpResponseMessage()
                {
                    Content = new JsonContent(new
                    {
                        PosConfigs = posConfigs,
                        Version = posFile.Version
                    })
                };
            }
            else
            {
                return new HttpResponseMessage()
                {
                    Content = new JsonContent(new
                    {
                        Message = "PosFile is up to date."
                    })
                };
            }
        }
    }
}