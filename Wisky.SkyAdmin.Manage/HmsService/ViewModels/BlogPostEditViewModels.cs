using AutoMapper;
using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public partial class BlogPostEditViewModel : BlogPostDetailsViewModel
    {
        public IEnumerable<SelectListItem> AvailableBlogReference { get; set; }
        public IEnumerable<SelectListItem> AvailableLocation { get; set; }
        public IEnumerable<SelectListItem> AvailableBlogCategories { get; set; }
        public string[] SelectedImages { get; set; }
        public int[] SelectedTags { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int[] SelectedBlogPostCollections { get; set; }
        public IEnumerable<SelectListItem> AvailableCollections { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public BlogPostEditViewModel() : base() { }
        
        public BlogPostEditViewModel(BlogPostViewModel original, IMapper mapper) : this()
        {
            mapper.Map(original, this);
        }

        public BlogPostEditViewModel(BlogPostDetailsViewModel original, IMapper mapper) : this()
        {
            mapper.Map(original, this);
            this.EndDate = original.BlogPost.EndDate.HasValue ? original.BlogPost.EndDate.Value.ToString("HH:mm dd/MM/yyyy") : DateTime.Now.ToString("HH:mm dd/MM/yyyy");
            this.StartDate = original.BlogPost.StartDate.HasValue ? original.BlogPost.StartDate.Value.ToString("HH:mm dd/MM/yyyy") : DateTime.Now.ToString("HH:mm dd/MM/yyyy");
            this.SelectedBlogPostCollections = original.BlogPostCollections.Select(q => q.Id).ToArray();
        }

    }
}