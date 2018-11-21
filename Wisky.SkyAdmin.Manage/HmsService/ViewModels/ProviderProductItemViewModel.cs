using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public partial class ProviderProductItemMappingViewModel
    {
        public ProviderViewModel Provider { get; set; }
        public ProductItemViewModel ProductItem { get; set; }
        public IEnumerable<ProviderProductItemMappingViewModel> ProviderProductItem { get; set; }
    }
}
