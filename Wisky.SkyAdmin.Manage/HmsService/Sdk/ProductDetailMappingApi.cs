using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.ViewModels;
using HmsService.Models.Entities;
using AutoMapper.QueryableExtensions;

namespace HmsService.Sdk
{
    public partial class ProductDetailMappingApi
    {
        public double GetPriceByStore(int storeId, int productId)
        {
            return this.BaseService.GetPriceByStore(storeId, productId);
        }
        public double GetDiscountByStore(int storeId, int productId)
        {
            return this.BaseService.GetDiscountByStore(storeId, productId);
        }

        public ProductDetailMappingViewModel GetProductDetailByStore(int storeId, int productId)
        {
            var entity = this.BaseService.GetProductDetailByStore(storeId, productId);

            if (entity != null)
            {
                return new ProductDetailMappingViewModel(entity);
            }
            else
            {
                return null;
            }
        }

        public bool CheckProductDetailByStore(int storeId, int productId)
        {
            var entity = this.BaseService.GetProductDetailByStore(storeId, productId);

            if (entity != null)
            {
                return entity.Active.Value;
            }
            else
            {
                return false;
            }
        }

        public int GetProductDetailIdByStore(int storeId, int productId)
        {
            var entity = this.BaseService.GetProductDetailByStore(storeId, productId);

            if (entity != null)
            {
                return entity.ProductDetailID;
            }
            else
            {
                return -1;
            }
        }


        public void CleanUnuseProductDetail()
        {
            var pds = this.BaseService.Get(a => !a.ProductID.HasValue).ToList();
            foreach (var productDetail in pds)
            {
                this.BaseService.Delete(productDetail);
            }
            this.BaseService.Save();
        }

        public async Task UpdateProductDetail(ProductDetailMappingViewModel model)
        {
            await this.BaseService.UpdateAsync(model.ToEntity());
        }

        public async Task CreateProductDetail(ProductDetailMappingViewModel model)
        {
            await this.BaseService.CreateAsync(model.ToEntity());
        }

        public IEnumerable<ProductDetailMapping> GetProductByStore(int storeId)
        {
            return this.BaseService.GetProductByStore(storeId).ToList();
        }
        public IEnumerable<ProductDetailMapping> GetProductByStoreId(int storeId)
        {
            var result = this.BaseService.GetProductByStore(storeId)
                                 .Where(q => q.Product.ProductCategory.IsExtra == false
                                  && q.Product.ProductCategory.IsDisplayed
                                  && q.Product.GeneralProductId == null
                                  ).ToList();
            return result;
        }
        public IEnumerable<ProductDetailMapping> GetProductByStoreID(int storeId, int brandId)
        {
           
            #region join product with productmaping
            var productApi = new ProductApi();
            var table1 = productApi.GetAllProductsByBrand(brandId);
            var table2 = this.BaseService.Get(a => a.StoreID == storeId && a.Active == true);
            var join = from t1 in table1
                       join t2 in table2
                       on t1.ProductID equals t2.ProductID
                       select new ProductDetailMapping
                       {
                           ProductDetailID = t2.ProductDetailID,
                           ProductID = t2.ProductID,
                           StoreID = t2.StoreID,
                           Price = t2.Price,
                           DiscountPrice = t2.DiscountPrice,
                           DiscountPercent = t2.DiscountPercent,
                           Active = t2.Active,
                           Product = t2.Product,
                           Store = t2.Store
                       };
            return join;
            #endregion
        }
    }
}
