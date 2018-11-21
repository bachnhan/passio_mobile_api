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
    public class PromotionNewApiController : ApiController
    {
        [HttpGet]
        [Route("api/promotionNew/GetPromotionList/{token}/{terminalId}")]
        public HttpResponseMessage GetPromotionList(string token, int terminalId)
        {
            Utils.CheckToken(token);
            //var promotionList = new List<PromotionApiViewModel>();
            var promotionApi = new PromotionApi();
            var promotionStoreMappingApi = new PromotionStoreMappingApi();
            var promotionDetailApi = new PromotionDetailApi();

            var promotionsList = new List<PromotionApiViewModel>();
            //var promotionDetailsList = new List<PromotionDetailApiViewModel>();
            try
            {
                var listPromotion = promotionStoreMappingApi.GetPromotionByStoreId(terminalId).ToList();
                foreach (var item in listPromotion)
                {
                    var promotionDetails = promotionDetailApi.GetDetailByCode(item.PromotionCode);
                    var promotionDetailsList = new List<PromotionDetailApiViewModel>();
                    promotionDetailsList.AddRange(promotionDetails.Select(q => new PromotionDetailApiViewModel()
                    {
                        BuyProductCode = q.BuyProductCode,
                        DiscountRate = q.DiscountRate,
                        DiscountAmount = q.DiscountAmount,
                        GiftProductCode = q.GiftProductCode,
                        GiftQuantity = q.GiftQuantity == null ? 0 : q.GiftQuantity.Value,
                        MaxBuyQuantity = q.MaxBuyQuantity,
                        MaxOrderAmount = q.MaxOrderAmount,
                        MinBuyQuantity = q.MinBuyQuantity,
                        MinOrderAmount = q.MinOrderAmount,
                        PromotionCode = q.PromotionCode,
                        PromotionDetailCode = q.PromotionDetailCode,
                        PromotionDetailID = q.PromotionDetailID,
                        RegExCode = q.RegExCode,
                        PointTrade = q.PointTrade,
                        MinPoint = q.MinPoint,
                        MaxPoint = q.MaxPoint,
                    }));
                    var p = new PromotionApiViewModel()
                    {
                        PromotionID = item.PromotionID,
                        Active = item.Active,
                        ApplyFromTime = item.ApplyFromTime,
                        ApplyLevel = item.ApplyLevel,
                        ApplyToTime = item.ApplyToTime,
                        Description = item.Description,
                        FromDate = item.FromDate,
                        ToDate = item.ToDate,
                        GiftType = item.GiftType,
                        PromotionType = item.PromotionType,
                        ImageCss = item.ImageCss,
                        IsForMember = item.IsForMember,
                        PromotionClassName = item.PromotionClassName,
                        PromotionCode = item.PromotionCode,
                        PromotionName = item.PromotionName,
                        BrandId = item.BrandId,
                        PromotionDetails = promotionDetailsList,
                        IsVoucher = item.IsVoucher,
                        IsApplyOnce = item.IsApplyOnce,
                        FromHappyDay = item.FromHappyDay,
                        ToHappyDay = item.ToHappyDay,
                        FromHoursHappy = item.FromHoursHappy,
                        ToHoursHappy = item.ToHoursHappy,
                    };
                    promotionsList.Add(p);
                }
                //var model = new PromotionPromotionDetailMappingViewModel()
                //{
                //    Promotions = promotionsList,
                //    PromotionDetails = promotionDetailsList,
                //};
                return new HttpResponseMessage()
                {
                    //Content = new JsonContent(model)
                    Content = new JsonContent(promotionsList)
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
