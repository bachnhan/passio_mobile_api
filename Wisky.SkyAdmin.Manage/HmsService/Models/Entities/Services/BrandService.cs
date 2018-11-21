using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{

    public partial interface IBrandService
    {
        IQueryable<Brand> GetAdminByBrandWithFilter(string keyword, KeyValuePair<string, bool> orderByProperty);
        System.Threading.Tasks.Task CreateBrandAsync(Brand entity);
        IQueryable<Brand> GetAllBrand();
        Task DeactiveBrandAsync(Brand entity);
        Brand GetBrandById(int id);
        Brand GetBrandByName(string name);
        Task ChangeBrandActivation(Brand entity);
    }
    public partial class BrandService
    {
        public IQueryable<Brand> GetAdminByBrandWithFilter(string keyword, KeyValuePair<string, bool> orderByProperty)
        {
            var result = this.GetActive(q =>
                (keyword == null || q.BrandName.Contains(keyword) || q.CompanyName.Contains(keyword)));

            BrandSortableProperty name;
            if (orderByProperty.Key != null && Enum.TryParse(orderByProperty.Key, out name))
            {
                switch (name)
                {
                    case BrandSortableProperty.Id:
                        result = result.OrderBy(q => q.Id, orderByProperty.Value);
                        break;
                    case BrandSortableProperty.Name:
                        result = result.OrderBy(q => q.BrandName, orderByProperty.Value);
                        break;
                    case BrandSortableProperty.CompanyName:
                        result = result.OrderBy(q => q.CompanyName, orderByProperty.Value);
                        break;
                }
            }
            else
            {
                result = result.OrderBy(q => q.Id);
            }

            return result;
        }

        public async System.Threading.Tasks.Task CreateBrandAsync(Brand entity)
        {
            entity.Active = true;
            entity.CreateDate = Utils.GetCurrentDateTime();
            await this.CreateAsync(entity);
        }

        public enum BrandSortableProperty
        {
            Id,
            Name,
            CompanyName
        }

        public IQueryable<Brand> GetAllBrand()
        {
            return this.GetActive();
        }

        public Brand GetBrandById(int id)
        {
            return this.FirstOrDefault(q => q.Id == id);            
        }

        public Brand GetBrandByName(string name)
        {
            return this.FirstOrDefault(q => q.BrandName == name);
        }

        public async Task DeactiveBrandAsync(Brand entity)
        {
            await this.UpdateAsync(entity);
        }

        public async Task ChangeBrandActivation(Brand entity)
        {
            await this.UpdateAsync(entity);
        }
    }
}
