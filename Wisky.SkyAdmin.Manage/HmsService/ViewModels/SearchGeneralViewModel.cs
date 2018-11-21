using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class SearchGeneralViewModel
    {
        public IEnumerable<ProductViewModel> MatchedProducts { get; set; }
        public ProductCollectionDetailsViewModel RecommendedProducts { get; set; }
    }
}
