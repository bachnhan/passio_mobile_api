using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class BlogsGeneralViewModel
    {
        public BlogPostCollectionWithPostsViewModel Blogs { get; set; }
        public ImageCollectionDetailsViewModel Banner { get; set; }
        public List<BlogPostCollectionWithPostsViewModel> Categories { get; set; }
        public IEnumerable<BlogPostViewModel> BlogView { get; set; }
        public BlogPostViewModel BlogDetail { get; set; }
    }
}
