using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using MessageCOM;
using Newtonsoft.Json;
using RedisCache;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Web.Http.Cors;
using System.Web.Mvc;
using Wisky.Api.Models;

namespace Wisky.Api.Controllers.API
{
    public class OrderApiController : BaseController
    {
        private static readonly string _storeCode = System.Configuration.ConfigurationManager.AppSettings["config.store.code"];
        private readonly string _accessToken = System.Configuration.ConfigurationManager.AppSettings["config.order.token"];
        private readonly string _enableToken = System.Configuration.ConfigurationManager.AppSettings["config.order.EnableToken"];


        private readonly ICustomerService _customerService = DependencyUtils.Resolve<ICustomerService>();
        private readonly IProductService _productService = DependencyUtils.Resolve<IProductService>();
        private readonly IOrderService _orderService = DependencyUtils.Resolve<IOrderService>();

        IDatabase db = null;

        private IDatabase GetDatabase()
        {
            if (db == null)
            {
                //Store to Redis
                var redisFactory = RedisConnectionFactory.Instance;
                var RedisConnection = redisFactory.GetConnection();
                db = RedisConnection != null ? RedisConnection.GetDatabase() : null;
            }
            return db;
        }

        //Máy POS gọi Api này để gửi đơn hàng từ POS lên trên server
        [HttpPost]
        public JsonResult SendOrderToServer(OrderApiViewModel model)
        {
            try
            {
                var status = 0;
                var orderApi = new OrderApi();
                var productApi = new ProductApi();
                var paymentApi = new PaymentApi();
                var orderdetailApi = new OrderDetailApi();

                #region Update status
                //Đơn hàng mới từ POS
                if (model.DeliveryStatus == (int)DeliveryStatus.New)
                {
                    if (model.OrderStatus == (int)OrderStatusEnum.PosFinished)
                    {
                        status = (int)OrderStatusEnum.Finish;
                    }
                    else if (model.OrderStatus == (int)OrderStatusEnum.PosProcessing
                                || model.OrderStatus == (int)OrderStatusEnum.Processing) //Giữ nguyên
                    {
                        status = (int)OrderStatusEnum.Processing;
                    }
                    else if (model.OrderStatus == (int)OrderStatusEnum.PosPreCancel)
                    {
                        status = (int)OrderStatusEnum.PreCancel;
                    }
                    else if (model.OrderStatus == (int)OrderStatusEnum.PosCancel)
                    {
                        status = (int)OrderStatusEnum.Cancel;
                    }
                }
                //Đơn hàng delivery
                else if (model.DeliveryStatus != (int)DeliveryStatus.New)
                {
                    //Giao hàng thành công
                    if (model.DeliveryStatus == (int)DeliveryStatus.Finish)
                    {
                        status = (int)OrderStatusEnum.Finish;
                    }
                    //Đơn hàng hủy trước chế biến
                    else if (model.DeliveryStatus == (int)DeliveryStatus.PreCancel)
                    {
                        status = (int)OrderStatusEnum.PreCancel;
                    }
                    //Đơn hàng hủy sau chế biến || Giao hàng không thành công
                    else if (model.DeliveryStatus == (int)DeliveryStatus.Cancel
                                || model.DeliveryStatus == (int)DeliveryStatus.Fail)
                    {
                        status = (int)OrderStatusEnum.Cancel;
                    }
                }
                #endregion

                //API return success case
                var result = new OrderStatusApiViewModel()
                {
                    OrderStatus = status,
                    InvoiceId = model.OrderCode,
                    DeliveryStatus = model.DeliveryStatus,
                    CheckInPerson = model.CheckInPerson,
                };

                //Check if order is existed in server
                var checkRent = orderApi.GetOrderByInvoiceId(model.OrderCode);

                //Nếu chưa có sẽ add đơn hàng vào db server
                if (checkRent == null)
                {
                    //Order
                    var order = new HmsService.Models.Entities.Order();
                    order.OrderStatus = status;
                    order.DeliveryStatus = model.DeliveryStatus;
                    order.CheckInPerson = model.CheckInPerson;
                    this.MapOrder(model, order);

                    //Orderdetail
                    foreach (var odm in model.OrderDetailMs.ToList())
                    {
                        var productId = productApi.GetProductByCode(odm.ProductCode).ProductID;
                        var orderdetail = new OrderDetail();
                        orderdetail.ProductID = productId;
                        orderdetail.StoreId = model.StoreId;
                        this.MapOrderDetail(odm, orderdetail);

                        //Orderdetail Promotion Mapping
                        //Đối với đơn hàng từ POS thì phải finish mới có promotion (đơn hàng khác chưa biết..)
                        if (model.DeliveryStatus == (int)DeliveryStatus.New
                                && status == (int)OrderStatusEnum.Finish)
                        {
                            foreach (var odpm in odm.OrderDetailPromotioMappingMs.ToList())
                            {
                                var mapping = new OrderDetailPromotionMapping();
                                mapping.PromotionId = (new PromotionApi())
                                            .GetByPromoCode(odpm.PromotionCode).PromotionID;
                                mapping.PromotionDetailId = (new PromotionDetailApi())
                                            .GetDetailByPromotionDetailCode(odpm.PromotionDetailCode).PromotionDetailID;
                                this.MapOrderdetailPromotionMapping(odpm, mapping);

                                orderdetail.OrderDetailPromotionMappings.Add(mapping);
                            }
                        }

                        order.OrderDetails.Add(orderdetail);
                    }

                    //Order Promotion Mapping
                    //Đối với đơn hàng từ POS thì phải finish mới có promotion (đơn hàng khác chưa biết..)
                    if (model.DeliveryStatus == (int)DeliveryStatus.New
                            && status == (int)OrderStatusEnum.Finish)
                    {
                        foreach (var opm in model.OrderPromotioMappingMs.ToList())
                        {
                            var mapping = new OrderPromotionMapping();
                            mapping.PromotionId = (new PromotionApi())
                                        .GetByPromoCode(opm.PromotionCode).PromotionID;
                            mapping.PromotionDetailId = (new PromotionDetailApi())
                                        .GetDetailByPromotionDetailCode(opm.PromotionDetailCode).PromotionDetailID;
                            this.MapOrderPromotionMapping(opm, mapping);

                            order.OrderPromotionMappings.Add(mapping);
                        }
                    }

                    //Finished || Processing Order -> Save Payment
                    if (status == (int)OrderStatusEnum.Finish
                        || status == (int)OrderStatusEnum.Processing)
                    {
                        var checkStatusMomo = false;
                        //Payment
                        foreach (var p in model.PaymentMs.ToList())
                        {
                            var payment = new Payment();
                            this.MapPayment(p, payment);
                            if (payment.Type == (int)PaymentTypeEnum.MoMo)
                            {
                                checkStatusMomo = true;
                            }
                            order.Payments.Add(payment);
                        }


                        if (checkStatusMomo == true && (int)OrderStatusEnum.Finish == status)
                        {
                            db = GetDatabase();
                            var redisKey = order.InvoiceID;
                            var orderNotify = new OrderNotifyMessage();
                            #region Mapping
                            orderNotify.OrderCode = order.InvoiceID;
                            orderNotify.CheckInDate = order.CheckInDate.Value.ToString("-yyMMdd-HHmm");
                            orderNotify.TotalAmount = (int)order.FinalAmount;
                            orderNotify.OrderDetail = order.OrderDetails.Select(q => new OrderDetailNotify
                            {
                                productCode = productApi.BaseService.Get(q.ProductID).Code,
                                productName = productApi.BaseService.Get(q.ProductID).ProductName,
                                productCategory = productApi.BaseService.Get(q.ProductID).ProductCategory.CateName,
                                amount = (int)q.FinalAmount,
                                description = "",
                            }).ToList();

                            orderNotify.AccountInfo = new CustomerNotify
                            {
                                name = "",
                                address = "",
                                email = "",
                                mobile = ""
                            };
                            #endregion
                            var redisValue = JsonConvert.SerializeObject(orderNotify);
                            db.HashSet("NotifyMomo", redisKey, redisValue);
                        }

                        if (status == (int)OrderStatusEnum.Finish)
                        {
                            //Fix Order sent Don't Have Payment when finish 
                            if (model.FinalAmount > 0)
                            {
                                if (model.FinalAmount != (model.TotalAmount - model.Discount - model.DiscountOrderDetail))
                                {
                                    return Json(new
                                    {
                                        OrderStatus = model.OrderStatus,
                                        InvoiceId = model.OrderCode,
                                        DeliveryStatus = model.DeliveryStatus,
                                        CheckInPerson = model.CheckInPerson,
                                        //Something wrong, return old status
                                    });
                                }
                                //TODO: Fix bug Miss Payment
                                if (model.PaymentMs.Count() == 0)
                                {
                                    var payment = new Payment();
                                    payment.Amount = model.FinalAmount;
                                    payment.Status = (int)OrderPaymentStatusEnum.Finish;
                                    payment.Type = model.Att1 != null ? (int)PaymentTypeEnum.MemberPayment : (int)PaymentTypeEnum.Cash;
                                    payment.PayTime = model.CheckInDate;
                                    payment.FCAmount = 0;
                                    order.Payments.Add(payment);
                                }
                            }
                        }

                    }
                    else
                    {
                        //Không lưu payment
                    }

                    //Save Order
                    var created = orderApi.CreateOrder(order);

                    if (created)
                    {
                        //Update tmpId
                        this.UpdateOrderDetailId(order);
                        orderApi.EditOrder(order);
                        //Success
                    }
                    else
                    {
                        result.OrderStatus = model.OrderStatus;
                        //Fail - Return old status
                    }
                }
                //Nếu đơn hàng đã có trên server sẽ update
                else if (checkRent != null)
                {
                    //Đơn hàng mới từ POS (delivery status = 0)
                    if (model.DeliveryStatus == (int)DeliveryStatus.New)
                    {
                        //Không được edit đơn hàng finish trên server !!!
                        if (checkRent.OrderStatus == (int)OrderStatusEnum.Finish)
                        {
                            result.OrderStatus = model.OrderStatus;
                            //Return old status
                        }
                        //Chỉ edit khi đơn hàng chưa finish trên server 
                        else if (checkRent.OrderStatus != (int)OrderStatusEnum.Finish)
                        {
                            var modifiedPayment = false;
                            var modifiedOrderDetail = false;

                            #region delete
                            //Chưa từng có orderdetail hoặc có sự thay đổi orderdetail
                            if (!checkRent.OrderDetails.Any()
                                || checkRent.LastModifiedOrderDetail == null
                                || (checkRent.LastModifiedOrderDetail != null &&
                                    checkRent.LastModifiedOrderDetail != model.LastModifiedOrderDetail))
                            {
                                //Delete all orderdetail
                                var lastOdList = checkRent.OrderDetails.ToList();
                                foreach (var od in lastOdList)
                                {
                                    checkRent.OrderDetails.Remove(od);
                                    var orderDetailCurrent = orderdetailApi.BaseService.Get(od.OrderDetailID);
                                    orderdetailApi.BaseService.Delete(orderDetailCurrent);
                                }
                                modifiedOrderDetail = true;
                            }
                            //Đơn hàng đang xử lý hoặc đã thành công mới cập nhật payment
                            if (status == (int)OrderStatusEnum.Finish
                               || status == (int)OrderStatusEnum.Processing)
                            {
                                //Chưa từng có payment hoặc có sự thay đổi payment
                                if (!checkRent.Payments.Any()
                                    || checkRent.Payments.Sum(p => p.Amount) == 0
                                    || checkRent.LastModifiedPayment == null
                                    || (checkRent.LastModifiedPayment != null &&
                                        checkRent.LastModifiedPayment != model.LastModifiedPayment))
                                {
                                    //Delete all payment
                                    var lastPList = checkRent.Payments.ToList();
                                    foreach (var p in lastPList)
                                    {
                                        checkRent.Payments.Remove(p);
                                        var currentpayment = paymentApi.BaseService.Get(p.PaymentID);
                                        paymentApi.BaseService.Delete(currentpayment);
                                    }
                                    modifiedPayment = true;
                                }
                            }
                            #endregion

                            //redload order db nếu có bất kì sự thay đổi
                            if (modifiedPayment || modifiedOrderDetail)
                            {
                                checkRent = null;
                                checkRent = orderApi.GetOrderByInvoiceId(model.OrderCode);
                            }

                            #region update
                            if (modifiedOrderDetail)
                            {
                                //Vì có sự thay đổi orderdetail -> order có thay đổi -> update order
                                //Order
                                this.MapOrder(model, checkRent);

                                //Orderdetail
                                foreach (var odm in model.OrderDetailMs.ToList())
                                {
                                    var productId = productApi.GetProductByCode(odm.ProductCode).ProductID;
                                    var orderdetail = new OrderDetail();
                                    orderdetail.ProductID = productId;
                                    orderdetail.StoreId = model.StoreId;
                                    this.MapOrderDetail(odm, orderdetail);

                                    //Orderdetail Promotion Mapping
                                    //Đối với đơn hàng từ POS thì phải finish mới có promotion (đơn hàng khác chưa biết..)
                                    if (status == (int)OrderStatusEnum.Finish)
                                    {
                                        foreach (var odpm in odm.OrderDetailPromotioMappingMs.ToList())
                                        {
                                            var mapping = new OrderDetailPromotionMapping();
                                            mapping.PromotionId = (new PromotionApi())
                                                        .GetByPromoCode(odpm.PromotionCode).PromotionID;
                                            mapping.PromotionDetailId = (new PromotionDetailApi())
                                                        .GetDetailByPromotionDetailCode(odpm.PromotionDetailCode).PromotionDetailID;
                                            this.MapOrderdetailPromotionMapping(odpm, mapping);

                                            orderdetail.OrderDetailPromotionMappings.Add(mapping);
                                        }
                                    }

                                    checkRent.OrderDetails.Add(orderdetail);
                                }

                                //Order Promotion Mapping
                                //Đối với đơn hàng từ POS thì phải finish mới có promotion (đơn hàng khác chưa biết..)
                                if (status == (int)OrderStatusEnum.Finish)
                                {
                                    foreach (var opm in model.OrderPromotioMappingMs.ToList())
                                    {
                                        var mapping = new OrderPromotionMapping();
                                        mapping.PromotionId = (new PromotionApi())
                                                    .GetByPromoCode(opm.PromotionCode).PromotionID;
                                        mapping.PromotionDetailId = (new PromotionDetailApi())
                                                    .GetDetailByPromotionDetailCode(opm.PromotionDetailCode).PromotionDetailID;
                                        this.MapOrderPromotionMapping(opm, mapping);

                                        checkRent.OrderPromotionMappings.Add(mapping);
                                    }
                                }
                            }

                            if (modifiedPayment)
                            {
                                //Payment
                                foreach (var p in model.PaymentMs.ToList())
                                {
                                    var payment = new Payment();
                                    this.MapPayment(p, payment);

                                    checkRent.Payments.Add(payment);
                                }
                            }

                            //Save
                            checkRent.OrderStatus = status;
                            checkRent.DeliveryStatus = model.DeliveryStatus;
                            checkRent.CheckInPerson = model.CheckInPerson;
                            orderApi.EditOrder(checkRent);

                            if (modifiedOrderDetail)
                            {
                                //Update tmpId
                                this.UpdateOrderDetailId(checkRent);
                                orderApi.EditOrder(checkRent);
                            }
                            //Success
                            #endregion
                        }
                    }
                    //Đơn hàng delivery
                    else if (model.DeliveryStatus != (int)DeliveryStatus.New)
                    {
                        //Không có sự thay đổi ở orderdetail
                        //Chỉ cập nhật lại payment nếu finish || processing
                        if (status == (int)OrderStatusEnum.Finish
                           || status == (int)OrderStatusEnum.Processing)
                        {
                            var modifiedPayment = false;

                            //delete
                            var pList = checkRent.Payments.ToList();
                            foreach (var p in pList)
                            {
                                modifiedPayment = true;
                                checkRent.Payments.Remove(p);
                                var paymentcurrent = paymentApi.BaseService.Get(p.PaymentID);
                                paymentApi.BaseService.Delete(paymentcurrent);
                            }

                            //redload order db nếu có bất kì sự thay đổi
                            if (modifiedPayment)
                            {
                                checkRent = null;
                                checkRent = orderApi.GetOrderByInvoiceId(model.OrderCode);
                            }

                            var checkStatusMomo = false;
                           
                            foreach (var p in model.PaymentMs.ToList())
                            {
                                var payment = new Payment();
                                this.MapPayment(p, payment);
                                if (payment.Type == (int)PaymentTypeEnum.MoMo)
                                {
                                    checkStatusMomo = true;
                                }
                                checkRent.Payments.Add(payment);
                            }
                            if (checkStatusMomo && status == (int)OrderStatusEnum.Finish)
                            {
                                db = GetDatabase();
                                var redisKey = checkRent.InvoiceID;
                                var orderNotify = new OrderNotifyMessage();
                                #region Mapping
                                orderNotify.OrderCode = checkRent.InvoiceID;
                                orderNotify.CheckInDate = checkRent.CheckInDate.Value.ToString("-yyMMdd-HHmm");
                                orderNotify.TotalAmount = (int)checkRent.FinalAmount;
                                orderNotify.OrderDetail = checkRent.OrderDetails.Select(q => new OrderDetailNotify
                                {
                                    productCode = q.Product.Code,
                                    productName = q.Product.ProductName,
                                    productCategory = q.Product.ProductCategory.CateName,
                                    amount = (int)q.FinalAmount,
                                    description = "",
                                }).ToList();

                                orderNotify.AccountInfo = new CustomerNotify
                                {
                                    name = "",
                                    address = "",
                                    email = "",
                                    mobile = ""
                                };
                                #endregion
                                var redisValue = JsonConvert.SerializeObject(orderNotify);
                                db.HashSet("NotifyMomo", redisKey, redisValue);
                                ////update
                                if (status == (int)OrderStatusEnum.Finish)
                                {
                                    //Fix Order sent Don't Have Payment when finish 
                                    if (model.FinalAmount > 0)
                                    {
                                        if (model.FinalAmount != (model.TotalAmount - model.Discount - model.DiscountOrderDetail))
                                        {
                                            return Json(new
                                            {
                                                OrderStatus = model.OrderStatus,
                                                InvoiceId = model.OrderCode,
                                                DeliveryStatus = model.DeliveryStatus,
                                                CheckInPerson = model.CheckInPerson,
                                                //Something wrong, return old status
                                            });
                                        }
                                        //TODO: Fix bug Miss Payment
                                        if (model.PaymentMs.Count() == 0)
                                        {
                                            var payment = new Payment();
                                            payment.Amount = model.FinalAmount;
                                            payment.Status = (int)OrderPaymentStatusEnum.Finish;
                                            payment.Type = model.Att1 != null ? (int)PaymentTypeEnum.MemberPayment : (int)PaymentTypeEnum.Cash;
                                            payment.PayTime = model.CheckInDate;
                                            payment.FCAmount = 0;
                                            checkRent.Payments.Add(payment);
                                        }
                                    }
                                }

                            }

                            checkRent.OrderStatus = status;
                            checkRent.DeliveryStatus = model.DeliveryStatus;
                            checkRent.CheckInPerson = model.CheckInPerson;
                            orderApi.EditOrder(checkRent);
                            //Success
                        }
                        // TH Thay thay đổi trạng thái hủy đơn hàng
                        else if (status == (int)OrderStatusEnum.PreCancel)
                        {
                            checkRent.OrderStatus = status;
                            checkRent.DeliveryStatus = model.DeliveryStatus;
                            checkRent.CheckInPerson = model.CheckInPerson;
                            orderApi.EditOrder(checkRent);
                        }
                        else
                        //Không có gì cập nhật
                        {
                            result.OrderStatus = model.OrderStatus;
                            //Return old status
                        }
                    }
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    OrderStatus = model.OrderStatus,
                    InvoiceId = model.OrderCode,
                    DeliveryStatus = model.DeliveryStatus,
                    CheckInPerson = model.CheckInPerson,
                    //Something wrong, return old status
                });
            }
        }


