using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models
{
    public class OperationStatus
    {
        public bool IsSuccess { set; get; }
        public string ErrorMessage { set; get; }
        public string ErrorCode { set; get; }
        public string InnerException { set; get; }
    }
    public class BaseApi
    {
        public OperationStatus OperationStatus { set; get; }
        public BaseApi()
        {
            if (OperationStatus == null)
                OperationStatus = new OperationStatus();
        }
    }
}
