using HmsService.Models;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Wisky.Api.Controllers.API
{
    public class TransactionNewApiController : ApiController
    {
        private static readonly string _storeCode = System.Configuration.ConfigurationManager.AppSettings["config.store.code"];
        private readonly string _accessToken = System.Configuration.ConfigurationManager.AppSettings["config.order.token"];
        private readonly string _enableToken = System.Configuration.ConfigurationManager.AppSettings["config.order.EnableToken"];
        
        private readonly ICustomerService _customerService = DependencyUtils.Resolve<ICustomerService>();
        private readonly IProductService _productService = DependencyUtils.Resolve<IProductService>();
        private readonly IOrderService _orderService = DependencyUtils.Resolve<IOrderService>();

        [HttpPost]
        [Route("api/transaction/CreateTransaction/{token}/{terminalId}")]
        public async Task<IHttpActionResult> CreateTransaction(string token, string terminalId, [FromBody] Card card)
        {
            var accountApi = new AccountApi();
            var customerApi = new CustomerApi();
            var tracsactionApi = new TransactionApi();
            var membershipCardApi = new MembershipCardApi();
            bool isSuccess = true;

            try
            {
                var membershipCard = membershipCardApi.GetMembershipCardByCode(card.code);
                // Kiểm tra membership card có hợp lệ
                if (membershipCard != null && membershipCard.Active == true
                    && membershipCard.Status == 1 /* chưa kiểm tra membershipType */)
                {
                    var customer = customerApi.GetCustomerEntityById(membershipCard.CustomerId);

                    // Kiểm tra customer đã có tài khoản mặc định
                    if (customer.AccountID.HasValue)
                    {
                        var defautAccount = accountApi.GetAccountEntityById(customer.AccountID.Value);

                        var model = new TransactionEditViewModel()
                        {
                            AccountId = customer.AccountID.Value,
                            IsIncreaseTransaction = card.isIncrease,
                            Amount = card.amount,
                            CurrencyCode = Enum.GetName(typeof(HmsService.Models.Currency), defautAccount.CurrencyID),
                            Date = DateTime.Now,
                            FCAmount = 0,
                        };

                        await tracsactionApi.CreateTransactionAsync(model);
                    }
                    else
                    {
                        isSuccess = false;
                    }
                }
            }
            catch (Exception)
            {
                isSuccess = false;
            }

            return Json(new { success = isSuccess });
        }
    }
}
