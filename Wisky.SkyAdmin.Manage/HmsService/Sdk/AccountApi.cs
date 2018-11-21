using AutoMapper.QueryableExtensions;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class AccountApi
    {
        public override async System.Threading.Tasks.Task CreateAsync(AccountViewModel model)
        {
            await this.BaseService.CreateAsync(model.ToEntity());
        }

        public void DeactiveAccountByMembershipCardID(int membershipCardId)
        {
            var listData = this.BaseService.Get(q => q.MembershipCardId == membershipCardId).ToList();
            foreach (var item in listData)
            {
                item.Active = false;
                this.BaseService.Update(item);
            }
        }
        
        public List<AccountViewModel> GetAccountsByCardId(int cardId)
        {
            return this.BaseService.Get(q => q.MembershipCardId == cardId).ProjectTo<AccountViewModel>(this.AutoMapperConfig).ToList();
        }

        public async Task<int> AddAccount(Account account)
        {
            await this.BaseService.CreateAsync(account);
            return account.AccountID;
        }
        public int UpdateAccount(AccountViewModel model)
        {
            var entity = this.BaseService.Get(model.AccountID);

            entity.AccountName = model.AccountName;
            entity.Level_ = model.Level_;
            entity.StartDate = model.StartDate;
            entity.FinishDate = model.FinishDate;
            entity.Balance = model.Balance;
            entity.ProductCode = model.ProductCode;
            entity.MembershipCardId = model.MembershipCardId;
            entity.Type = model.Type; //Change to property
            entity.BrandId = model.BrandId;
            entity.Active = model.Active;

            this.BaseService.Update(entity);
            return entity.AccountID;

        }
        public IQueryable<Account> GetActiveAccounts()
        {
            var statusActive = (int)MembershipStatusEnum.Inactive;
            return this.BaseService.Get(q => q.Active.Value && q.MembershipCardId != null && q.MembershipCard.IsSample != true && q.MembershipCard.Status != statusActive);
        }

        public IQueryable<Account> GetBrandActiveAccounts(int brandId, int storeId)
        {
            return this.BaseService.GetBrandActiveAccounts(brandId, storeId);
        }

        public IQueryable<AccountViewModel> GetActiveAccountByCusId(int id)
        {
            return this.BaseService.Get().Where(q => q.Active.Value && q.MembershipCard.CustomerId == id).ProjectTo<AccountViewModel>(this.AutoMapperConfig);
        }

        public IQueryable<AccountViewModel> GetActiveAccountByCardId(int id)
        {
            return this.BaseService.Get(q => q.Active.Value).Where(q => q.MembershipCard.Id == id).ProjectTo<AccountViewModel>(this.AutoMapperConfig);
        }

        public IQueryable<AccountViewModel> GetAccountByCusId(int id)
        {
            return this.BaseService.Get().Where(q => q.MembershipCard.CustomerId == id && q.MembershipCard.Active == true).ProjectTo<AccountViewModel>(this.AutoMapperConfig);
        }
        public IQueryable<Account> GetAccountByCustomerId(int id)
        {
            return this.BaseService.Get().Where(q => q.MembershipCard.CustomerId == id);
        }
        public IQueryable<Account> GetAccountByCustomerIdAndMembershipId(int id, int membershipId)
        {
            return this.BaseService.Get().Where(q => q.MembershipCard.CustomerId == id && q.MembershipCardId == membershipId);
        }
        public IQueryable<Account> GetAccountByCustomerIdAndMemberTypeId(int id, int memberTypeId)
        {
            //var memberTypeApi = new MembershipCardTypeMappingApi();
            ////var membershipList = memberTypeApi.GetActiveByCustomerIdAndTypeId(id, memberTypeId);

            //if (memberTypeId == 4)//Credit
            //{
            //    var accountList = this.BaseService.GetActive().Where(q => q.MembershipCard.CustomerId == id && q.AccountTypeId == (int)AccountTypeEnum.CreditAccount);
            //    return accountList;
            //}
            //else if (memberTypeId == 5)//Gift
            //{
            //    var accountList = this.BaseService.GetActive().Where(q => q.MembershipCard.CustomerId == id && q.AccountTypeId == (int)AccountTypeEnum.GiftAccount);
            //    return accountList;
            //}
            //else if (memberTypeId == 6)//Member
            //{
            //    var accountList = this.BaseService.GetActive().Where(q => q.MembershipCard.CustomerId == id && q.AccountTypeId == (int)AccountTypeEnum.PointAccount);
            //    return accountList;
            //}
            //else
            //{
            return null;
            //}
        }

        public IEnumerable<Account> GetAccountByBrandId(int brandId)
        {
            return this.BaseService.GetActive().Where(q => q.BrandId == brandId).ToList();
        }

        //public async System.Threading.Tasks.Task UpdateAccountAsync(AccountViewModel model)
        //{
        //    var entity = await this.BaseService.GetAsync(model.AccountID);
        //    bool clearTrans = false;
        //    if(entity.IsAsset==true && model.IsAsset == false)
        //    {
        //        clearTrans = true;
        //    }
        //    //Asign value
        //    entity.IsDetailAccount = model.IsDetailAccount;
        //    entity.AccountName = model.AccountName;
        //    entity.AccountNameEnglish = model.AccountNameEnglish;
        //    entity.StartDate = model.StartDate;
        //    entity.FinishDate = model.FinishDate;
        //    entity.IsCredit = model.IsCredit;
        //    entity.BankName = model.BankName;
        //    entity.BankAccountNo = model.BankAccountNo;
        //    entity.OwnerName = model.OwnerName;
        //    entity.IsAsset = model.IsAsset;
        //    entity.Balance = model.Balance;
        //    if (entity.IsDetailAccount)
        //    {
        //        if (!entity.IsCredit)
        //        {
        //            entity.BankName = null;
        //            entity.BankAccountNo = null;
        //            entity.OwnerName = null;
        //        }
        //        if (!entity.IsAsset)
        //        {
        //            if (clearTrans == true)
        //            {
        //                TransactionEditViewModel clearTransaction = new TransactionEditViewModel();
        //                clearTransaction.AccountId = model.AccountID;
        //                clearTransaction.CurrencyCode = Enum.GetName(typeof(HmsService.Models.Currency), model.CurrencyID);
        //                clearTransaction.Date = DateTime.Now;
        //                if (model.Balance == null)
        //                {
        //                    clearTransaction.Amount = 0;
        //                }
        //                else {
        //                    clearTransaction.Amount = (decimal)model.Balance;
        //                }
        //                clearTransaction.Notes = "Xóa tài sản";
        //                clearTransaction.IsIncreaseTransaction = false;
        //                var transactionApi = new TransactionApi();
        //                await transactionApi.CreateTransactionAsync(clearTransaction);
        //            }
        //            entity.Balance = null;
        //        }
        //        else
        //        {
        //            if (entity.LatestUpdateBalance == null)
        //            {
        //                entity.LatestUpdateBalance = Utils.GetCurrentDateTime();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        entity.BankName = null;
        //        entity.BankAccountNo = null;
        //        entity.OwnerName = null;
        //        entity.Balance = null;
        //        entity.IsAsset = false;
        //        entity.IsCredit = false;
        //    }
        //    if (entity.StartDate.HasValue)
        //    {
        //        entity.StartDate = entity.StartDate.Value.Date + new TimeSpan(0, 0, 0);
        //        entity.FinishDate = entity.FinishDate.Value.Date + new TimeSpan(0, 0, 0);
        //    }
        //    //Set start level
        //    //Đợi hỏi thầy về edit
        //    entity.Level_ = 1;
        //    //Set CategoryId and ParentId
        //    //Đợi hỏi lại thầy
        //    await this.BaseService.UpdateAsync(entity);
        //}

        public async Task UpdateAccountAsync(AccountViewModel model)
        {
            var entity = await this.BaseService.GetAsync(model.AccountID);

            entity.AccountName = model.AccountName;
            entity.Level_ = model.Level_;
            entity.StartDate = model.StartDate;
            entity.FinishDate = model.FinishDate;
            entity.Balance = model.Balance;
            entity.ProductCode = model.ProductCode;
            entity.MembershipCardId = model.MembershipCardId;
            entity.Type = model.Type; //Change to property
            entity.BrandId = model.BrandId;
            entity.Active = model.Active;

            await this.BaseService.UpdateAsync(entity);
        }

        public Account GetAccountEntityByAccountCode(string accountCode)
        {
            return this.BaseService.GetAccountEntityByAccountCode(accountCode);
        }

        public async System.Threading.Tasks.Task UpdateAccountCustomerAsync(AccountEditViewModel model)
        {
            var entity = await this.BaseService.GetAsync(model.AccountID);
            //TODO: Update lại sau khi đổi entities
            //entity.MembershipCard.CustomerId = model.Me.CustomerID;
            await this.BaseService.UpdateAsync(entity);
        }

        public Account GetAccountEntityById(int accountId)
        {
            return this.BaseService.GetAccountById(accountId);
        }

        public async Task ActivateAccountAsync(AccountViewModel model)
        {
            model.Active = true;
            await this.EditAsync(model.AccountID, model);
        }

        public async System.Threading.Tasks.Task DeactivateAccountAsync(AccountViewModel model)
        {
            model.Active = false;
            await this.EditAsync(model.AccountID, model);
        }

        public async Task DeactivateAccountAsync(int id)
        {
            var entity = await this.BaseService.FirstOrDefaultAsync(a => a.AccountID == id);
            entity.Active = false;
            await this.BaseService.UpdateAsync(entity);
        }

        public async Task<AccountViewModel> GetAccountByAccountCode(string AccountCode)
        {
            var account = await this.BaseService.GetAccountByAccountCodeAsync(AccountCode);
            if (account == null)
            {
                return null;
            }
            else
            {
                return new AccountViewModel(account);
            }
        }

        public async Task<AccountViewModel> GetAccountByAccountName(string AccountName)
        {
            var account = await this.BaseService.GetAccountByAccountNameActiveAsync(AccountName);
            if (account == null)
            {
                return null;
            }
            else
            {
                return new AccountViewModel(account);
            }
        }

        //public IQueryable<AccountViewModel> GetActiveAccounts(int? brandId)
        //{
        //    return this.BaseService.GetActiveAccount(brandId).ProjectTo<AccountViewModel>(this.AutoMapperConfig);
        //}

        public IQueryable<Account> GetAccountByMembershipId(int MembershipCardID)
        {
            return this.BaseService.GetAccountByMembershipId(MembershipCardID);
        }

        public IQueryable<Account> GetAllAccountsByMembershipId(int membershipCardId)
        {
            return this.BaseService.GetAllAccountsByMembershipId(membershipCardId);
        }

        //public IQueryable<AccountViewModel> GetActiveAccountsIndex(int? brandId)
        //{
        //TODO: Thêm membershipcard vào AccountViewModel
        //try
        //{
        //    var result = this.BaseService.GetActiveAccount(brandId)
        //        .Select(q => new AccountViewModel()
        //        {
        //            AccountID = q.AccountID,
        //            CustomerID = q.CustomerID,
        //            AccountCode = q.AccountCode,
        //            AccountName = q.AccountName,
        //            Balance = q.Balance,
        //            AccountNameEnglish = q.AccountNameEnglish,
        //            FinishDate = q.FinishDate,
        //            StartDate = q.StartDate,
        //            Customer = q.Customer != null ? new CustomerViewModel()
        //            {
        //                CustomerID = q.Customer.CustomerID,
        //                AccountID = q.Customer.AccountID,
        //                Name = q.Customer.Name,
        //            } : null,
        //        });
        //    return result;
        //}
        //catch (Exception e)
        //{
        //    return null;
        //}
        //}

        public async Task DeactivateAllAccountExceptCredit(int id)
        {
            var list = this.BaseService.Get(a => a.MembershipCardId == id).ToList();
            foreach (var item in list)
            {
                item.Active = false;
                await this.BaseService.UpdateAsync(item);
            }
        }

        public virtual void CreateAccountByMemCard(string cardCode, decimal? defaultBalance, int brandId, int cardId, int accTypeId)
        {
            AccountViewModel account = new AccountViewModel();
            account.AccountCode = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            account.AccountName = cardCode + "NewUser";
            account.Balance = defaultBalance ?? 0;
            account.Type = accTypeId;
            account.BrandId = brandId;
            account.Level_ = 0;
            account.Active = true;
            account.MembershipCardId = cardId;            
            this.Create(account);
        }
        public virtual void CreateGiftAccountByMemCard(string cardCode, decimal? defaultBalance, int brandId, int cardId, int accTypeId, string productCode)
        {
            AccountViewModel account = new AccountViewModel();
            account.AccountCode = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            account.AccountName = cardCode + "NewUser";
            account.Balance = defaultBalance ?? 0;
            account.Type = accTypeId;
            account.ProductCode = productCode;
            account.BrandId = brandId;
            account.Level_ = 0;
            account.Active = true;
            account.MembershipCardId = cardId;
            this.Create(account);
        }

        //public Account GetMembershipAccountByCardId(int cardId)
        //{
        //    var account = this.BaseService.FirstOrDefault(a => a.MembershipCardId == cardId && a.AccountType.Type == (int)MembershipCardTypeEnum.PointCard);
        //    return account;
        //}

        //public Account GetMembershipAccountByCustomerId(int customerId)
        //{
        //    return this.BaseService.FirstOrDefault(q => q.Active.Value && q.MembershipCard.CustomerId == customerId && q.AccountType.Type == (int)MembershipCardTypeEnum.PointCard);
        //}

        public bool ValidateCard(MembershipCard card)
        {
            var listAccount = this.BaseService.Get(a => a.MembershipCardId == card.Id);
            if (listAccount.Count() == 0) return true;
            else if (listAccount.FirstOrDefault(a => a.Level_ != (int)MembershipCardLevel.Copper) != null) return false;
            else return true;
        }

        //public bool IsAvailableToLevelUp(Account acc)
        //{
        //    try
        //    {
        //        if (acc.AccountType.Type != (int)MembershipCardTypeEnum.PointCard)
        //        {
        //            return false;
        //        }
        //        else if (acc.Balance < (int)MembershipCardLevelPoint.LevelSilver)
        //        {
        //            return true;
        //        }
        //        else if (acc.Balance >= (int)MembershipCardLevelPoint.LevelSilver && acc.Balance < (int)MembershipCardLevelPoint.LevelGold && acc.Level_ < (int)MembershipCardLevel.Silver)
        //        {
        //            return true;
        //        }
        //        else if (acc.Balance >= (int)MembershipCardLevelPoint.LevelGold && acc.Level_ < (int)MembershipCardLevel.Gold)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        public IQueryable<AccountEditViewModel> GetAccountsByMembershipCardId(int cardId)
        {
            var list = this.BaseService.Get(a => a.Active.Value)
                                       .Where(a => a.MembershipCardId != null && a.MembershipCardId == cardId)
                                       .Select(a => new AccountEditViewModel
                                       {
                                           AccountCode = a.AccountCode,
                                           AccountID = a.AccountID,
                                           AccountName = a.AccountName,
                                           Active = a.Active,
                                           Balance = a.Balance,
                                           BrandId = a.BrandId,
                                           FinishDate = a.FinishDate,
                                           Level_ = a.Level_,
                                           MembershipCardId = a.MembershipCardId,
                                           ProductCode = a.ProductCode,
                                           StartDate = a.StartDate,
                                           Type = a.Type,
                                           isServer = true,
                                       });

            return list;
        }

        public void DeactivateAccount(int id)
        {
            var acc = this.BaseService.FirstOrDefault(a => a.AccountID == id);
            if (acc != null)
            {
                acc.Active = false;
            }
            this.BaseService.Update(acc);
        }
    }
}
