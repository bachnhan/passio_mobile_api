using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class IndexGeneralViewModel
    {
        public ImageCollectionDetailsViewModel MainSlider { get; set; }
        public ImageCollectionDetailsViewModel Features { get; set; }
        public ImageCollectionDetailsViewModel PartnerLogos { get; set; }
        public ImageCollectionDetailsViewModel Gallery { get; set; }

        public IEnumerable<ProductViewModel> AllProducts { get; set; }
        public IEnumerable<ProductCategoryDetailsViewModel> AllCategories { get; set; }

        public WebPageViewModel PageIntroduction { get; set; }
    }
}
