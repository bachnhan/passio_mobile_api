using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Promotion.Controllers
{
    public class PromotionController : DomainBasedController
    {
        public static CloudBlobContainer _imagesBlobContainer;

        public PromotionController()
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

        // GET: Promotion/Promotion
        public ActionResult IndexPromotion(int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            return View();
        }
        // load datatable
        public JsonResult IndexList(JQueryDataTableParamModel param, int brandId, int isMember, int active)
        {
            //ko in ra group nữa
            //var groupService = DependencyUtils.Resolve<IGroupService>();


            var api = new PromotionApi();
            //var promotionList = api.GetActivePromotion(brandId).ToList();
            List<PromotionViewModel> promotionList;
            if (isMember == 1)
            {
                promotionList = api.GetAllPromotion(brandId)
                    .Where(q => q.IsForMember == true)
                    .ToList();
            }
            else if (isMember == 2)
            {
                promotionList = api.GetAllPromotion(brandId)
                   .Where(q => q.IsForMember == false)
                   .ToList();
            }
            else
            {
                promotionList = api.GetAllPromotion(brandId).ToList();
            }
            switch (active)
            {
                case 2: promotionList = promotionList.Where(q => q.Active == true).ToList(); break;
                case 3: promotionList = promotionList.Where(q => q.Active == false).ToList(); break;
                default:
                    break;
            }
            int count = param.iDisplayStart;
            var rs = promotionList

                .Select(a => new IConvertible[]
                {
                    ++count,
                    a.PromotionCode,
                    a.PromotionName,
                    //a.Description,
                    a.ImageUrl,
                    a.FromDate.ToString("dd/MM/yyyy"),
                    a.ToDate.ToString("dd/MM/yyyy"),
                    a.IsForMember==true?"Có":"Không",
                    a.Active,
                    //groupService.GetGroupById(a.Group).Name,
                    a.PromotionID
                }).ToList();
            int totalRecords = promotionList.Count();
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        #region Create
        public async Task<ActionResult> Create(string storeId, int brandId)
        {
            PromotionEditViewModel model = new PromotionEditViewModel();
            ViewBag.storeId = storeId;
            StoreApi storeapi = new StoreApi();
            var allStore = storeapi.GetActiveStoreByBrandId(brandId);
            model.AvailableStore = allStore.ToSelectList(q => q.Name, q => q.ID + "", q => false);
            PrepareCreate(model);
            return View(model);
        }

        public void PrepareCreate(PromotionEditViewModel model)
        {
            var groupService = DependencyUtils.Resolve<IGroupService>();
            var group = new string[4] { PromotionTypeEnum.Internal.DisplayName(), PromotionTypeEnum.Separate.DisplayName(), PromotionTypeEnum.Common.DisplayName(), PromotionTypeEnum.Server.DisplayName() };
            model.AvailableGroup = group.Select((r, value) => new SelectListItem { Text = r, Value = value.ToString(), Selected = false });
        }

        [HttpPost]
        public async Task<JsonResult> CreatePromotion(PromotionEditViewModel model, int brandId, bool isOneVoucher)
        {
            int promotionId = -1;
            var promotionApi = new PromotionApi();
            var voucherApi = new VoucherApi();
            var storeApi = new StoreApi();
            try
            {
                var promotion = promotionApi.GetByPromoCode(model.PromotionCode);
                if (promotion != null)
                {
                    return Json(new { success = false, message = "Mã khuyến mãi đã tồn tại!" });
                }
                else
                {
                    model.BrandId = brandId;
                    model.IsApplyOnce = false;
                    model.Active = true;
                    model.ImageCss = model.GiftType == 0 ? HmsService.Models.ImageCssEnum.percent.ToString() : HmsService.Models.ImageCssEnum.gift.ToString();
                    model.PromotionClassName = "CommonPromotion";
                    model.VoucherUsedQuantity = 0;

                    promotionId = await promotionApi.CreatePromotionApplyforStore(model, model.storeArray);

                    if (promotionId != -1)
                    {
                        if (model.storeArray != null)
                        {
                            List<StoreViewModel> stores = new List<StoreViewModel>();
                            foreach (var storeId in model.storeArray)
                            {
                                var applyStore = storeApi.Get(storeId);
                                stores.Add(applyStore);
                            }
                            await Utils.PostNotiMessageToStores(stores, (int)NotifyMessageType.PromotionChange);
                        }

                        if (model.IsVoucher.Value)
                        {
                            if (isOneVoucher)
                            {
                                var voucher = new VoucherViewModel();
                                voucher.PromotionID = promotionId;
                                voucher.Quantity = model.VoucherQuantity.Value;
                                voucher.Active = true;
                                voucher.UsedQuantity = 0;
                                voucher.VoucherCode = model.PromotionCode + voucherApi.GetGeneratedCode(5);

                                voucherApi.Create(voucher);
                            }
                            else
                            {
                                VoucherViewModel voucher = null;
                                for (int i = 0; i < model.VoucherQuantity; ++i)
                                {
                                    voucher = new VoucherViewModel();
                                    voucher.PromotionID = promotionId;
                                    voucher.Quantity = 1;
                                    voucher.Active = true;
                                    voucher.UsedQuantity = 0;
                                    voucher.VoucherCode = model.PromotionCode + voucherApi.GetGeneratedCode(5);

                                    voucherApi.Create(voucher);
                                }
                            }
                        }
                        return Json(new { success = true, message = "Thêm khuyến mãi thành công!", promotionId = promotionId });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Thêm khuyến mãi thất bại!", promotionId = promotionId });
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Thêm khuyến mãi thất bại", promotionId = promotionId });
            }
        }
        #endregion

        #region Edit

        public async Task<ActionResult> Edit(int id)
        {
            var api = new PromotionApi();
            var promotion = api.GetPromotionById(id);
            PromotionEditViewModel model = new PromotionEditViewModel(promotion, this.Mapper);
            var imageFontIcon = model.ImageCss.Replace(".", "");
            StoreApi storeapi = new StoreApi();
            var allStore = await storeapi.GetActiveAsync();
            PromotionStoreMappingApi mappingApi = new PromotionStoreMappingApi();
            var storeApplyPromotion = mappingApi.GetActivePromotionStoreMappingByPromotionID(promotion.PromotionID);

            model.AvailableStore = allStore.ToSelectList(q => q.Name, q => q.ID + "",
                q => storeApplyPromotion.FirstOrDefault(a => a.StoreId == q.ID) != null ? true : false);

            if (!string.IsNullOrWhiteSpace(imageFontIcon))
            {
                model.ImageCssEnum = (ImageCssEnum)Enum.Parse(typeof(IconCategoryEnum), imageFontIcon);
            }
            else
            {
                model.ImageCssEnum = ImageCssEnum.glass;
            }

            PrepareCreate(model);
            return this.View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="imageChoice">imageChoice = Add- add ảnh mới; = Keep - giữ nguyên; Remove - xóa ảnh</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> EditPromotion(PromotionEditViewModel model, string imageChoice)
        {
            var api = new PromotionApi();
            var storeApi = new StoreApi();
            if (model.PromotionName == null || model.PromotionName == "")
            {
                return Json(new { success = false, message = "Tên khuyến mãi không được để trống" });
            }
            if (model.Description == null || model.Description == "")
            {
                return Json(new { success = false, message = "Phần mô tả không được để trống" });
            }
            try
            {
                //var uploadImage = model.imageUpload;
                //if (imageChoice == "Add")
                //{
                //    //CloudBlockBlob imageBlob = await UploadAndSaveBlobAsync(uploadImage);
                //    //model.ImageUrl = imageBlob.Uri.ToString();
                //}
                //else 
                if (imageChoice == "Remove")
                {
                    model.ImageUrl = null;
                }
                PromotionStoreMappingApi mappingApi = new PromotionStoreMappingApi();
                var allPromotionStore = mappingApi.GetAllPromotionStoreMappingByPromotionID(model.PromotionID).ToList();
                var activePromotionStore = mappingApi.GetActivePromotionStoreMappingByPromotionID(model.PromotionID).ToList();

                if (activePromotionStore.Count != 0)
                {
                    var stores = new List<StoreViewModel>();
                    foreach (var item in activePromotionStore)
                    {
                        item.Active = false;
                        var store = storeApi.Get(item.StoreId);
                        stores.Add(store);
                        await mappingApi.EditAsync(item.Id, item);
                    }
                    await Utils.PostNotiMessageToStores(stores, (int)NotifyMessageType.PromotionChange);
                }
                if (model.storeArray != null)
                {
                    var stores = new List<StoreViewModel>();
                    foreach (var item in model.storeArray)
                    {
                        if (allPromotionStore.FirstOrDefault(q => q.StoreId == item) != null)
                        {
                            var temp = allPromotionStore.FirstOrDefault(q => q.StoreId == item);
                            temp.Active = true;
                            await mappingApi.EditAsync(temp.Id, temp);
                        }
                        else
                        {
                            await mappingApi.CreatePromotionStoreMapping(model.PromotionID, item);
                        }
                        var applyStore = storeApi.Get(item);
                        stores.Add(applyStore);
                    }
                    await Utils.PostNotiMessageToStores(stores, (int)NotifyMessageType.PromotionChange);
                }

                await api.EditAsync(model.PromotionID, model);
                return Json(new { success = true, message = "Cập nhật khuyến mãi thành công" });
            }
            catch (Exception e)
            {
                throw e;

            }

        }
        #endregion
        //Deactive Promotion
        public async Task<JsonResult> DeactivePromotion(int id)
        {
            var api = new PromotionApi();
            var storeApi = new StoreApi();
            var promotionStoreApi = new PromotionStoreMappingApi();
            var storeList = new List<StoreViewModel>();
            try
            {
                var storeIds = promotionStoreApi.GetActivePromotionStoreMappingByPromotionID(id).Select(q => q.StoreId);
                foreach (var storeId in storeIds)
                {
                    var store = storeApi.Get(storeId);
                    storeList.Add(store);
                }
                await api.DeactivePromotionAsync(id);
                await Utils.PostNotiMessageToStores(storeList, (int)NotifyMessageType.PromotionChange);
                return Json(new
                {
                    success = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new
                {
                    success = false,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> DeletePromotion(int id)
        {
            var api = new PromotionApi();
            try
            {
                await api.DeleteAsync(id);
                return Json(new
                {
                    success = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new
                {
                    success = false,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetStoreList(int brandId)
        {
            try
            {
                StoreApi storeApi = new StoreApi();
                var store = storeApi.GetActiveStoreByBrandId(brandId).Where(a => a.Name != "Tổng đài").AsEnumerable();

                int count = 0;
                count = 1;
                var result = store.Select(q => new IConvertible[]{
                                            ++count,
                                            q.Name,
                                            q.ID }).ToArray();
                return Json(new { success = true, aaData = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
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

        public JsonResult GetOrderDataUsingPromotion(int brandId, int promotionId, int ApplyLevel, string startDate, string endDate, int seletedStoreId)
        {
            var orderApi = new OrderApi();
            var orderDetailApi = new OrderDetailApi();
            var orderPromoMappingApi = new OrderPromotionMappingApi();
            var orderDetailPromoMappingApi = new OrderDetailPromotionMappingApi();
            var dateReportApi = new DateReportApi();
            var storeApi = new StoreApi();


            var fromDate = startDate.ToDateTime().GetStartOfDate();
            var toDate = endDate.ToDateTime().GetEndOfDate();

            try
            {
                List<ReportInfo> orderWithPromotion = new List<ReportInfo>();
                if (ApplyLevel == 0) // Hóa đơn
                {
                    var order = orderApi.GetAllOrderByDate(fromDate, toDate, brandId);
                    var orderPromotionMapping = orderPromoMappingApi.GetActiveReturnEntities()
                                .Where(q => q.PromotionId == promotionId);

                    if (seletedStoreId > 0)
                    {
                        order = order.Where(q => q.StoreID == seletedStoreId);
                    }

                    var rs = order.Join(orderPromotionMapping, p => p.RentID, q => q.OrderId, (p, q) => p).ToList();

                    for (var d = fromDate; d <= toDate; d = d.AddDays(1))
                    {
                        var eDate = d.GetEndOfDate();
                        var ordersInDate = rs.Where(q => d <= q.CheckInDate && q.CheckInDate <= eDate);
                        orderWithPromotion.Add(new ReportInfo
                        {
                            Date = d,
                            Count = ordersInDate.Count(),
                            TotalAmount = ordersInDate.Count() > 0 ? ordersInDate.Sum(q => q.TotalAmount) : 0,
                            FinalAmount = ordersInDate.Count() > 0 ? ordersInDate.Sum(q => q.FinalAmount) : 0
                        });
                    }
                }
                else // Sản phẩm
                {
                    var orderDetailList = orderDetailApi.GetAllOrderDetailByTimeRange(fromDate, toDate, brandId);
                    var orderDetailMapping = orderDetailPromoMappingApi.GetActiveReturnEntities()
                                .Where(q => q.PromotionId == promotionId);

                    if (seletedStoreId > 0)
                    {
                        orderDetailList = orderDetailList.Where(q => q.StoreId == seletedStoreId);
                    }

                    var rs = orderDetailList.Join(orderDetailMapping,
                            p => p.OrderDetailID, q => q.OrderDetailId, (p, q) => p).ToList();

                    for (var d = fromDate; d <= toDate; d = d.AddDays(1))
                    {
                        var eDate = d.GetEndOfDate();
                        var ordersInDate = rs.Where(q => d <= q.OrderDate && q.OrderDate <= eDate);
                        orderWithPromotion.Add(new ReportInfo
                        {
                            Date = d,
                            Count = ordersInDate.Count(),
                            TotalAmount = ordersInDate.Count() > 0 ? ordersInDate.Sum(q => q.TotalAmount) : 0,
                            FinalAmount = ordersInDate.Count() > 0 ? ordersInDate.Sum(q => q.FinalAmount) : 0
                        });
                    }
                }

                // Đếm số lượng Order theo từng ngày KHÔNG CÓ Promotion
                var orders = dateReportApi.GetDateReportTimeRangeAndBrand(fromDate, toDate, brandId);
                if (seletedStoreId > 0)
                {
                    orders = orders.Where(q => q.StoreID == seletedStoreId);
                }

                var orderWithoutPromotion = orders.GroupBy(q => q.Date).Select(q => new ReportInfo
                {
                    Date = q.Key,
                    Count = q.Sum(o => o.TotalOrder),
                    //TotalAmount = q.Sum(o => o.TotalAmount ?? 0),
                    //FinalAmount = q.Sum(o => o.FinalAmount ?? 0)
                    TotalAmount = q.Select(o => o.TotalAmount ?? 0).DefaultIfEmpty(0).Sum(),
                    FinalAmount = q.Select(o => o.TotalAmount ?? 0).DefaultIfEmpty(0).Sum()
                }).ToList();

                var today = DateTime.Now.GetEndOfDate();
                if (today <= toDate) // Đếm order của hôm nay
                {
                    var todayOrder = orderApi.GetTodayOrders(brandId);
                    if (seletedStoreId > 0)
                    {
                        todayOrder = todayOrder.Where(q => q.StoreID == seletedStoreId);
                    }
                    orderWithoutPromotion.Add(new ReportInfo
                    {
                        Date = today,
                        Count = todayOrder.Count(),
                        TotalAmount = todayOrder.Count() > 0 ? todayOrder.Sum(q => q.TotalAmount) : 0,
                        FinalAmount = todayOrder.Count() > 0 ? todayOrder.Sum(q => q.FinalAmount) : 0
                    });

                    // Add ngày thừa vào list
                    for (var d = today.AddDays(1); d <= toDate; d = d.AddDays(1))
                    {
                        orderWithoutPromotion.Add(new ReportInfo
                        {
                            Date = d,
                            Count = 0,
                            TotalAmount = 0,
                            FinalAmount = 0
                        });
                    }
                }

                orderWithPromotion = orderWithPromotion.OrderBy(q => q.Date).ToList();
                orderWithoutPromotion = orderWithoutPromotion.OrderBy(q => q.Date).ToList();

                var dateList = orderWithPromotion.Select(q => q.Date.ToString("dd/MM/yyyy")).ToList();
                var amountChart = new
                {
                    dateList = dateList,
                    order = orderWithoutPromotion.Select(q => q.Count),
                    orderPromotion = orderWithPromotion.Select(q => q.Count)
                };

                var moneyChart = new
                {
                    dateList = dateList,
                    orderTotal = orderWithoutPromotion.Select(q => q.TotalAmount),
                    orderFinal = orderWithoutPromotion.Select(q => q.FinalAmount),
                    orderPromotionTotal = orderWithPromotion.Select(q => q.TotalAmount),
                    orderPromotionFinal = orderWithPromotion.Select(q => q.FinalAmount),
                };

                var datatable = new List<IConvertible[]>();
                for (int i = 0; i < dateList.Count(); ++i)
                {
                    var row = new IConvertible[]
                    {
                        dateList[i],
                        orderWithPromotion[i].Count,
                        orderWithPromotion[i].TotalAmount,
                        orderWithPromotion[i].FinalAmount,
                        orderWithoutPromotion[i].Count,
                        orderWithoutPromotion[i].FinalAmount,
                    };
                    datatable.Add(row);
                }

                return Json(new
                {
                    success = true,
                    amountChart = amountChart,
                    moneyChart = moneyChart,
                    datatable = datatable
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Trả về tổng số hóa đơn dùng mỗi ngày
        /// </summary>
        /// <param name="brandId"></param>
        /// <param name="promotionId"></param>
        //public JsonResult GetOrderCountUsingPromotion(int brandId, int promotionId, int ApplyLevel, DateTime fromDate, DateTime toDate, int seletedStoreId)
        //{
        //    var orderApi = new OrderApi();
        //    var orderDetailApi = new OrderDetailApi();
        //    var orderPromoMappingApi = new OrderPromotionMappingApi();
        //    var orderDetailPromoMappingApi = new OrderDetailPromotionMappingApi();
        //    var dateReportApi = new DateReportApi();
        //    var storeApi = new StoreApi();

        //    try
        //    {
        //        ////toDate = toDate.AddDays(1);
        //        //var orderList = new OrderApi().getAllFieldthatNeedforReportByDate(fromDate, toDate);
        //        //var orderPromotionMapping = new OrderPromotionMappingApi().GetActiveReturnEntities();
        //        //var orderDetailList = new OrderDetailApi().getAllFieldthatNeedforReportByDate(fromDate, toDate);
        //        //var orderDetailPromotionMapping = new OrderDetailPromotionMappingApi().GetActiveReturnEntities();
        //        //var storeInBrand = new StoreApi().GetActiveStoreByBrandId(brandId);
        //        //List<TempResult> result = null;
        //        ////DateTime toDate2 = toDate.GetEndOfDate(); // Đổi cách lấy để xem performance
        //        //if (seletedStoreId == -1) // lấy tất cả Store
        //        //{
        //        //    if (ApplyLevel == 0)//hóa đơn
        //        //    {
        //        //        result = (from a in orderList
        //        //                  join b in orderPromotionMapping on a.RentID equals b.OrderId
        //        //                  //where b.PromotionId == promotionId && a.CheckInDate >= fromDate && a.CheckInDate <= toDate2
        //        //                  where b.PromotionId == promotionId
        //        //                  group a by a.CheckInDate.Value.Date into g
        //        //                  orderby g.Key descending
        //        //                  select new TempResult
        //        //                  {
        //        //                      Date = g.Key,
        //        //                      Count = g.Count()
        //        //                  }).AsEnumerable().ToList();
        //        //    }
        //        //    else
        //        //    { // sản phẩm
        //        //        result = (from v in
        //        //                      (from a in orderDetailList
        //        //                       join b in orderDetailPromotionMapping on a.OrderDetailID equals b.OrderDetailId
        //        //                       where b.PromotionId == promotionId
        //        //                       group a by a.RentID into ab
        //        //                       select new
        //        //                       {
        //        //                           RentID = ab.Key,
        //        //                       }
        //        //                       )

        //        //                  join c in orderList on v.RentID equals c.RentID
        //        //                  //where c.CheckInDate >= fromDate && c.CheckInDate <= toDate2
        //        //                  group c by c.CheckInDate.Value.Date into z
        //        //                  orderby z.Key descending
        //        //                  select new TempResult
        //        //                  {
        //        //                      Date = z.Key,
        //        //                      Count = z.Count()
        //        //                  }).ToList();
        //        //    }
        //        //}
        //        //else // lấy theo từng Store
        //        //{
        //        //    if (ApplyLevel == 0)//hóa đơn
        //        //    {
        //        //        result = (from a in orderList
        //        //                  join b in orderPromotionMapping on a.RentID equals b.OrderId
        //        //                  //where b.PromotionId == promotionId && a.CheckInDate >= fromDate && a.CheckInDate <= toDate2 && a.StoreID == seletedStoreId
        //        //                  where b.PromotionId == promotionId && a.StoreID == seletedStoreId
        //        //                  group a by a.CheckInDate.Value.Date into g
        //        //                  orderby g.Key descending
        //        //                  select new TempResult
        //        //                  {
        //        //                      Date = g.Key,
        //        //                      Count = g.Count()
        //        //                  }).ToList();
        //        //    }
        //        //    else
        //        //    { // sản phẩm
        //        //        result = (from v in
        //        //                      (from a in orderDetailList
        //        //                       join b in orderDetailPromotionMapping on a.OrderDetailID equals b.OrderDetailId
        //        //                       where b.PromotionId == promotionId
        //        //                       group a by a.RentID into ab
        //        //                       select new
        //        //                       {
        //        //                           RentID = ab.Key,
        //        //                       }
        //        //                       )

        //        //                  join c in orderList on v.RentID equals c.RentID
        //        //                  //where c.CheckInDate >= fromDate && c.CheckInDate <= toDate2 && c.StoreID == seletedStoreId
        //        //                  where c.StoreID == seletedStoreId
        //        //                  group c by c.CheckInDate.Value.Date into z
        //        //                  orderby z.Key descending
        //        //                  select new TempResult
        //        //                  {
        //        //                      Date = z.Key,
        //        //                      Count = z.Count()
        //        //                  }).ToList();
        //        //    }
        //        //}


        //        //for (int i = 0; i < result.Count; i++)
        //        //{
        //        //    if (result[0].Date.CompareTo(toDate.Date) < 0)
        //        //    {
        //        //        result.Insert(0, new TempResult { Date = result[0].Date.AddDays(1), Count = 0 });
        //        //    }
        //        //    else if (i < result.Count - 2 && result[i].Date.AddDays(-1).CompareTo(result[i + 1].Date) > 0)
        //        //    {
        //        //        result.Insert(i + 1, new TempResult { Date = result[i].Date.AddDays(-1), Count = 0 });
        //        //    }
        //        //    else if (result[result.Count - 1].Date.CompareTo(fromDate.Date) > 0)
        //        //    {
        //        //        result.Add(new TempResult { Date = result[result.Count - 1].Date.AddDays(-1), Count = 0 });
        //        //    }
        //        //}

        //        //var result2 = orderList
        //        //    //.Where(q => q.CheckInDate >= fromDate && q.CheckInDate <= toDate2 && storeInBrand.Any(a => a.ID == q.StoreID))
        //        //    .Where(q => storeInBrand.Any(a => a.ID == q.StoreID))
        //        //    .GroupBy(q => q.CheckInDate.Value.Date)
        //        //    .OrderByDescending(q => q.Key)
        //        //    .Select(q => new { Date = q.Key, Count = q.Count() }).ToList();

        //        //for (int i = 0; i < result2.Count; i++)
        //        //{
        //        //    if (result2[0].Date.CompareTo(toDate.Date) < 0)
        //        //    {
        //        //        result2.Insert(0, new { Date = result2[0].Date.AddDays(1), Count = 0 });
        //        //    }
        //        //    else if (i < result2.Count - 2 && result2[i].Date.AddDays(-1).CompareTo(result2[i + 1].Date) > 0)
        //        //    {
        //        //        result2.Insert(i + 1, new { Date = result2[i].Date.AddDays(-1), Count = 0 });
        //        //    }
        //        //    else if (result2[result2.Count - 1].Date.CompareTo(fromDate.Date) > 0)
        //        //    {
        //        //        result2.Add(new { Date = result2[result2.Count - 1].Date.AddDays(-1), Count = 0 });
        //        //    }
        //        //}

        //        // Tối ưu, tăng performance cho code ở trên
        //        // Đếm số lượng Order theo từng ngày CÓ Promotion
        //        List<ReportInfo> orderWithPromotion = new List<ReportInfo>();
        //        if (ApplyLevel == 0) // Hóa đơn
        //        {
        //            var order = orderApi.GetAllOrderByDate(fromDate, toDate, brandId);
        //            var orderPromotionMapping = orderPromoMappingApi.GetActiveReturnEntities()
        //                        .Where(q => q.PromotionId == promotionId);

        //            if (seletedStoreId > 0)
        //            {
        //                order = order.Where(q => q.StoreID == seletedStoreId);
        //            }

        //            var rs = order.Join(orderPromotionMapping, p => p.RentID, q => q.OrderId, (p, q) => p).ToList();

        //            for (var d = fromDate; d <= toDate; d = d.AddDays(1))
        //            {
        //                var eDate = d.GetEndOfDate();
        //                var ordersInDate = rs.Where(q => q.CheckInDate >= d && eDate <= q.CheckInDate);
        //                orderWithPromotion.Add(new ReportInfo
        //                {
        //                    Date = d,
        //                    Count = ordersInDate.Count(),
        //                    TotalAmount = ordersInDate.Sum(q => q.TotalAmount),
        //                    FinalAmount = ordersInDate.Sum(q => q.FinalAmount)
        //                });
        //            }
        //        }
        //        else // Sản phẩm
        //        {
        //            var orderDetailList = orderDetailApi.GetAllOrderDetailByTimeRange(fromDate, toDate, brandId);
        //            var orderDetailMapping = orderDetailPromoMappingApi.GetActiveReturnEntities()
        //                        .Where(q => q.PromotionId == promotionId);
        //            var rs = orderDetailList.Join(orderDetailMapping,
        //                    p => p.OrderDetailID, q => q.OrderDetailId, (p, q) => p).ToList();

        //            for (var d = fromDate; d <= toDate; d = d.AddDays(1))
        //            {
        //                var eDate = d.GetEndOfDate();
        //                var ordersInDate = rs.Where(q => q.OrderDate >= d && eDate <= q.OrderDate);
        //                orderWithPromotion.Add(new ReportInfo
        //                {
        //                    Date = d,
        //                    Count = ordersInDate.Count(),
        //                    TotalAmount = ordersInDate.Sum(q => q.TotalAmount),
        //                    FinalAmount = ordersInDate.Sum(q => q.FinalAmount)
        //                });
        //            }
        //        }

        //        // Đếm số lượng Order theo từng ngày KHÔNG CÓ Promotion
        //        var orderWithoutPromotion = dateReportApi.GetDateReportTimeRangeAndBrand(fromDate, toDate, brandId)
        //                     .GroupBy(q => q.Date).Select(q => new ReportInfo
        //                     {
        //                         Date = q.Key,
        //                         Count = q.Sum(o => o.TotalOrder),
        //                         TotalAmount = q.Sum(o => o.TotalAmount ?? 0),
        //                         FinalAmount = q.Sum(o => o.FinalAmount ?? 0)
        //                     }).ToList();

        //        var today = DateTime.Now;
        //        if (today.GetEndOfDate() == toDate.GetEndOfDate()) // Đếm order của hôm nay
        //        {
        //            var todayOrder = orderApi.GetTodayOrders(brandId);

        //            orderWithoutPromotion.Add(new ReportInfo
        //            {
        //                Date = today,
        //                Count = todayOrder.Count(),
        //                FinalAmount = todayOrder.Sum(q => q.FinalAmount)
        //            });
        //        }

        //        orderWithPromotion = orderWithPromotion.OrderByDescending(q => q.Date).ToList();
        //        orderWithoutPromotion = orderWithoutPromotion.OrderByDescending(q => q.Date).ToList();

        //        return Json(new { success = true, orderWithPromotion = orderWithPromotion, order = orderWithoutPromotion }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new { success = false, data = e.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}


        ///// <summary>
        ///// Trả về tông số tiền thu được của những order xài promotion này
        ///// </summary>
        ///// <param name="brandId"></param>
        ///// <param name="promotionId"></param>
        //public JsonResult GetOrderMoneyUsingPromotion(int brandId, int promotionId, int ApplyLevel, DateTime fromDate, DateTime toDate, int seletedStoreId)
        //{
        //    try
        //    {
        //        //toDate = toDate.AddDays(1);
        //        var orderList = new OrderApi().getAllFieldthatNeedforReportByDate(fromDate, toDate);
        //        var orderPromotionMapping = new OrderPromotionMappingApi().GetActiveReturnEntities();
        //        var orderDetailList = new OrderDetailApi().getAllFieldthatNeedforReportByDate(fromDate, toDate);
        //        var orderDetailPromotionMapping = new OrderDetailPromotionMappingApi().GetActiveReturnEntities();
        //        var promotions = new PromotionApi().GetPromotionByBrandId(brandId);
        //        var storeInBrand = new StoreApi().GetActiveStoreByBrandId(brandId);
        //        List<TempResult> result = null;
        //        //DateTime toDate2 = toDate.AddDays(1);
        //        if (seletedStoreId == -1) // lấy tất cả Store
        //        {
        //            if (ApplyLevel == 0)//hóa đơn
        //            {
        //                result = (from a in orderList
        //                          join b in orderPromotionMapping on a.RentID equals b.OrderId
        //                          //where b.PromotionId == promotionId && a.CheckInDate >= fromDate && a.CheckInDate <= toDate2
        //                          where b.PromotionId == promotionId
        //                          group a by a.CheckInDate.Value.Date into g
        //                          orderby g.Key descending
        //                          select new TempResult
        //                          {
        //                              Date = g.Key,
        //                              Count = g.Sum(x => x.TotalAmount),
        //                              Count2 = g.Sum(x => x.FinalAmount)
        //                          }).ToList();


        //            }
        //            else
        //            { // sản phẩm
        //                result = (from v in
        //                               (from a in orderDetailList
        //                                join b in orderDetailPromotionMapping on a.OrderDetailID equals b.OrderDetailId
        //                                where b.PromotionId == promotionId
        //                                group a by a.RentID into ab
        //                                select new
        //                                {
        //                                    RentID = ab.Key,
        //                                }
        //                                )

        //                          join c in orderList on v.RentID equals c.RentID
        //                          //where c.CheckInDate >= fromDate && c.CheckInDate <= toDate2
        //                          group c by c.CheckInDate.Value.Date into z
        //                          orderby z.Key descending
        //                          select new TempResult
        //                          {
        //                              Date = z.Key,
        //                              Count = z.Sum(x => x.TotalAmount),
        //                              Count2 = z.Sum(x => x.FinalAmount)
        //                          }).ToList();
        //            }
        //        }
        //        else // lấy theo từng Store
        //        {
        //            if (ApplyLevel == 0)//hóa đơn
        //            {
        //                result = (from a in orderList
        //                          join b in orderPromotionMapping on a.RentID equals b.OrderId
        //                          //where b.PromotionId == promotionId && a.CheckInDate >= fromDate && a.CheckInDate <= toDate2 && a.StoreID == seletedStoreId
        //                          where b.PromotionId == promotionId && a.StoreID == seletedStoreId
        //                          group a by a.CheckInDate.Value.Date into g
        //                          orderby g.Key descending
        //                          select new TempResult
        //                          {
        //                              Date = g.Key.Date,
        //                              Count = g.Sum(x => x.TotalAmount),
        //                              Count2 = g.Sum(x => x.FinalAmount)
        //                          }).ToList();
        //            }
        //            else
        //            { // sản phẩm
        //                result = (from v in
        //                              (from a in orderDetailList
        //                               join b in orderDetailPromotionMapping on a.OrderDetailID equals b.OrderDetailId
        //                               where b.PromotionId == promotionId
        //                               group a by a.RentID into ab
        //                               select new
        //                               {
        //                                   RentID = ab.Key,
        //                               }
        //                               )

        //                          join c in orderList on v.RentID equals c.RentID
        //                          //where c.CheckInDate >= fromDate && c.CheckInDate <= toDate2 && c.StoreID == seletedStoreId
        //                          where c.StoreID == seletedStoreId
        //                          group c by c.CheckInDate.Value.Date into z
        //                          orderby z.Key descending
        //                          select new TempResult
        //                          {
        //                              Date = z.Key,
        //                              Count = z.Sum(x => x.TotalAmount),
        //                              Count2 = z.Sum(x => x.FinalAmount)
        //                          }).ToList();
        //            }
        //        }
        //        for (int i = 0; i < result.Count; i++)
        //        {
        //            if (result[0].Date.CompareTo(toDate) < 0)
        //            {
        //                result.Insert(0, new TempResult { Date = result[0].Date.AddDays(1), Count = 0 });
        //            }
        //            else if (i < result.Count - 2 && result[i].Date.AddDays(-1).CompareTo(result[i + 1].Date) > 0)
        //            {
        //                result.Insert(i + 1, new TempResult { Date = result[i].Date.AddDays(-1), Count = 0 });
        //            }
        //            else if (result[result.Count - 1].Date.CompareTo(fromDate) > 0)
        //            {

        //                result.Add(new TempResult { Date = result[result.Count - 1].Date.AddDays(-1), Count = 0 });
        //            }
        //        }
        //        var result2 = orderList
        //            //.Where(q => q.CheckInDate >= fromDate && q.CheckInDate <= toDate2 && storeInBrand.Any(a => a.ID == q.StoreID))
        //            .Where(q => storeInBrand.Any(a => a.ID == q.StoreID))
        //            .GroupBy(q => q.CheckInDate.Value.Date)
        //            .OrderByDescending(q => q.Key)
        //            .Select(q => new { Date = q.Key, Count = q.Sum(x => x.FinalAmount) }).ToList();
        //        for (int i = 0; i < result2.Count; i++)
        //        {
        //            if (result2[0].Date.CompareTo(toDate.Date) < 0)
        //            {
        //                result2.Insert(0, new { Date = result2[0].Date.AddDays(1), Count = 0.0 });
        //            }
        //            else if (i < result2.Count - 2 && result2[i].Date.AddDays(-1).CompareTo(result2[i + 1].Date) > 0)
        //            {
        //                result2.Insert(i + 1, new { Date = result2[i].Date.AddDays(-1), Count = 0.0 });
        //            }
        //            else if (result2[result2.Count - 1].Date.CompareTo(fromDate.Date) > 0)
        //            {
        //                result2.Add(new { Date = result2[result2.Count - 1].Date.AddDays(-1), Count = 0.0 });
        //            }
        //        }
        //        return Json(new { success = true, orderWithPromotion = result, order = result2 }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new { success = false, data = e.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpPost]
        public JsonResult getHotInformationByPromotionId(int brandId, int promotionId, int ApplyLevel, string startDate, string endDate)
        {
            var orderApi = new OrderApi();
            var orderDetailApi = new OrderDetailApi();
            var orderPromoMappingApi = new OrderPromotionMappingApi();
            var orderDetailPromoMappingApi = new OrderDetailPromotionMappingApi();
            var storeApi = new StoreApi();

            var fromDate = startDate.ToDateTime().GetStartOfDate();
            var toDate = endDate.ToDateTime().GetEndOfDate();

            //var orderList = new OrderApi().getAllFieldthatNeedforReportByDate(fromDate.Value, toDate.Value);
            //var orderPromotionList = new OrderPromotionMappingApi().GetActiveReturnEntities();
            //var orderDetailList = new OrderDetailApi().getAllFieldthatNeedforReportByDate(fromDate.Value, toDate.Value);
            //var orderDetailPromotionList = new OrderDetailPromotionMappingApi().GetActiveReturnEntities();
            //var storeList = new StoreApi().GetActiveReturnEntities().AsEnumerable();
            double moneyWithPromotion = 0;
            double moneyWithoutPromotion = 0;
            ReportInfo highestMoneyinDay = null;
            StoreReportInfo topSellerStore = null;

            if (ApplyLevel == 0)// hóa đơn
            {
                //var temp =
                //             (from a in orderList
                //              join b in orderPromotionList on a.RentID equals b.OrderId
                //              //where b.PromotionId == promotionId && a.CheckInDate >= fromDate && a.CheckInDate <= toDate
                //              where b.PromotionId == promotionId
                //              select new
                //              {
                //                  a.FinalAmount
                //              });
                //moneyWithPromotion = temp.Count() == 0 ? 0 : temp.Sum(q => q.FinalAmount);

                //var temp2 =
                //            (from a in orderList
                //             join b in orderPromotionList on a.RentID equals b.OrderId
                //             //where b.PromotionId == promotionId && a.CheckInDate >= fromDate && a.CheckInDate <= toDate
                //             where b.PromotionId == promotionId
                //             select new
                //             {
                //                 a.TotalAmount
                //             });
                //moneyWithoutPromotion = temp.Count() == 0 ? 0 : temp2.Sum(q => q.TotalAmount);

                //var temp3 = (from a in orderList
                //             join b in orderPromotionList on a.RentID equals b.OrderId
                //             //where b.PromotionId == promotionId && a.CheckInDate >= fromDate && a.CheckInDate <= toDate
                //             where b.PromotionId == promotionId
                //             group a by a.CheckInDate.Value.Date into c
                //             select new
                //             {
                //                 Date = c.Key,
                //                 FinalAmount = c.Sum(q => q.FinalAmount)
                //             });
                //highestMoneyinDay = temp3.Count() == 0 ? null : temp3.OrderByDescending(q => q.FinalAmount).First();

                //var temp4 = (from x in storeList
                //             join y in (from a in orderList
                //                        join b in orderPromotionList on a.RentID equals b.OrderId
                //                        //where b.PromotionId == promotionId && a.CheckInDate >= fromDate && a.CheckInDate <= toDate
                //                        where b.PromotionId == promotionId
                //                        group a by a.StoreID into c
                //                        select new
                //                        {
                //                            StoreID = c.Key.Value,
                //                            Money = c.Sum(q => q.FinalAmount)
                //                        })
                //               on x.ID equals y.StoreID
                //             select new
                //             {
                //                 StoreName = x.Name,
                //                 Money = y.Money
                //             }
                //                 );
                //topSellerStore = temp4.Count() == 0 ? null : temp4.OrderByDescending(q => q.Money).First();

                // Rút gọn comment ở trên
                var orderList = orderApi.GetAllOrderByDate(fromDate, toDate, brandId);
                var orderPromotionMapping = orderPromoMappingApi.GetActiveReturnEntities()
                            .Where(q => q.PromotionId == promotionId);

                var rs = orderList.Join(orderPromotionMapping, p => p.RentID, q => q.OrderId, (p, q) => p).ToList();

                moneyWithPromotion = rs.Sum(q => q.FinalAmount);
                moneyWithoutPromotion = rs.Sum(q => q.TotalAmount);

                if (rs.Count > 0)
                {
                    topSellerStore = rs.Where(q => q.StoreID != null)
                    .GroupBy(q => q.StoreID).Select(q => new StoreReportInfo
                    {
                        StoreId = q.Key.Value,
                        FinalAmount = q.Sum(o => o.FinalAmount)
                    }).OrderByDescending(q => q.FinalAmount).FirstOrDefault();
                    var store = storeApi.GetStoreById(topSellerStore.StoreId);
                    topSellerStore.StoreName = store != null ? store.Name : "";
                }
                else
                {
                    topSellerStore = new StoreReportInfo()
                    {
                        StoreName = "N/A",
                        FinalAmount = 0
                    };
                }

                var dateReportList = new List<ReportInfo>();
                for (var d = fromDate; d <= toDate; d = d.AddDays(1))
                {
                    var eDate = d.GetEndOfDate();
                    var ordersInDate = rs.Where(q => d <= q.CheckInDate && q.CheckInDate <= eDate);
                    dateReportList.Add(new ReportInfo
                    {
                        Date = d,
                        FinalAmount = ordersInDate.Sum(q => q.FinalAmount)
                    });
                }
                highestMoneyinDay = dateReportList.OrderByDescending(q => q.FinalAmount).FirstOrDefault();
            }
            else
            {
                //var temp =
                //             (from a in orderDetailList
                //              join b in orderDetailPromotionList on a.OrderDetailID equals b.OrderDetailId
                //              //where b.PromotionId == promotionId && a.OrderDate >= fromDate && a.OrderDate <= toDate
                //              where b.PromotionId == promotionId
                //              select new
                //              {
                //                  a.FinalAmount
                //              });

                //moneyWithPromotion = temp.Count() == 0 ? 0 : temp.Sum(q => q.FinalAmount);

                //var temp2 =
                //              (from a in orderDetailList
                //               join b in orderDetailPromotionList on a.OrderDetailID equals b.OrderDetailId
                //               //where b.PromotionId == promotionId && a.OrderDate >= fromDate && a.OrderDate <= toDate
                //               where b.PromotionId == promotionId
                //               select new
                //               {
                //                   a.TotalAmount
                //               });
                //moneyWithoutPromotion = temp2.Count() == 0 ? 0 : temp2.Sum(q => q.TotalAmount);

                //var temp3 = (from a in orderDetailList
                //             join b in orderDetailPromotionList on a.OrderDetailID equals b.OrderDetailId
                //             //where b.PromotionId == promotionId && a.OrderDate >= fromDate && a.OrderDate <= toDate
                //             where b.PromotionId == promotionId
                //             group a by a.OrderDate.Date into c
                //             select new
                //             {
                //                 Date = c.Key,
                //                 FinalAmount = c.Sum(q => q.FinalAmount)
                //             });
                //highestMoneyinDay = temp3.Count() == 0 ? null : temp3.OrderByDescending(q => q.FinalAmount).First();

                //var temp4 = (from x in storeList
                //             join y in (from a in orderDetailList
                //                        join b in orderDetailPromotionList on a.OrderDetailID equals b.OrderDetailId
                //                        join c in orderList on a.RentID equals c.RentID
                //                        //where b.PromotionId == promotionId && a.OrderDate >= fromDate && a.OrderDate <= toDate
                //                        where b.PromotionId == promotionId
                //                        group a by c.StoreID into z
                //                        select new
                //                        {
                //                            StoreID = z.Key.Value,
                //                            Money = z.Sum(q => q.FinalAmount)
                //                        })
                //               on x.ID equals y.StoreID
                //             select new
                //             {
                //                 StoreName = x.Name,
                //                 Money = y.Money
                //             }
                //                 );

                //topSellerStore = temp4.Count() == 0 ? null : temp4.OrderByDescending(q => q.Money).First();

                var orderDetailList = orderDetailApi.GetAllOrderDetailByTimeRange(fromDate, toDate, brandId);
                var orderDetailMapping = orderDetailPromoMappingApi.GetActiveReturnEntities()
                            .Where(q => q.PromotionId == promotionId);
                var rs = orderDetailList.Join(orderDetailMapping,
                        p => p.OrderDetailID, q => q.OrderDetailId, (p, q) => p).ToList();

                moneyWithPromotion = rs.Sum(q => q.FinalAmount);
                moneyWithoutPromotion = rs.Sum(q => q.TotalAmount);

                if (rs.Count > 0)
                {
                    topSellerStore = rs.GroupBy(q => q.StoreId).Select(q => new StoreReportInfo
                    {
                        StoreId = q.Key.Value,
                        FinalAmount = q.Sum(o => o.FinalAmount)
                    }).OrderByDescending(q => q.FinalAmount).FirstOrDefault();
                    var store = storeApi.GetStoreById(topSellerStore.StoreId);
                    topSellerStore.StoreName = store != null ? store.Name : "";
                }
                else
                {
                    topSellerStore = new StoreReportInfo()
                    {
                        StoreName = "N/A",
                        FinalAmount = 0
                    };
                }

                var dateReportList = new List<ReportInfo>();
                for (var d = fromDate; d <= toDate; d = d.AddDays(1))
                {
                    var eDate = d.GetEndOfDate();
                    var ordersInDate = rs.Where(q => d <= q.OrderDate && q.OrderDate <= eDate);
                    dateReportList.Add(new ReportInfo
                    {
                        Date = d,
                        FinalAmount = ordersInDate.Sum(q => q.FinalAmount)
                    });
                }
                highestMoneyinDay = dateReportList.OrderByDescending(q => q.FinalAmount).FirstOrDefault();
            }

            return Json(new
            {
                moneyWithPromotion = moneyWithPromotion,
                moneyWithoutPromotion = moneyWithoutPromotion,
                highestMoneyinDay = highestMoneyinDay,
                topSellerStore = topSellerStore
            }, JsonRequestBehavior.AllowGet);
        }

        private class ReportInfo
        {
            public DateTime Date { get; set; }
            public int Count { get; set; }
            public double TotalAmount { get; set; }
            public double FinalAmount { get; set; }
        }

        private class StoreReportInfo
        {
            public int StoreId { get; set; }
            public string StoreName { get; set; }
            public double FinalAmount { get; set; }
        }

        int CheckExistPromoDetail(string promoCode)
        {
            var promoDetailApi = new PromotionDetailApi();
            var promotionDetails = promoDetailApi.GetDetailByCode(promoCode);
            for (int i = 0 ; i < promotionDetails.Count(); i++)
            {
                var p = promotionDetails.ToArray()[i];
                var toBeCheckedPromoDetails = promotionDetails.OrderBy(a => a.PromotionDetailID).Skip(i + 1);
                if (p.BuyProductCode == null)
                {
                    foreach (var tobeChecked in toBeCheckedPromoDetails)
                    {
                        if ((p.MinOrderAmount <= tobeChecked.MaxOrderAmount && p.MaxOrderAmount >= tobeChecked.MinOrderAmount) ||
                                    (p.MaxOrderAmount >= tobeChecked.MinOrderAmount && p.MinOrderAmount <= tobeChecked.MaxOrderAmount) ||
                                    (tobeChecked.MaxOrderAmount >= p.MinOrderAmount && p.MaxOrderAmount == null) ||
                                    (tobeChecked.MaxOrderAmount == null && tobeChecked.MinOrderAmount <= p.MaxOrderAmount) ||
                                    (p.MaxOrderAmount == null && tobeChecked.MaxOrderAmount == null))
                        {
                            return i + 1;
                        }
                    }
                }
                else
                {
                    foreach (var tobeChecked in toBeCheckedPromoDetails)
                    {
                        if ((p.MinBuyQuantity <= tobeChecked.MaxBuyQuantity && p.MaxBuyQuantity >= tobeChecked.MinBuyQuantity) ||
                                    (p.MaxBuyQuantity >= tobeChecked.MinBuyQuantity && p.MinBuyQuantity <= tobeChecked.MaxBuyQuantity) ||
                                    (tobeChecked.MaxBuyQuantity >= p.MinBuyQuantity && p.MaxBuyQuantity == null) ||
                                    (tobeChecked.MaxBuyQuantity == null && tobeChecked.MinBuyQuantity <= p.MaxBuyQuantity) ||
                                    (p.MaxBuyQuantity == null && tobeChecked.MaxBuyQuantity == null))
                        {
                            return i + 1;
                        }


                    }
                }
            }
            return -1;
        }

        public async Task<JsonResult> ChangeIsApplyOnce(int promotionId, bool isApplyOnce)
        {
            var promotionApi = new PromotionApi();

            try
            {
                var promoCode = promotionApi.Get(promotionId).PromotionCode;
                if (!String.IsNullOrEmpty(promoCode))
                {
                    await promotionApi.EditPromotionIsApplyOnce(promotionId, isApplyOnce);
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi trong quá trình xử lý, vui lòng liên hệ Admin"});

                }

            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại." });
            }
        }

        [HttpPost]
        public JsonResult getStoreByPromotionId(int brandId, int promotionId)
        {
            var storeApi = new StoreApi();
            //PromotionStoreMappingApi promotionStoreMapping = new PromotionStoreMappingApi();
            //var promotionStoreMappingList = promotionStoreMapping.GetActivePromotionStoreMappingByPromotionID(promotionId);
            //var storeList = storeApi.GetStoresByStoreIds(promotionStoreMappingList).ToList();

            var storeList = storeApi.GetStoresByPromotionId(promotionId, brandId).Select(q => new
            {
                Text = q.Name,
                Value = q.ID
            }).ToList();

            return Json(new
            {
                data = storeList

            }, JsonRequestBehavior.AllowGet);
        }


    }

    public class TempResult
    {
        public DateTime Date { get; set; }
        public double Count { get; set; }
        public double Count2 { get; set; }

        public TempResult(DateTime date, int count)
        {
            this.Date = date;
            this.Count = count;
        }

        public TempResult()
        {
        }
    }


}