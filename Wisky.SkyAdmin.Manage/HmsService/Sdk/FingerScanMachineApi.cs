using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class FingerScanMachineApi
    {
        public FingerScanMachineViewModel GetMachine(string ip)
        {
            var entity = this.BaseService.GetMachineByIp(ip);
            if (entity == null) return null;
            return new FingerScanMachineViewModel(entity);
        }
    }
}
