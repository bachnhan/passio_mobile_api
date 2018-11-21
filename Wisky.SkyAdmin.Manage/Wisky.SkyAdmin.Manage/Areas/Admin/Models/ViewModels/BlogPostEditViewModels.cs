using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public partial class BlogPostEditViewModel : BlogPostDetailsViewModel
    {

        public string[] SelectedImages { get; set; }

        public int[] SelectedBlogPostCollections { get; set; }
        public IEnumerable<SelectListItem> AvailableCollections { get; set; }

        public BlogPostEditViewModel() : base() { }

        public BlogPostEditViewModel(BlogPostViewModel original, IMapper mapper) : this()
        {
            mapper.Map(original, this);
        }

        public BlogPostEditViewModel(BlogPostDetailsViewModel original, IMapper mapper) : this()
        {
            mapper.Map(original, this);
            this.SelectedBlogPostCollections = original.BlogPostCollections.Select(q => q.Id).ToArray();
        }

    }
}