using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class InventoryTemplateReportEditViewModel : InventoryTemplateReportViewModel
    {
        public InventoryTemplateReportEditViewModel() { }
        public InventoryTemplateReportEditViewModel(HmsService.Models.Entities.InventoryTemplateReport entity) : base(entity) { }
        public InventoryTemplateReportEditViewModel(IEnumerable<InventoryTemplateReportViewModel> original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public InventoryTemplateReportEditViewModel(InventoryTemplateReportViewModel original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public IEnumerable<TemplateReportProductItemMappingEditViewModel> TemplateReportProductItemMappings { get; set; }
    }
}
