using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using HmsService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using System.Data.Entity;

namespace HmsService.Sdk
{

    public partial class CustomerApi
    {

       public bool IsCustomerByFacbookId(string facebookId, int brandId)
        {
            var entity = this.BaseService.Get(q => q.FacebookId.Equals(facebookId) && q.BrandId == brandId).FirstOrDefault();
            if (entity == null)
            {
                return false;
            }
            return true;
        }
        public CustomerViewModel GetCustomerByEmail(string email, int brandId)
        {
            var entity = this.BaseService.Get(q => q.Email.Equals(email.Trim()) && q.BrandId == brandId).FirstOrDefault();
            if (entity == null)
            {
                return null;
            }
            var customer = new CustomerViewModel(entity);
            return customer;
        }

        public CustomerViewModel GetCustomerByEmail(string email)
        {
            var entity = this.BaseService.Get(q => q.Email.Equals(email.Trim())).FirstOrDefault();
            if (entity == null)
            {
                return null;
            }
            var customer = new CustomerViewModel(entity);
            return customer;
        }

        public CustomerViewModel GetCustomerByEmailOrPhone(string keySearch, int brandId)
        {
            if (Utils.IsDigitsOnly(keySearch))
            {
                var result = this.BaseService.Get(q => q.Phone.Equals(keySearch.Trim())&& q.BrandId == brandId).FirstOrDefault();
                if (result!= null)
                {
                    return new CustomerViewModel(result);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var result = this.BaseService.Get(q => q.Email.Equals(keySearch.Trim()) && q.BrandId == brandId).FirstOrDefault();
                if (result != null)
                {
                    return new CustomerViewModel(result);
                }
                else
                {
                    return null;
                }
            }
            
        }

        public IEnumerable<Customer> SearchCustomer(string search , int branId, int skip, int take) {
            var datasource  = this.BaseService.Get(q => q.BrandId == branId).AsEnumerable();
            return this.BaseService.SearchCustomer(datasource,search, skip, take);
        }
        public async Task<CustomerViewModel> GetCustomerByAccountIdAsync(int Id)
        {
            var entity = await this.BaseService.GetCustomerByAccountIdAsync(Id);
            if (entity == null)
            {
                return null;
            }
            var customer = new CustomerViewModel(entity);
            return customer;
        }

        public IQueryable<Customer> GetAllCustomer(int brandId)
        {
            return this.BaseService.GetAllCustomer(brandId);
        }

        public IQueryable<Customer> GetAllCustomerByBrandId(int brandId)
        {
            return this.BaseService.GetAllCustomer(brandId);
        }

        public IEnumerable<Customer> GetCustomersByBrandId(int brandId)
        {
            return this.BaseService.GetAllCustomer(brandId).ToList();
        }

        public IQueryable<CustomerViewModel> GetCustomersByStore(int storeId, int brandId)
        {
            return this.BaseService.GetCustomersByStore(storeId, brandId)
                .ProjectTo<CustomerViewModel>(this.AutoMapperConfig);

        }

        public IQueryable<CustomerViewModel> GetCustomersByBrand(int brandId)
        {
            var result = this.BaseService.GetCustomersByBrand(brandId)
                .ProjectTo<CustomerViewModel>(this.AutoMapperConfig);
            return result;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomerByBrandIdIenumerable(int brandId)
        {
            return await this.BaseService.Get(q => q.BrandId == brandId).ToListAsync();
        }

        public IQueryable<Customer> GetCustomersByDateRange(DateTime startDate, DateTime endDate, int brandId)
        {
            return  this.BaseService.GetCustomersByDateRange(startDate, endDate,brandId);
        }

        public IQueryable<Customer> GetCustomersByDateRangeAndStoreId(int? storeId, DateTime startDate, DateTime endDate, int brandId)
        {
            return this.BaseService.GetCustomersByDateRangeAndStoreId(storeId.Value, startDate, endDate, brandId);
        }
        /// <summary>
        /// update customer 
        /// </summary>
        /// <param name="model">Customer </param>
        public void UpdateEntityCustomer(Customer model)
        {
            this.BaseService.Update(model);
        }
        public void UpdateCustomer(CustomerViewModel model)
        {
            Customer entity = new Customer();
            model.CopyToEntity(entity);
            this.BaseService.UpdateCustomer(entity);
        }

        //TODO: Tạo Account từ membership
        //public void CreateAccount(CustomerViewModel model)
        //{
        //    var entity = this.BaseService.Get(model.CustomerID);
        //    entity.MembershipCard = model.MembershipCard;
        //    this.BaseService.UpdateCustomer(entity);
        //}

        public CustomerViewModel GetCustomerById(int customerId)
        {
            var accountApi = new AccountApi();
            var entity = this.BaseService.Get(customerId);
            if (entity == null)
            {
                return null;
            }
            var customer = new CustomerViewModel(entity);
            //using accountApi to bind Default account to customer 
            var accId = customer.AccountID;
            if (accId != null)
            {
                customer.DefaultAccount = accountApi.Get(accId.Value);
            }
            return customer;
        }

        public CustomerViewModel GetCustomerByID(int customerId)
        {
            var customer =  this.BaseService.Get(customerId);
            if (customer == null)
            {
                return null;
            }
            else
            {
                return new CustomerViewModel(customer);
            }
        }

        public Customer GetCustomerEntityById(int customerId)
        {
            return this.BaseService.Get(customerId);
        }

        public CustomerEditViewModel GetEditCustomerById(int customerId)
        {
            var accountApi = new AccountApi();
            var entity = this.BaseService.Get(customerId);
            if (entity == null)
            {
                return null;
            }
            var customer = new CustomerEditViewModel(entity);
            customer.DefaultAccount = accountApi.Get(customer.AccountID);
            return customer;
        }

        public async Task CreateCustomer(CustomerViewModel model)
        {
            await this.CreateAsync(model);
        }
		
        public int AddCustomer(CustomerViewModel customer)
        {
            var entity = customer.ToEntity();
            this.BaseService.CreateCustomer(entity);
            return entity.CustomerID;
        }

		public async Task<int> CreateCustomerReturnId(CustomerViewModel model)
        {
            var entity = model.ToEntity();
            int id = await this.BaseService.CreateCustomerAsync(entity);
            //await this.CreateAsync(model);

            return id;
        }

        public CustomerViewModel CreateCustomer2(CustomerViewModel model)
        {
            Customer a;
            if ((a = this.BaseService.FirstOrDefault(q => q.Email == model.Email)) == null)
            {
                this.Create(model);
                return model;
            }
            return new CustomerViewModel(a);
        }

        public async Task<CustomerViewModel> CreateCustomerByNameAddressAndPhone(CustomerViewModel model)
        {
            var entity = model.ToEntity();
            await this.BaseService.CreateAsync(entity);
            return new CustomerViewModel(entity);
        }

        public CustomerViewModel GetCustomerByNameAddressAndPhone(string name, string address, string phone)
        {
            var entity = this.BaseService.GetCustomerByNameAddressAndPhone(name, address, phone);
            return entity == null ? null : new CustomerViewModel(entity);
        }

        public IQueryable<Customer> GetCustomersByFilter(CustomerFilter filter, int brandId)
        {
            if (filter == null)
            {
                return this.BaseService.GetCustomersByBrand(brandId);
            }
            return this.BaseService.GetCustomersByFilter(filter, brandId);

        }

        public async Task EditDefaultAccountAsync(CustomerViewModel model)
        {
            var entity = await this.BaseService.GetAsync(model.CustomerID);
            entity.AccountID = model.AccountID;
            await this.BaseService.UpdateAsync(entity);
        }

        public async Task EditDefaultAccountAsync(CustomerViewModel model, int Id)
        {
            var entity = await this.BaseService.GetAsync(model.CustomerID);
            entity.AccountID = Id;
            await this.BaseService.UpdateAsync(entity);
        }

        public IQueryable<Customer> GetCustomersByAccountPhone(string accountPhone, int brandId)
        {
            return this.BaseService.GetCustomersByAccountPhone(accountPhone, brandId);
        }
    }

}
