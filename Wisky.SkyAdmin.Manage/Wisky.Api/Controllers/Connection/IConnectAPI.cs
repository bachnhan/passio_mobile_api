using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web.Mvc;



namespace Wisky.Api.Connection
{
    interface IConnectAPI
    {
        ResultObj ThirdPartyCardDetail(string code,int brandID,int storeID);
        ResultObj Payment(string code, int brandID, int storeID, decimal amount, string detail);
    }
}
