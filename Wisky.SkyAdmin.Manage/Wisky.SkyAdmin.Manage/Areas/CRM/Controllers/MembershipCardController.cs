using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Areas.CRM.Models;
using Wisky.SkyAdmin.Manage.Controllers;
using Newtonsoft.Json;

namespace Wisky.SkyAdmin.Manage.Areas.CRM.Controllers
{
    public class MembershipCardController : DomainBasedController
    {
        // GET: CRM/MembershipCard
        public ActionResult CreateAccount()
        {
            ProductApi api = new ProductApi();
            var array = api.BaseService.Get(a => a.Active).Select(a => new SelectListItem
            {
                Text = a.ProductName,
                Value = a.ProductID.ToString(),
            }).ToList();
            ViewBag.array = array;
            return PartialView();
        }

        public ActionResult Create(int id)
        {
            var model = new MembershipCardEditViewModels();
            model.CustomerId = id;
            PrepareModel(model);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Create(MembershipCardEditViewModels model)
        {
            try
            {
                List<AccountEditViewModel> list = JsonConvert.DeserializeObject<List<AccountEditViewModel>>(model.newAccounts);
                var cardApi = new MembershipCardApi();
                var accountApi = new AccountApi();

                #region Create Card
                model.Active = true;
                model.CreatedTime = DateTime.Now;
                model.Status = (int)MembershipStatusEnum.Inactive;

                var entity = model.ToEntity();

                var card = cardApi.BaseService.FirstOrDefault(a => a.Active && a.MembershipCardCode == model.MembershipCardCode);
                if (card != null)
                {
                    entity = card;
                }
                else
                {
                    card = cardApi.BaseService.FirstOrDefault(a => a.Active && a.CustomerId == null);
                    if (card != null)
                    {
                        entity = card;
                    }
                }

                entity.Active = model.Active;
                entity.C_Level = model.C_Level;
                entity.CSV = model.CSV;
                entity.Status = model.Status;
                entity.MembershipTypeId = model.MembershipTypeId;
                entity.CustomerId = model.CustomerId;
                if (card == null) entity.CreatedTime = model.CreatedTime;
                if (card == null)
                {
                    if (cardApi.ValidateCardName(model.MembershipCardCode))
                    {
                        entity.MembershipCardCode = model.MembershipCardCode;
                    }
                    else
                    {
                        entity.MembershipCardCode = DateTime.Now.ToString("ddMMyyyyhhmmssfff");
                    }
                }

                if (card == null) await cardApi.BaseService.CreateAsync(entity);
                else await cardApi.BaseService.UpdateAsync(entity);
                #endregion

                int cardId = entity.Id;

                #region Create Accounts
                foreach (AccountEditViewModel acc in list)
                {
                    acc.Active = true;

                    var entity2 = acc.ToEntity();

                    var account = accountApi.BaseService.FirstOrDefault(a => a.MembershipCardId == cardId && a.Type == acc.Type);
                    if (account != null)
                    {
                        entity2 = account;
                    }
                    else
                    {
                        account = accountApi.BaseService.FirstOrDefault(a => a.MembershipCardId == null && a.Type == acc.Type);
                        if (account != null)
                        {
                            entity2 = account;
                        }
                        else
                        {
                            account = accountApi.BaseService.FirstOrDefault(a => a.MembershipCardId == null && a.Type == null);
                            if (account != null)
                            {
                                entity2 = account;
                            }
                        }
                    }

                    if (account == null) entity2.AccountCode = acc.AccountCode;
                    entity2.AccountName = acc.AccountName;
                    entity2.StartDate = acc.StartDate;
                    entity2.FinishDate = acc.FinishDate;
                    entity2.Balance = acc.Balance;
                    entity2.MembershipCardId = cardId;
                    entity2.Type = acc.Type;
                    entity2.BrandId = model.BrandId;
                    entity2.Active = acc.Active;
                    if (acc.Type == (int)AccountTypeEnum.GiftAccount) entity2.ProductCode = acc.ProductCode;
                    else entity2.ProductCode = null;

                    if (account == null) await accountApi.BaseService.CreateAsync(entity2);
                    else await accountApi.BaseService.UpdateAsync(entity2);
                }
                #endregion

                return Json(new { success = true });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, mess = ex.Message });
            }
        }

        public ActionResult Detail(int id)
        {
            var cardApi = new MembershipCardApi();
            var card = cardApi.BaseService.Get(a => a.Active && a.CustomerId != null).FirstOrDefault(a => a.Id == id);         
            var model = card.ToViewModel<HmsService.Models.Entities.MembershipCard, MembershipCardEditViewModels>();
            
            PrepareModel(model);
            return PartialView(model);
        }