        // Nếu có update model, update mapping API model trong đây!!!
        #region Mapper & Updater
        /// <summary>
        /// Check lại parentId của extra orderdetail,
        /// Check lại tmpId của mapping,
        /// Check lại orderId của mapping,
        /// Check lại orderdetailId của mapping,
        /// </summary>
        private void UpdateOrderDetailId(HmsService.Models.Entities.Order order)
        {
            foreach (var orderDetail in order.OrderDetails)
            {
                //Lưu parentId cho orderdetail extra
                if (orderDetail.ParentId != null && orderDetail.ParentId > -1)
                {
                    var parentOrderDetail = order.OrderDetails.FirstOrDefault(od => od.TmpDetailId == orderDetail.ParentId);
                    if (parentOrderDetail != null)
                    {
                        orderDetail.ParentId = parentOrderDetail.OrderDetailID;
                    }
                }
                //Lưu lại orderPromotionMappingId cho orderdetail
                if (orderDetail.OrderPromotionMappingId != null)
                {
                    var mapping = order.OrderPromotionMappings.FirstOrDefault(m => m.TmpMappingId == orderDetail.OrderPromotionMappingId);
                    if (mapping != null)
                    {
                        orderDetail.OrderPromotionMappingId = mapping.Id;
                    }
                    else
                    {
                        orderDetail.OrderPromotionMappingId = null;
                    }
                }
                //Lưu lại orderdetailPromotionMappingId cho orderdetail
                if (orderDetail.OrderDetailPromotionMappingId != null)
                {
                    OrderDetailPromotionMapping mapping = null;
                    foreach (var od in order.OrderDetails)
                    {
                        mapping = od.OrderDetailPromotionMappings.FirstOrDefault(m => m.TmpMappingId == orderDetail.OrderDetailPromotionMappingId);
                        if (mapping != null) break;
                    }
                    if (mapping != null)
                    {
                        orderDetail.OrderDetailPromotionMappingId = mapping.Id;
                    }
                    else
                    {
                        orderDetail.OrderDetailPromotionMappingId = null;
                    }
                }
            }

            //Lưu lại tmpMappingId cho order
            foreach (var mapping in order.OrderPromotionMappings)
            {
                mapping.TmpMappingId = mapping.Id;
            }
            //Lưu lại tmpMappingId cho orderdetail
            foreach (var orderDetail in order.OrderDetails)
            {
                foreach (var mapping in orderDetail.OrderDetailPromotionMappings)
                {
                    mapping.TmpMappingId = mapping.Id;
                }
                orderDetail.TmpDetailId = orderDetail.OrderDetailID;
            }
        }

