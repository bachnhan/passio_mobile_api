//using HmsService.Models;
//using HmsService.Sdk;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using HmsService.Models.Entities;
//using Wisky.SkyAdmin.Manage.Areas.CRM.Models;
//using System.Threading.Tasks;
//using HmsService.ViewModels;

//namespace Wisky.SkyAdmin.Manage.Areas.CRM.Controllers
//{
//    public class LevelUpController : Controller
//    {
//        // GET: CRM/LevelUp
//        public ActionResult Index()
//        {
//            return View();
//        }

//        public ActionResult Detail(int cardId)
//        {
//            var model = new MembershipCardLevelViewModels();
//            //model.CustomerId = customerId;
//            model.CurrentCardId = cardId;

//            var cardApi = new MembershipCardApi();
//            var card = cardApi.BaseService.FirstOrDefault(a => a.Id == cardId);
//            model.CardModel = card.ToViewModel<HmsService.Models.Entities.MembershipCard, MembershipCardEditViewModels>();

//            model.CustomerId = card.CustomerId.Value;

//            PrepareModel(model);
//            return PartialView(model);
//        }

//        private void PrepareModel(MembershipCardLevelViewModels model)
//        {
//            var accountApi = new AccountApi();
//            var cardApi = new MembershipCardApi();

//            var account = accountApi.GetMembershipAccountByCardId(model.CurrentCardId);
//            int? level = null;
//            if (account.Balance >= (int)MembershipCardLevelPoint.LevelSilver && account.Balance < (int)MembershipCardLevelPoint.LevelGold)
//            {
//                level = (int)MembershipCardLevel.Silver;
//            } else if (account.Balance >= (int)MembershipCardLevelPoint.LevelGold)
//            {
//                level = (int)MembershipCardLevel.Gold;
//            }
//            model.ListLevelUpCards = cardApi.BaseService.Get(a => a.CustomerId == null && a.Accounts.FirstOrDefault(b => b.Level_ == level && b.AccountType.Type == (int)MembershipCardTypeEnum.PointCard) != null)
//                .Select(a => new SelectListItem
//                {
//                    Text = a.MembershipCardCode,
//                    Value = a.Id.ToString(),
//                });
//        }

//        [HttpPost]
//        public async Task<ActionResult> LevelUp(int newCardId, int currentCardId)
//        {
//            var cardApi = new MembershipCardApi();
//            var accountApi = new AccountApi();
//            var typeApi = new MembershipCardTypeMappingApi();

//            try
//            {
//                var currentCard = await cardApi.BaseService.FirstOrDefaultAsync(a => a.Id == currentCardId);
//                var newCard = await cardApi.BaseService.FirstOrDefaultAsync(a => a.Id == newCardId);

//                newCard.CustomerId = currentCard.CustomerId;
//                newCard.CSV = currentCard.CSV;
//                newCard.Active = true;
//                newCard.Status = currentCard.Status;
//                newCard.CreatedTime = DateTime.Now;
//                newCard.BrandId = currentCard.BrandId;
//                newCard.GroupId = currentCard.GroupId;
                
//                await cardApi.BaseService.UpdateAsync(newCard);

//                currentCard.CustomerId = null;
//                await cardApi.BaseService.UpdateAsync(currentCard);

//                await typeApi.DeactivateAllMapping(newCard.Id);
//                var listCurrent = typeApi.BaseService.Get(a => a.MembershipCardId == currentCard.Id && a.Active.Value).ToList();
//                foreach (var item in listCurrent)
//                {
//                    item.MembershipCardId = newCard.Id;
//                    item.Active = true;
//                    await typeApi.BaseService.UpdateAsync(item);
//                }

//                await accountApi.DeactivateAllAccountExceptCredit(newCard.Id);
//                var listAccount = accountApi.BaseService.Get(a => a.MembershipCardId == currentCard.Id && a.Active.Value).ToList();
//                await accountApi.DeactivateAllAccountExceptCredit(currentCard.Id);
//                foreach (var item in listAccount)
//                {
//                    Account acc;
//                    if ((acc = newCard.Accounts.FirstOrDefault(a => a.AccountTypeId == item.AccountTypeId)) != null)
//                    {
//                        acc.AccountName = item.AccountName;
//                        acc.StartDate = DateTime.Now;
//                        acc.FinishDate = item.FinishDate;
//                        acc.Balance = item.Balance;
//                        acc.ProductCode = item.ProductCode;
//                        acc.BrandId = item.BrandId;
//                        acc.Active = true;
//                        await accountApi.BaseService.UpdateAsync(acc);
//                    } else
//                    {
//                        item.MembershipCardId = newCard.Id;
//                        item.Active = true;
//                        await accountApi.BaseService.UpdateAsync(item);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, error = ex.Message });
//            }

//            return Json(new { success = true });
//        }
//    }
//}