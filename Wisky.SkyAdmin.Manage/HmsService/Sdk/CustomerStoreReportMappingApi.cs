using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class CustomerStoreReportMappingApi
    {
        public IQueryable<CustomerStoreReportMappingViewModel> GetCustomerStoreByCustomerId(int customerId)
        {
            var entities = this.BaseService.GetCustomerStoreByCustomerId(customerId)
                .ProjectTo<CustomerStoreReportMappingViewModel>(this.AutoMapperConfig);
            if (entities == null)
            {
                return null;
            }
            else
            {
                return entities;
            }
        }
    }
}
