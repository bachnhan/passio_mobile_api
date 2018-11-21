using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class StoreNavigationViewModel
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string StoreName { get; set; }
        public string User { get; set; }
        public IEnumerable<string> ListManager { get; set; }
    }
}
