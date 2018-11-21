using HmsService.Models.Entities.Repositories;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ICustomerStoreReportMappingService
    {
        IQueryable<CustomerStoreReportMapping> GetCustomerStoreByCustomerId(int customerId);
    }

    public partial class CustomerStoreReportMappingService
    {
        public IQueryable<CustomerStoreReportMapping> GetCustomerStoreByCustomerId(int customerId)
        {
            return this.GetActive(q => q.CustomerID == customerId);
        }
    }
}
