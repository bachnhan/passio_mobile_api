using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class BrandApi
    {
        public PagingViewModel<BrandViewModel> GetAdminWithFilterAsync(string keyword, int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc)
        {
            var pagedList = this.BaseService.GetAdminByBrandWithFilter(keyword, sortKeyAsc)
                .ProjectTo<BrandViewModel>(this.AutoMapperConfig)
                .Page(currPage, pageSize);

            return new PagingViewModel<BrandViewModel>(pagedList);
        }

        public IQueryable<BrandViewModel> GetActiveBrands()
        {
            return this.BaseService.GetActive().ProjectTo<BrandViewModel>(this.AutoMapperConfig);
        }

        public async Task CreateBrandAsync(BrandViewModel model)
        {
            await this.BaseService.CreateBrandAsync(model.ToEntity());
        }

        public IEnumerable<Brand> GetAllBrand()
        {
            var brands = this.BaseService.GetAllBrand().ToList();
            return brands;
        }
        
        public IQueryable<BrandViewModel> GetAllActiveAndInactiveBrands()
        {
            return this.BaseService.Get().ProjectTo<BrandViewModel>(this.AutoMapperConfig);
        }

        public BrandViewModel GetBrandById(int id)
        {
            var brand = this.BaseService.GetBrandById(id);
            if (brand != null)
            {
                return new BrandViewModel(brand);
            }
            else
            {
                return null;
            }
        }
        public BrandViewModel GetBrandByName(string name)
        {
            var brand = this.BaseService.GetBrandByName(name);
            if (brand != null)
            {
                return new BrandViewModel(brand);
            }
            else
            {
                return null;
            }
        }

        public async Task DeactiveBrandAsync(int id)
        {
            var brand = this.BaseService.GetBrandById(id);
            brand.Active = false;
            await this.BaseService.DeactiveBrandAsync(brand);
        }

        public async Task ChangeBrandActivationAsync(int id)
        {
            var brand = this.BaseService.GetBrandById(id);
            brand.Active = !brand.Active;
            await this.BaseService.ChangeBrandActivation(brand);
        }

        public async Task EditBrandAsync(BrandViewModel model)
        {
            var brand = this.BaseService.GetBrandById(model.Id);
            brand.CompanyName = model.CompanyName;
            brand.ContactPerson = model.ContactPerson;
            brand.PhoneNumber = model.PhoneNumber;
            brand.BrandName = model.BrandName;
            brand.Fax = model.Fax;
            brand.Description = model.Description;
            brand.Website = model.Website;
            brand.Active = true;
            await this.BaseService.UpdateAsync(brand);
        }
    }
}
