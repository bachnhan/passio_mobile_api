using HmsService.Models.Entities.Services;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Sdk;

namespace Wisky.Api.Controllers.API
{
    public class VoucherApiController : BaseController
    {
        // GET: VoucherApi

        private static readonly string _storeCode = System.Configuration.ConfigurationManager.AppSettings["config.store.code"];
        private readonly string _accessToken = System.Configuration.ConfigurationManager.AppSettings["config.order.token"];
        private readonly string _enableToken = System.Configuration.ConfigurationManager.AppSettings["config.order.EnableToken"];

        private readonly IVoucherService _customerService = DependencyUtils.Resolve<IVoucherService>();
        private readonly IPromotionService _productService = DependencyUtils.Resolve<IPromotionService>();


        [HttpPost]
        //[Route("VoucherApi/CheckVoucherCode/{token}/{terminalId}")]
        public JsonResult CheckVoucherCode(VoucherModel model)
        {
            try
            {
                var voucherApi = new VoucherApi();
                var voucher = voucherApi.GetVoucherByCode(model.voucherCode);

                if (voucher != null)
                {
                    if (voucher.Active == true && voucher.Quantity > voucher.UsedQuantity)
                    {
                        var promotionApi = new PromotionApi();
                        return Json(new
                        {
                            Status = true,
                            PromotionCode = promotionApi.Get(voucher.PromotionID).PromotionCode,
                            Success = true,
                            Message = "Voucher có thể áp dụng",
                            VoucherCode = model.voucherCode
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new
                        {
                            Status = false,
                            PromotionCode = "",
                            Success = true,
                            Message = "Voucher đã được sử dụng",
                            VoucherCode = model.voucherCode
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new
                {
                    Status = false,
                    PromotionCode = "",
                    Success = false,
                    Message = "Mã Voucher không tồn tại hoặc đã được sử dụng",
                    VoucherCode = model.voucherCode
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra"
                });
            }
        }


        [HttpPost]
        //[Route("VoucherApi/UseVoucherCode/{token}/{terminalId}")]
        public JsonResult UseVoucherCode(VoucherModel model)
        {
            try
            {
                var voucherApi = new VoucherApi();
                var voucher = voucherApi.GetVoucherByCode(model.voucherCode);

                if (voucher != null)
                {
                    if (voucher.Active && voucher.Quantity > voucher.UsedQuantity)
                    {
                        // Update số lượng sử dụng promotion
                        var promotionApi = new PromotionApi();
                        var promotion = promotionApi.Get(voucher.PromotionID);
                        // Giảm số lượng đang hoạt động
                        voucher.UsedQuantity += 1;
                        promotion.VoucherUsedQuantity += 1;
                        if (voucher.Quantity <= voucher.UsedQuantity)
                        {
                            voucher.Active = false;
                        }
                        //
                        voucherApi.Update(voucher);
                        if (promotion.VoucherQuantity <= promotion.VoucherUsedQuantity)
                        {
                            promotion.Active = false;
                        }
                        promotionApi.Update(promotion);
                        return Json(new
                        {
                            success = true,
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new
                {
                    success = false,
                    message = "Mã Voucher không tồn tại hoạt đã được sử dụng"
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra"
                });
            }
        }
    }

    public class VoucherModel
    {
        public string token { get; set; }
        public int terminalId { get; set; }
        public string voucherCode { get; set; }
    }
}