using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class DistrictApi
    {
        public IEnumerable<DistrictViewModel> GetAllDistrict()
        {
            return this.Get().ToList();
        }

        public IQueryable<District> GetAllDistrictByProvinceID(int provinceId)
        {
            return this.BaseService.Get(q => q.ProvinceCode == provinceId);
        }

        public IQueryable<District> GetAllDistrictByProvinceIDAndAreaId(int provinceId, int areaId)
        {
            if (areaId != 0)
            {
                return this.BaseService.Get(q => q.ProvinceCode == provinceId && areaId == q.AreaDistrictId);
            }
            else
            {
                return this.BaseService.Get(q => q.ProvinceCode == provinceId);
            }
        }
    }
}
