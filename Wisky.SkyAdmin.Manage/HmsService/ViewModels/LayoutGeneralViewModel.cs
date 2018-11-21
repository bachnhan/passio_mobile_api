using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class LayoutGeneralViewModel
    {
        public IEnumerable<StoreWebSettingViewModel> Settings { get; set; }
        public IEnumerable<ProductCategoryTreeViewModel> CategoryTree { get; set; }
        public BlogPostCollectionWithPostsViewModel News { get; set; }
        public BlogPostCollectionWithPostsViewModel Testimonials { get; set; }

        public ProductCollectionDetailsViewModel HotProducts { get; set; }
        public ProductCollectionDetailsViewModel PopularProducts { get; set; }
        public IEnumerable<BlogPostViewModel> BlogView { get; set; }
    }
}
