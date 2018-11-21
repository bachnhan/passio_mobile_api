using SkyWeb.DatVM.Data;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.ViewModels;

namespace HmsService.Models.Entities.Services
{

    public partial interface IProductCollectionService
    {
        IQueryable<ProductCollection> GetByStoreId(int storeId);
        IQueryable<ProductCollection> GetByBrandId(int brandId);
        Task<ProductCollectionDetails> GetDetailsAsync(int id);
        Task<ProductCollectionDetails> GetDetailsByNameAsync(string name, int brandId);
    }

    public partial class ProductCollectionService
    {

        public IQueryable<ProductCollection> GetByStoreId(int storeId)
        {
            return this.GetActive(q => q.StoreId == storeId);
        }
        public IQueryable<ProductCollection> GetByBrandId(int brandId)
        {
            return this.GetActive(q => q.BrandId.Value == brandId);
        }

        public async Task<ProductCollectionDetails> GetDetailsAsync(int id)
        {
            var entity = await this.GetAsync(id);

            return new ProductCollectionDetails()
            {
                ProductCollection = entity,
                Items = entity.ProductCollectionItemMappings.AsQueryable()
                    .Where(q => q.Active && q.Product.IsAvailable)
                    .Include(q => q.Product)
                    .Include(q => q.Product.ProductCategory)
                    .Include(q => q.Product.ProductSpecifications)
                    .OrderBy(q => q.Position),
            };
        }

        public async Task<ProductCollectionDetails> GetDetailsByNameAsync(string name, int brandId)
        {
            var entity = await this.GetActive(q => q.Active == true && q.Name == name && q.BrandId == brandId)
                .FirstOrDefaultAsync();

            return new ProductCollectionDetails()
            {
                ProductCollection = entity,
                Items = entity.ProductCollectionItemMappings.AsQueryable()
                    .Where(q => q.Active && q.Product.IsAvailable)
                    .Include(q => q.Product)
                    .Include(q => q.Product.ProductCategory)
                    .Include(q => q.Product.ProductSpecifications)
                    .OrderBy(q => q.Position),
            };
        }
    }

    public class ProductCollectionDetails : IEntity
    {
        public ProductCollection ProductCollection { get; set; }
        public IQueryable<ProductCollectionItemMapping> Items { get; set; }
    }

}
