using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IAccountService
    {
        //System.Threading.Tasks.Task CreateAccountAsync(Account entity);
        //System.Threading.Tasks.Task UpdateAccountAsync(Account entity);
        //System.Threading.Tasks.Task DeleteAccountAsync(Account entity);
        System.Threading.Tasks.Task<Account> GetAccountByAccountCodeAsync(string AccountCode);
        System.Threading.Tasks.Task<Account> GetAccountByAccountNameActiveAsync(string AccountName);
        Account GetAccountEntityByAccountCode(string accountCode);
        //IQueryable<Account> GetActiveAccount(int? brandId);
        Account GetAccountById(int id);
        IQueryable<Account> GetAccountByMembershipId(int MembershipCardId);
        IQueryable<Account> GetAllAccountsByMembershipId(int membershipCardId);
        IQueryable<Account> GetBrandActiveAccounts(int storeId, int brandId);
    }

    public partial class AccountService
    {
        public Account GetAccountById(int id)
        {
            return this.Get(id);
        }

        public IQueryable<Account> GetBrandActiveAccounts(int brandId, int storeId)
        {
            var statusActive = (int)MembershipStatusEnum.Inactive;
            if(storeId != 0)
            {
                return this.Get(q => q.MembershipCard != null && q.BrandId == brandId && q.MembershipCard.StoreId == storeId && q.MembershipCard.MembershipCardType.Active.Value);
            }
            else
            {
                return this.Get(q => q.MembershipCard != null && q.BrandId == brandId && q.MembershipCard.MembershipCardType.Active.Value);
            }
        }

        //public IQueryable<Account> GetActiveAccount(int? brandId)
        //{
        //    return this.Get(q => q.Active.Value).Where(q => q.AccountType.BrandId == brandId);
        //}

        public Account GetAccountEntityByAccountCode(string accountCode)
        {
            return this.FirstOrDefault(q => q.AccountCode == accountCode);
        }

        //public async System.Threading.Tasks.Task CreateAccountAsync(Account entity)
        //{
        //if (entity.IsDetailAccount)
        //{
        //    if (!entity.IsCredit)
        //    {
        //        entity.BankName = null;
        //        entity.BankAccountNo = null;
        //        entity.OwnerName = null;
        //    }
        //    if (!entity.IsAsset)
        //    {
        //        entity.Balance = null;
        //    }
        //    else
        //    {
        //        entity.LatestUpdateBalance = Utils.GetCurrentDateTime();
        //    }
        //}
        //else
        //{
        //    entity.BankName = null;
        //    entity.BankAccountNo = null;
        //    entity.OwnerName = null;
        //    entity.Balance = null;
        //    entity.IsAsset = false;
        //    entity.IsCredit = false;
        //}
        //if (entity.StartDate.HasValue)
        //{
        //    entity.StartDate = entity.StartDate.Value.Date + new TimeSpan(0, 0, 0);
        //    entity.FinishDate = entity.FinishDate.Value.Date + new TimeSpan(0, 0, 0);
        //}
        ////Set start level
        //entity.Level_ = 1;
        //entity.Inactive = false;
        ////Set CategoryId and ParentId
        ////Đợi hỏi lại thầy
        //await this.CreateAsync(entity);
        //}

        //public async System.Threading.Tasks.Task UpdateAccountAsync(Account entity)
        //{
        //if (entity.IsDetailAccount)
        //{
        //    if (!entity.IsCredit)
        //    {
        //        entity.BankName = null;
        //        entity.BankAccountNo = null;
        //        entity.OwnerName = null;
        //    }
        //    if (!entity.IsAsset)
        //    {
        //        entity.Balance = null;
        //    }
        //    else
        //    {
        //        if (entity.LatestUpdateBalance == null)
        //        {
        //            entity.LatestUpdateBalance = Utils.GetCurrentDateTime();
        //        }
        //    }
        //}
        //else
        //{
        //    entity.BankName = null;
        //    entity.BankAccountNo = null;
        //    entity.OwnerName = null;
        //    entity.Balance = null;
        //    entity.IsAsset = false;
        //    entity.IsCredit = false;
        //}
        //if (entity.StartDate.HasValue)
        //{
        //    entity.StartDate = entity.StartDate.Value.Date + new TimeSpan(0, 0, 0);
        //    entity.FinishDate = entity.FinishDate.Value.Date + new TimeSpan(0, 0, 0);
        //}
        ////Set start level
        ////Đợi hỏi thầy về edit
        //entity.Level_ = 1;
        //entity.Inactive = false;
        ////Set CategoryId and ParentId
        ////Đợi hỏi lại thầy
        //await this.UpdateAsync(entity);
        //}

        //public async System.Threading.Tasks.Task DeleteAccountAsync(Account entity)
        //{
        //    entity.Active = true;
        //    await this.UpdateAsync(entity);
        //}

        public async System.Threading.Tasks.Task<Account> GetAccountByAccountCodeAsync(string AccountCode)
        {
            return await this.FirstOrDefaultAsync(q => q.AccountCode == AccountCode);
        }

        public async Task<Account> GetAccountByAccountNameActiveAsync(string AccountName)
        {
            return await this.FirstOrDefaultAsync(a => a.AccountName.Equals(AccountName) && a.Active.Value);
        }
        public IQueryable<Account> GetAccountByMembershipId(int MembershipCardId)
        {
            return this.GetActive(q => q.Active.Value && q.MembershipCardId == MembershipCardId);
        }
        public IQueryable<Account> GetAllAccountsByMembershipId(int membershipCardId)
        {
            return this.Get(q => q.MembershipCardId == membershipCardId);
        }
    }
}
