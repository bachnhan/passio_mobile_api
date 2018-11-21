using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class TemplateReportProductItemMappingEditViewModel : TemplateReportProductItemMappingViewModel
    {
        public string ItemName { get; set; }
        public string ItemCategory { get; set; }
        
        public TemplateReportProductItemMappingEditViewModel(TemplateReportProductItemMappingViewModel original, IMapper mapper)
        {
            mapper.Map(original.ProductItem.ItemName, this.ItemName);
            mapper.Map(original.ProductItem.CateName, this.ItemCategory);
        }
        public TemplateReportProductItemMappingEditViewModel() : base() { }
        public TemplateReportProductItemMappingEditViewModel(HmsService.Models.Entities.TemplateReportProductItemMapping entity) : base(entity) { }
    }
}
