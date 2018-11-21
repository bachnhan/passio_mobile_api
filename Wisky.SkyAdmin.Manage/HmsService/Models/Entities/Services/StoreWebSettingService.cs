using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{

    public partial interface IStoreWebSettingService
    {
        IQueryable<StoreWebSetting> GetActiveByStore(int storeId);
        System.Threading.Tasks.Task MassUpdate(IEnumerable<KeyValuePair<int, string>> values, int storeId);
    }

    public partial class StoreWebSettingService
    {

        public IQueryable<StoreWebSetting> GetActiveByStore(int storeId)
        {
            Thread.Sleep(500);
            return this.GetActive(q => q.StoreId == storeId);
        }

        public async System.Threading.Tasks.Task MassUpdate(IEnumerable<KeyValuePair<int, string>> values, int storeId)
        {
            foreach (var value in values)
            {
                var entity = await this.GetAsync(value.Key);

                if (entity.StoreId != storeId || !entity.Active)
                {
                    throw new UnauthorizedAccessException();
                }

                entity.Value = value.Value;
            }

            await this.SaveAsync();
        }

    }
}
