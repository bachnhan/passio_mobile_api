using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IEmployeeService
    {
        Employee GetEmployeeById(int id);
        Employee GetEmployeeByEnroll(string enrollNumber);
        IQueryable<Employee> GetEmployeeByStoreId(int storeId);
        IQueryable<Employee> GetEmployeeByBrandId(int brandId);
    }

    public partial class EmployeeService
    {
        public Employee GetEmployeeById(int id)
        {
            return this.FirstOrDefault(q => q.Id == id);
        }
        public Employee GetEmployeeByEnroll(string enrollNumber)
        {
            return this.FirstOrDefault(q => q.EmpEnrollNumber == enrollNumber);
        }

        public IQueryable<Employee> GetEmployeeByStoreId(int storeId)
        {
            return this.Get(q => q.MainStoreId == storeId && q.Active == true);
        }

        public IQueryable<Employee> GetEmployeeByBrandId(int brandId)
        {
            return this.Get(q => q.BrandId == brandId && q.Active == true);
        }

    }
}
