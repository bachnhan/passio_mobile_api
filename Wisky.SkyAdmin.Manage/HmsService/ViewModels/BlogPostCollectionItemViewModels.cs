using HmsService.Models.Entities;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{

    public partial class BlogPostCollectionItemMappingViewModel
    {
    }

    public partial class BlogPostCollectionItemWithBlogPostViewModel : BaseEntityViewModel<BlogPostCollectionItemMapping>
    {

        public BlogPostViewModel BlogPost { get; set; }

        public BlogPostCollectionItemWithBlogPostViewModel() : base() { }
        public BlogPostCollectionItemWithBlogPostViewModel(BlogPostCollectionItemMapping entity) : base(entity) { }

    }

}
