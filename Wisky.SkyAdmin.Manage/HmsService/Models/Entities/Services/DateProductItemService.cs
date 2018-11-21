using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IDateProductItemService
    {
        #region StoreProduct
        IQueryable<DateProductItem> GetDateProductItemByDayAndStore(DateTime startTime, int storeID);
        IQueryable<DateProductItem> GetProductItemByDate(IEnumerable<DateProduct> listProduct);
        IQueryable<DateProductItem> GetDateProductItemByTimeRange(DateTime startTime, DateTime endTime, int storeID);
        IQueryable<DateProductItem> GetBrandDateProductItemByTimeRange(DateTime startTime, DateTime endTime, int brandId);
        #endregion
    }
    public partial class DateProductItemService
    {
        private readonly IProductItemCompositionMappingService compositionService;
        #region StoreProduct
        public IQueryable<DateProductItem> GetDateProductItemByDayAndStore(DateTime startTime, int storeID)
        {
            return this.Get(d => d.Date.Year == startTime.Year && d.Date.Month == startTime.Month && d.Date.Day == startTime.Day &&
                        d.StoreId == storeID);
        }
        public IQueryable<DateProductItem> GetProductItemByDate(IEnumerable<DateProduct> listProduct)
        {
            List<ProductItemCompositionMapping> listComposition = new List<ProductItemCompositionMapping>();            
            foreach (var item in listProduct)
            {
                var itemsInProduct = compositionService.GetProductItemByProductID(item.ProductId);
                foreach (var item2 in itemsInProduct)
                {
                    item2.Quantity *= item.Quantity;
                    listComposition.Add(item2);
                }
            }
            var productItems = listComposition.GroupBy(p => p.ItemID).Select(sp => new DateProductItem()
            {
                ProductItemID = sp.FirstOrDefault().ItemID,
                ProductItemName = sp.FirstOrDefault().ProductItem.ItemName,
                Quantity = (int)sp.Sum(s => s.Quantity),
                Unit = sp.FirstOrDefault().ProductItem.Unit,
            });

            return productItems.AsQueryable();
        }
        public IQueryable<DateProductItem> GetDateProductItemByTimeRange(DateTime startTime, DateTime endTime, int storeID)
        {
            return this.Get(a => startTime <= a.Date && endTime >= a.Date && storeID == a.StoreId);
        }
        public IQueryable<DateProductItem> GetBrandDateProductItemByTimeRange(DateTime startTime, DateTime endTime, int brandId)
        {
            var dateProductItems = this.Get(a => startTime <= a.Date && endTime >= a.Date);
            var storeService = DependencyUtils.Resolve<IStoreService>();
            var stores = storeService.GetActiveStoreByBrandId(brandId).Select(q => q.ID).ToList();
            return dateProductItems.Join(stores, q => q.StoreId, p => p, (q, p) => q);
        }
        #endregion
    }
}
