using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.ViewModels;

namespace HmsService.Sdk
{
    partial class ShiftApi
    {
      

        public ShiftViewModel FindShiftOfStore(int storeId, DateTime time)
        {
            var entity = this.BaseService.FirstOrDefault(q => q.Active && q.StoreId == storeId
                                                        && q.StartTime <= time && time <= q.EndTime);
            return new ShiftViewModel(entity);
        }
    }
}
