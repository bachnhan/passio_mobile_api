using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ITransactionService
    {
        System.Threading.Tasks.Task CreateTransactionAsync(Transaction entity);
        IQueryable<Transaction> GetCustomersTransactions(int customerID);
        IQueryable<Transaction> GetCustomersTransactionsByCardId(int cardId);
        IQueryable<Transaction> GetAllTransaction();
        IQueryable<Transaction> GetAllTransactionByStoreId(int storeId);
        IQueryable<Transaction> GetAllTransactionByStoreIdWithDateRange(int storeId, DateTime startTime, DateTime endTime);
        IQueryable<Transaction> GetAllBrandByTimeRange(int brandId, DateTime startTime, DateTime endTime);
    }

    public partial class TransactionService
    {
        public async System.Threading.Tasks.Task CreateTransactionAsync(Transaction entity)
        {
            entity.Date = Utils.GetCurrentDateTime();
            await this.CreateAsync(entity);
        }

        public IQueryable<Transaction> GetCustomersTransactions(int customerID)
        {
            return this.Get(q => q.Account.MembershipCard.CustomerId == customerID);
        }
        public IQueryable<Transaction> GetCustomersTransactionsByCardId(int cardId)
        {
            return this.Get(q => q.Account.MembershipCard.Id == cardId);
        }

        public IQueryable<Transaction> GetAllTransaction()
        {
            return this.GetActive();
        }
        public IQueryable<Transaction> GetAllTransactionByStoreId(int storeId)
        {
            return this.GetActive().Where(q => q.StoreId == storeId);
        }

        public IQueryable<Transaction> GetAllTransactionByStoreIdWithDateRange(int storeId, DateTime startTime, DateTime endTime)
        {
            return this.Get(q => q.StoreId == storeId && q.Date >= startTime && q.Date <= endTime);
        }
        public IQueryable<Transaction> GetAllBrandByTimeRange(int brandId, DateTime startTime, DateTime endTime)
        {
            return this.GetActive().Where(q => q.BrandId == brandId && q.Date >= startTime && q.Date <= endTime);
        }
    }
}
