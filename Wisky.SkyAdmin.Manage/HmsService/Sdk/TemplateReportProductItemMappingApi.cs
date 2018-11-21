using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class TemplateReportProductItemMappingApi
    {
        public IEnumerable<TemplateReportProductItemMappingViewModel> GetAllTemplateItems(int templateId)
        {
            return BaseService.Get(q => q.InventoryTemplateReportId == templateId)
                .ProjectTo<TemplateReportProductItemMappingViewModel>(AutoMapperConfig).ToList();
        }

        public IQueryable<TemplateReportProductItemMappingViewModel> GetQueryTemplateItems(int templateId)
        {
            return BaseService.Get(q => q.InventoryTemplateReportId == templateId)
                .ProjectTo<TemplateReportProductItemMappingViewModel>(AutoMapperConfig);
        }
    }
}
