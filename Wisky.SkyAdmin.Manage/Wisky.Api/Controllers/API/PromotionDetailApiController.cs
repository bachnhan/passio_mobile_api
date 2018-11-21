using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Wisky.Api.Controllers.API
{
    public class PromotionDetailApiController : ApiController
    {
        [HttpPost]
        [Route("api/promotionDetail/GetPromotionDetails/{token}/{terminalId}")]
        public HttpResponseMessage GetPromotionDetails(string token, string terminalId, string voucherCode)
        {
            Utils.CheckToken(token);
            var voucherApi = new VoucherApi();
            var entity = voucherApi.GetEntityVoucherByCode(voucherCode);
            if(entity == null)
            {
                return new HttpResponseMessage()
                {
                    Content = new JsonContent(new
                    {
                        Message = "Không tìm thấy mã Voucher",
                    })
                };
            }
            if(entity.UsedQuantity >= entity.Quantity)
            {
                return new HttpResponseMessage()
                {
                    Content = new JsonContent(new
                    {
                        Message = "Đã hết số lượng Voucher",
                    })
                };
            }
            var promoDetail = new PromotionDetailViewModel(entity.PromotionDetail);
            return new HttpResponseMessage()
            {
                Content = new JsonContent(new
                {
                    promotionCode = promoDetail.PromotionCode,
                    promotionDetailCode = promoDetail.PromotionDetailCode
                })
            };
        }

        [HttpGet]
        //[Route("api/promotion/GetPromotionList/{token}/{promotionCode}")]
        //public List<PromotionDetailApiViewModel> GetPromotionDetailList(string token, int terminalId)
        public HttpResponseMessage GetPromotionDetailList(string token)
        {
            var listResult = new List<PromotionDetailApiViewModel>();
            Utils.CheckToken(token);
            var headers = Request.Headers;
            string promotionCode = "";
            try
            {
                if (headers.Contains("promotionCode"))
                {
                    promotionCode = headers.GetValues("promotionCode").First();
                }
                var promotionDetailList = new List<PromotionDetailApiViewModel>();
                var promotionApi = new PromotionApi();
                var promotionDetailApi = new PromotionDetailApi();
                var storeApi = new StoreApi();
                var listPromotionDetail = promotionDetailApi.GetDetailByCode(promotionCode).ToList();
                foreach (var item in listPromotionDetail)
                {
                    var p = new PromotionDetailApiViewModel()
                    {
                        PromotionDetailID = item.PromotionDetailID,
                        BuyProductCode = item.BuyProductCode,
                        DiscountRate = item.DiscountRate,
                        GiftProductCode = item.GiftProductCode,
                        GiftQuantity = item.GiftQuantity.GetValueOrDefault(),
                        MaxBuyQuantity = item.MaxBuyQuantity,
                        MaxOrderAmount = item.MaxOrderAmount,
                        MinBuyQuantity = item.MinBuyQuantity,
                        MinOrderAmount = item.MinOrderAmount,
                        PromotionCode = item.PromotionCode,
                        RegExCode = item.RegExCode,
                    };
                    listResult.Add(p);
                }
                return new HttpResponseMessage()
                {
                    Content = new JsonContent(listResult)
                };
            }
            catch (Exception e)
            {
                return new HttpResponseMessage()
                {
                    Content = new JsonContent(new
                    {
                        ErrorMessage = e.Message,
                    })
                };
            }
        }
    }
}
