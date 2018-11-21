using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class DateProductItemApi
    {
        #region StoreProduct
        public IEnumerable<DateProductItem> GetDateProductItemByDayAndStore(DateTime startTime, int storeID)
        {
            var dateProducts = this.BaseService.GetDateProductItemByDayAndStore(startTime, storeID)
              //  .ProjectTo<DateProductItemViewModel>(this.AutoMapperConfig)
                .ToList();
            return dateProducts;
        }
        public IEnumerable<DateProductItem> GetDateProductItemByTimeRange(DateTime startTime, DateTime endTime, int storeID)
        {
            var dateProducts = this.BaseService.GetDateProductItemByTimeRange(startTime, endTime, storeID)
               // .ProjectTo<DateProductItemViewModel>(this.AutoMapperConfig)
                .ToList();
            return dateProducts;
        }
        public IQueryable<DateProductItem> GetQueryDateProductItemByTimeRange(DateTime startTime, DateTime endTime, int storeID)
        {
            var dateProducts = this.BaseService.GetDateProductItemByTimeRange(startTime, endTime, storeID);
            return dateProducts;
        }
        public IEnumerable<DateProductItem> GetDateProductItemByTimeRange(DateTime startTime, DateTime endTime, int storeId, int brandId)
        {
            if(storeId == 0)
            {
                return BaseService.GetBrandDateProductItemByTimeRange(startTime, endTime, brandId)
                    .ToList();
            }
            else
            {
                return BaseService.GetDateProductItemByTimeRange(startTime, endTime, storeId)
                    .ToList();
            }
        }
        public async Task DeleteDateProductItemAsync(int Id)
        {
            var entity = await this.BaseService.GetAsync(Id);
            await this.BaseService.DeleteAsync(entity);
        }
        #endregion
    }
}
