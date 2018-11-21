using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class CustomerProductMappingApi
    {
        public IQueryable<CustomerProductMappingViewModel> GetCustomerProductByCustomerId(int customerId)
        {
            var entities = this.BaseService.GetCustomerProductByCustomerId(customerId)
                .ProjectTo<CustomerProductMappingViewModel>(this.AutoMapperConfig);
            if (entities == null)
            {
                return null;
            }
            else
            {
                return entities;
            }
        }

        public CustomerProductMappingViewModel GetByCustomerIdProductId(int customerId, int productId)
        {
            var r = this.BaseService.GetActive(c => c.CustomerID == customerId && c.ProductID == productId)
                .ProjectTo<CustomerProductMappingViewModel>(this.AutoMapperConfig).ToList();
            if (r == null || r.Count == 0)
            {
                return null;
            } else
            {
                return r[0];
            }
        }
    }
}
