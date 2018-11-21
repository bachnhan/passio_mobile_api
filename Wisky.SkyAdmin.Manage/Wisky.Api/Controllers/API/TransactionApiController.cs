using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using Newtonsoft.Json;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Wisky.Api.Connection;

namespace Wisky.Api.Controllers.API
{
    public class TransactionApiController : BaseController
    {
        private static readonly string _storeCode = System.Configuration.ConfigurationManager.AppSettings["config.store.code"];
        private readonly string _accessToken = System.Configuration.ConfigurationManager.AppSettings["config.order.token"];
        private readonly string _enableToken = System.Configuration.ConfigurationManager.AppSettings["config.order.EnableToken"];


        private readonly ICustomerService _customerService = DependencyUtils.Resolve<ICustomerService>();
        private readonly IProductService _productService = DependencyUtils.Resolve<IProductService>();
        private readonly IOrderService _orderService = DependencyUtils.Resolve<IOrderService>();

        [HttpPost]
        //[Route("api/trans/CreateTransaction/{token}/{terminalId}")]
        public JsonResult CreateTransaction(Card model)
        {
            var customerApi = new CustomerApi();
            var tracsactionApi = new TransactionApi();
            var storeApi = new StoreApi();
            var membershipCardApi = new MembershipCardApi();
            bool isSuccess = false;
            var store = storeApi.Get(model.terminalId);
            if (store != null)
            {
                try
                {
                    var membershipCard = membershipCardApi.GetMembershipCardByCode(model.code);
                    // Kiểm tra membership card có hợp lệ
                    if (membershipCard != null && membershipCard.Active == true
                        && membershipCard.Status == (int)MembershipStatusEnum.Active /* chưa kiểm tra membershipType */)
                    {
                        var customer = customerApi.GetCustomerEntityById(membershipCard.CustomerId.GetValueOrDefault());
                        var transactionModel = new TransactionEditViewModel()
                        {
                            AccountId = membershipCard.Accounts.Where(q => q.Type == (int)AccountTypeEnum.CreditAccount).FirstOrDefault()?.AccountID ?? 0,
                            IsIncreaseTransaction = model.isIncrease,
                            Amount = model.amount,
                            CurrencyCode = "VND"/*Enum.GetName(typeof(HmsService.Models.Currency), defautAccount..CurrencyID)*/,
                            Date = DateTime.Now,
                            FCAmount = 0,
                            Status = (int)TransactionStatus.Approve,
                            TransactionType = (int)TransactionTypeEnum.Default
                        };
                        isSuccess = tracsactionApi.CreateTransaction(transactionModel, customer, store.BrandId.Value);
                    }
                }
                catch (Exception ex)
                {
                    isSuccess = false;
                }
            }
            return Json(new { success = isSuccess });
        }

        [HttpPost]
        public JsonResult CreateTransaction2(int Enum, CardVersion2 model)
        {
            switch (Enum)
            {
                case (int)PaymentTypeEnum.MemberPayment:
                    return CreateTransactionInSystem(model);
                case (int)PaymentTypeEnum.GiftTalk:
                    return CreateTransactionForGiftTalk(model);
                default:
                    goto case (int) PaymentTypeEnum.MemberPayment;
            }
        }
        [HttpPost]
        public JsonResult CreateTransactionInSystem(CardVersion2 model)
        {
            var accountApi = new AccountApi();
            var customerApi = new CustomerApi();
            var transactionApi = new TransactionApi();
            var membershipCardApi = new MembershipCardApi();
            var storeApi = new StoreApi();
            var transList = new List<TransactionEditViewModel>();
            var brandId = storeApi.GetStoreById(model.terminalId).BrandId.Value;
            var isSuccess = true;
            try
            {
                var numOfChangeAccount = 0;
                foreach (var accDetail in model.accounts)
                {
                    if (accDetail.IsChange)
                    {
                        var account = accountApi.GetAccountEntityByAccountCode(accDetail.AccountCode);
                        numOfChangeAccount++;
                        if (account != null)
                        {
                            var transactionModel = new TransactionEditViewModel()
                            {
                                AccountId = account.AccountID,
                                IsIncreaseTransaction = accDetail.IsIncrease,
                                Amount = accDetail.ChangeAmount,
                                CurrencyCode = "VND",
                                Date = DateTime.Now,
                                FCAmount = 0,
                                StoreId = model.terminalId,
                                BrandId = brandId,
                                Status = (int)TransactionStatus.Approve,
                                TransactionType = (int)TransactionTypeEnum.Default,
                                UserId = !String.IsNullOrEmpty(model.UserId) ? model.UserId : null,
                            };

                            transList.Add(transactionModel);
                        }
                    }
                }

                if (numOfChangeAccount == transList.Count)
                {
                    foreach (var item in transList)
                    {
                        var account = accountApi.GetAccountEntityById(item.AccountId);
                        var customerId = membershipCardApi.GetMembershipCardById(account.MembershipCardId.GetValueOrDefault()).CustomerId;
                        var customer = customerApi.GetCustomerEntityById(customerId);
                        isSuccess = transactionApi.CreateTransaction(item, customer, brandId);
                        if (isSuccess == false)
                        {
                            return Json(new { success = false });
                        }
                    }
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }
        [HttpPost]
        public JsonResult CreateTransactionForGiftTalk(CardVersion2 model)
        {
            var storeApi = new StoreApi();
            int brandId = storeApi.GetStoreById(model.terminalId).BrandId.GetValueOrDefault();
            try
            {
                decimal amount = 0;
                foreach (var accDetail in model.accounts)
                {
                    amount += accDetail.ChangeAmount;
                }
                var giftTalkConnectApi = new GiftTalkConnectApi();
                var cardDetail = giftTalkConnectApi.ThirdPartyCardDetail(model.accounts[0].AccountCode, brandId, model.terminalId).result;
                var detail = new
                {
                    bill_id = model.bill_id,
                    Customer = model.Customer,
                    table_number = model.table_number
                };
                if (cardDetail.balance >= amount)
                {
                    var checkPayment = giftTalkConnectApi.Payment(cardDetail.account, brandId, model.terminalId,
                        amount, JsonConvert.SerializeObject(detail));
                    if (checkPayment.success == true)
                    {
                        return Json(new
                        {
                            success = true,
                            message = checkPayment.message,
                            Enum = checkPayment.Enum,
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = checkPayment.message,
                            Enum = checkPayment.Enum,
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Tài khoản không đủ số dư."
                    });
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể thanh toán"
                });
            }
            return Json(new
            {
                success = true,
                message = "Giao dịch thành công."
            });
        }
    }

    public class Card
    {
        public string token { get; set; }
        public int terminalId { get; set; }
        public string code { get; set; }
        public decimal amount { get; set; }
        public bool isIncrease { get; set; }
    }

    /// <summary>
    ///     Đây là card version 2.0
    ///     Nhận một list các account đã thay đổi 
    ///     và tạo transaction cho các account đó
    /// </summary>
    public class CardVersion2
    {
        public int terminalId { get; set; }
        public List<AccountDetail> accounts { get; set; }
        public string UserId { get; set; }
        public string table_number { get; set; }
        public string bill_id { get; set; }
        public string Customer { get; set; }
    }

    public class AccountDetail
    {
        public string AccountCode { get; set; }
        public bool IsChange { get; set; }
        public bool IsIncrease { get; set; }
        public decimal ChangeAmount { get; set; }

    }
}
