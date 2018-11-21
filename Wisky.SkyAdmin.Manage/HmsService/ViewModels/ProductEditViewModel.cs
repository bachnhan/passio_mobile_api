using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;

namespace HmsService.ViewModels
{

    public class ProductEditViewModel : ProductDetailsViewModel
    {

        [Required]
        public string ProductName
        {
            get { return base.Product.ProductName; }
            set { base.Product.ProductName = value; }
        }

        [Required]
        public string Code
        {
            get { return base.Product.Code; }
            set { base.Product.Code = value; }
        }

        [Required]
        public string Description
        {
            get { return base.Product.Description; }

            set { base.Product.Description = value; }
        }

        [Required]
        public string SeoName
        {
            get { return base.Product.SeoName; }
            set { base.Product.SeoName = value; }
        }

        public int CatID
        {
            get { return base.Product.CatID; }
            set { base.Product.CatID = value; }
        }

        public bool Active
        {
            get { return base.Product.Active; }
            set { base.Product.Active = value; }
        }

        public int ProductID
        {
            get { return base.Product.ProductID; }
            set { base.Product.ProductID = value; }
        }

        public string SeoKeyWords
        {
            get { return base.Product.SeoKeyWords; }
            set { base.Product.SeoKeyWords = value; }
        }
        public bool? IsAvailable
        {
            get { return base.Product.IsAvailable; }
            set { base.Product.IsAvailable = true; }
        }
        public IEnumerable<ProductEditSpecItem> Specifications { get; set; }

        public IEnumerable<SelectListItem> AvailableCategories { get; set; }
        public IEnumerable<SelectListItem> AvailableCollections { get; set; }

        public IEnumerable<string> SelectedImages { get; set; }

        public int[] SelectedProductCollections { get; set; }
        //public IEnumerable<ProductComboDetailViewModel> ProductComboDetails { get; set; }
        public bool? isComboChanged { get; set; }

        public KeyValuePair<string, string>[] SpecValues
        {
            get
            {
                return this.Specifications
                    .Where(q => !string.IsNullOrWhiteSpace(q.Name))
                    .Select(q => new KeyValuePair<string, string>(q.Name, q.Value))
                    .ToArray();
            }
        }

        public ProductEditViewModel() : base() { }

        public ProductEditViewModel(ProductDetailsViewModel original, IMapper mapper) : this()
        {
            mapper.Map(original, this);

            this.SelectedProductCollections = original.ProductCollections.Select(q => q.ProductCollectionId).ToArray();
        }

    }

    public class ProductEditSpecItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

}