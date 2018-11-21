using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{

    public partial interface IInventoryTemplateReportService
    {
        IQueryable<InventoryTemplateReport> GetBrandActiveTemplate(int brandId);
    }

    public partial class InventoryTemplateReportService
    {
        public IQueryable<InventoryTemplateReport> GetBrandActiveTemplate(int brandId)
        {
            return GetActive(q => q.BrandId == brandId);
        }

    }
}