using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class CostInventoryMappingApi
    {
        public CostInventoryMapping CreateReturnCostInventoryMapping(CostInventoryMapping CostInventoryMapping)
        {
            this.BaseService.Create(CostInventoryMapping);
            return CostInventoryMapping;
        }

        public void DeleteCostInventoryMapping(CostInventoryMapping entity)
        {
            this.BaseService.Delete(entity);
        }

        //public IQueryable<CostInventoryMapping> GetBy()
        //{
        //    return this.BaseService.Get()
        //        .ProjectTo<TViewModel>(this.AutoMapperConfig)
        //        .ToList();
        //}
    }
}
