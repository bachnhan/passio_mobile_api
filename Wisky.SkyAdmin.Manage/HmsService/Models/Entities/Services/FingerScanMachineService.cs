using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IFingerScanMachineService
    {
        FingerScanMachine GetMachineByIp(string ip);
    }
    public partial class FingerScanMachineService
    {
        public FingerScanMachine GetMachineByIp(string ip)
        {
            return this.FirstOrDefault(q => q.Ip == ip);
        }
    }
}
