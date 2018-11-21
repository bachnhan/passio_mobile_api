using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wisky.SkyAdmin.Manage.Areas.DashBoard.Models
{
    public class DashboardInfo
    {
        public double TotalAmount { get; set; }
        public double FinalAmount { get; set; }
        public double TotalDiscount { get; set; }
        public double TotalCancel { get; set; }
        public double TotalPreCancel { get; set; }
        public double TotalOrderCancel { get; set; }
    }
}