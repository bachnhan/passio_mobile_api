using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AutoMapper;
using HmsService.Models;
using HmsService.ViewModels;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Models.ViewModels
{

    public class ProductCategoryEditViewModel : ProductCategoryViewModel
    {

        public IEnumerable<SelectListItem> AvailableCategories { get; set; }
        public ProductCategoryType CategoryTypes { get; set; }
        public IconCategoryEnum IconEnum { get; set; }
        public IEnumerable<SelectListItem> AvailableCategoryExtras { get; set; }
        public int[] SelectedProductCategoryExtras { get; set; }

        [Required]
        public override string CateName
        {
            get
            {
                return base.CateName;
            }

            set
            {
                base.CateName = value;
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

        public ProductCategoryEditViewModel() : base() { }

        public ProductCategoryEditViewModel(ProductCategoryViewModel original, IMapper mapper) : this()
        {
            mapper.Map(original, this);
        }

    }
    
}