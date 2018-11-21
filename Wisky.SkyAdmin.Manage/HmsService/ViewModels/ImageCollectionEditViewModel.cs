using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using SkyWeb.DatVM.Mvc;

namespace HmsService.ViewModels
{
    public class ProductImageCollectionDetailsViewModel : BaseEntityViewModel<ImageCollectionDetails>
    {
        public ProductImageCollectionViewModel ProductImageCollectionViewModel { get; set; }
        public IEnumerable<ProductImageCollectionItemMappingViewModel> Items { get; set; }
        public int NumberOfImage
        {
            get
            {
                return Items?.Count()??0;
            }
        }

        public string Name
        {
            get
            {
                return ProductImageCollectionViewModel.Name;
            }
        }
        public ProductImageCollectionDetailsViewModel() : base() { }
        public ProductImageCollectionDetailsViewModel(ImageCollectionDetails entity) : base(entity) { }


        public ProductImageCollectionDetailsViewModel(ProductImageCollectionDetailsViewModel original, IMapper mapper) : this()
        {
            mapper.Map(original, this);
        }
    }

    public class ImageCollectionDetailsViewModel : BaseEntityViewModel<ImageCollectionDetailsOfImages>
    {
        public ImageCollectionViewModel ImageCollection { get; set; }
        public IEnumerable<ImageCollectionItemViewModel> Items { get; set; }
        public int NumberOfImage
        {
            get
            {
                return Items?.Count() ?? 0;
            }
        }

        public string Name
        {
            get
            {
                return ImageCollection.Name;
            }
        }
        public ImageCollectionDetailsViewModel() : base() { }
        public ImageCollectionDetailsViewModel(ImageCollectionDetailsOfImages entity) : base(entity) { }


        public ImageCollectionDetailsViewModel(ImageCollectionDetailsViewModel original, IMapper mapper) : this()
        {
            mapper.Map(original, this);
        }
    }
}
