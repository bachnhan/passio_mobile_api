using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Models
{
    public class ProductReportViewModel
    {
        public List<ItemReport> ListItemReport { get; set; }
        public ProductReportViewModel()
        {
            this.ListItemReport = new List<ItemReport>();
        }
    }

    public class ItemReport
    {
        public string Title { get; set; }
        public int ColSpan { get; set; }
        public List<String> PromotionName { get; set; }

        public ItemReport()
        {
            this.PromotionName = new List<String>();
        }
    }
}