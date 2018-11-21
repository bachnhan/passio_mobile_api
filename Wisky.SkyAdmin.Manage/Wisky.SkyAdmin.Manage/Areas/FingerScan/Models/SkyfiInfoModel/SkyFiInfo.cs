using System;
using System.Collections.Generic;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan.Models.SkyfiInfoModel
{
    public class SkyFiInfo
    {
        public SkyFiInfo()
        {
            this.Employees = new List<EmployeeInfo>();
        }

        //public SkyFiInfo(Machine machine)
        //{
        //    this.BrandId = Int32.Parse(machine.BrandId);
        //    this.StoreId = Int32.Parse(machine.StoreId);
        //    this.MachineId = Int32.Parse(machine.DeviceId);
        //    this.MachineNumber = Int32.Parse(machine.DeviceUsername);
        //    this.MachineSerial = machine.DeviceSerial;

        //    this.Employees = new List<EmployeeInfo>();
        //}

        public int BrandId { get; set; }
        public int StoreId { get; set; }
        public int MachineId { get; set; }
        public string MachineSerial { get; set; }
        public int MachineNumber { get; set; }

        public List<EmployeeInfo> Employees { get; set; }
    }
}
