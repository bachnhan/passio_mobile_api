using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.ViewModels;

namespace HmsService.Models.Entities.Services
{
    public partial interface IProductDetailMappingService
    {
        double GetPriceByStore(int storeId, int productId);
        ProductDetailMapping GetProductDetailByStore(int storeId, int productId);
        IQueryable<ProductDetailMapping> GetProductByStore(int storeId);
        double GetDiscountByStore(int storeId, int productId);
    }
    public partial class ProductDetailMappingService
    {
        public double GetPriceByStore(int storeId, int productId)
        {
            var productDetail = this.Get(pd => pd.StoreID == storeId && pd.ProductID == productId).FirstOrDefault();

            if (productDetail != null)
            {
                return productDetail.Price.GetValueOrDefault();
            }

            return 0;
        }

        public ProductDetailMapping GetProductDetailByStore(int storeId, int productId)
        {
            var productDetail = this.Get(pd => pd.StoreID == storeId && pd.ProductID == productId).FirstOrDefault();

            if (productDetail != null)
            {
                return productDetail;
            }

            return null;
        }

        public IQueryable<ProductDetailMapping> GetProductByStore(int storeId)
        {
            var result = this.GetActive(q => q.StoreID == storeId && q.Active == true);
            return result;
        }

        public double GetDiscountByStore(int storeId, int productId)
        {
            var product = this.Get(a => a.ProductID == productId && a.StoreID == storeId).FirstOrDefault();

            if (product != null)
            {
                return product.DiscountPrice.GetValueOrDefault();
            }
            return 0;
        }
    }
}
