using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
   public class TempProductByCategoryViewModel
    {
        public string CategotyName { get; set; }
        public List<string> ListProductName { get; set; }
        public List<int> ListQuantity { get; set; }
    }
}
