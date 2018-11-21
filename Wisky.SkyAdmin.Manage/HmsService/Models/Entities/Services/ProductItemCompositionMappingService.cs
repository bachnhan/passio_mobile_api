using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IProductItemCompositionMappingService
    {
        #region StoreReport
        IQueryable<ProductItemCompositionMapping> GetProductItemByProductID(int productID);
        #endregion

        ProductItemCompositionMapping GetItem(int productId, int itemId);
        Task DeleteItemCompositionMapping(int productId, int itemId);
        Task CreateItemCompositionMapping(ProductItemCompositionMapping entity);
        Task UpdateItemCompositionMapping(ProductItemCompositionMapping entity);        
    }
    public partial class ProductItemCompositionMappingService
    {
        #region StoreReport
        public IQueryable<ProductItemCompositionMapping> GetProductItemByProductID(int productID)
        {
            return this.Get(i => i.ProducID == productID);
        }
        #endregion

        public ProductItemCompositionMapping GetItem(int productId, int itemId)
        {
            return this.FirstOrDefault(q => q.ProducID == productId && q.ItemID == itemId);
        }
        public async Task DeleteItemCompositionMapping(int productId, int itemId)
        {
            var entity = this.GetItem(productId, itemId);
            await this.DeleteAsync(entity);

        }
        public async Task CreateItemCompositionMapping(ProductItemCompositionMapping entity)
        {            
            await this.CreateAsync(entity);

        }
        public async Task UpdateItemCompositionMapping(ProductItemCompositionMapping entity)
        {
            await this.UpdateAsync(entity);
        }        
    }
}
