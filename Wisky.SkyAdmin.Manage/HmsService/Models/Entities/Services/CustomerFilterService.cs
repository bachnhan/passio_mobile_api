using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ICustomerFilterService
    {
        IQueryable<CustomerFilter> GetAllFilter(int brandId);
    }
    public partial class CustomerFilterService
    {
        public IQueryable<CustomerFilter> GetAllFilter(int brandId)
        {
            return this.GetActive(q => q.BrandId == brandId);
        }

    }
}
