using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IStoreGroupService
    {
        IQueryable<StoreGroup> GetAllStoreGroup();
        StoreGroup GetStoreGroupbyId(int id);
        IQueryable<StoreGroup> GetStoreGroupByBrand(int brandId);
    }
    public partial class StoreGroupService
    {
        public IQueryable<StoreGroup> GetAllStoreGroup()
        {
            return this.Get();
        }

        public StoreGroup GetStoreGroupbyId(int id)
        {
            return this.FirstOrDefault(q => q.GroupID == id);
        }

        public IQueryable<StoreGroup> GetStoreGroupByBrand(int brandId)
        {
            return this.Get(s => s.BrandId == brandId);
        }
    }
}
