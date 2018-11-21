using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public partial class PosFileViewModel
    {
        public IEnumerable<PosConfigViewModel> PosConfigs { get; set; }
    }
}