using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities.Repositories;
using System.Data.SqlTypes;

using System.Diagnostics;
using System.Data.Entity.SqlServer;
using HmsService.Sdk;

namespace HmsService.Models.Entities.Services
{
    public partial interface ICustomerService
    {
        Customer GetCustomerByEmail(string email);
        IQueryable<Customer> GetCustomersByStore(int storeId, int brandId);
        IQueryable<Customer> GetCustomersByBrand(int brandId);
        IQueryable<Customer> GetAllCustomer(int brandId);
        Task<Customer> GetCustomerByAccountIdAsync(int accountId);
        IQueryable<Customer> GetCustomersByFilter(CustomerFilter filter, int brandId);
        IQueryable<Customer> GetCustomersByAccountPhone(string accountPhone, int brandId);
        IQueryable<Customer> GetCustomersByDateRange(DateTime startDate, DateTime endDate, int brandId);
        IQueryable<Customer> GetCustomersByDateRangeAndStoreId(int? storeId, DateTime startDate, DateTime endDated, int brandId);
        CustomerProductDetail GetProductByCustomerId(int id);
        IEnumerable<Customer> SearchCustomer(IEnumerable<Customer> datasource, string key, int skip, int take);
        void UpdateCustomer(Customer model);
        int CreateCustomer(Customer customer);
        Task<int> CreateCustomerAsync(Customer customer);
        Customer GetCustomerByNameAddressAndPhone(string name, string address, string phone);
    }

    public partial class CustomerService
    {
        public Customer GetCustomerByEmail(string email)
        {
            var customerRepo = SkyWeb.DatVM.Mvc.Autofac.DependencyUtils.Resolve<ICustomerRepository>();
            customerRepo.GetActive();
            var customer = customerRepo.Get(m => email.Equals(m.Email)).FirstOrDefault();
            return customer;
        }

        public IEnumerable<Customer> SearchCustomer (IEnumerable<Customer> datasource, string key, int skip, int take)
        {
            var result = from s in datasource where s.Name.Contains(key) select s;
            return result.Skip(skip).Take(take).ToList();
        }

        public async Task<Customer> GetCustomerByAccountIdAsync(int accountId)
        {
            var accountRepo = SkyWeb.DatVM.Mvc.Autofac.DependencyUtils.Resolve<IAccountRepository>();
            var account = await accountRepo.GetAsync(accountId);
            return account.MembershipCard.Customer;
        }

        public IQueryable<Customer> GetCustomersByFilter(CustomerFilter filter, int brandId)
        {
            int curentYear = Utils.GetCurrentDateTime().Year;
            var customerList = this.GetCustomersByBrand(brandId)
            .Where(q => filter.IsEnableGender != true || q.Gender == filter.Gender)
            .Where(q => filter.IsEnableAge != true || ((curentYear - q.BirthDay.Value.Year) >= filter.AgeFrom)
                                                       && ((curentYear - q.BirthDay.Value.Year) <= filter.AgeTo))
            .Where(q => filter.IsEnableBirthday != true || (filter.BirthdayFrom != null && filter.BirthdayTo != null
                                                                && (SqlFunctions.DatePart("dayofyear", q.BirthDay.Value) >= SqlFunctions.DatePart("dayofyear", filter.BirthdayFrom.Value))
                                                                && (SqlFunctions.DatePart("dayofyear", q.BirthDay.Value) <= SqlFunctions.DatePart("dayofyear", filter.BirthdayTo.Value)))
                                                        );
            return customerList;
        }

        public IQueryable<Customer> GetCustomersByAccountPhone(string accountPhone, int brandId)
        {
            var customerList = this.GetCustomersByBrand(brandId)
            .Where(q =>q != null && q.AccountPhone.Equals(accountPhone));
            return customerList;
        }