        /// <summary>
        /// Map order data form source(model) to destination
        /// </summary>
        private void MapOrder(OrderApiViewModel source, HmsService.Models.Entities.Order destination)
        {
            //destination.OrderStatus = source.OrderStatus;
            //destination.CheckInPerson = source.CheckInPerson;
            //destination.DeliveryStatus = source.DeliveryStatus;
            destination.InvoiceID = source.OrderCode;
            destination.CheckInDate = source.CheckInDate;
            destination.CheckOutDate = source.CheckInDate;      //TODO: change
            destination.ApproveDate = source.CheckInDate;       //TODO: change
            destination.TotalAmount = source.TotalAmount;
            destination.Discount = source.Discount;
            destination.DiscountOrderDetail = source.DiscountOrderDetail;
            destination.FinalAmount = source.FinalAmount;
            destination.OrderType = source.OrderType;
            destination.Notes = source.Notes;
            destination.FeeDescription = source.FeeDescription;
            destination.CheckOutPerson = source.CheckOutPerson;
            destination.ApprovePerson = source.ApprovePerson;
            destination.CustomerID = source.CustomerID;
            destination.IsFixedPrice = source.IsFixedPrice;
            destination.ServedPerson = source.ServedPerson;
            destination.StoreID = source.StoreId;
            destination.SourceID = source.SourceID;
            destination.SourceType = 0;
            destination.DeliveryAddress = source.DeliveryAddress;
            destination.TotalInvoicePrint = source.TotalInvoicePrint;
            destination.VAT = source.VAT;
            destination.VATAmount = source.VATAmount;
            destination.NumberOfGuest = source.NumberOfGuest;
            destination.Att1 = source.Att1;
            destination.Att2 = source.Att2;
            destination.Att3 = source.Att3;
            destination.Att4 = source.Att4;
            destination.Att5 = source.Att5;
            destination.GroupPaymentStatus = source.GroupPaymentStatus;
            destination.DeliveryReceiver = source.DeliveryCustomer;
            destination.DeliveryPhone = source.DeliveryPhone;
            destination.LastModifiedPayment = source.LastModifiedPayment;
            destination.LastModifiedOrderDetail = source.LastModifiedOrderDetail;
        }

