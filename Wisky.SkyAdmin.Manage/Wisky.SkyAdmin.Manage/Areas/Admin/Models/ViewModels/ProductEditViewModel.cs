using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using HmsService.ViewModels;
using HmsService.Models;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Models.ViewModels
{

    public class ProductEditViewModel : ProductDetailsViewModel
    {

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        public string ProductName
        {
            get { return base.Product.ProductName; }
            set { base.Product.ProductName = value; }
        }

        [Required(ErrorMessage = "Vui lòng nhập mã sản phẩm")]
        public string Code
        {
            get { return base.Product.Code; }
            set { base.Product.Code = value; }
        }

        //[Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm")]
        public string Description
        {
            get { return base.Product.Description; }

            set { base.Product.Description = value; }
        }

        [Required(ErrorMessage = "Vui lòng nhập SEOName")]
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
        public bool IsAvailable
        {
            get { return base.Product.IsAvailable; }
            set { base.Product.IsAvailable = value; }
        }

        public bool IsExtra
        {
            get;
            set;
        }
        public int SelectedType
        {
            get;
            set;
        }
        public IEnumerable<ProductEditSpecItem> Specifications { get; set; }
        public IEnumerable<SelectListItem> AvailableCategoriesMemberCard { get; set; }
        public IEnumerable<SelectListItem> AvailableCategories { get; set; }
        public IEnumerable<SelectListItem> AvailableCollections { get; set; }

        public IEnumerable<SelectListItem> AvailableComboProducts { get; set; }
        public IEnumerable<string> SelectedImages { get; set; }

        public int[] SelectedProductCollections { get; set; }

        public ProductColorGroup ProductColorGroup { get; set; }

        public ProductGroup ProductGroup { get; set; }
        public string Att1
        {
            get { return base.Product.Att1; }
            set { base.Product.Att1 = value; }
        }
        public string Att2
        {
            get { return base.Product.Att2; }
            set { base.Product.Att2 = value; }
        }
        public string Att3
        {
            get { return base.Product.Att3; }
            set { base.Product.Att3 = value; }
        }
        public string Att4
        {
            get { return base.Product.Att4; }
            set { base.Product.Att4 = value; }
        }
        public string Att5
        {
            get { return base.Product.Att5; }
            set { base.Product.Att5 = value; }
        }
        public string Att6
        {
            get { return base.Product.Att6; }
            set { base.Product.Att6 = value; }
        }
        public string Att7
        {
            get { return base.Product.Att7; }
            set { base.Product.Att7 = value; }
        }
        public string Att8
        {
            get { return base.Product.Att8; }
            set { base.Product.Att8 = value; }
        }
        public string Att9
        {
            get { return base.Product.Att9; }
            set { base.Product.Att9 = value; }
        }
        public string Att10
        {
            get { return base.Product.Att10; }
            set { base.Product.Att10 = value; }
        }
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