        private void PrepareModel(MembershipCardEditViewModels model)
        {
            var typeAPi = new MembershipCardTypeApi();
            var accountApi = new AccountApi();           
            
            model.Type = typeAPi.Get(model.MembershipTypeId).TypeName;
            model.listType = typeAPi.BaseService.Get(a => a.Active.Value).Select(a => new SelectListItem
            {
                Text = a.TypeName,
                Value = a.Id.ToString(),
            });
            model.listAccounts = accountApi.GetAccountsByMembershipCardId(model.Id);
        }

        [HttpPost]
        public async Task<ActionResult> DeactivateOrActivateAccount(int id)
        {
            try
            {
                var cardApi = new MembershipCardApi();
                await cardApi.DeactivateOrActivateMembershipCard(id);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult ValidateCard(string cardCode)
        {
            IEnumerable<AccountEditViewModel> listAccount = null;
            object model = new object();

            try
            {
                var accountApi = new AccountApi();
                var cardApi = new MembershipCardApi();
                var productApi = new ProductApi();

                //var name = productApi.GetProductByCode().ProductName;
                var card = cardApi.GetMembershipCardByCode(cardCode);
                if (card != null)
                {
                    
                    listAccount = accountApi.GetAccountsByMembershipCardId(card.Id);
                    model = card.ToViewModel<HmsService.Models.Entities.MembershipCard, MembershipCardEditViewModels>();
                   
                }
                else
                {
                    listAccount = new List<AccountEditViewModel>();
                }

                int count = 1;
                var dataList = listAccount.Select(a => new IConvertible[] {
                    count++,
                    a.AccountCode,
                    a.AccountName,
                    ((AccountTypeEnum[])Enum.GetValues(typeof(AccountTypeEnum))).FirstOrDefault(b => (int)b == a.Type).DisplayName(),
                    a.Balance,
                    a.StartDate != null ? a.StartDate.Value.ToString("dd/MM/yyyy") : "",
                   // a.FinishDate != null ? a.FinishDate.Value.ToString("dd/MM/yyyy") : "",
                   a.ActiveTime= DateTime.Now.Subtract((DateTime)a.StartDate).Days,
                   // a.ProductCode == null ? "N/A" : a.ProductCode,
                    a.ProductCode == null ?  "N/A" : productApi.GetProductByCode(a.ProductCode).ProductName,
                    a.AccountID,
                    a.isServer,
                });

                return Json(new { success = true, data = dataList, model = model });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeactivateAccount(int id)
        {
            var accountApi = new AccountApi();
            try
            {
                await accountApi.DeactivateAccountAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteMembershipcard(int id)
        {
            var cardAPi = new MembershipCardApi();
            var accountApi = new AccountApi();
            var transactionApi = new TransactionApi();
            try
            {
                accountApi.DeactiveAccountByMembershipCardID(id);
                transactionApi.CancelAllTransaction(id);
                await cardAPi.DeactivateCardAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult InitCardsDatatable(int customerId)
        {
            try
            {
                var cardsApi = new MembershipCardApi();
                int count = 1;
                IEnumerable<HmsService.Models.Entities.MembershipCard> list = cardsApi.BaseService.Get(a => a.Active).Where(a => a.CustomerId != null && a.CustomerId == customerId);
                var data = list.Select(a => new IConvertible[]
                            {
                            count++,
                            a.MembershipCardCode == null ? "" : a.MembershipCardCode,
                            a.MembershipCardType == null ? "" : a.MembershipCardType.TypeName,
                            a.C_Level == null ? "" : ((MembershipCardLevel[])Enum.GetValues(typeof(MembershipCardLevel))).FirstOrDefault(b => (int)b == a.C_Level).DisplayName(),
                            a.Status == null ? "" : ((MembershipStatusEnum[])Enum.GetValues(typeof(MembershipStatusEnum))).FirstOrDefault(b => (int)b == a.Status).DisplayName(),
                            //((MembershipCardLevel[])Enum.GetValues(typeof(MembershipCardLevel))).FirstOrDefault(b => (int)b == a.C_Level) == null ? "" : ((MembershipCardLevel[])Enum.GetValues(typeof(MembershipCardLevel))).FirstOrDefault(b => (int)b == a.C_Level).DisplayName(),
                            //((MembershipStatusEnum[])Enum.GetValues(typeof(MembershipStatusEnum))).FirstOrDefault(b => (int)b == a.Status) == null ? "" : ((MembershipStatusEnum[])Enum.GetValues(typeof(MembershipStatusEnum))).FirstOrDefault(b => (int)b == a.Status).DisplayName(),
                            a.CreatedTime.ToString("dd/MM/yyyy"),
                            a.Id,
                            });

                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

