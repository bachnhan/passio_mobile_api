using HmsService.Models.Entities.Services;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{

    public partial class BlogPostDetailsViewModel : BaseEntityViewModel<BlogPostDetails>
    {

        public BlogPostViewModel BlogPost { get; set; }
        public IEnumerable<BlogPostCollectionViewModel> BlogPostCollections { get; set; }
        public IEnumerable<BlogPostImageViewModel> BlogPostImages { get; set; }
        public virtual string ImageUrl { get; set; }
        public BlogPostDetailsViewModel() : base() { }
        public BlogPostDetailsViewModel(BlogPostDetails entity) : base(entity) { }

    }

}
