using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{

    public partial interface IStoreService
    {
        IQueryable<Store> GetAdminByStoreWithFilter(string keyword, KeyValuePair<string, bool> orderByProperty);
        IQueryable<Store> GetAdminByStoreOfBrandWithFilter(string keyword, KeyValuePair<string, bool> orderByProperty, int brandId);
        IQueryable<Store> GetAllStore(int brandId);
        IEnumerable<StoreUser> GetAllStoreUser();
        IEnumerable<Store> GetStoresToAssign();
        IQueryable<Store> GetStoreByBrandId(int brandId);
        IQueryable<Store> GetActiveStoreByBrandId(int brandId);
        IQueryable<Store> GetStoresByBrandIdAndType(int brandId, int type);
        System.Threading.Tasks.Task<IQueryable<StoreUser>> GetStoreUserByStoreIdAsync(int storeId);
        System.Threading.Tasks.Task DeActiveStore(int Id);
        Task CreateStoreAsync(Store entity, IEnumerable<Product> listProduct);
        #region SystemReport
        IQueryable<Store> GetStores();
        IQueryable<Store> GetStoresByBrand(int brandId);
        IQueryable<Store> GetStoresByPromotionId(int promotionId, int brandId);
        string GetStoreNameByID(int id);
        #endregion
        #region StoreReport
        Task<Store> GetStoreByID(int storeid);
        Store GetStoreById(int storeId);
        IQueryable<Store> GetStoresByUser(string username);
        IQueryable<Store> GetStoreByGroupId(int groupId);
        #endregion
    }

    public partial class StoreService
    {
      
        public IEnumerable<StoreUser> GetAllStoreUser()
        {
            return this.GetActive().Select(q => new StoreUser()
            {
                Username = "", //username lam sao lay ?
                Store = q,
            });
        }

        public IEnumerable<Store> GetStoresToAssign()
        {
            return this.GetActive().Where(a => a.isAvailable == true);
        }

        public async System.Threading.Tasks.Task DeActiveStore(int Id)
        {
            var entity = await this.GetAsync(Id);
            entity.isAvailable = false;
            await this.UpdateAsync(entity);
        }

        public IQueryable<Store> GetAdminByStoreWithFilter(string keyword, KeyValuePair<string, bool> orderByProperty)
        {
            var result = this.GetActive(q => q.isAvailable == true &&
                (keyword == null || q.Name.Contains(keyword) || q.ShortName.Contains(keyword)));

            StoreSortableProperty name;
            if (orderByProperty.Key != null && Enum.TryParse(orderByProperty.Key, out name))
            {
                switch (name)
                {
                    case StoreSortableProperty.Id:
                        result = result.OrderBy(q => q.ID, orderByProperty.Value);
                        break;
                    case StoreSortableProperty.Name:
                        result = result.OrderBy(q => q.Name, orderByProperty.Value);
                        break;
                    case StoreSortableProperty.ShortName:
                        result = result.OrderBy(q => q.ShortName, orderByProperty.Value);
                        break;
                    case StoreSortableProperty.HasNews:
                        result = result.OrderBy(q => q.HasNews, orderByProperty.Value);
                        break;
                    case StoreSortableProperty.HasProducts:
                        result = result.OrderBy(q => q.HasProducts, orderByProperty.Value);
                        break;
                    case StoreSortableProperty.HasImageCollections:
                        result = result.OrderBy(q => q.HasImageCollections, orderByProperty.Value);
                        break;
                }
            }
            else
            {
                result = result.OrderBy(q => q.ID);
            }

            return result;


        }

        public IQueryable<Store> GetAdminByStoreOfBrandWithFilter(string keyword, KeyValuePair<string, bool> orderByProperty, int brandId)
        {
            var result = this.GetActive(q => q.isAvailable == true && q.BrandId == brandId &&
                (keyword == null || q.Name.Contains(keyword) || q.ShortName.Contains(keyword)));

            StoreSortableProperty name;
            if (orderByProperty.Key != null && Enum.TryParse(orderByProperty.Key, out name))
            {
                switch (name)
                {
                    case StoreSortableProperty.Id:
                        result = result.OrderBy(q => q.ID, orderByProperty.Value);
                        break;
                    case StoreSortableProperty.Name:
                        result = result.OrderBy(q => q.Name, orderByProperty.Value);
                        break;
                    case StoreSortableProperty.ShortName:
                        result = result.OrderBy(q => q.ShortName, orderByProperty.Value);
                        break;
                    case StoreSortableProperty.HasNews:
                        result = result.OrderBy(q => q.HasNews, orderByProperty.Value);
                        break;
                    case StoreSortableProperty.HasProducts:
                        result = result.OrderBy(q => q.HasProducts, orderByProperty.Value);
                        break;
                    case StoreSortableProperty.HasImageCollections:
                        result = result.OrderBy(q => q.HasImageCollections, orderByProperty.Value);
                        break;
                }
            }
            else
            {
                result = result.OrderBy(q => q.ID);
            }

            return result;
        }

        protected override void OnCreate(Store entity)
        {
            base.OnCreate(entity);
            entity.isAvailable = true;
            entity.CreateDate = Utils.GetCurrentDateTime();
        }

        protected override void OnDeactivate(Store entity)
        {
            entity.isAvailable = false;
        }

        public IQueryable<Store> GetAllStore(int brandId)
        {
            return this.GetActive(q => q.BrandId == brandId);
        }

        public IQueryable<Store> GetActiveStoreByBrandId(int brandId)
        {
            // return this.GetActive().Where(q => q.BrandId == brandId && q.isAvailable.Value);
            return this.Get(q => q.BrandId == brandId && q.isAvailable == true);
        }

        public IQueryable<Store> GetStoreByBrandId(int brandId)
        {
            return this.Get().Where(q => q.BrandId == brandId && q.isAvailable == true && q.Lat!="0" && q.Lon!="0");
        }

        public IQueryable<Store> GetStoresByBrandIdAndType(int brandId, int type)
        {
            return this.Get().Where(q => (bool)q.isAvailable && q.BrandId == brandId && q.Type == type);
        }

        public IQueryable<Store> GetStoresByBrand(int brandId)
        {
            return this.Get(s => s.BrandId == brandId && s.isAvailable == true);
        }

        public async System.Threading.Tasks.Task<IQueryable<StoreUser>> GetStoreUserByStoreIdAsync(int storeId)
        {
            return (await this.GetStoreByID(storeId)).StoreUsers.AsQueryable();
        }

        public Store GetStoreById(int storeId)
        {
            return this.Get(storeId);
        }

        public async Task CreateStoreAsync(Store entity, IEnumerable<Product> listProduct)
        {
            try
            {
                foreach (var item in listProduct)
                {
                    var mapping = new ProductDetailMapping()
                    {
                        DiscountPercent = item.DiscountPercent,
                        DiscountPrice = item.DiscountPrice,
                        Price = item.Price,
                        ProductID = item.ProductID,
                        Active = true,
                    };
                    entity.ProductDetailMappings.Add(mapping);
                }
                await this.CreateAsync(entity);
            }
            catch(Exception e)
            {
            }
        }

        public IQueryable<Store> GetStoresByPromotionId(int promotionId, int brandId)
        {
            var promotionStoreMappingService = DependencyUtils.Resolve<IPromotionStoreMappingService>();
            var promotionMappingList = promotionStoreMappingService.GetActivePromotionStoreMappingByPromotionID(promotionId);
            return this.GetAllStore(brandId).Join(promotionMappingList, p => p.ID, q => q.StoreId, (p, q) => p);
        }

        #region SystemReport
        public IQueryable<Store> GetStores()
        {
            return this.Get(a => a.isAvailable == true);
        }
        public string GetStoreNameByID(int id)
        {
            var rs = this.Get(a => a.ID == id).FirstOrDefault();
            return rs.Name;
        }
        #endregion
        #region StoreReport
        public async Task<Store> GetStoreByID(int storeid)
        {
            return await this.GetAsync(storeid);
        }

        public IQueryable<Store> GetStoresByUser(string username)
        {
            var result = this.Get(a => a.StoreUsers.Any(b => b.Username.Equals(username) && (bool)(a.isAvailable)));
            return result;
        }
        #endregion
        public IQueryable<Store> GetStoreByGroupId(int groupId)
        {
            var rs = this.Get().Where(
                   a => a.StoreGroupMappings.Any(b => b.StoreGroupID == groupId));
            return rs;
        }
    }

    public enum StoreSortableProperty
    {
        Id,
        Name,
        ShortName,
        HasNews,
        HasProducts,
        HasImageCollections,
    }

}
