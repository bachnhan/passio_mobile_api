using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class ConfigMenuStoreViewModel
    {
        public IEnumerable<SelectedMenuItem> SelectedMenu { get; set; }
        public string FilterString { get; set; }
        public int BrandId { get; set; }
        public int[] storeArray { get; set; }
    }
}
