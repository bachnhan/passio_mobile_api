using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class StoreComparisonReportViewModel
    {
        public Store Store { get; set; }
        public string Date { get; set; }
        public double TotalAmount { get; set; }
        public double FinalAmount { get; set; }
    }
}
