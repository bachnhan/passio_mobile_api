using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.ViewModels;
using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;

namespace HmsService.Sdk
{
    public partial class ProductItemCompositionMappingApi
    {
        #region StoreReport
        public IEnumerable<ProductItemCompositionMapping> GetProductItemByProductID(int productID)
        {
            var productItem = this.BaseService.GetProductItemByProductID(productID)
               // .ProjectTo<CompositionViewModel>(this.AutoMapperConfig)
                .ToList();
            return productItem;
        }
        #endregion
        public async Task DeleteProductItemCompositionMapping(int productId, int itemId)
        {
            await this.BaseService.DeleteItemCompositionMapping(productId, itemId);
        }
        public async Task CreateProductItemCompositionMapping(ProductItemCompositionMappingViewModel model)
        {
            await this.BaseService.CreateItemCompositionMapping(model.ToEntity());
        }
        public async Task UpdateProductItemCompositionMapping(ProductItemCompositionMappingViewModel model)
        {
            var composition = this.BaseService.GetItem(model.ProducID, model.ItemID);
            composition.Quantity = model.Quantity;
            await this.BaseService.UpdateItemCompositionMapping(composition);
        }
        public ProductItemCompositionMappingViewModel GetItem(int productId, int itemId)
        {
            var composition = this.BaseService.GetItem(productId, itemId);
            if(composition == null)
            {
                return null;
            }
            else
            {
                return new ProductItemCompositionMappingViewModel(composition);
            }
        }

    }
}
