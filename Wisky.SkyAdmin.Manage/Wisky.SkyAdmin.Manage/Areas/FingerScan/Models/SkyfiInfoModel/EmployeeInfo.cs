using System.Collections.Generic;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan.Models.SkyfiInfoModel
{
    public class EmployeeInfo
    {
        public EmployeeInfo()
        {
            Fingers = new List<FingerInfo>();
        }

        public int MachineNumber { get; set; }
        public string EnrollNumber { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Privelage { get; set; }
        public bool Enabled { get; set; }
        public List<FingerInfo> Fingers { get; set; }

    }
}
