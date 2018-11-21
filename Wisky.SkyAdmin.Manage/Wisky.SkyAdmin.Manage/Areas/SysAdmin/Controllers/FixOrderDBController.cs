using HmsService.Sdk;
using HmsService.ViewModels;
using HmsService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.IO.IsolatedStorage;
using HmsService.Models.Entities;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    public class FixOrderDBController : Controller
    {
        // GET: SysAdmin/FixOrderDB
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FixMappingProduct()
        {
            var productApi = new ProductApi();
            var storeApi = new StoreApi();
            var listProduct = productApi.BaseService.Get(q => q.ProductType == (int)ProductTypeEnum.General).ToList();
            var stores = storeApi.BaseService.Get(q => q.isAvailable == true).ToList();
            var productMappingApi = new ProductDetailMappingApi();
            foreach (var product in listProduct)
            {
                foreach (var store in stores)
                {
                    var mapping = productMappingApi.BaseService.FirstOrDefault(q => q.StoreID == store.ID && q.ProductID == product.ProductID);
                    if (mapping == null)
                    {
                        mapping = new ProductDetailMapping();
                        mapping.ProductID = product.ProductID;
                        mapping.StoreID = store.ID;
                        mapping.Price = product.Price;
                        mapping.DiscountPrice = product.DiscountPrice;
                        mapping.DiscountPercent = product.DiscountPercent;
                        mapping.Active = true;
                        productMappingApi.BaseService.Create(mapping);
                    }
                }
            }
            return null;
        }

        public int FixOrderMissingPayment()
        {
            var orderApi = new OrderApi();
            var paymentApi = new PaymentApi();
            string date = "01/04/2018";
            var startdate = date.ToDateTime().GetStartOfDate();
            var allOrder = orderApi.BaseService.Get(q => q.CheckInDate > startdate && q.FinalAmount > 0 && q.OrderStatus == (int)OrderStatusEnum.Finish && q.Payments.Sum(k=>k.Amount) != q.FinalAmount).ToList();
            foreach (var item in allOrder)
            {
                if (item.Payments.Any(q => q.Type == 9))
                {
                    var tempSum = item.Payments.Where(q => q.Type != 9).Sum(q => q.Amount);
                    var paymentUpdate = item.Payments.FirstOrDefault(q => q.Type == 9);
                    paymentUpdate.Amount = item.FinalAmount - tempSum;
                    paymentApi.BaseService.Update(paymentUpdate);
                }
                else
                {
                    var checkDelete = false;
                    var firstPayment = item.Payments.FirstOrDefault();
                    if (item.Payments.Where(q => q.Type != 9).Count() >= 2)
                    {
                        checkDelete = true;
                    }
                    var sumAll = item.Payments.Sum(q => q.Amount);
                    var paymentInsert = new HmsService.Models.Entities.Payment();
                    paymentInsert.ToRentID = firstPayment.ToRentID;
                    paymentInsert.Amount = item.FinalAmount - sumAll;
                    paymentInsert.FCAmount = 0;
                    paymentInsert.Status = 0;
                    paymentInsert.Type = 9;
                    paymentInsert.PayTime = firstPayment.PayTime;
                    if (checkDelete)
                    {
                        var itemDelete = item.Payments.LastOrDefault(q => q.Type != 9);
                        paymentApi.BaseService.Delete(itemDelete);
                    }
                    else
                    {
                        paymentApi.BaseService.Create(paymentInsert);
                    }
                }
            }
            return allOrder.Count();
        }

        // Hàm sửa CustomerID bị null
        public async Task<JsonResult> FixBug1()
        {
            try
            {
                var orderApi = new OrderApi();
                var membershipCardApi = new MembershipCardApi();

                var orderList = orderApi.FixDB1().ToList();
                int count = 0;
                foreach (var order in orderList)
                {
                    try
                    {
                        var code = order.Att1.Substring(order.Att1.IndexOf(':') + 1);
                        var membershipCard = membershipCardApi.GetMembershipCardByCode(code);

                        if (membershipCard != null)
                        {
                            order.CustomerID = membershipCard.CustomerId;
                            await orderApi.UpdateOrderEntity(order);
                            ++count;
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }

                return Json(new { success = true, count = count, allOrder = orderList.Count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, count = 0, allOrder = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Fix Duplicate CheckFinger 
        /// </summary>
        /// <returns></returns>
        /// 
        public ActionResult FixCheckFinger()
        {
            var checkFingerApi = new CheckFingerApi();
            var dataCheck = checkFingerApi.Get().GroupBy(q => q.DateTime).ToList();
            foreach (var item in dataCheck)
            {
                if (item.Count() > 1)
                {
                    var index = 0;
                    CheckFingerViewModel master = null;
                    foreach (var tmp in item)
                    {
                        if (index > 0)
                        {
                            if (master.EmpEnrollNumber == tmp.EmpEnrollNumber)
                            {
                                checkFingerApi.Delete(tmp.Id);
                            }
                        }
                        else
                        {
                            master = tmp;
                        }
                        index++;
                    }
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        // Hàm sửa Attr1 bị null
        public async Task<JsonResult> FixBug2()
        {
            try
            {
                var orderApi = new OrderApi();
                var membershipCardApi = new MembershipCardApi();
                var membershipCardTypeApi = new MembershipCardTypeApi();
                var promotionDetailApi = new PromotionDetailApi();

                var orderList = orderApi.FixDB2().ToList();
                int count = 0;
                foreach (var order in orderList)
                {
                    try
                    {
                        var membershipCard = membershipCardApi.GetMembershipCardActiveByCustomerId(order.CustomerID.Value)
                        .FirstOrDefault();
                        if (membershipCard != null)
                        {
                            if (membershipCard.MembershipTypeId != null)
                            {
                                var appendCode = membershipCard.MembershipCardType.AppendCode;

                                var promotionDetail = promotionDetailApi.GetPromotionDetailByAppendCode(appendCode).FirstOrDefault();
                                if (promotionDetail != null)
                                {
                                    order.Att1 = promotionDetail.PromotionCode + ":" + membershipCard.MembershipCardCode;
                                    ++count;
                                    await orderApi.UpdateOrderEntity(order);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }

                }

                return Json(new { success = true, count = count, allOrder = orderList.Count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, count = 0, allOrder = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult FixMappingEmployee()
        {
            var storeApi = new StoreApi();
            var mappingEmpApi = new EmployeeInStoreApi();
            var employeeApi = new EmployeeApi();
            var listStores = storeApi.GetActive();
            foreach (var item in listStores)
            {
                var employeeList = employeeApi.BaseService.Get(q => q.MainStoreId == item.ID).ToList();
                foreach (var item1 in employeeList)
                {
                    var mapping = mappingEmpApi.BaseService.FirstOrDefault(q => q.EmployeeId == item1.Id && q.StoreId == item.ID);
                    if (mapping == null)
                    {
                        mapping = new EmployeeInStore();
                        mapping.StoreId = item.ID;
                        mapping.EmployeeId = item1.Id;
                        mapping.AssignedDate = Utils.GetCurrentDateTime();
                        mapping.Status = 1;
                        mapping.Active = true;
                        mappingEmpApi.BaseService.Create(mapping);
                    }
                }
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FixPayment()
        {
            try
            {
                var orderApi = new OrderApi();
                var paymentApi = new PaymentApi();
                // fix bug report
                var listOrderFail = orderApi.BaseService.Get(q => q.OrderType == 6 && q.OrderStatus == 2 && q.FinalAmount > 0 && q.Payments.Count() == 0).ToList();
                if (listOrderFail.Count() > 0)
                {
                    foreach (var item in listOrderFail)
                    {
                        var payment = new PaymentViewModel();
                        payment.Amount = item.FinalAmount;
                        payment.PayTime = item.CheckInDate.Value.AddMinutes(1);
                        payment.RealAmount = item.FinalAmount;
                        payment.Type = (int)PaymentTypeEnum.Cash;
                        payment.Status = (int)PaymentStatusEnum.Approved;
                        payment.ToRentID = item.RentID;
                        try
                        {
                            paymentApi.Create(payment);
                        }
                        catch (Exception)
                        {
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult fixOrder()
        {
            try
            {
                var orderApi = new OrderApi();
                var paymentApi = new PaymentApi();
                string date = "09/01/2017";
                var startdate = date.ToDateTime().GetStartOfDate();
                var enddate = date.ToDateTime().GetEndOfDate();
                var timespan = Utils.GetCurrentDateTime().GetStartOfDate().Subtract(startdate);
                var totalsub = (int)timespan.TotalDays;
                var failOrder = orderApi.BaseService.Get(q => q.StoreID == 37 && q.CheckInDate >= startdate && q.CheckInDate <= enddate).ToList();
                foreach (var item in failOrder)
                {
                    item.CheckInDate = item.CheckInDate.Value.AddDays(totalsub);
                    orderApi.BaseService.Update(item);
                    foreach (var item1 in item.Payments)
                    {
                        item1.PayTime = item1.PayTime.AddDays(totalsub);
                        paymentApi.BaseService.Update(item1);
                    }

                }

            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public ActionResult fixOrderRisk()
        {
            var startDate = "24/05/2017";
            var endDate = "24/05/2017";
            var startTime = Utils.ToDateTime(startDate).GetStartOfDate();
            var endTime = Utils.ToDateTime(endDate).GetEndOfDate();
            var orderApi = new OrderApi();
            var ListOrderMiss = orderApi.BaseService.Get(r => r.CheckInDate >= startTime
                                   && r.CheckInDate <= endTime);

            var ListOrder = ListOrderMiss.Select(
                q => new OrderFixbug
                {
                    InvoiceId = q.InvoiceID,
                    Att1 = q.Att1,
                    Att2 = q.Att2,
                    CheckInDate = q.CheckInDate.Value,
                    CheckInHour = q.CheckinHour.Value,
                    CheckInPerson = q.CheckInPerson,
                    CustomerId = (q.CustomerID == null) ? 0 : q.CustomerID.Value,
                    DeliveryAdress = q.DeliveryAddress,
                    DeliveryPhone = q.DeliveryPhone,
                    DeliveryReceiver = q.DeliveryReceiver,
                    Discount = q.Discount,
                    DiscountOrderDetail = q.DiscountOrderDetail,
                    FinalAmount = q.FinalAmount,
                    GroupPaymentStatus = q.GroupPaymentStatus,
                    IsFixedPrice = q.IsFixedPrice,
                    OrderDetailTotalQuantity = q.OrderDetailsTotalQuantity == null ? 0 : q.OrderDetailsTotalQuantity.Value,
                    OrderStatus = q.OrderStatus,
                    OrderType = q.OrderType,
                    SourceType = q.SourceType,
                    StoreId = q.StoreID.Value,
                    TotalAmount = q.TotalAmount,
                    TotalInvoicePrint = q.TotalInvoicePrint == null ? 0 : q.TotalInvoicePrint.Value,
                });

            var listPayment = new List<PaymentFixbug>();
            var listOrderDetails = new List<OrderDetaiFixDB>();
            var listOrderPromotionMapping = new List<OrderPromotionMappingFixDB>();
            foreach (var item in ListOrderMiss)
            {
                foreach (var paymentTmp in item.Payments)
                {
                    var payTmp = new PaymentFixbug();
                    payTmp.Amount = paymentTmp.Amount;
                    payTmp.InvoiceId = item.InvoiceID;
                    payTmp.FCAmount = paymentTmp.FCAmount;
                    payTmp.CardCode = paymentTmp.CardCode;
                    payTmp.CurrencyCode = paymentTmp.CurrencyCode;
                    payTmp.Notes = paymentTmp.Notes;
                    payTmp.PayTime = paymentTmp.PayTime;
                    payTmp.RealAmount = paymentTmp.RealAmount == null ? 0 : paymentTmp.RealAmount.Value;
                    payTmp.Status = paymentTmp.Status;
                    payTmp.Type = paymentTmp.Type;
                    listPayment.Add(payTmp);
                }
                foreach (var orderDetail in item.OrderDetails)
                {
                    var orderdetailTmp = new OrderDetaiFixDB();
                    orderdetailTmp.InvoiceId = item.InvoiceID;
                    orderdetailTmp.IsAddition = orderDetail.IsAddition;
                    orderdetailTmp.ItemQuantity = orderDetail.ItemQuantity;
                    orderdetailTmp.OrderDate = orderDetail.OrderDate;
                    orderdetailTmp.ParentId = orderDetail.ParentId;
                    orderdetailTmp.ProductID = orderDetail.ProductID;
                    orderdetailTmp.ProductOrderType = orderDetail.ProductOrderType;
                    orderdetailTmp.ProductType = orderDetail.ProductType;
                    orderdetailTmp.Quantity = orderDetail.Quantity;
                    orderdetailTmp.Status = orderDetail.Status;
                    orderdetailTmp.StoreId = orderDetail.StoreId;
                    orderdetailTmp.TaxPercent = orderDetail.TaxPercent;
                    orderdetailTmp.TaxValue = orderDetail.TaxValue;
                    orderdetailTmp.TmpDetailId = orderDetail.TmpDetailId;
                    orderdetailTmp.TotalAmount = orderDetail.TotalAmount;
                    orderdetailTmp.UnitPrice = orderDetail.UnitPrice;
                    orderdetailTmp.DetailDescription = orderDetail.DetailDescription;
                    orderdetailTmp.Discount = orderDetail.Discount;
                    orderdetailTmp.FinalAmount = orderDetail.FinalAmount;
                    listOrderDetails.Add(orderdetailTmp);
                }

                foreach (var OrderPromotionMaping in item.OrderPromotionMappings)
                {
                    var promotionmappingTMP = new OrderPromotionMappingFixDB();
                    promotionmappingTMP.Active = OrderPromotionMaping.Active;
                    promotionmappingTMP.DiscountAmount = OrderPromotionMaping.DiscountAmount;
                    promotionmappingTMP.DiscountRate = OrderPromotionMaping.DiscountRate;
                    promotionmappingTMP.IvoiceId = item.InvoiceID;
                    promotionmappingTMP.MappingIndex = OrderPromotionMaping.MappingIndex;
                    promotionmappingTMP.PromotionDetailId = OrderPromotionMaping.PromotionDetailId;
                    promotionmappingTMP.PromotionId = OrderPromotionMaping.PromotionId;
                    listOrderPromotionMapping.Add(promotionmappingTMP);
                }
            }
            try
            {
                string OrderFile = JsonConvert.SerializeObject(ListOrder, Formatting.Indented);
                string PaymentFile = JsonConvert.SerializeObject(listPayment, Formatting.Indented);
                string OrderdetailFile = JsonConvert.SerializeObject(listOrderDetails, Formatting.Indented);
                string PromotionMappingFile = JsonConvert.SerializeObject(listOrderPromotionMapping, Formatting.Indented);
                System.IO.File.WriteAllText(@"D:\Order.txt", OrderFile);
                System.IO.File.WriteAllText(@"D:\Payment.txt", PaymentFile);
                System.IO.File.WriteAllText(@"D:\OrderDetail.txt", OrderdetailFile);
                System.IO.File.WriteAllText(@"D:\Promotion.txt", PromotionMappingFile);

                //string namefile = "dataOrder";
                //await SaveFile(namefile, jsonfile);
            }
            catch (Exception ex)
            {

                throw;
            }


            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult UpdateOrderFail()
        {
            string jsonOrder = System.IO.File.ReadAllText(@"D:\Order.txt");
            List<OrderFixbug> orderfixbug = JsonConvert.DeserializeObject<List<OrderFixbug>>(jsonOrder);
            string jsonOrderdetails = System.IO.File.ReadAllText(@"D:\OrderDetail.txt");
            List<OrderDetaiFixDB> orderDetailfixbug = JsonConvert.DeserializeObject<List<OrderDetaiFixDB>>(jsonOrderdetails);
            string jsonPayment = System.IO.File.ReadAllText(@"D:\Payment.txt");
            List<PaymentFixbug> Paymentfixbug = JsonConvert.DeserializeObject<List<PaymentFixbug>>(jsonPayment);
            string jsonPromotion = System.IO.File.ReadAllText(@"D:\Promotion.txt");
            List<OrderPromotionMappingFixDB> promotionfixbug = JsonConvert.DeserializeObject<List<OrderPromotionMappingFixDB>>(jsonPromotion);

            var orderApi = new OrderApi();
            var orderDetailApi = new OrderDetailApi();
            var paymentApi = new PaymentApi();
            var orderPromotionMappingApi = new OrderPromotionMappingApi();
            foreach (var item in orderfixbug)
            {
                var orderTmp = new Order();
                orderTmp.InvoiceID = item.InvoiceId;
                orderTmp.Att1 = item.Att1;
                orderTmp.Att2 = item.Att2;
                orderTmp.CheckInDate = item.CheckInDate;
                orderTmp.CheckinHour = item.CheckInHour;
                orderTmp.CheckInPerson = item.CheckInPerson;
                orderTmp.CustomerID = item.CustomerId;
                orderTmp.DeliveryAddress = item.DeliveryAdress;
                orderTmp.DeliveryPhone = item.DeliveryPhone;
                orderTmp.DeliveryReceiver = item.DeliveryReceiver;
                orderTmp.Discount = item.Discount;
                orderTmp.DiscountOrderDetail = item.DiscountOrderDetail;
                orderTmp.FinalAmount = item.FinalAmount;
                orderTmp.GroupPaymentStatus = item.GroupPaymentStatus;
                orderTmp.IsFixedPrice = item.IsFixedPrice;
                orderTmp.OrderDetailsTotalQuantity = item.OrderDetailTotalQuantity;
                orderTmp.OrderStatus = item.OrderStatus;
                orderTmp.OrderType = item.OrderType;
                orderTmp.SourceType = item.SourceType;
                orderTmp.StoreID = item.StoreId;
                orderTmp.TotalAmount = item.TotalAmount;
                orderTmp.TotalInvoicePrint = item.TotalInvoicePrint;

                var checkorder = orderApi.BaseService.Get(q => q.InvoiceID == orderTmp.InvoiceID).FirstOrDefault();
                if (checkorder == null)
                {
                    orderApi.BaseService.Create(orderTmp);

                    var listorderDetail = orderDetailfixbug.Where(q => q.InvoiceId == item.InvoiceId);
                    foreach (var orderDdetailItem in listorderDetail)
                    {
                        var orderDetailtmp = new OrderDetail();
                        orderDetailtmp.RentID = orderTmp.RentID;
                        orderDetailtmp.IsAddition = orderDdetailItem.IsAddition;
                        orderDetailtmp.ItemQuantity = orderDdetailItem.ItemQuantity;
                        orderDetailtmp.OrderDate = orderDdetailItem.OrderDate;
                        orderDetailtmp.ParentId = orderDdetailItem.ParentId;
                        orderDetailtmp.ProductID = orderDdetailItem.ProductID;
                        orderDetailtmp.ProductOrderType = orderDdetailItem.ProductOrderType;
                        orderDetailtmp.ProductType = orderDdetailItem.ProductType;
                        orderDetailtmp.Quantity = orderDdetailItem.Quantity;
                        orderDetailtmp.Status = orderDdetailItem.Status;
                        orderDetailtmp.StoreId = orderDdetailItem.StoreId;
                        orderDetailtmp.TaxPercent = orderDdetailItem.TaxPercent;
                        orderDetailtmp.TaxValue = orderDdetailItem.TaxValue;
                        orderDetailtmp.TmpDetailId = orderDdetailItem.TmpDetailId;
                        orderDetailtmp.TotalAmount = orderDdetailItem.TotalAmount;
                        orderDetailtmp.UnitPrice = orderDdetailItem.UnitPrice;
                        orderDetailtmp.DetailDescription = orderDdetailItem.DetailDescription;
                        orderDetailtmp.Discount = orderDdetailItem.Discount;
                        orderDetailtmp.FinalAmount = orderDdetailItem.FinalAmount;

                        orderDetailApi.BaseService.Create(orderDetailtmp);
                    }
                    var listPayment = Paymentfixbug.Where(q => q.InvoiceId == item.InvoiceId);

                    foreach (var paymentitem in listPayment)
                    {
                        var paymentTMP = new HmsService.Models.Entities.Payment();
                        paymentTMP.ToRentID = orderTmp.RentID;
                        paymentTMP.Amount = paymentitem.Amount;
                        paymentTMP.FCAmount = paymentitem.FCAmount;
                        paymentTMP.CardCode = paymentitem.CardCode;
                        paymentTMP.CurrencyCode = paymentitem.CurrencyCode;
                        paymentTMP.Notes = paymentitem.Notes;
                        paymentTMP.PayTime = paymentitem.PayTime;
                        paymentTMP.RealAmount = paymentitem.RealAmount;
                        paymentTMP.Status = paymentitem.Status;
                        paymentTMP.Type = paymentitem.Type;

                        paymentApi.BaseService.Create(paymentTMP);
                    }

                    var listPromotionMapping = promotionfixbug.Where(q => q.IvoiceId == item.InvoiceId);
                    foreach (var promotionitem in listPromotionMapping)
                    {
                        var promotionOrder = new OrderPromotionMapping();
                        promotionOrder.Active = promotionitem.Active;
                        promotionOrder.DiscountAmount = promotionitem.DiscountAmount;
                        promotionOrder.DiscountRate = promotionitem.DiscountRate;
                        promotionOrder.OrderId = orderTmp.RentID;
                        promotionOrder.MappingIndex = promotionitem.MappingIndex;
                        promotionOrder.PromotionDetailId = promotionitem.PromotionDetailId;
                        promotionOrder.PromotionId = promotionitem.PromotionId;
                        orderPromotionMappingApi.BaseService.Create(promotionOrder);
                    }
                }
                else
                {
                    if (checkorder.OrderDetails != null)
                    {
                        if (checkorder.OrderDetails.Count <= 0)
                        {
                            orderApi.BaseService.Delete(checkorder);
                            orderApi.BaseService.Create(orderTmp);
                            var listorderDetail = orderDetailfixbug.Where(q => q.InvoiceId == item.InvoiceId);
                            foreach (var orderDdetailItem in listorderDetail)
                            {
                                var orderDetailtmp = new OrderDetail();
                                orderDetailtmp.RentID = orderTmp.RentID;
                                orderDetailtmp.IsAddition = orderDdetailItem.IsAddition;
                                orderDetailtmp.ItemQuantity = orderDdetailItem.ItemQuantity;
                                orderDetailtmp.OrderDate = orderDdetailItem.OrderDate;
                                orderDetailtmp.ParentId = orderDdetailItem.ParentId;
                                orderDetailtmp.ProductID = orderDdetailItem.ProductID;
                                orderDetailtmp.ProductOrderType = orderDdetailItem.ProductOrderType;
                                orderDetailtmp.ProductType = orderDdetailItem.ProductType;
                                orderDetailtmp.Quantity = orderDdetailItem.Quantity;
                                orderDetailtmp.Status = orderDdetailItem.Status;
                                orderDetailtmp.StoreId = orderDdetailItem.StoreId;
                                orderDetailtmp.TaxPercent = orderDdetailItem.TaxPercent;
                                orderDetailtmp.TaxValue = orderDdetailItem.TaxValue;
                                orderDetailtmp.TmpDetailId = orderDdetailItem.TmpDetailId;
                                orderDetailtmp.TotalAmount = orderDdetailItem.TotalAmount;
                                orderDetailtmp.UnitPrice = orderDdetailItem.UnitPrice;
                                orderDetailtmp.DetailDescription = orderDdetailItem.DetailDescription;
                                orderDetailtmp.Discount = orderDdetailItem.Discount;
                                orderDetailtmp.FinalAmount = orderDdetailItem.FinalAmount;

                                orderDetailApi.BaseService.Create(orderDetailtmp);
                            }
                            var listPayment = Paymentfixbug.Where(q => q.InvoiceId == item.InvoiceId);

                            foreach (var paymentitem in listPayment)
                            {
                                var paymentTMP = new HmsService.Models.Entities.Payment();
                                paymentTMP.ToRentID = orderTmp.RentID;
                                paymentTMP.Amount = paymentitem.Amount;
                                paymentTMP.FCAmount = paymentitem.FCAmount;
                                paymentTMP.CardCode = paymentitem.CardCode;
                                paymentTMP.CurrencyCode = paymentitem.CurrencyCode;
                                paymentTMP.Notes = paymentitem.Notes;
                                paymentTMP.PayTime = paymentitem.PayTime;
                                paymentTMP.RealAmount = paymentitem.RealAmount;
                                paymentTMP.Status = paymentitem.Status;
                                paymentTMP.Type = paymentitem.Type;

                                paymentApi.BaseService.Create(paymentTMP);
                            }

                            var listPromotionMapping = promotionfixbug.Where(q => q.IvoiceId == item.InvoiceId);
                            foreach (var promotionitem in listPromotionMapping)
                            {
                                var promotionOrder = new OrderPromotionMapping();
                                promotionOrder.Active = promotionitem.Active;
                                promotionOrder.DiscountAmount = promotionitem.DiscountAmount;
                                promotionOrder.DiscountRate = promotionitem.DiscountRate;
                                promotionOrder.OrderId = orderTmp.RentID;
                                promotionOrder.MappingIndex = promotionitem.MappingIndex;
                                promotionOrder.PromotionDetailId = promotionitem.PromotionDetailId;
                                promotionOrder.PromotionId = promotionitem.PromotionId;
                                orderPromotionMappingApi.BaseService.Create(promotionOrder);
                            }
                        }
                    }


                }
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }



    public class OrderFixbug
    {
        public string InvoiceId { get; set; }
        public DateTime CheckInDate { get; set; }
        public double TotalAmount { get; set; }
        public double Discount { get; set; }
        public double DiscountOrderDetail { get; set; }
        public double FinalAmount { get; set; }
        public int OrderStatus { get; set; }
        public int OrderType { get; set; }
        public string CheckInPerson { get; set; }
        public int CustomerId { get; set; }
        public bool IsFixedPrice { get; set; }
        public int StoreId { get; set; }
        public int SourceType { get; set; }
        public string DeliveryAdress { get; set; }
        public int OrderDetailTotalQuantity { get; set; }
        public int CheckInHour { get; set; }
        public int TotalInvoicePrint { get; set; }
        public string Att1 { get; set; }
        public string Att2 { get; set; }
        public int GroupPaymentStatus { get; set; }
        public string DeliveryReceiver { get; set; }
        public string DeliveryPhone { get; set; }

    }

    public class PaymentFixbug
    {
        public string InvoiceId { get; set; }
        public double Amount { get; set; }
        public string CurrencyCode { get; set; }
        public decimal FCAmount { get; set; }
        public string Notes { get; set; }
        public DateTime PayTime { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public double RealAmount { get; set; }
        public string CardCode { get; set; }
    }

    public class OrderDetaiFixDB
    {
        public string InvoiceId { get; set; }
        public int ProductID { get; set; }
        public double TotalAmount { get; set; }
        public int Quantity { get; set; }
        public System.DateTime OrderDate { get; set; }
        public int Status { get; set; }
        public double FinalAmount { get; set; }
        public bool IsAddition { get; set; }
        public string DetailDescription { get; set; }
        public double Discount { get; set; }
        public Nullable<double> TaxPercent { get; set; }
        public Nullable<double> TaxValue { get; set; }
        public double UnitPrice { get; set; }
        public Nullable<int> ProductType { get; set; }
        public Nullable<int> ParentId { get; set; }
        public Nullable<int> StoreId { get; set; }
        public Nullable<int> ProductOrderType { get; set; }
        public int ItemQuantity { get; set; }
        public Nullable<int> TmpDetailId { get; set; }
    }

    public class OrderPromotionMappingFixDB
    {
        public string IvoiceId { get; set; }
        public int PromotionId { get; set; }
        public Nullable<int> PromotionDetailId { get; set; }
        public decimal DiscountAmount { get; set; }
        public Nullable<double> DiscountRate { get; set; }
        public bool Active { get; set; }
        public int MappingIndex { get; set; }
    }

}