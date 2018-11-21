using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    using AutoMapper.QueryableExtensions;
    using System;
    using System.Collections.Generic;
    using ViewModels;

    public partial class InventoryTemplateReportApi
    {
        public IEnumerable<InventoryTemplateReportViewModel> GetBrandActiveTemplate(int brandId)
        {
            return BaseService.GetBrandActiveTemplate(brandId)
                            .ProjectTo<InventoryTemplateReportViewModel>(AutoMapperConfig).ToList();
        }
        public InventoryTemplateReportEditViewModel GetTemplateEditViewModel(int templateId)
        {
            var entity = BaseService.Get(templateId);
            var mappings = entity.TemplateReportProductItemMappings.Select(q => new TemplateReportProductItemMappingEditViewModel
            {
                Id = q.Id,
                InventoryTemplateReportId = q.InventoryTemplateReportId,
                ProductItemId = q.ProductItemId,
                MappingIndex = q.MappingIndex,
                ItemName = q.ProductItem.ItemName,
                ItemCategory = q.ProductItem.ProductItemCategory.CateName
            });
            var model = new InventoryTemplateReportEditViewModel(entity);
            model.TemplateReportProductItemMappings = mappings;
            return model;       
        }
    }
}
