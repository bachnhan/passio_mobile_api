using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class ProvinceApi
    {
        public IEnumerable<ProvinceViewModel> GetAllProvince()
        {
            return this.Get().ToList();
        }

        public IQueryable<Province> GetProvinces()
        {
            return this.BaseService.Get();
        }

        public IQueryable<Province> GetProvincesBaseOnArea(int areaId)
        {
            if (areaId == 0)
            {
                return this.BaseService.Get();
            }
            else
            {
                return this.BaseService.Get(q => q.AreaProvinceId == areaId);
            }
        }
    }
}
