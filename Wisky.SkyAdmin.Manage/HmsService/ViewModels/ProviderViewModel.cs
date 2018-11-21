using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public partial class ProviderViewModel
    {
        public IEnumerable<ProviderViewModel> ProviderList { get; set; }
        public double Total { get; set; }
    }
}
