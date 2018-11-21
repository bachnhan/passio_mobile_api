using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class TokenUserApi
    {
        public bool UpdateUserToken(string username, string token)
        {
            var result = this.BaseService.UpdateUserToken(username, token);
            return result;
        }
    }
}
