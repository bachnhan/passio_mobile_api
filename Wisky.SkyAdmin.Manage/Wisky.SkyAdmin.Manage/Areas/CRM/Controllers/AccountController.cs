using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.CRM.Controllers
{
    public class AccountController : DomainBasedController
    {
        // GET: CRM/Account
        public async Task<ActionResult> Index(int Id)
        {
            var customerApi = new CustomerApi();
            var model = await customerApi.GetAsync(Id);
            return View(model);
        }

        public ActionResult IndexAccounts()
        {
            return View();
        }

        public async Task<ActionResult> Detail(int Id)
        {
            var accountApi = new AccountApi();
            var model = await accountApi.GetAsync(Id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        public ActionResult Test()
        {
            var accoutApi = new AccountApi();
            var list = accoutApi.GetAccountByCusId(1);
            return View(list);
        }

        public JsonResult GetAccountsWithFilter(int id, int filterCard)
        {
            try
            {
                var accountApi = new AccountApi();
                var accounts = accountApi.GetAccountByMembershipId(id);

                if (filterCard >= 0)
                {
                    accounts = accounts.Where(a => a.Type == filterCard);
                }

                var rs = accounts.Select(q => new
                {
                    Text = q.AccountName,
                    Value = q.AccountID
                });

                return Json(new { success = true, accountList = rs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Create(int Id)
        {
            var AccountViewModel = new AccountEditViewModel();
            PrepareCreate(AccountViewModel, Id);
            AccountViewModel.Customer.CustomerID = Id;
            AccountViewModel.StartDate = AccountViewModel.FinishDate = DateTime.Now;

            //return View(AccountViewModel);
            return View("Create", AccountViewModel);
        }


        [HttpPost]
        public async Task<ActionResult> Create(AccountEditViewModel model)
        {
            var accountApi = new AccountApi();
            var customerApi = new CustomerApi();

            //try
            //{
            var Customer = customerApi.GetCustomerById((int)model.Customer.CustomerID);
            accountApi.Create(model);
            if (Customer.DefaultAccount.AccountName == null)
            {
                await SetDefaultAccountWithId(model.AccountID);
                //return Json(new
                //{
                //    success = true,
                //    message = "Tạo tài khoản thành công!",
                //}, JsonRequestBehavior.AllowGet);
                return RedirectToAction("CustomerDetail", "Customer", new { Id = model.Customer.CustomerID });
            }
            else
            {
                return RedirectToAction("CustomerDetail", "Customer", new { Id = model.Customer.CustomerID });
            }
            //}
            //catch (Exception)
            //{
            //    return Json(new { success = false, message = "Error" }, JsonRequestBehavior.AllowGet);
            //}

           // var customer = customerApi.GetCustomerById(customerID);
            //if (customer.DefaultAccount == null || customer.AccountID == null)
            //{
            //    return RedirectToAction("SetDefaultAccountWithId", "Customer", new { Id = model.AccountID });
            //}
            //return RedirectToAction("Index", "Account", new { Id = model.CustomerID });
        }
        public async Task<JsonResult> SetDefaultAccountWithId(int Id)
        {
            var customerApi = new CustomerApi();
            var accountApi = new AccountApi();
            var customer = await customerApi.GetCustomerByAccountIdAsync(Id);
            try
            {
                await customerApi.EditDefaultAccountAsync(customer, Id);
                var defaultAcc = accountApi.Get(Id);

                defaultAcc.Active = true;
                await accountApi.EditAsync(Id, defaultAcc);

                var defaultAccId = defaultAcc.AccountID;
                var defaultAccName = defaultAcc.AccountName;
                var defaultAccBalance = defaultAcc.Balance;

                return Json(new { success = true, detail = new { accID = defaultAccId, accName = defaultAccName, balance = defaultAccBalance }, message = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error" }, JsonRequestBehavior.AllowGet);
            }
            //return RedirectToAction("Index", "Account", new { Id = customer.CustomerID });
        }

        private void PrepareCreate(AccountEditViewModel model, int Id)
        {
            var customerApi = new CustomerApi();
            model.Customer = customerApi.GetCustomerById(Id);
            //= customerApi.GetAllCustomer(brandId).Select(q => new SelectListItem()
            //{
            //    Text = q.Customer.Name,
            //    Value = q.Customer.CustomerID.ToString(),
            //    Selected = false,
            //});
        }

        public async Task<ActionResult> Edit(int Id)
        {
            var accountApi = new AccountApi();
            var customerApi = new CustomerApi();
            var model = await accountApi.GetAsync(Id);
            model.Customer= await customerApi.GetCustomerByAccountIdAsync(Id);
            //return View(model);
            return View("Edit", model);
        }

        //[HttpPost]
        //public async Task<ActionResult> Edit(AccountViewModel model)
        //{
        //    //if (!this.ModelState.IsValid)
        //    //{
        //    //    return View(model);
        //    //}

        //    var accountApi = new AccountApi();
        //    try
        //    {
        //        await accountApi.UpdateAccountAsync(model);
        //        return RedirectToAction("CustomerDetail", "Customer", new { Id = model.Customer.CustomerID });
        //        //return Json(new { id = model.AccountID, name = model.AccountName, finish = model.FinishDate, bank = model.BankName, balance = model.Balance.GetValueOrDefault().ToString("#,##"), success = true, message = "Cập nhật tài khoản thành công!" });
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { success = false, message = "Error" }, JsonRequestBehavior.AllowGet);
        //    }
        //    //return RedirectToAction("IndexAccounts", "Account");
        //}

        public async Task<JsonResult> ActivationToggle(int Id)
        {
            var accountApi = new AccountApi();
            var account = accountApi.Get(Id);
            bool inactive = !account.Active.Value;
            if (inactive)
            {
                return await Activate(account);
            }
            else
            {
                return await Deactivate(account);
            }
        }

        public async Task<JsonResult> Activate(AccountViewModel model)
        {
            try
            {
                var accountApi = new AccountApi();
                if (model == null || model.Active.Value)
                {
                    return Json(new { success = false, message = "Kích hoạt tài khoản thất bại. Xin thử lại!" });
                }
                await accountApi.ActivateAccountAsync(model);
                return Json(new { success = true, isActivated = model.Active, message = "Kích hoạt tài khoản thành công!" });
            }
            catch
            {
                return Json(new { success = false, message = "Kích hoạt tài khoản thất bại. Xin thử lại!" });
            }
        }

        public async Task<JsonResult> Deactivate(AccountViewModel model)
        {
            try
            {
                var accountApi = new AccountApi();
                if (model == null || !model.Active.Value)
                {
                    return Json(new { success = false, message = "Vô hiệu hóa tài khoản thất bại. Xin thử lại!" });
                }
                await accountApi.DeactivateAccountAsync(model);
                return Json(new { success = true, isActivated = model.Active, message = "Vô hiệu hóa tài khoản thành công!" });
            }
            catch
            {
                return Json(new { success = false, message = "Vô hiệu hóa tài khoản thất bại. Xin thử lại!" });
            }
        }

        //public async Task<JsonResult> Delete(int Id)
        //{
        //    try
        //    {
        //        var accountApi = new AccountApi();
        //        var model = await accountApi.GetAsync(Id);
        //        if (model == null || model.Inactive)
        //        {
        //            return Json(new { success = false });
        //        }
        //        await accountApi.DeleteAccountAsync(model);
        //        return Json(new { success = true });
        //    }
        //    catch (System.Exception e)
        //    {
        //        return Json(new { success = false });
        //    }
        //}

        //public JsonResult GetListAccounts(JQueryDataTableParamModel param, int? Id, int brandId)
        //{
        //    int count = 0;
        //    IEnumerable<IConvertible[]> rs = null;
        //    int totalRecords;
        //    int displayRecord;
        //    var accountApi = new AccountApi();
        //    try
        //    {
        //        var accounts = accountApi.GetActiveAccountsIndex(brandId).ToList();
        //        totalRecords = accounts.Count();

        //        count = param.iDisplayStart + 1;

        //        //rs = (await accounts
        //        //            .OrderByDescending(a => a.Customer.Name)
        //        //        .Skip(param.iDisplayStart)
        //        //        .Take(param.iDisplayLength)
        //        //        .ToListAsync())
        //        rs = accounts.Select(a => new IConvertible[]
        //                    {
        //                count++,
        //                a.Customer != null ? a.Customer.Name : "",
        //                a.AccountCode,
        //                a.AccountName,
        //                a.Balance,
        //                a.Customer != null ? ( a.Customer.AccountID == a.AccountID ? 2 : 1 ): 0,
        //                a.AccountID,
        //                    });

        //        displayRecord = accounts.Count();

        //        return Json(new
        //        {
        //            sEcho = param.sEcho,
        //            iTotalRecords = totalRecords,
        //            iTotalDisplayRecords = displayRecord,
        //            aaData = rs
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new { success = false });
        //    }
        //}

        public async Task<JsonResult> LoadTransactionByAccountOrCustomer(JQueryDataTableParamModel param, int? Id, int cardId, DateTime? FromDate, DateTime? ToDate)
        {

            
            int count = 0;
            int totalRecords;
            int displayRecords;
            var accountApi = new AccountApi();
            var transactionApi = new TransactionApi();
            var transactions = transactionApi.GetTransactionByCardId(cardId);
            if (Id != 0)
            {
                transactions = transactions.Where(q => q.AccountId == Id.Value);
            }
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                transactions = transactions.Where(a => a.Amount.ToString().Contains(param.sSearch));
            }
            if(FromDate != null && ToDate != null)
            {
                transactions = transactions.Where(q => DateTime.Compare(q.Date, FromDate.Value) >= 0 && DateTime.Compare(q.Date, ToDate.Value) <= 0);
            }

            count = param.iDisplayStart + 1;
            totalRecords = await transactions.CountAsync();

            try
            {
                if (transactions.Count() > 0)
                {
                    var rs = (await transactions
                             .OrderByDescending(a => a.Id)
                             .Skip(param.iDisplayStart)
                             .Take(param.iDisplayLength)
                             .ToListAsync())
                             .Select(a => new IConvertible[]
                                 {
                            count++,
                            accountApi.Get(a.AccountId).AccountName,
                            accountApi.Get(a.AccountId).Active,
                            a.Amount.ToString("#,##"),
                            a.CurrencyCode,
                            a.Date.ToString("dd/MM/yyyy HH:mm:ss"),
                            a.IsIncreaseTransaction/* ? "Tăng" : "Giảm"*/,
                            a.Id
                                 });

                    displayRecords = await transactions.CountAsync();

                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = totalRecords,
                        iTotalDisplayRecords = displayRecords,
                        aaData = rs
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rs = transactions.ToList().Select(a => new IConvertible[]
                        {
                            accountApi.Get(a.AccountId).AccountName,
                            accountApi.Get(a.AccountId).Active,
                            a.Amount,
                            a.CurrencyCode,
                            a.Date.ToString("dd/MM/yyyy"),
                            a.IsIncreaseTransaction/* ? "Tăng" : "Giảm"*/,
                            a.Id
                        });
                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = rs
                    }, JsonRequestBehavior.AllowGet);
                }

            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng liên hệ admin!!!" });
            }
        }

        public async Task<ActionResult> GetCustomerAccountData(JQueryDataTableParamModel param, int Id, int modeSelect, int modeItemSelect)
        {
            var accountApi = new AccountApi();
            var membershipApi = new MembershipCardApi();
            //var membershipMappingApi = new MembershipCardTypeMappingApi();

            IQueryable<HmsService.Models.Entities.Account> customerAccounts;

            if((modeSelect == 0 && modeItemSelect == 0) || (modeSelect == 1 && modeItemSelect == 0))
            {
                customerAccounts = accountApi.GetAccountByCustomerId(Id);                
            }
            else if(modeSelect == 0 && modeItemSelect != 0)
            {
                customerAccounts = accountApi.GetAccountByCustomerIdAndMembershipId(Id, modeItemSelect);                
            }
            //else if(modeSelect == 1 && modeItemSelect != 0)
            //{
            //    customerAccounts = accountApi.GetAccountByCustomerIdAndMemberTypeId(Id, modeItemSelect);                
            //}
            else
            {
                return Json(new { success = false, message = "Error" }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    customerAccounts = customerAccounts.Where(a => a.AccountCode.Contains(param.sSearch)
                            || a.AccountName.Contains(param.sSearch));
                }
                var totalRecords = await customerAccounts.CountAsync();
                int count = param.iDisplayStart + 1;
                var rs = (await customerAccounts
                    .OrderBy(q => q.AccountName)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToListAsync())
                    .Select(q => new IConvertible[]
                        {
                                count++,//0
                                q.AccountCode,//1
                                q.AccountName,//2
                                //q.AccountTypeId,//3
                                q.MembershipCard.MembershipCardCode,//4
                                q.AccountID,//5
                                q.Active,//6
                                q.FinishDate,//7
                                q.ProductCode,//8
                                //q.BankName,
                                q.Balance.GetValueOrDefault().ToString("#,##"),//9
                        });

                var displayRecords = await customerAccounts.CountAsync();
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = displayRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);                
            }
            catch
            {
                return Json(new { success = false, message = "Error" }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetProductForAccountDetail(string Code)
        {
            try
            {
                var productApi = new ProductApi();
                var name = "N/A";
                if (productApi.GetProductByCode(Code) != null)
                {
                    name = productApi.GetProductByCode(Code).ProductName;
                }
                return Json(new
                {
                    success = true,
                    ProductName = name
                });
            }
            catch(Exception e)
            {
                return Json(new
                {
                    success = false,
                });
            }
        }

        //[HttpPost]
        public async Task<JsonResult> ValidateAccount(string AccountCode, string AccountName)
        {
            var accountApi = new AccountApi();
            string message;
            bool valid;
            bool validCode = await ValidateAccountCode(AccountCode);
            bool validName = await ValidateAccountName(AccountName);
            if (validCode && validName)
            {
                message = "success";
                valid = true;
            }
            else
            {
                if (!validCode && !validName)
                {
                    valid = false;
                    message = "duplicate both";
                }
                else
                {
                    valid = false;
                    message = !validName ? "duplicate name" : "duplicate code";
                }
            }
            return Json(new { success = valid, message = message });
        }

        public async Task<bool> ValidateAccountCode(string AccountCode)
        {
            var accountApi = new AccountApi();
            var account = await accountApi.GetAccountByAccountCode(AccountCode);
            return account == null;
        }

        public async Task<bool> ValidateAccountName(string AccountName)
        {
            var accountApi = new AccountApi();
            var account = await accountApi.GetAccountByAccountName(AccountName);
            return account == null;
        }

        public async Task<ActionResult> EditAccountCustomer(int Id, int brandId)
        {
            var accountApi = new AccountApi();
            var model = new AccountEditViewModel(await accountApi.GetAsync(Id), this.Mapper);
            PrepareCreate(model, brandId);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> EditAccountCustomer(AccountEditViewModel model)
        {
            var accountApi = new AccountApi();
            await accountApi.UpdateAccountCustomerAsync(model);
            return RedirectToAction("IndexAccounts", "Account");
        }

        #region Add balance to account
        public async Task<ActionResult> AddBalance(int Id)
        {
            var accountApi = new AccountApi();
            var model = await accountApi.GetAsync(Id);
            return PartialView("AddBalance", model);   
        }

        //[HttpPost]
        //public async Task<JsonResult> AddBalance(AccountViewModel model)
        //{
        //    var accountApi = new AccountApi();

        //    #region add transaction
        //    var transactionApi = new TransactionApi();
        //    TransactionEditViewModel transaction = new TransactionEditViewModel();
        //    transaction.AccountId = model.AccountID;
        //    transaction.Amount = model.addBalanceValue;
        //    transaction.CurrencyCode = "VND";
        //    transaction.Date = DateTime.Now;
        //    transaction.IsIncreaseTransaction = true;

        //    await transactionApi.CreateTransactionAsync(transaction);
        //    #endregion

        //    bool check;
        //    try
        //    {
        //        model.Balance = model.Balance + model.addBalanceValue;
        //        await accountApi.UpdateAccountAsync(model);
        //        check = true;
        //    }
        //    catch (Exception)
        //    {
        //        check = false;
        //    }
        //    return Json(new
        //    {
        //        success = check,

        //    });

        //}
        #endregion

        //[HttpPost]
        //public JsonResult FilterListByAccountMode(int accountMode, int customerId)
        //{
        //    var api = new MembershipCardApi();
        //    var typeApi = new MembershipTypeApi();
            
        //    if(accountMode == 0)
        //    {
        //        var list = api.GetMembershipCardActiveByCustomerId(customerId).Select(q => new { Id = q.Id, Name = q.MembershipCardCode}).ToArray();
        //        return Json(list);
        //    }
        //    else
        //    {
        //        var list = typeApi.GetListActive().Select(q => new { Id = q.Id, Name = q.TypeName}).ToArray();
        //        return Json(list);
        //    }
        //}
    }
}