using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ICustomerProductMappingService
    {
        IQueryable<CustomerProductMapping> GetCustomerProductByCustomerId(int customerId);
    }

    public partial class CustomerProductMappingService
    {
        public IQueryable<CustomerProductMapping> GetCustomerProductByCustomerId(int customerId)
        {
            return this.GetActive(q => q.CustomerID == customerId);
        }
    }
}
