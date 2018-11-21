using AutoMapper.QueryableExtensions;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class CustomerFilterApi
    {
        public IQueryable<CustomerFilterViewModel> GetAllFilter(int brandId)
        {
            var model = this.BaseService.GetAllFilter(brandId)
                .ProjectTo<CustomerFilterViewModel>(this.AutoMapperConfig);
            return model;
        }

        public CustomerFilterViewModel GetFilterById(int? id)
        {
            var entity = this.BaseService.Get(id);
            return new CustomerFilterViewModel(entity);
        }

        public async Task UpdateCustomerFilter(CustomerFilterEditViewModel model)
        {
            var entity = model.ToEntity();
            await this.BaseService.UpdateAsync(entity);
        }
    }
}