        public CustomerProductDetail GetProductByCustomerId(int id)
        {
            var customer = this.Get(id);

            var products = customer.CustomerProductMappings
                .Where(q => q.Product.Active)
                .Select(q => q.Product);

            var customerProductDetail = new CustomerProductDetail()
            {
                Customer = customer,
                Products = products,
                PruductsCount = products.Count(),
            };
            return customerProductDetail;
        }

        public IQueryable<Customer> GetAllCustomer(int brandId)
        {
            return this.GetActive().Where(q => q.CustomerType.BrandId == brandId);
        }

        public IQueryable<Customer> GetCustomersByStore(int storeId, int brandId)
        {
            return this.GetActive().Where(q => q.CustomerStoreReportMappings.Any(s => s.StoreID == storeId) && q.CustomerType.BrandId == brandId);
        }

        public IQueryable<Customer> GetCustomersByBrand(int brandId)
        {
            return this.GetActive().Where(q => q.BrandId == brandId);
        }

        public IQueryable<Customer> GetCustomersByDateRangeAndStoreId(int? storeId, DateTime startDate, DateTime endDate, int brandId)
        {
            DateTime startOfDate = new DateTime();
            DateTime endOfDate = new DateTime();
            if (startDate == null)
            {
                startOfDate = SqlDateTime.MinValue.Value;
            }
            if (endDate == null)
            {
                endOfDate = SqlDateTime.MaxValue.Value;
            }
            startOfDate = startDate.Date;
            endOfDate = endDate.AddDays(1).AddTicks(-1);

            //var orderApi = new OrderApi();
            //var CustomerIDs = orderApi.Get().Where(q => q.CheckInDate >= startOfDate && q.CheckInDate <= endOfDate
            //&& q.StoreID == storeId).Select(r => r.CustomerID).ToList();
            //var customerApi = new CustomerApi();
            //var customers = this.Get().Where(q => CustomerIDs.Contains(q.CustomerID));

            var customers = this.GetCustomersByStore(storeId.Value, brandId)
              .Where(q => q.Orders
              .Any(r =>
                          r.CheckInDate >= startOfDate
                       && r.CheckInDate <= endOfDate));

            return customers;
        }

        public IQueryable<Customer> GetCustomersByDateRange(DateTime startDate, DateTime endDate, int brandId)
        {
            DateTime startOfDate = new DateTime();
            DateTime endOfDate = new DateTime();
            if (startDate == null)
            {
                startOfDate = SqlDateTime.MinValue.Value;
            }
            if (endDate == null)
            {
                endOfDate = SqlDateTime.MaxValue.Value;
            }
            startOfDate = startDate.Date;
            endOfDate = endDate.AddDays(1).AddTicks(-1);

            //var orderApi = new OrderApi();
            //var CustomerIDs = orderApi.Get().Where(q => q.CheckInDate >= startOfDate && q.CheckInDate <= endOfDate).Select(r => r.CustomerID).ToList();
            //var customerApi = new CustomerApi();
            //var customers = this.GetActive().Where(q => CustomerIDs.Contains(q.CustomerID));

            var customers = this.GetActive().Where(q => q.CustomerType.BrandId.Value == brandId)
                .Where(q => q.Orders
                .Any(r =>
                            r.CheckInDate >= startOfDate
                         && r.CheckInDate <= endOfDate));

            return customers;
        }

        public void UpdateCustomer(Customer model)
        {
            this.Update(model);
            this.Save();
        }

        public int CreateCustomer(Customer customer)
        {
            try
            {
                this.Create(customer);
                return customer.CustomerID;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public async Task<int> CreateCustomerAsync(Customer customer)
        {
            try
            {
                await this.CreateAsync(customer);
                return customer.CustomerID;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public Customer GetCustomerByNameAddressAndPhone(string name, string address, string phone)
        {
            return this.GetActive().Where(q => q.Name == name && q.Address == address && q.Phone == phone).FirstOrDefault<Customer>();
        }
    }
    public class CustomerProductDetail
    {
        public Customer Customer { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public int PruductsCount { get; set; }

    }
}