        /// <summary>
        /// Map orderdetail data form source(model) to destination
        /// </summary>
        private void MapOrderDetail(OrderDetailApiViewModel source, OrderDetail destination)
        {
            //destination.ProductID = source.ProductId,
            //destination.StoreId = source.StoreId;
            destination.TotalAmount = source.TotalAmount;
            destination.Quantity = source.Quantity;
            destination.OrderDate = source.OrderDate;
            destination.Status = source.Status;
            destination.FinalAmount = source.FinalAmount;
            destination.Discount = source.Discount;
            destination.TaxValue = source.TaxValue;
            destination.UnitPrice = source.UnitPrice;
            destination.ProductType = source.ProductType;
            destination.ParentId = source.ParentId;
            destination.ProductOrderType = source.ProductOrderType;
            destination.ItemQuantity = source.ItemQuantity;
            destination.TmpDetailId = source.TmpDetailId;
            destination.OrderPromotionMappingId = source.OrderPromotionMappingId;
            destination.OrderDetailPromotionMappingId = source.OrderDetailPromotionMappingId;
        }

        /// <summary>
        /// Map payment data form source(model) to destination
        /// </summary>
        private void MapPayment(PaymentApiViewModel source, Payment destination)
        {
            destination.Amount = source.Amount;
            destination.CurrencyCode = source.CurrencyCode;
            destination.FCAmount = source.FCAmount;
            destination.Notes = source.Notes;
            destination.PayTime = source.PayTime;
            destination.Status = source.Status;
            destination.Type = source.Type;
            destination.CardCode = source.CardCode;
        }

