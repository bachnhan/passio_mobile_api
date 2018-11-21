using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class CustomerTypeApi
    {
        public IQueryable<CustomerTypeViewModel> GetAllCustomerTypes(int? brandId)
        {
            var customerTypes = this.BaseService.GetAllCustomerType(brandId)
                .ProjectTo<CustomerTypeViewModel>(this.AutoMapperConfig);
            return customerTypes;
        }

        public CustomerTypeViewModel GetCustomerTypeById(int id)
        {
            var customerType = this.BaseService.GetCustomerTypeById(id);
            if (customerType == null)
            {
                return null;
            }
            else
            {
                return new CustomerTypeViewModel(customerType);
            }
        }


        public CustomerTypeViewModel GetCustomerTypeByName(string name)
        {
            var customerType = this.BaseService.GetCustomerTypeByName(name);
            if (customerType == null)
            {
                return null;
            }
            else
            {
                return new CustomerTypeViewModel(customerType);
            }
        }
        
    }
}
