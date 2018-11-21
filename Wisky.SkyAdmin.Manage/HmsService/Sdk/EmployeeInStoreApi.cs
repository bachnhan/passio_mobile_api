using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.Models.Entities;

namespace HmsService.Sdk
{
    partial class EmployeeInStoreApi
    {
        public EmployeeInStore GetEmployeeByIdAndStoreID(int empId, int storeId)
        {
            return this.BaseService.FirstOrDefault(q => q.EmployeeId == empId && q.StoreId == storeId && q.Active == true);
        }
        public IQueryable<EmployeeInStore> GetBaseMapping(int storeId, int storeSelected)
        {
            var listMappingStore = this.BaseService.Get(q => q.StoreId == storeId && q.Employee.MainStoreId == storeSelected);
            return listMappingStore;
        }

        public bool CheckEmployee(int basestore, int employeeId)
        {
            return this.BaseService.Get(q => q.EmployeeId == employeeId && q.Active).Any(q => q.StoreId == basestore);
        }

        public IQueryable<EmployeeInStore> GetCurrentEmployeeInStore(int storeId)
        {
            return this.BaseService.GetActive(q => q.StoreId == storeId);
        }
    }
}
