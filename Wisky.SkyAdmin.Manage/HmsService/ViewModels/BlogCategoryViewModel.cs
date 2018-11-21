using HmsService.Models.Entities;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    partial class BlogCategoryViewModel : BaseEntityViewModel<BlogCategory>
    {
        public IEnumerable<BlogCategoryViewModel> ChildCategory { get; set; }
        public bool selectedCate { get; set; }
        public IEnumerable<BlogPostImageViewModel> BlogPostImages { get; set; }
        public string[] SelectedImages { get; set; }
        public IEnumerable<SelectListItem> AvailableBlogCategories { get; set; }

    }
}
