using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ICustomerTypeService
    {
        IQueryable<CustomerType> GetAllCustomerType(int? brandId);
        CustomerType GetCustomerTypeById(int Id);
        CustomerType GetCustomerTypeByName(string name);
    }
    public partial class CustomerTypeService
    {
        public IQueryable<CustomerType> GetAllCustomerType(int? brandId)
        {
            return this.GetActive().Where(q => q.BrandId == brandId);
        }
        public CustomerType GetCustomerTypeById(int Id)
        {
            return this.FirstOrDefault(q => q.ID == Id);
        }

        public CustomerType GetCustomerTypeByName(string name)
        {
            return this.FirstOrDefault(q => q.CustomerType1 == name);
        }
    }
}
