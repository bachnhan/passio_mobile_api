using HmsService.Models;
using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class AreaDeliveryApi
    {
        public IQueryable<AreaDelivery> GetAllAreaProvince()
        {
            var deliverytype = (int)AreaTypeEnum.Province;
            return this.BaseService.Get(q => q.AreaType == deliverytype);
        }

        public IQueryable<AreaDelivery> GetAllAreaDistrictByAreaId(int areaId)
        {
            var deliverytype = (int)AreaTypeEnum.District;
            return this.BaseService.Get(q => q.AreaType == deliverytype && q.AreaID == areaId);
        }
    }
}
