using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class CheckFingerApi
    {
        public CheckFingerViewModel GetCheckFingerByTime(int empId, DateTime time)
        {
            var entity = this.BaseService.GetEmployeeCheckByEmpIdAndTime(empId, time);
            if (entity == null) return null;
            return new CheckFingerViewModel(entity);
        }

        public IQueryable<CheckFinger> GetCheckFingerByEmp(string empNumber)
        {
            return this.BaseService.Get(q => q.EmpEnrollNumber == empNumber);
        }

        public IQueryable<CheckFinger> GetCheckFingerByBrand(int brandId, DateTime startTime, DateTime endTime)
        {
            return this.BaseService.Get(q => q.BrandId == brandId && q.Active == true && q.DateTime >= startTime && q.DateTime <= endTime);
        }

        public bool Add(CheckFingerViewModel model)
        {
            try
            {
                var entity = model.ToEntity();
                this.BaseService.Create(entity);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public IEnumerable<CheckFinger> GetCheckFingerByStoreByTimeRange(int storeId, DateTime startTime, DateTime endTime)
        {
            return this.BaseService.Get(q => q.Active == true && q.StoreId == storeId
                                        && q.DateTime >= startTime && q.DateTime <= endTime);
        }
    }
}
