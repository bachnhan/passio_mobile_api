using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{

    public partial class ProductCollectionViewModel
    {

        public IEnumerable<ProductViewModel> Products { get; set; }
        public int ProductCollectionId { get; set; }
    }

}
