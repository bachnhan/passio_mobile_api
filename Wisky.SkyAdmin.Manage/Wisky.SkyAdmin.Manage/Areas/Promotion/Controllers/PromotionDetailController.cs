using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using HmsService.Models.Entities;
using static Wisky.SkyAdmin.Manage.Areas.SystemReport.Controllers.TimeReportController;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Wisky.SkyAdmin.Manage.Areas.Promotion.Controllers
{
    public class PromotionDetailController : DomainBasedController
    {
        public static CloudBlobContainer _imagesBlobContainer;

        public PromotionDetailController()
        {
            InitializeStorage();
        }

        private void InitializeStorage()
        {
            // Open storage account using credentials from .cscfg file.
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Get context object for working with blobs, and 
            // set a default retry policy appropriate for a web user interface.
            var blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            // Get a reference to the blob container.
            _imagesBlobContainer = blobClient.GetContainerReference("images");

            // Get context object for working with queues, and 
            // set a default retry policy appropriate for a web user interface.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);
            //
            //            // Get a reference to the queue.
            //            imagesQueue = queueClient.GetQueueReference("images");
        }

        #region Index
        // GET: Promotion/PromotionDetail
        public ActionResult IndexDetail(int id, int storeId, int brandId)
        {
            ViewBag.storeId = storeId.ToString();
            ViewBag.brandId = brandId.ToString();
            var api = new PromotionApi();
            var promotion = api.GetByIdBrandId(id, brandId);
            PromotionViewModel pvm = new PromotionViewModel(promotion);
            PromotionEditViewModel model = new PromotionEditViewModel(pvm, this.Mapper);
            return View(model);
        }

        private int GetPromotionType(PromotionViewModel promo)
        {
            if (promo.ApplyLevel == 0)
            {
                if (promo.GiftType == 0 && promo.IsForMember == true)
                {
                    return (int)PromotionTypeEnumForDetail.Order_DiscountPercent_IsMember;
                }
                if (promo.GiftType == 0 && promo.IsForMember == false)
                {
                    return (int)PromotionTypeEnumForDetail.Order_DiscountPercent_Everyone;
                }
                if (promo.GiftType == 1 && promo.IsForMember == true)
                {
                    return (int)PromotionTypeEnumForDetail.Order_Gift_IsMember;
                }
                if (promo.GiftType == 1 && promo.IsForMember == false)
                {
                    return (int)PromotionTypeEnumForDetail.Order_Gift_Everyone;
                }
                if (promo.GiftType == 2 && promo.IsForMember == true)
                {
                    return (int)PromotionTypeEnumForDetail.Order_DiscountCash_IsMember;
                }
                if (promo.GiftType == 2 && promo.IsForMember == true)
                {
                    return (int)PromotionTypeEnumForDetail.Order_DiscountCash_IsMember;
                }
                else
                {
                    return -1;
                }
            }
            if (promo.ApplyLevel == 1)
            {
                if (promo.GiftType == 0 && promo.IsForMember == true)
                {
                    return (int)PromotionTypeEnumForDetail.OrderDetail_DiscountPercent_IsMember;
                }
                if (promo.GiftType == 0 && promo.IsForMember == false)
                {
                    return (int)PromotionTypeEnumForDetail.OrderDetail_DiscountPercent_Everyone;
                }
                if (promo.GiftType == 1 && promo.IsForMember == true)
                {
                    return (int)PromotionTypeEnumForDetail.OrderDetail_Gift_IsMember;
                }
                if (promo.GiftType == 1 && promo.IsForMember == false)
                {
                    return (int)PromotionTypeEnumForDetail.OrderDetail_Gift_Everyone;
                }
                if (promo.GiftType == 2 && promo.IsForMember == true)
                {
                    return (int)PromotionTypeEnumForDetail.OrderDetail_DiscountCash_IsMember;
                }
                if (promo.GiftType == 2 && promo.IsForMember == true)
                {
                    return (int)PromotionTypeEnumForDetail.OrderDetail_DiscountCash_IsMember;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }

        }


        /// <summary>
        /// hiện tại cho Min = Max OrderAmount vì nếu làm theo kiểu cũ:
        /// MinOrderAmount = 4000, MaxOrderAmount = 6000 --> chỉ có hóa đơn mua từ 4000 --> 6000 mới được tặng
        /// , hóa đơn mua trên 6000 sẽ ko được tặng.
        /// -->Hướng giải quyết là sẽ quy lại về dạng chỉ có 1 số tiền tối thiểu để áp dụng
        /// , ví dụ: mua hóa đơn trên 200 thì được giảm 15% --> Max = Min = 200
        /// tương tự với sản phẩm và các hình thức khuyến mãi khác
        /// ví dụ với cách áp dụng cũ của sản phẩm là:
        /// MinBuyQuantity = 2, MaxBuyQuantity = 5  --> chỉ có mua trong khoảng này mới được tặng --> sai đối với thực tế
        /// --> Hướng giải quyết :
        /// Quy về chỉ có 1 loại khuyến mãi của sản phẩm là lượng tối thiểu 
        /// --> MinBuyQuantity == MaxBuyQuantity trong code
        /// Ví dụ: khuyến mãi cứ mua 2 lon coca là được tặng 1 lon bò húc --> MaxBuyQuantity = MinBuyQuantity = 2
        /// --> mua 5 lon coca sẽ được tặng 2 lon bò húc, 6 lon coca thì tặng 3 lon bò húc, ...
        /// 
        /// Tóm tắt: ko dùng MaxBuyQuantity với MaxOrderAmount, cho Max = Min
        /// </summary>
        /// <param name="param"></param>
        /// <param name="promoCode"></param>
        /// <returns></returns>
        public JsonResult LoadPromotionDetail(JQueryDataTableParamModel param, string promoCode)
        {

            var promoApi = new PromotionApi();
            var promo = promoApi.GetByPromoCode(promoCode);
            var api = new PromotionDetailApi();
            var productApi = new ProductApi();
            var detailList = api.GetDetailByCode(promoCode);
            int totalCount = detailList.Count();
            int count = param.iDisplayStart;
            var result = detailList.ToList()
                    //.Where(a => string.IsNullOrEmpty(param.sSearch) || a..ToLower().Contains(param.sSearch.ToLower()))
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength);
            #region Order
            if (promo.ApplyLevel == 0)//Order: theo hoa
            {

                object rs = null;
                if (promo.IsForMember == true)
                {
                    if (promo.UsingPoint == true)
                    {
                        rs = result.Select(a => new IConvertible[]
                        {
                            ++count,
                            a.RegExCode.IndexOf("_")-3 > 0 ? new MembershipCardTypeApi().GetMembershipCardTypeNameByAppendCode(a.RegExCode.Substring(3, a.RegExCode.IndexOf("_")-3)) : "" ,// Quy định mã code ^((AppendCode_)(RegexCode)) ;
                            a.RegExCode.Substring(3,a.RegExCode.IndexOf("_")-3),
                            a.PointTrade,
                            a.MinPoint,
                            a.MaxPoint,
                            string.Format(CultureInfo.InvariantCulture,"{0:n0}đ", a.MinOrderAmount),
                            a.MaxOrderAmount.HasValue ? string.Format(CultureInfo.InvariantCulture,"{0:n0}đ", a.MaxOrderAmount) : ("Lớn hơn " + string.Format(CultureInfo.InvariantCulture,"{0:n0}đ", a.MinOrderAmount)),
                            a.GiftProductCode?? "N/A",
                            a.DiscountRate!=null? a.DiscountRate:Convert.ToDouble((a.DiscountAmount!=null?a.DiscountAmount:Convert.ToDecimal(a.GiftQuantity.Value))),
                            a.PromotionDetailID,
                        });
                    }
                    else
                    {
                        rs = result.Select(a => new IConvertible[]
                        {
                            ++count,
                            a.RegExCode.IndexOf("_")-3 > 0 ? new MembershipCardTypeApi().GetMembershipCardTypeNameByAppendCode(a.RegExCode.Substring(3, a.RegExCode.IndexOf("_")-3)) : "" ,// Quy định mã code ^((AppendCode_)(RegexCode)) ;
                            a.RegExCode.Substring(3,a.RegExCode.IndexOf("_")-3),
                            string.Format(CultureInfo.InvariantCulture,"{0:n0}đ", a.MinOrderAmount),
                            a.MaxOrderAmount.HasValue ? string.Format(CultureInfo.InvariantCulture,"{0:n0}đ", a.MaxOrderAmount) : ("Lớn hơn " + string.Format(CultureInfo.InvariantCulture,"{0:n0}đ", a.MinOrderAmount)),
                            a.GiftProductCode?? "N/A",
                            a.DiscountRate!=null? a.DiscountRate:Convert.ToDouble((a.DiscountAmount!=null?a.DiscountAmount:Convert.ToDecimal(a.GiftQuantity.Value))),
                            a.PromotionDetailID,
                        });
                    }

                }
                else
                {
                    rs = result.Select(a => new IConvertible[]
                    {
                    ++count,
                    string.Format(CultureInfo.InvariantCulture,
                        "{0:n0}đ", a.MinOrderAmount),
                    a.MaxOrderAmount.HasValue ? string.Format(CultureInfo.InvariantCulture,
                        "{0:n0}đ", a.MaxOrderAmount) : ("Lớn hơn " + string.Format(CultureInfo.InvariantCulture,
                        "{0:n0}đ", a.MinOrderAmount)),
                    a.GiftProductCode?? "N/A",
                    a.DiscountRate!=null? a.DiscountRate:Convert.ToDouble((a.DiscountAmount!=null?a.DiscountAmount:Convert.ToDecimal(a.GiftQuantity.Value))),
                    a.PromotionDetailID
                    });
                }
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalCount,
                    iTotalDisplayRecords = totalCount,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            else//Order Detail: theo sản phẩm
            {
                object rs = null;
                if (promo.IsForMember == true)
                {

                    rs = result.Select(a => new IConvertible[]
                     {
                    ++count,
                    a.RegExCode.IndexOf("_")-3 > 0 ? new MembershipCardTypeApi().GetMembershipCardTypeNameByAppendCode(a.RegExCode.Substring(3, a.RegExCode.IndexOf("_")-3)) : "" ,// Quy định mã code ^((AppendCode_)(RegexCode)) ;
                    a.RegExCode.Substring(3,a.RegExCode.IndexOf("_")-3),//Mã Promotion
                    a.BuyProductCode == null || a.BuyProductCode == "0" ? "Bất kỳ":productApi.GetProductByCode(a.BuyProductCode).ProductName,
                    a.MinBuyQuantity,
                    a.MaxBuyQuantity.HasValue ? a.MaxBuyQuantity.Value.ToString() : "Lớn hơn " + a.MinBuyQuantity,
                    a.GiftProductCode?? "N/A",
                    a.DiscountRate!=null? a.DiscountRate:Convert.ToDouble((a.DiscountAmount!=null?a.DiscountAmount:Convert.ToDecimal(a.GiftQuantity.Value))),
                    a.PromotionDetailID
                     });
                }
                else
                {
                    rs = result.Select(a => new IConvertible[]
                     {
                    ++count,
                    a.BuyProductCode == null || a.BuyProductCode == "0" ? "Bất kỳ":productApi.GetProductByCode(a.BuyProductCode).ProductName,
                    a.MinBuyQuantity,
                    a.MaxBuyQuantity.HasValue ? a.MaxBuyQuantity.Value.ToString() : "Lớn hơn " + a.MinBuyQuantity,
                    a.GiftProductCode?? "N/A",
                    a.DiscountRate!=null? a.DiscountRate:Convert.ToDouble((a.DiscountAmount!=null?a.DiscountAmount:Convert.ToDecimal(a.GiftQuantity.Value))),
                    a.PromotionDetailID
                     });
                }
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalCount,
                    iTotalDisplayRecords = totalCount,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion
        }
        #endregion

        #region Create
        public ActionResult Create(int id, int brandId)
        {
            var productApi = new ProductApi();
            var products = productApi.GetGiftProducts();
            ViewBag.products = products;
            var api = new PromotionApi();
            var promo = api.GetByIdBrandId(id, brandId);
            PromotionDetailViewModel model = new PromotionDetailViewModel();
            ViewBag.Promo = promo;
            PrepareCreate(model);
            model.PromotionCode = promo.PromotionCode;
            model.isMember = promo.IsForMember == true ? 1 : 0;
            var groupMembercardList = new MembershipCardTypeApi().GetMembershipCardTypeByBrand(brandId).ToList();
            model.MembershipCardTypeList = groupMembercardList;

            return View(model);
        }
        public void PrepareCreate(PromotionDetailViewModel model)
        {
            var api = new ProductApi();
            var products = api.GetGiftProducts();
            model.AvailableProduct = products.ToSelectList(a => a.ProductName, a => a.ProductID.ToString(), a => false);
        }

        [HttpPost]
        public async Task<JsonResult> CreateDetail(PromotionDetailViewModel model, string GroupCode, bool isAppliedOnce)
        {
            var api = new PromotionDetailApi();
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.GiftProductCode != null)
                    {
                        if (model.GiftQuantity == null)
                        {
                            model.GiftQuantity = 1;
                        }
                    }
                    if (GroupCode != null)
                        model.RegExCode = "^((" + GroupCode + @"_)(\d+)$)";  // hiện tại passio code là GroupCode_ + 10 số ví dụ: GroupCode = 15 --> 15_1234567890
                    int r = checkPromotionDetail(model, true);
                    if (r == -1 || !isAppliedOnce)
                    {
                        //model.MaxOrderAmount = model.MinOrderAmount;
                        //model.MaxBuyQuantity = model.MinBuyQuantity;
                        int count = new PromotionDetailApi().GetDetailByCode(model.PromotionCode).ToList().Count;
                        model.PromotionDetailCode = model.PromotionCode + (count < 9 ? "00" + (count + 1) : "0" + (count + 1));

                        var entity = model.ToEntity();
                        await api.CreatePromotionDetail(entity);
                        
                        return Json(new { success = true, message = "Thêm quy định thành công" });
                    }

                    return Json(new { success = false, message = "Thêm quy định thất bại do mâu thuẫn với quy định thứ " + r });
                }
                catch (Exception e)
                {
                    return Json(new { success = false, message = "Thêm quy định thất bại" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Thêm quy định thất bại" });
            }
        }

        /// <summary>
        /// Kiểm tra điều kiện add được promotionDetail hay ko
        /// </summary>
        /// <param name="model">Model cuả promotion cần kiểm tra</param>
        /// <returns>-1: ko lỗi; > -1: có lỗi</returns>
        private int checkPromotionDetail(PromotionDetailViewModel model, bool isCreate)
        {
            string code = model.PromotionCode;
            var promotionDetails = new PromotionDetailApi().GetDetailByCode(code).ToList();
            if (model.BuyProductCode == null)
            {
                int i = 1;
                foreach (var p in promotionDetails)
                {
                    if (model.RegExCode == p.RegExCode)
                    {
                        if (isCreate || (!isCreate && p.PromotionDetailID != model.PromotionDetailID))
                        {
                            //                  model                                                                                               model
                            //          |-------------------|                                                                               |-------------------|
                            //
                            //          p                                                                                                                       p
                            //  |---------------|                                                                                                   |----------------------|
                            if ((p.MinOrderAmount <= model.MaxOrderAmount && p.MaxOrderAmount >= model.MinOrderAmount) ||
                                (p.MaxOrderAmount >= model.MinOrderAmount && p.MinOrderAmount <= model.MaxOrderAmount) ||
                                (model.MaxOrderAmount >= p.MinOrderAmount && p.MaxOrderAmount == null) ||
                                (model.MaxOrderAmount == null && model.MinOrderAmount <= p.MaxOrderAmount) ||
                                (p.MaxOrderAmount == null && model.MaxOrderAmount == null))
                            {
                                return i;
                            }

                        }
                        i++;
                    }
                }
            }
            else
            {
                int i = 1;
                foreach (var p in promotionDetails)
                {

                    if (p.BuyProductCode == model.BuyProductCode && p.GiftProductCode == model.GiftProductCode)
                    {
                        if (isCreate || (!isCreate && p.PromotionDetailID != model.PromotionDetailID))
                        {
                            if ((p.MinBuyQuantity <= model.MaxBuyQuantity && p.MaxBuyQuantity >= model.MinBuyQuantity) ||
                                    (p.MaxBuyQuantity >= model.MinBuyQuantity && p.MinBuyQuantity <= model.MaxBuyQuantity) ||
                                    (model.MaxBuyQuantity >= p.MinBuyQuantity && p.MaxBuyQuantity == null) ||
                                    (model.MaxBuyQuantity == null && model.MinBuyQuantity <= p.MaxBuyQuantity) ||
                                    (p.MaxBuyQuantity == null && model.MaxBuyQuantity == null))
                            {
                                return i;
                            }
                        }
                    }
                    i++;
                }
            }
            return -1;
        }
        #endregion

        #region Delete
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            var api = new PromotionDetailApi();

            try
            {
                var promoDetail = api.GetDetailById(id);
                int promotionDetailId = promoDetail.PromotionDetailID;
                var promotion = new PromotionApi().GetByPromoCode(promoDetail.PromotionCode);
                if (promotion.IsVoucher.HasValue && promotion.IsVoucher.Value)
                {
                    VoucherApi voucherApi = new VoucherApi();
                    var list = voucherApi.GetByPromotionDetailId(promotionDetailId);
                    foreach (var item in list)
                    {
                        voucherApi.Delete(item.VoucherID);
                    }
                }
                await api.DeletePromotionDetail(promoDetail);
                return Json(new { success = true, message = "Xóa quy định thành công" });
            }
            catch
            {
                return Json(new { success = false, message = "Xóa quy định thất bại" });
            }
        }
        #endregion

        #region Edit
        public ActionResult Edit(int id, int brandId)
        {
            var productApi = new ProductApi();
            var products = productApi.GetGiftProducts();
            ViewBag.products = products;
            var api = new PromotionDetailApi();
            var promoDetail = api.GetDetailById(id);
            var proApi = new PromotionApi();
            var promo = proApi.GetByPromoCode(promoDetail.PromotionCode);
            ViewBag.Promo = promo;
            PromotionDetailViewModel model = new PromotionDetailViewModel(promoDetail);
            PrepareCreate(model);
            model.isMember = promo.IsForMember == true ? 1 : 0;
            var MembercardTypeList = new MembershipCardTypeApi().GetAllMembershipCardTypeByBrand(brandId).ToList();
            model.MembershipCardTypeList = MembercardTypeList;

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> EditDetail(PromotionDetailViewModel model, string GroupCode, bool isAppliedOnce)
        {
            var api = new PromotionDetailApi();
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.GiftProductCode != null)
                    {
                        if (model.GiftQuantity == null)
                        {
                            model.GiftQuantity = 1;
                        }
                    }
                    if (GroupCode != null)
                        model.RegExCode = "^((" + GroupCode + @"_)(\d+)$)";
                    int r = checkPromotionDetail(model, false);
                    if (r == -1 || !isAppliedOnce)
                    {
                        //model.MaxBuyQuantity = model.MinBuyQuantity;
                        //model.MaxOrderAmount = model.MinOrderAmount;
                        // hiện tại passio code là GroupCode_ + 10 số ví dụ: GroupCode = 15 --> 15_1234567890
                        var entity = api.GetDetailById(model.PromotionDetailID);
                        model.CopyToEntity(entity);
                        await api.UpdatePromotionDetail(entity);
                        return Json(new { success = true, message = "Sửa quy định thành công" });
                    }

                    return Json(new { success = false, message = "Sửa quy định thất bại do mâu thuẫn với quy định thứ " + r });

                }
                catch (Exception e)
                {
                    return Json(new { success = false, message = "Sửa quy định thất bại" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Sửa quy định thất bại" });
            }
        }
        #endregion

        //Get Products for Promotion
        public JsonResult LoadAllProducts(int brandId)
        {
            var productApi = new ProductApi();
            var products = productApi.GetProductByBrandId(brandId).Where(q => q.IsAvailable == true && q.ProductCategory.IsExtra == false).ToList();
            return Json(new
            {
                success = true,
                data = products.Select(a => new
                {
                    id = a.Code,
                    text = a.ProductName
                })
            });
        }
        public JsonResult LoadAllProductEdit(int brandId)
        {
            var productApi = new ProductApi();
            var products = productApi.GetProductByBrandId(brandId).Where(q => q.IsAvailable == true && q.ProductCategory.IsExtra == false).ToList();
            return Json(new
            {
                success = true,
                data = products.Select(a => new
                {
                    id = a.Code,
                    text = a.ProductName
                })
            });
        }

        public JsonResult LoadAllPromotionType(int promotionId)
        {
            var promotionType = new string[4] { PromotionTypeEnum.Internal.DisplayName(), PromotionTypeEnum.Separate.DisplayName(), PromotionTypeEnum.Common.DisplayName(), PromotionTypeEnum.Server.DisplayName() };

            var selectedPromotionType = new PromotionApi().GetPromotionById(promotionId).PromotionType;
            return Json(new
            {
                selectedPromotionType = selectedPromotionType,
                promotionType = promotionType,
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadApplyStore(int promotionId, int brandId)
        {
            StoreApi storeApi = new StoreApi();
            var store = storeApi.GetActiveStoreByBrandId(brandId).Where(a => a.Name != "Tổng đài").AsEnumerable();
            var applyStore = new PromotionStoreMappingApi().
                GetActivePromotionStoreMappingByPromotionID(promotionId).
                Select(a => new { storeId = a.StoreId });

            int count = 0;
            count = 0;
            var result = store.Select(q => new IConvertible[]{
                ++count,
                q.Name,
                q.ID,
                applyStore.FirstOrDefault(a => a.storeId == q.ID) != null
            }).ToArray();
            return Json(new
            {
                iTotalRecords = result.Count(),
                iTotalDisplayRecords = result.Count(),
                aaData = result,
                applyStore = applyStore
            }, JsonRequestBehavior.AllowGet);
        }





        public JsonResult PromotionDiscountOrderDetailReport(string promotionCode)
        {
            PromotionDetailApi pdApi = new PromotionDetailApi();
            var odPromotionDetailsApi = new OrderDetailPromotionMappingApi();
            ProductApi pApi = new ProductApi();

            var promotionDetail = pdApi.GetDetailByCode(promotionCode).ToList();
            List<object> list = new List<object>();

            for (int i = 0; i < promotionDetail.Count(); i++)
            {
                var product = pApi.GetProductByCode(promotionDetail[i].BuyProductCode);
                decimal amountDiscount = odPromotionDetailsApi.GetActive()
                    //TODO: coi lại chỗ này sau khi đổi db
                    //.Where(q => q.PromotionDetailId == promotionDetail[i].PromotionDetailID)
                    .Sum(q => q.DiscountAmount);
                list.Add(new Object[] { product.ProductName, amountDiscount });
            }

            return Json(new
            {
                data = list,
            }, JsonRequestBehavior.AllowGet);
        }

        private async Task<CloudBlockBlob> UploadAndSaveBlobAsync(HttpPostedFileBase imageFile)
        {
            //            Trace.TraceInformation("Uploading image file {0}", Guid.NewGuid().ToString());

            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            // Retrieve reference to a blob. 
            CloudBlockBlob imageBlob = _imagesBlobContainer.GetBlockBlobReference(blobName);
            // Create the blob by uploading a local file.
            using (var fileStream = imageFile.InputStream)
            {
                await imageBlob.UploadFromStreamAsync(fileStream);
            }

            //            Trace.TraceInformation("Uploaded image file to {0}", imageBlob.Uri.ToString());

            return imageBlob;
        }

        public JsonResult PromotionDateReportComparison(int brandId, int promotionId, int applyLevel, string comparisonTime)
        {
            var orderApi = new OrderApi();
            var orderDetailApi = new OrderDetailApi();
            var orderPromoMappingApi = new OrderPromotionMappingApi();
            var orderDetailPromoMappingApi = new OrderDetailPromotionMappingApi();
            var dateReportApi = new DateReportApi();
            var storeApi = new StoreApi();

            var fromDate = comparisonTime.ToDateTime().GetStartOfDate();
            var toDate = fromDate.GetEndOfDate();

            try
            {
                var storeDetailList = storeApi.GetStoreByBrandId(brandId).Select(q => new StoreReport
                {
                    StoreId = q.ID,
                    StoreName = q.Name
                }).ToList();

                if (applyLevel == 0) // Hóa đơn
                {
                    var order = orderApi.GetAllOrderByDate(fromDate, toDate, brandId);
                    var orderPromotionMapping = orderPromoMappingApi.GetActiveReturnEntities()
                                .Where(q => q.PromotionId == promotionId);

                    var rs = order.Join(orderPromotionMapping, p => p.RentID, q => q.OrderId, (p, q) => p).ToList();

                    foreach (var store in storeDetailList)
                    {
                        var list = rs.Where(q => q.StoreID == store.StoreId);
                        store.TotalAmount = list.Sum(q => q.TotalAmount);
                        store.FinalAmount = list.Sum(q => q.FinalAmount);
                        store.Discount = list.Sum(q => q.Discount) + list.Sum(q => q.DiscountOrderDetail);
                    }
                }
                else // Sản phẩm
                {
                    var orderDetailList = orderDetailApi.GetAllOrderDetailByTimeRange(fromDate, toDate, brandId);
                    var orderDetailMapping = orderDetailPromoMappingApi.GetActiveReturnEntities()
                                .Where(q => q.PromotionId == promotionId);
                    var rs = orderDetailList.Join(orderDetailMapping,
                            p => p.OrderDetailID, q => q.OrderDetailId, (p, q) => p).ToList();

                    foreach (var store in storeDetailList)
                    {
                        var list = rs.Where(q => q.StoreId == store.StoreId);
                        store.TotalAmount = list.Sum(q => q.TotalAmount);
                        store.FinalAmount = list.Sum(q => q.FinalAmount);
                        store.Discount = list.Sum(q => q.Discount);
                    }
                }

                return Json(new
                {
                    success = true,
                    storeList = storeDetailList.Select(q => q.StoreName),
                    totalList = storeDetailList.Select(q => q.TotalAmount),
                    finalList = storeDetailList.Select(q => q.FinalAmount),
                    discountList = storeDetailList.Select(q => q.Discount)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng thử lại." }, JsonRequestBehavior.AllowGet);
            }
        }

        private class StoreReport
        {
            public int StoreId { get; set; }
            public string StoreName { get; set; }
            public double TotalAmount { get; set; }
            public double FinalAmount { get; set; }
            public double Discount { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> UploadImage()
        {
            try
            {
                CloudBlockBlob imageBlob = null;
                var imageFile = Request.Files["file"];
                ProductImageViewModel productImage = new ProductImageViewModel()
                {
                    Position = 0,
                    Active = true
                };

                if (imageFile != null && imageFile.ContentLength != 0)
                {
                    imageBlob = await UploadAndSaveBlobAsync(imageFile);
                    productImage.ImageUrl = imageBlob.Uri.ToString();
                }

                return Json(new { success = true, imageUrl = productImage.ImageUrl }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// Ko được dùng highCharts nữa vì nếu dùng cho mục đích Commercial thì sẽ bị tính phí
        /// <returns></returns>
        //public JsonResult VoucherIsApplyOneReport(int brandId, int promotionId)
        //{
        //    PromotionApi pApi = new PromotionApi();
        //    var voucher = pApi.GetByIdBrandId(promotionId, brandId);
        //    List<object> list = new List<object>();
        //    PromotionDetailApi pdApi = new PromotionDetailApi();
        //    var detail = pdApi.GetDetailByCode(voucher.PromotionCode);
        //    VoucherApi vApi = new VoucherApi();
        //    ProductApi productApi = new ProductApi();
        //    foreach (var item in detail)
        //    {
        //        var quantity = vApi.getVoucherUsedbyPromotionDetailId(item.PromotionDetailID).Count();
        //        list.Add(new object[] { productApi.GetProductById(int.Parse(item.BuyProductCode)).ProductName, quantity });
        //    }



        //    return Json(new {

        //        data = list,
        //    },
        //        JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult VoucherIsApplyOneReport2(int brandId, int promotionId)
        //{
        //    PromotionApi pApi = new PromotionApi();
        //    var voucher = pApi.GetPromotionById(promotionId);
        //    List<object> list = new List<object>();
        //    OrderApi oApi = new OrderApi();

        //    var OrderVoucher = oApi.GetAllOrderByDateByPromotionId(voucher.FromDate,
        //                voucher.ToDate, brandId, voucher.PromotionID);

        //   var restOrder =  oApi.GetAllOrderByDateDifferentFromPromotionId(voucher.FromDate,
        //       voucher.ToDate, brandId, voucher.PromotionID);
        //    double moneyRestOrder = 0;
        //    double moneyOrderVoucher = 0;

        //    foreach (var restOrderItem in restOrder)
        //    {
        //        moneyRestOrder += restOrderItem.FinalAmount;
        //    }
        //    foreach(var orderVoucherItem in OrderVoucher)
        //    {
        //        moneyOrderVoucher += orderVoucherItem.FinalAmount;
        //    }

        //    list.Add(new object[] {"Order có Voucher này",OrderVoucher.Count() });
        //    list.Add(new object[] {"Order còn lại", restOrder.Count() });

        //    return Json(new
        //    {

        //        data = list,
        //        moneyRestOrder= moneyRestOrder,
        //        moneyOrderVoucher = moneyOrderVoucher
        //    },
        //        JsonRequestBehavior.AllowGet);
        //}

        public async Task<ActionResult> DeactivePromotion(int id)
        {
            PromotionApi pApi = new PromotionApi();
            var promotion = pApi.GetPromotionById(id);
            promotion.Active = false;
            await pApi.EditAsync(id, promotion);
            PromotionEditViewModel model = new PromotionEditViewModel(promotion, this.Mapper);
            return RedirectToAction("IndexDetail", "PromotionDetail", new { id = id });
        }
        public async Task<ActionResult> ActivePromotion(int id)
        {
            PromotionApi pApi = new PromotionApi();
            var promotion = pApi.GetPromotionById(id);
            promotion.Active = true;
            await pApi.EditAsync(id, promotion);
            PromotionEditViewModel model = new PromotionEditViewModel(promotion, this.Mapper);
            return RedirectToAction("IndexDetail", "PromotionDetail", new { id = id });
        }

        public ActionResult ReportPartial(int id)
        {
            var promotion = new PromotionApi().GetPromotionById(id);
            PromotionEditViewModel model = new PromotionEditViewModel(promotion, this.Mapper);
            return PartialView("_ReportPartial", model);
        }

        public ActionResult ReportPartial2(int id)
        {
            var promotion = new PromotionApi().GetPromotionById(id);
            PromotionEditViewModel model = new PromotionEditViewModel(promotion, this.Mapper);
            return PartialView("_ReportPartial2", model);
        }

        public ActionResult CustomerOrder(int customerId, int promotionId, string sTime, string eTime, int selectedStoreId)
        {
            CustomerApi cApi = new CustomerApi();
            var model = cApi.GetCustomerById(customerId);
            ViewData["customerName"] = model.Name;
            ViewData["customerId"] = customerId;
            ViewData["promotionId"] = promotionId;
            ViewData["selectedStoreId"] = selectedStoreId;
            ViewData["sTime"] = sTime;
            ViewData["eTime"] = eTime;

            return View("_CustomerOrder");
        }

        public ActionResult ListPartial(int promotionId, int storeId, int brandId, string fromDate, string toDate)
        {
            var sDate = fromDate.ToDateTime().GetStartOfDate();
            var eDate = toDate.ToDateTime().GetEndOfDate();
            var list = new OrderApi().GetAllOrderByDateByPromotionId(sDate, eDate, brandId, promotionId);
            OrderViewModel model = new OrderViewModel(list, this.Mapper);
            ViewData.Add("brandId", brandId);
            ViewData.Add("storeId", storeId);
            ViewData.Add("promotionId", promotionId);
            return PartialView("_OrderListPartial", model);
        }

        public JsonResult DateReportAllComparison(string comparisonTime, int brandId, int promotionId, int applyLevel)
        {
            var storeApi = new StoreApi();
            var orderApi = new OrderApi();
            var dateReportApi = new DateReportApi();

            //DateTime sTime = comparisonTime.ToDateTime().GetStartOfDate();
            //DateTime eTime = comparisonTime.ToDateTime().GetEndOfDate();
            DateTime time = DateTime.ParseExact(comparisonTime, "d/MM/yyyy", DateTimeFormatInfo.InvariantInfo);
            DateTime dateNow = DateTime.Now;

            var chartReports = new List<ReportChartComparisonView>();
            if (applyLevel == (int)PromotionApplyLevelEnum.Order)
            {
                chartReports = orderApi.GetAllOrderByDateByPromotionId(time, time.AddDays(1), brandId, promotionId)
                        .GroupBy(q => q.StoreID).Select(q => new ReportChartComparisonView
                        {
                            StoreId = q.Key,
                            TotalAmount = q.Sum(p => p.TotalAmount),
                            FinalAmount = q.Sum(p => p.FinalAmount),
                            Discount = q.Sum(p => p.TotalAmount) - q.Sum(p => p.FinalAmount),
                        }).ToList();
            }
            else
            {
                chartReports = orderApi.GetAllOrderByDateByPromotionIdOrderDetail(time, time.AddDays(1), brandId, promotionId)
                        .GroupBy(q => q.StoreID).Select(q => new ReportChartComparisonView
                        {
                            StoreId = q.Key,
                            TotalAmount = q.Sum(p => p.TotalAmount),
                            FinalAmount = q.Sum(p => p.FinalAmount),
                            Discount = q.Sum(p => p.TotalAmount) - q.Sum(p => p.FinalAmount),
                        }).ToList();
            }

            var storeList = new List<string>();
            foreach (var item in chartReports)
            {
                if (item.StoreId.HasValue)
                {
                    storeList.Add(storeApi.GetStoreById(item.StoreId.Value).Name);
                }
            }
            var totalList = chartReports.Select(q => q.TotalAmount).ToList();
            var finalList = chartReports.Select(q => q.FinalAmount).ToList();
            var discountList = chartReports.Select(q => q.Discount).ToList();
            var allStoreApply = new PromotionStoreMappingApi().GetActivePromotionStoreMappingByPromotionID(promotionId).ToList();
            foreach (var item in allStoreApply)
            {
                var storeName = storeApi.GetStoreById(item.StoreId).Name;
                if (!storeList.Contains(storeName))
                {
                    storeList.Add(storeName);
                    totalList.Add(0);
                    discountList.Add(0);
                }
            }

            return Json(new
            {
                storeList,
                totalList,
                finalList,
                discountList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetVoucherExportList(int brandId, int promotionId, string promotionCode)
        {
            //IQueryable<VoucherViewModel> voucherlist = null;
            //int promotionDetailId = -1;
            //var firstPromotionDetail = new PromotionDetailApi().GetDetailByCode(promotionCode).FirstOrDefault();
            //if (firstPromotionDetail != null)
            //{
            //    promotionDetailId = firstPromotionDetail.PromotionDetailID;
            //    //voucherlist = new VoucherApi().getAllVoucherByPromotionIdByPromotionDetailID(promotionId, promotionDetailId);
            //}
            var voucherApi = new VoucherApi();
            var voucherlist = voucherApi.getAllVoucherByPromotionId(promotionId);
            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Voucher Code";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Trạng thái";
                var EndHeaderChar = StartHeaderChar;
                var EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws.Cells["" + StartHeaderChar + StartHeaderNumber +
                    ":" + EndHeaderChar + EndHeaderNumber].Style.Font.Bold = true;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber +
                    ":" + EndHeaderChar + EndHeaderNumber].AutoFitColumns();
                ws.Cells["" + StartHeaderChar + StartHeaderNumber +
                    ":" + EndHeaderChar + EndHeaderNumber]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber +
                    ":" + EndHeaderChar + EndHeaderNumber]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells         
                if (voucherlist != null)
                    foreach (var data in voucherlist)
                    {
                        ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = data.VoucherCode;
                        ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = data.UsedQuantity == 1 ? "Đã dùng" : "Chưa dùng";
                        StartHeaderChar = 'A';
                    }
                string brandName;
                var brandApi = new BrandApi();
                brandName = brandApi.GetBrandById(brandId).BrandName;
                string fileName = "Danh sách Voucher.xlsx";
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }

    }
}