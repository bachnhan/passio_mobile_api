using AutoMapper;
using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public partial class BlogPostCollectionViewModel
    {
    }

    public partial class BlogPostCollectionEditViewModel : BlogPostCollectionViewModel
    {
        public IEnumerable<SelectListItem> AvailableBlogCollections { get; set; }
        [Required]
        public override string Name
        {
            get
            {
                return base.Name;
            }

            set
            {
                base.Name = value;
            }
        }
       

        [Required]
        public override string SeoName
        {
            get
            {
                return base.SeoName;
            }

            set
            {
                base.SeoName = value;
            }
        }

        public BlogPostCollectionEditViewModel() : base() { }

        public BlogPostCollectionEditViewModel(BlogPostCollectionViewModel original, IMapper mapper) : this()
        {
            mapper.Map(original, this);
        }
    }

    public partial class BlogPostCollectionDetailsViewModel
    {

        public BlogPostCollectionViewModel BlogPostCollection { get; set; }
        public int PostCount { get; set; }

    }

    public partial class BlogPostCollectionWithPostsViewModel : BlogPostCollectionViewModel
    {
        
        public IEnumerable<BlogPostCollectionItemWithBlogPostViewModel> BlogPostCollectionItems { get; set; }

        public BlogPostCollectionWithPostsViewModel() : base() { }
        public BlogPostCollectionWithPostsViewModel(BlogPostCollection entity) : base(entity) { }

    }

}