        /// <summary>
        /// Map order promotion mapping data form source(model) to destination
        /// </summary>
        private void MapOrderPromotionMapping(OrderPromotionMappingModel source, OrderPromotionMapping destination)
        {
            //destination.PromotionId = source.PromotionCode;
            //destination.PromotionDetailId = source.PromotionDetailCode;
            //destination.OrderId = source.OrderId;
            destination.DiscountAmount = (int)source.DiscountAmount;
            destination.DiscountRate = source.DiscountRate;
            destination.MappingIndex = source.MappingIndex;
            destination.TmpMappingId = source.TmpMappingId;
            destination.Active = true;
        }

        /// <summary>
        /// Map orderdetail promotion mapping data form source(model) to destination
        /// </summary>
        private void MapOrderdetailPromotionMapping(OrderDetailPromotionMappingModel source, OrderDetailPromotionMapping destination)
        {
            //destination.PromotionId = source.PromotionCode;
            //destination.PromotionDetailId = source.PromotionDetailCode;
            //destination.OrderDetailId = source.OrderDetailId;
            destination.DiscountAmount = (int)source.DiscountAmount;
            destination.DiscountRate = source.DiscountRate;
            destination.MappingIndex = source.MappingIndex;
            destination.TmpMappingId = source.TmpMappingId;
            destination.Active = true;
        }
        #endregion

        [HttpPost]
        public JsonResult NotifyOrderToPosJson(MessageCOM.NotifyOrder msg)
        {
            string code = "";
            code = _storeCode + "-" + msg.StoreId.ToString();
            db = GetDatabase();

            try
            {

                db.ListLeftPush(code, msg.ToJson());

                return Json(new
                {
                    msg = "OK"
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    msg = "FAIL"
                });
            }

        }

