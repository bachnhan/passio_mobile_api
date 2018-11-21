using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using HmsService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class TransactionApi
    {
        public async System.Threading.Tasks.Task EditTransactionAsync(TransactionEditViewModel model)
        {
            var entity = await this.BaseService.GetAsync(model.Id);
            //Asign value
            entity.CurrencyCode = model.CurrencyCode;
            entity.Amount = model.Amount;
            entity.FCAmount = model.FCAmount;
            entity.Date = model.Date;
            entity.Notes = model.Notes;
            entity.IsIncreaseTransaction = model.IsIncreaseTransaction;

            await this.BaseService.UpdateAsync(entity);
        }
        public IQueryable<TransactionEditViewModel> GetAllTransaction()
        {
            return this.BaseService.GetAllTransaction()
                .ProjectTo<TransactionEditViewModel>(this.AutoMapperConfig);
        }

        public IQueryable<TransactionViewModel> GetAllBrandByTimeRange(int brandId, DateTime startTime, DateTime endTime)
        {
            return this.BaseService.GetAllBrandByTimeRange(brandId, startTime, endTime)
                .ProjectTo<TransactionEditViewModel>(this.AutoMapperConfig);
        }

        public IEnumerable<Transaction> GetAllTransactionByStoreIDIEnum(int branId, int storeId, DateTime startTime, DateTime endTime)
        {
            if (storeId > 0)
            {
                return this.BaseService.GetAllTransactionByStoreIdWithDateRange(storeId, startTime, endTime).AsEnumerable();
            } else
            {
                return this.BaseService.GetAllBrandByTimeRange(branId, startTime, endTime).AsEnumerable();
            }
        }

        public IQueryable<Transaction> GetAllTransactionByStoreIDIQuery(int branId, int storeId, DateTime startTime, DateTime endTime)
        {
            if (storeId > 0)
            {
                return this.BaseService.GetAllTransactionByStoreIdWithDateRange(storeId, startTime, endTime);
            }
            else
            {
                return this.BaseService.GetAllBrandByTimeRange(branId, startTime, endTime);
            }
        }

        public void CancelAllTransaction(int membershipCardId)
        {
            var accountApi = new AccountApi();
            var accounts = accountApi.BaseService.Get(q => q.MembershipCardId == membershipCardId).ToList();
            foreach (var item in accounts)
            {
                var transactions = this.BaseService.Get(q => q.AccountId == item.AccountID).ToList();
                foreach (var itemTrans in transactions)
                {
                    itemTrans.Status = (int)TransactionStatus.Cancel;
                    this.BaseService.Update(itemTrans);
                }
            }
        }

        public IQueryable<Transaction> GetAllTransactionbyEntity()
        {
            return this.BaseService.GetAllTransaction();

        }

        public IQueryable<TransactionEditViewModel> GetAllTransactionByStoreId(int storeId)
        {
            return this.BaseService.GetAllTransactionByStoreId(storeId)
                .ProjectTo<TransactionEditViewModel>(this.AutoMapperConfig);
        }
            
        public IQueryable<TransactionEditViewModel> GetAllTransactionByStoreIdWithDateRange(int storeId, DateTime startTime, DateTime endTime)
        {
            return this.BaseService.GetAllTransactionByStoreIdWithDateRange(storeId, startTime, endTime)
                .ProjectTo<TransactionEditViewModel>(this.AutoMapperConfig);
        }

        public IQueryable<Transaction> GetAllTransactionByStoreIdWithDateRangeEntity(int storeId, DateTime startTime, DateTime endTime)
        {
            return this.BaseService.GetAllTransactionByStoreIdWithDateRange(storeId, startTime, endTime);
        }

        public IQueryable<TransactionViewModel> GetTransactionByCustomerId(int? Id)
        {
            return this.BaseService.GetCustomersTransactionsByCardId(Id.Value).ProjectTo<TransactionViewModel>(this.AutoMapperConfig);
        }

        public IQueryable<TransactionViewModel> GetTransactionByCardId(int? Id)
        {
            return this.BaseService.GetCustomersTransactionsByCardId(Id.Value).ProjectTo<TransactionViewModel>(this.AutoMapperConfig);
        }

        public async System.Threading.Tasks.Task CreateTransactionAsync(TransactionEditViewModel model, Customer customer, int brandId)
        {
            var accountApi = new AccountApi();
            var account = await accountApi.GetAsync(model.AccountId);
            var message = "";
            account.Balance = account.Balance.HasValue ? account.Balance : 0;
            var membershipCard = (new MembershipCardApi()).GetMembershipCardById(account.MembershipCardId.GetValueOrDefault());
            if (model.Status == (int)TransactionStatus.Approve)
            {
                if (model.IsIncreaseTransaction)
                {
                    account.Balance += model.Amount;
                    message = (new BrandApi().Get(account.BrandId).BrandName) + ". "
                        + DateTime.Now.ToString("dd/MM/yyyy") + ";" + DateTime.Now.ToString("HH:mm") +".  "
                        + "TK: "+ "xxxxx" + membershipCard.MembershipCardCode.Substring(5,membershipCard.MembershipCardCode.Length - 5) +". "
                        + "Nạp tiền: " + Utils.ToMoney((double)model.Amount) + "VNĐ" +". "
                        + "Số dư tài khoản: " + Utils.ToMoney((double)account.Balance)+"VNĐ";
                }
                else
                {
                    account.Balance -= model.Amount;
                    message = (new BrandApi().Get(account.BrandId).BrandName) + ". "
                        + DateTime.Now.ToString("dd/MM/yyyy") + ";" + DateTime.Now.ToShortTimeString() +"."
                        + "TK: xxxxx" + membershipCard.MembershipCardCode.Substring(5, membershipCard.MembershipCardCode.Length - 5) +". "
                        + "Thanh toán: " + Utils.ToMoney((double)model.Amount) +" VNĐ" +". "
                        + "Tại: " + (new StoreApi().GetStoreById(model.StoreId)).Name +". "
                        + "Số dư tài khoản: " + Utils.ToMoney((double)account.Balance) + " VNĐ";
                }
                accountApi.Edit(account.AccountID, account);
            }
            await this.CreateAsync(model);
            if (customer != null && customer.Phone != null && model.Status == (int)TransactionStatus.Approve)
            {
                try
                {
                    Utils.SendSMS(customer.Phone, message, brandId);
                }
                catch (Exception)
                {
                }
               
            }
        }

        public bool CreateTransaction(TransactionEditViewModel model, Customer customer, int brandId)
        {
            try
            {
                var accountApi = new AccountApi();
                var account = accountApi.Get(model.AccountId);
                var message = "";
                account.Balance = account.Balance.HasValue ? account.Balance : 0;
                var membershipCard = (new MembershipCardApi()).GetMembershipCardById(account.MembershipCardId.GetValueOrDefault());
                if (model.Status == (int)TransactionStatus.Approve)
                {
                    if (account.Type == (int)AccountTypeEnum.CreditAccount)
                    {
                        if (model.IsIncreaseTransaction)
                        {
                            account.Balance += model.Amount;
                            message = (new BrandApi().Get(account.BrandId).BrandName) + ". "
                                + DateTime.Now.ToString("dd/MM/yyyy") + ";" + DateTime.Now.ToString("HH:mm") + ". "
                                + "TK: " + "xxxxx" + membershipCard.MembershipCardCode.Substring(5, membershipCard.MembershipCardCode.Length - 5) + ". "
                                + "Nạp tiền: " + Utils.ToMoney((double)model.Amount) + "VNĐ" + ". "
                                + "Số dư tài khoản: " + Utils.ToMoney((double)account.Balance) + "VNĐ";
                        }
                        else
                        {
                            account.Balance -= model.Amount;
                            
                            message = (new BrandApi().Get(account.BrandId).BrandName) + ". "
                                + DateTime.Now.ToString("dd/MM/yyyy") + ";" + DateTime.Now.ToString("HH:mm") + ". "
                                + "TK: xxxxx" + membershipCard.MembershipCardCode.Substring(5, membershipCard.MembershipCardCode.Length - 5) + ". "
                                + "Thanh toán: " + Utils.ToMoney((double)model.Amount) + " VNĐ" + "."
                                + "Tại: " + (new StoreApi().GetStoreById(model.StoreId)).Name + "."
                                + "Số dư tài khoản: " + Utils.ToMoney((double)account.Balance) + " VNĐ";
                        }
                        accountApi.Edit(account.AccountID, account);
                    } else
                    {
                        if (account.Type == (int)AccountTypeEnum.PointAccount)
                        {
                            if (model.IsIncreaseTransaction)
                            {
                                account.Balance = account.Balance + model.Amount;
                            } else
                            {
                                account.Balance = account.Balance - model.Amount;
                            }
                            accountApi.Edit(account.AccountID, account);
                        }
                    }
                    
                }
                this.Create(model);
                if (customer != null && customer.Phone != null && model.Status == (int)TransactionStatus.Approve && message != "")
                {
                    try
                    {
                        Utils.SendSMS(customer.Phone, message, brandId);
                    }
                    catch (Exception ex)
                    {
                    }
                   
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async System.Threading.Tasks.Task DeleteTracsactionAsync(TransactionEditViewModel model)
        {
            var accountApi = new AccountApi();
            var account = await accountApi.GetAsync(model.AccountId);

            if (model.IsIncreaseTransaction)
            {
                account.Balance -= model.Amount;
            }
            else
            {
                account.Balance += model.Amount;
            }

            //await accountApi.UpdateAccountAsync(account);
            await this.DeleteAsync(model);
        }
    }
}