        [HttpPost]
        public JsonResult SendOrderWithVoucherToServer(List<OrderDetailViewModel> orderDetails, string voucherCode, int storeId, string note)
        {
            try
            {
                //Api to handle vouchers
                var promotionApi = new PromotionApi();
                var promotionDetailApi = new PromotionDetailApi();
                var voucherApi = new VoucherApi();
                var orderDetailPromotionMappingApi = new OrderDetailPromotionMappingApi();
                var orderPromotionMappingApi = new OrderPromotionMappingApi();
                var orderApi = new OrderApi();
                var orderDetailApi = new OrderDetailApi();
                var productApi = new ProductApi();
                var paymentApi = new PaymentApi();

                VoucherViewModel voucher = null;
                PromotionViewModel promotion = null;
                PromotionDetailViewModel promotionDetail = null;

                if (voucherCode != null)
                {
                    voucher = voucherApi.GetVoucherByCode(voucherCode);
                    promotion = promotionApi.GetPromotionById(voucher.PromotionID);
                    if (voucher.PromotionDetailID != null)
                    {
                        promotionDetail = promotionDetailApi.GetDetailViewModelById(voucher.PromotionDetailID.Value);
                    }

                }

                DateTime now = DateTime.Now;
                bool isValidVoucher = voucher != null && promotionDetail != null && promotion != null
                    && voucher.Quantity > voucher.UsedQuantity
                    && (!promotion.IsVoucher.HasValue || promotion.IsVoucher.Value)
                    && promotion.Active
                    && now >= promotion.FromDate.GetStartOfDate() && now <= promotion.ToDate.GetEndOfDate()
                    && now.Hour >= promotion.ApplyFromTime && now.Hour <= promotion.ApplyToTime
                    && promotion.VoucherQuantity > promotion.VoucherUsedQuantity;
                bool isUsed = false;
                double discountOrder = 0, discountOrderDetail = 0;
                List<OrderDetailViewModel> listGiftOrderDetail = new List<OrderDetailViewModel>();

                var promotionGiftType = (PromotionGiftTypeEnum)promotion.GiftType;
                Product giftProduct = null;
                double discount = 0;

                if (promotionGiftType == PromotionGiftTypeEnum.DiscountAmount || promotionGiftType == PromotionGiftTypeEnum.DiscountRate)
                {
                    if (promotion.ApplyLevel == (int)PromotionApplyLevelEnum.OrderDetail)
                    {
                        var discountProductCode = promotionDetail.GiftProductCode != null ? promotionDetail.GiftProductCode : promotionDetail.BuyProductCode;
                        giftProduct = productApi.GetProductByCode(discountProductCode);
                        discount = promotionDetail.DiscountAmount == null ? (promotionDetail.DiscountRate.Value / 100) : (double)promotionDetail.DiscountAmount.Value;
                    }
                    else
                    {
                        giftProduct = null;
                        discount = promotionDetail.DiscountAmount == null ? (promotionDetail.DiscountRate.Value / 100) : (double)promotionDetail.DiscountAmount.Value;
                    }
                }
                else if (promotionGiftType == PromotionGiftTypeEnum.Gift)
                {
                    var discountProductCode = promotionDetail.GiftProductCode != null ? promotionDetail.GiftProductCode : promotionDetail.BuyProductCode;
                    giftProduct = productApi.GetProductByCode(discountProductCode);
                    discount = 0;
                }

                ICollection<OrderDetail> orderDetailList = new List<OrderDetail>();

                //Lưu thông tin order
                foreach (var itemDetail in orderDetails)
                {
                    //Lấy product thuộc order detail hiện tại
                    var product = productApi.GetProductByCode(itemDetail.ProductCode);

                    //Promotion mức sản phẩm
                    if (promotion.ApplyLevel == (int)PromotionApplyLevelEnum.OrderDetail && itemDetail.ProductCode == promotionDetail.BuyProductCode && isValidVoucher)
                    {
                        //Kiểm tra số lượng sản phẩm cần mua để áp dụng promotion
                        bool isInRange = true;

                        //So sánh nếu có max quantity, nếu ko có max quantity thì cho qua, tiếp tục áp dụng promotion
                        isInRange = (!promotionDetail.MaxBuyQuantity.HasValue || promotionDetail.MaxBuyQuantity.Value >= itemDetail.Quantity) && isInRange;

                        //So sánh nếu có min quantity, nếu ko có max quantity thì cho qua, tiếp tục áp dụng promotion
                        isInRange = (!promotionDetail.MinBuyQuantity.HasValue || promotionDetail.MinBuyQuantity.Value <= itemDetail.Quantity) && isInRange;

                        //Promotion giảm giá (chỉ sản phẩm)
                        if (promotionGiftType == PromotionGiftTypeEnum.DiscountAmount || promotionGiftType == PromotionGiftTypeEnum.DiscountRate)
                        {
                            var giftProductCode = promotionDetail.GiftProductCode != null ? promotionDetail.GiftProductCode : promotionDetail.BuyProductCode;
                            var discountDetail = orderDetails.Where(q => q.ProductCode == giftProductCode).FirstOrDefault();

                            //Chỉ áp dụng giảm giá nếu có mua sản phẩm và số lượng mua đủ
                            if (discountDetail != null && isInRange)
                            {
                                //Chỉ giảm giá đủ số lượng tối đa của promotion nếu mua vượt quá số lượng đó
                                if (promotionDetail.GiftQuantity.HasValue && discountDetail.Quantity > promotionDetail.GiftQuantity.Value)
                                {
                                    discountDetail.Quantity = discountDetail.Quantity - promotionDetail.GiftQuantity.Value;

                                    orderDetailList.Add(new OrderDetail()
                                    {
                                        TotalAmount = discountDetail.UnitPrice * promotionDetail.GiftQuantity.Value,
                                        FinalAmount = promotionDetail.DiscountAmount == null ? (discountDetail.UnitPrice * (1 - discount) * promotionDetail.GiftQuantity.Value) : ((discountDetail.UnitPrice - discount) * promotionDetail.GiftQuantity.Value),
                                        Discount = 0,
                                        ProductID = giftProduct.ProductID,
                                        Quantity = promotionDetail.GiftQuantity.Value,
                                        ItemQuantity = 0,
                                        Status = (int)OrderDetailStatusEnum.New, //TODO:
                                        UnitPrice = discountDetail.UnitPrice,
                                        OrderDate = DateTime.Now,
                                        StoreId = storeId,
                                        OrderDetailPromotionMappingId = null,
                                        OrderPromotionMappingId = null
                                    });

                                    isUsed = true;

                                    var curDetail = orderDetailList.Last();
                                    curDetail.Discount = curDetail.TotalAmount - curDetail.FinalAmount;
                                    discountOrderDetail += curDetail.Discount;
                                }
                                //Giảm giá toàn bộ khi không có quy định số lượng hoặc có nhưng trong giới hạn
                                else
                                {
                                    //Giảm giá %
                                    if (promotionDetail.DiscountAmount == null)
                                    {
                                        discountDetail.FinalAmount = (discountDetail.UnitPrice * (1 - discount) * discountDetail.Quantity);
                                    }
                                    //Giảm giá trực tiếp
                                    else
                                    {
                                        discountDetail.FinalAmount = ((discountDetail.UnitPrice - discount) * discountDetail.Quantity);
                                    }
                                }

                                isUsed = true;
                            }
                        }
                        //Promotion tặng quà
                        else if (promotionGiftType == PromotionGiftTypeEnum.Gift)
                        {
                            //2 trường hợp so sánh nếu fail bất kỳ thì không áp dụng promotion
                            if (isInRange)
                            {
                                orderDetailList.Add(new OrderDetail()
                                {
                                    Discount = 0,
                                    TotalAmount = giftProduct.Price * promotionDetail.GiftQuantity.Value,
                                    FinalAmount = 0, //Free gift
                                    ProductID = giftProduct.ProductID,
                                    Quantity = promotionDetail.GiftQuantity.Value,
                                    ItemQuantity = 0,
                                    Status = (int)OrderDetailStatusEnum.New, //TODO:
                                    UnitPrice = giftProduct.Price,
                                    OrderDate = DateTime.Now,
                                    StoreId = storeId,
                                    OrderDetailPromotionMappingId = null,
                                    OrderPromotionMappingId = null
                                });

                                var curDetail = orderDetailList.Last();
                                curDetail.Discount = curDetail.TotalAmount - curDetail.FinalAmount;
                                discountOrderDetail += curDetail.Discount;

                                isUsed = true;
                            }
                        }
                    }

                    var orderDetail = new OrderDetail()
                    {
                        Discount = 0,
                        TotalAmount = itemDetail.UnitPrice * itemDetail.Quantity,
                        FinalAmount = itemDetail.FinalAmount != 0 ? itemDetail.FinalAmount : itemDetail.UnitPrice * itemDetail.Quantity,
                        ProductID = product.ProductID,
                        Quantity = itemDetail.Quantity,
                        ItemQuantity = 0,
                        Status = (int)OrderDetailStatusEnum.New,
                        UnitPrice = itemDetail.UnitPrice,
                        OrderDate = DateTime.Now,
                        StoreId = storeId,
                        OrderDetailPromotionMappingId = itemDetail.OrderDetailPromotionMappingId,
                        OrderPromotionMappingId = itemDetail.OrderPromotionMappingId
                    };

                    orderDetailList.Add(orderDetail);

                    var currentDetail = orderDetailList.Last();
                    currentDetail.Discount = currentDetail.TotalAmount - currentDetail.FinalAmount;
                    discountOrderDetail += currentDetail.Discount;
                }

                var amount = orderDetailList.Sum(q => q.FinalAmount);

                //Promotion mức order
                if (promotion.ApplyLevel == (int)PromotionApplyLevelEnum.Order && isValidVoucher)
                {
                    //Kiểm tra số tiền hóa đơn cần đạt để áp dụng promotion
                    bool isInRange = true;

                    //So sánh nếu có max quantity, nếu ko có max quantity thì cho qua, tiếp tục áp dụng promotion
                    isInRange = (!promotionDetail.MaxOrderAmount.HasValue || promotionDetail.MaxOrderAmount.Value >= amount) && isInRange;

                    //Promotion giảm giá
                    if ((promotionGiftType == PromotionGiftTypeEnum.DiscountAmount || promotionGiftType == PromotionGiftTypeEnum.DiscountRate) && isInRange)
                    {
                        amount = promotionDetail.DiscountAmount == null ? amount * (1 - discount) : amount - discount;
                    }
                    //Promotion tặng quà
                    else if (promotionGiftType == PromotionGiftTypeEnum.Gift)
                    {
                        var boughtOrderDetail = promotionDetail.BuyProductCode == null ? null : orderDetails.Where(q => q.ProductCode == promotionDetail.BuyProductCode).FirstOrDefault();
                        if (promotionDetail.BuyProductCode == null || boughtOrderDetail != null) { }
                        {
                            //So sánh nếu có min quantity, nếu ko có max quantity thì cho qua, tiếp tục áp dụng promotion
                            isInRange = (!promotionDetail.MinBuyQuantity.HasValue || promotionDetail.MinBuyQuantity.Value <= boughtOrderDetail.Quantity) && isInRange;
                        }

                        //2 trường hợp so sánh nếu fail bất kỳ thì không áp dụng promotion
                        if (isInRange)
                        {
                            orderDetailList.Add(new OrderDetail()
                            {
                                Discount = 0,
                                TotalAmount = giftProduct.Price * promotionDetail.GiftQuantity.Value,
                                FinalAmount = 0, //Free gift
                                ProductID = giftProduct.ProductID,
                                Quantity = promotionDetail.GiftQuantity.Value,
                                ItemQuantity = 0,
                                Status = (int)OrderDetailStatusEnum.New, //TODO:
                                UnitPrice = giftProduct.Price,
                                OrderDate = DateTime.Now,
                                StoreId = storeId,
                                OrderDetailPromotionMappingId = null,
                                OrderPromotionMappingId = null
                            });

                            isUsed = true;
                        }
                    }
                }

                //Thông tin order
                var order = new HmsService.Models.Entities.Order()
                {
                    CheckInDate = DateTime.Now,
                    TotalAmount = orderDetailList.Sum(q => q.FinalAmount),
                    FinalAmount = amount, //đã kiểm tra và áp dung promotion
                    Discount = 0, //TODO
                    DiscountOrderDetail = 0, //TODO
                    OrderStatus = (int)OrderStatusEnum.New,
                    OrderType = (int)OrderTypeEnum.OnlineProduct,
                    StoreID = storeId,
                    Notes = note, //TODO
                    IsFixedPrice = true, //TODO
                    InvoiceID = HmsService.Models.Utils.GetCurrentDateTime().Ticks.ToString() + "-" + storeId, //TODO
                    DeliveryStatus = (int)DeliveryStatus.New,
                    CheckInPerson = null,
                    SourceType = 0,
                    VAT = 0,
                    VATAmount = 0,
                    Att1 = null,
                };

                discountOrder = (orderDetailList.Sum(q => q.TotalAmount) - order.FinalAmount) - discountOrderDetail;
                order.Discount = discountOrder;
                order.DiscountOrderDetail = discountOrderDetail;
                order.OrderDetails = orderDetailList;

                var check = orderApi.CreateOrder(order);
                if (check)
                {
                    //TODO: create mapping, update voucher and promotion detail, check voucher before use
                    if (isUsed)
                    {
                        if (promotion.ApplyLevel == (int)PromotionApplyLevelEnum.OrderDetail)
                        {
                            var promoOrderDetail = order.OrderDetails.Where(q => q.Product.Code == promotionDetail.BuyProductCode).FirstOrDefault();
                            if (promotionDetail.DiscountAmount != null || promotionDetail.DiscountRate != null)
                            {
                                var orderDetailMappingId = orderDetailPromotionMappingApi.CreateAndReturnId(new OrderDetailPromotionMappingViewModel
                                {
                                    PromotionDetailId = promotionDetail.PromotionDetailID,
                                    PromotionId = promotion.PromotionID,
                                    DiscountAmount = promotionDetail.DiscountAmount.HasValue ? promotionDetail.DiscountAmount.Value : (decimal)(promoOrderDetail.TotalAmount - promoOrderDetail.FinalAmount),
                                    DiscountRate = promotionDetail.DiscountRate,
                                    Active = true,
                                    OrderDetailId = promoOrderDetail.OrderDetailID,
                                });

                                var orderMappingId = orderPromotionMappingApi.CreateAndReturnId(new OrderPromotionMappingViewModel
                                {
                                    OrderId = order.RentID,
                                    PromotionDetailId = promotionDetail.PromotionDetailID,
                                    PromotionId = promotion.PromotionID,
                                    DiscountAmount = 0,
                                    DiscountRate = 0,
                                    Active = true
                                });
                                var giftProductCode = promotionDetail.GiftProductCode != null ? promotionDetail.GiftProductCode : promotionDetail.BuyProductCode;
                                var giftOrderDetail = order.OrderDetails.Where(q => q.Product.Code == giftProductCode).FirstOrDefault();
                                if (orderDetailMappingId != -1 && orderMappingId != -1)
                                {
                                    giftOrderDetail.OrderDetailPromotionMappingId = orderDetailMappingId;
                                    giftOrderDetail.OrderPromotionMappingId = orderMappingId;
                                    orderDetailApi.BaseService.Update(giftOrderDetail);
                                }
                                else
                                {
                                    return Json(new { success = false, message = "Tạo mapping thất bại hahahaha" });
                                }
                            }
                            else if (!String.IsNullOrEmpty(promotionDetail.GiftProductCode.Trim()))
                            {
                                var orderDetailMappingId = orderDetailPromotionMappingApi.CreateAndReturnId(new OrderDetailPromotionMappingViewModel
                                {
                                    PromotionDetailId = promotionDetail.PromotionDetailID,
                                    PromotionId = promotion.PromotionID,
                                    DiscountAmount = 0,
                                    DiscountRate = promotionDetail.DiscountRate,
                                    Active = true,
                                    OrderDetailId = promoOrderDetail.OrderDetailID,
                                });

                                var orderMappingId = orderPromotionMappingApi.CreateAndReturnId(new OrderPromotionMappingViewModel
                                {
                                    OrderId = order.RentID,
                                    PromotionDetailId = promotionDetail.PromotionDetailID,
                                    PromotionId = promotion.PromotionID,
                                    DiscountAmount = promotionDetail.DiscountAmount.HasValue ? promotionDetail.DiscountAmount.Value : 0,
                                    DiscountRate = promotionDetail.DiscountRate,
                                    Active = true
                                });
                                var giftProductCode = promotionDetail.GiftProductCode != null ? promotionDetail.GiftProductCode : promotionDetail.BuyProductCode;
                                var giftOrderDetail = order.OrderDetails.Where(q => q.Product.Code == giftProductCode && q.FinalAmount == 0).FirstOrDefault();
                                if (orderDetailMappingId != -1)
                                {
                                    giftOrderDetail.OrderDetailPromotionMappingId = orderDetailMappingId;
                                    giftOrderDetail.OrderPromotionMappingId = orderMappingId;
                                    orderDetailApi.BaseService.Update(giftOrderDetail);
                                }
                                else
                                {
                                    return Json(new { success = false, message = "Tạo mapping thất bại hahahaha" });
                                }
                            }
                        }
                        else if (promotion.ApplyLevel == (int)PromotionApplyLevelEnum.Order)
                        {
                            var promoOrderDetail = order.OrderDetails.Where(q => q.Product.Code == promotionDetail.BuyProductCode).FirstOrDefault();
                            if (promotionDetail.DiscountAmount != null || promotionDetail.DiscountRate != null)
                            {
                                var orderMappingId = orderPromotionMappingApi.CreateAndReturnId(new OrderPromotionMappingViewModel
                                {
                                    OrderId = order.RentID,
                                    PromotionDetailId = promotionDetail.PromotionDetailID,
                                    PromotionId = promotion.PromotionID,
                                    DiscountAmount = promotionDetail.DiscountAmount.HasValue ? promotionDetail.DiscountAmount.Value : 0,
                                    DiscountRate = promotionDetail.DiscountRate,
                                    Active = true
                                });
                                var giftProductCode = promotionDetail.GiftProductCode != null ? promotionDetail.GiftProductCode : promotionDetail.BuyProductCode;
                                if (orderMappingId != -1 && !string.IsNullOrEmpty(giftProductCode))
                                {
                                    var giftOrderDetail = order.OrderDetails.Where(q => q.Product.Code == giftProductCode && q.FinalAmount == 0).FirstOrDefault();
                                    giftOrderDetail.OrderPromotionMappingId = orderMappingId;
                                    orderDetailApi.BaseService.Update(giftOrderDetail);
                                }
                                else
                                {
                                    return Json(new { success = false, message = "Tạo mapping thất bại hahahaha" });
                                }
                            }
                            else if (promotion.GiftType == (int)PromotionGiftTypeEnum.Gift)
                            {
                                var orderMappingId = orderPromotionMappingApi.CreateAndReturnId(new OrderPromotionMappingViewModel
                                {
                                    OrderId = order.RentID,
                                    PromotionDetailId = promotionDetail.PromotionDetailID,
                                    PromotionId = promotion.PromotionID,
                                    DiscountAmount = promotionDetail.DiscountAmount.HasValue ? promotionDetail.DiscountAmount.Value : 0,
                                    DiscountRate = promotionDetail.DiscountRate,
                                    Active = true
                                });

                                var giftProductCode = promotionDetail.GiftProductCode != null ? promotionDetail.GiftProductCode : promotionDetail.BuyProductCode;
                                var giftOrderDetail = order.OrderDetails.Where(q => q.Product.Code == giftProductCode && q.FinalAmount == 0).FirstOrDefault();
                                if (orderMappingId != -1)
                                {
                                    giftOrderDetail.OrderPromotionMappingId = orderMappingId;
                                    orderDetailApi.BaseService.Update(giftOrderDetail);
                                }
                                else
                                {
                                    return Json(new { success = false, message = "Tạo mapping thất bại hahahaha" });
                                }
                            }
                        }

                        voucher.UsedQuantity += 1;
                        promotion.VoucherUsedQuantity = promotion.VoucherUsedQuantity == null ? null : promotion.VoucherUsedQuantity + 1;

                        voucherApi.Update(voucher);
                        promotionApi.Update(promotion);
                    }

                    return Json(new
                    {
                        success = true,
                        OrderStatus = (int)OrderStatusEnum.New,
                        DeliveryStatus = (int)DeliveryStatus.New,
                        InvoiceId = order.InvoiceID,
                        message = isUsed ? "Đã tạo order và áp dụng voucher thành công" : "Đã tạo order và chưa áp dụng voucher"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Tạo order thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }


        /// <summary>
        /// Order to API from outside 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        [EnableCors(origins: "http://passiocoffee.com", headers: "*", methods: "*")]
        public JsonResult OrderRequest(OrderRequestViewModel model)
        {
            if (_enableToken.Equals("true"))
            {
                if (model.AccessToken != _accessToken)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Invalid Token."
                    });
                }
            }

            int storeId;
            var validateMess = ValidateOrderRequest(model, out storeId);

            if (validateMess != null)
            {
                return Json(new { Success = false, Message = validateMess });
            }

            //add customer

            var customer = new Customer()
            {
                Name = model.CustomerName,
                Phone = model.Phone,
                Type = (int)MembershipTypeEnum.Customer,
            };

            if (_customerService.CreateCustomer(customer) == -1)
            {
                return Json(new { Success = false, Message = "Lỗi không xác định." });
            }


            //get product
            double orderFee = 0;
            List<Product> products = new List<Product>();
            foreach (var item in model.ProductList)
            {
                var product = _productService.GetProducts().FirstOrDefault(p => p.Code.Equals(item.ProductCode));
                if (product == null)
                {
                    return Json(new { Success = false, Message = "Không tồn tại sản phẩm " + item.ProductCode + "." });
                }
                orderFee += product.Price;
                products.Add(product);
            }

            //add order
            var order = new HmsService.Models.Entities.Order()
            {
                CustomerID = customer.CustomerID,
                CheckInDate = DateTime.Now,
                TotalAmount = orderFee,
                FinalAmount = orderFee,
                Discount = 0,
                DiscountOrderDetail = 0,
                Notes = model.Note,
                SourceType = (int)SourceTypeEnum.WebSite,
                //AdditionFee = 0,
                IsFixedPrice = true, //tam
                OrderStatus = (int)OrderStatusEnum.New, //tam
                OrderType = (int)OrderTypeEnum.Delivery,
                DeliveryStatus = (int)DeliveryStatus.New,
                StoreID = null,
                SourceID = 43, //id tong dai ko biet la j,
                InvoiceID = DateTime.Now.Ticks.ToString() + "-" + storeId,
                DeliveryAddress = model.DeliveryAddress
            };


            //add orderDetail
            for (int i = 0; i < model.ProductList.Count; i++)
            {
                var orderDetail = new OrderDetail()
                {
                    //CustomerID = customer.CustomerID,

                    ProductID = products[i].ProductID,
                    Quantity = model.ProductList[i].Quantity,
                    TotalAmount = products[i].Price * model.ProductList[i].Quantity,
                    OrderDate = DateTime.Now,
                    Status = (int)OrderDetailStatusEnum.New,
                    FinalAmount = products[i].Price * model.ProductList[i].Quantity,
                    IsAddition = false,
                    Discount = 0,
                    ItemQuantity = model.ProductList[i].Quantity,
                    UnitPrice = products[i].Price,
                    ProductType = products[i].ColorGroup
                };
                order.OrderDetails.Add(orderDetail);
                //_orderDetailService.CreateOrderDetail(orderDetail);
            }

            if (!_orderService.CreateOrder(order))
            {
                return Json(new { Success = false, Message = "Lỗi không xác định." });
            }

            //Update 
            return Json(new { Success = true });
        }


        private string ValidateOrderRequest(OrderRequestViewModel model, out int storeId)
        {
            storeId = -1;
            //customer name
            if (string.IsNullOrEmpty(model.CustomerName))
            {
                return "Missing customer name.";
            }
            //delivery address
            if (string.IsNullOrEmpty(model.DeliveryAddress))
            {
                return "Missing delivery address.";
            }
            //customer phone
            if (string.IsNullOrEmpty(model.Phone))
            {
                return "Missing customer name.";
            }
            //product list
            if (model.ProductList == null || model.ProductList.Count == 0)
            {
                return "Empty order.";
            }
            //store
            //if (string.IsNullOrEmpty(model.StoreName))
            //{
            //    return "Missing store name.";
            //}
            try
            {
                //var store = _storeService.GetStores().FirstOrDefault(s => model.StoreName.Equals(s.Name));
                //if (store == null)
                //{
                //    return "Store does not exist.";
                //}
                //storeId = store.ID;
            }
            catch (Exception)
            {

                throw;
            }


            return null;
        }

    }
}