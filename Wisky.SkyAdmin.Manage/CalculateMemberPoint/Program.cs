using AutoMapper;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CalculateMemberPoint
{
    class Program
    {
        private static DateTime defaultCheckTime = new DateTime(2018,11,5);
        private static DateTime checkTime;
        private static DateTime newCheckTime;
        private static string path = Directory.GetCurrentDirectory() + "\\LogAccumFile.txt";
        private static string CheckTimePath = Directory.GetCurrentDirectory() + "\\CheckTime.txt";
        private static string lastLogDate = "";
        private static FileStream fs = new FileStream(path, FileMode.Append);
        private static StreamWriter sw = new StreamWriter(fs,Encoding.Default);
        private static HmsEntities _db = new HmsEntities();
        private static string notificationUri = ConstantManager.NOTIFICATION_URI;
        private static string appAuthorization = ConstantManager.APP_AUTHORIZATION;
        private static int brandId = (int)BrandEnum.Passio;
        private static List<int> listMobileTypeId = ConstantManager.LIST_MOBILE_TYPE_ID;
        private static List<NotificationModel> listNotification = new List<NotificationModel>();

        static void Main(string[] args)
        {
            while (true) {
                Console.WriteLine("Enter frist run date time day,month,year, 0 to skip: ");
                var defaultCheckTimeString = Console.ReadLine();
                if (defaultCheckTimeString.Trim().Equals("0"))
                {
                    break;
                }
                else {
                    try
                    {
                        var c = defaultCheckTimeString.Split(',');
                        defaultCheckTime = new DateTime(Int32.Parse(c[2].Trim()), Int32.Parse(c[1].Trim()), Int32.Parse(c[0].Trim()));
                        break;
                    }
                    catch {
                        Console.WriteLine("Input eror, please try again.");
                        continue;
                    }
                }
            }
            while (true)
            {
                try
                {
                    //Get Current Date
                    var currentTime = DateTime.Now;
                    string currentDateStr = currentTime.ToShortDateString();
                    //If current date different from last log date
                    if (currentDateStr != lastLogDate)
                    {
                        sw.WriteLine("Run member point calculate in {0}", currentDateStr.ToString());
                        lastLogDate = currentDateStr;
                    }
                    //Run Report
                    checkTime = defaultCheckTime;
                    try
                    {
                        checkTime = DateTime.Parse(File.ReadAllText(CheckTimePath));
                    }
                    catch {
                        File.WriteAllText(CheckTimePath,checkTime.ToString());
                    }
                    newCheckTime = currentTime;
                    Console.WriteLine("Run member point calculate from {0} to {1}", checkTime, newCheckTime);
                    var startTime = DateTime.Now;
                    PerformWork();
                    var endTime = DateTime.Now;
                    Console.WriteLine("Run time {0}", endTime - startTime);
                    Console.WriteLine("Run completely");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //Log to file
                    sw.WriteLine(DateTime.Now.ToString() + "    " + ex.Message);
                    sw.Flush();
                }
                Thread.Sleep(60000);
            }
        }

        private static void PerformWork()
        {
            CalculateMemberPoint();
        }

        private static void CalculateMemberPoint()
        {
            //Set deplay 1 day for sync order
            var delayCheckTime = checkTime.AddDays(-1);
            var ordersGroupByCustomer = _db.Orders.Where(q => q.CheckInDate >= delayCheckTime && q.CheckInDate <= newCheckTime && q.Store.BrandId == brandId &&
            q.OrderType != (int)OrderTypeEnum.DropProduct && (q.OrderStatus == (int)OrderStatusEnum.Finish || q.OrderStatus == (int)OrderStatusEnum.Cancel
            || q.OrderStatus == (int)OrderStatusEnum.PreCancel) && q.CustomerID != null && q.CustomerID != 0 && (q.isSync != true))
            .GroupBy(p => p.CustomerID).ToList();
            listNotification = new List<NotificationModel>();
            foreach (var ordersByCustomer in ordersGroupByCustomer)
            {
                var customerId = ordersByCustomer.Key;
                var orders = ordersByCustomer.ToList();
                //Check MembershipCard
                var membershipCard = _db.MembershipCards.Where(q => q.CustomerId == customerId && q.MembershipCardType != null 
                && listMobileTypeId.Contains((int)q.MembershipTypeId)).FirstOrDefault();
                if (membershipCard == null)
                {
                    foreach (var order in orders)
                    {
                        order.isSync = true;
                    }
                    continue;
                }
                //Check Point Account
                var pointAccount = membershipCard.Accounts.Where(q => q.Type == (int)AccountTypeEnum.PointAccount).FirstOrDefault();
                if (pointAccount == null)
                {
                    foreach (var order in orders)
                    {
                        order.isSync = true;
                    }
                    continue;
                }
                //Set point account balance = 0 if balance is null
                if (pointAccount.Balance == null)
                {
                    pointAccount.Balance = 0;
                }
                if (orders.Count > 0)
                {
                    int memberPoint = 0;
                    foreach (var order in orders)
                    {
                        //Calculate memberpoint for order
                        order.MemberPoint = (int)(order.FinalAmount / (ConstantManager.MEMBER_POINT_AMOUNT));
                        switch (order.OrderStatus)
                        {
                            case (int)OrderStatusEnum.Finish:
                                memberPoint += (int)order.MemberPoint;
                                //if (order.OrderType != (int)OrderTypeEnum.OrderCard) {
                                //    memberPoint += (int)order.MemberPoint;
                                //}
                                break;
                            case (int)OrderStatusEnum.PreCancel:
                                memberPoint -= (int)order.MemberPoint;
                                break;
                            case (int)OrderStatusEnum.Cancel:
                                memberPoint -= (int)order.MemberPoint;
                                break;
                            default:
                                break;
                        }
                        //store order card message notification
                        if (order.OrderType == (int)OrderTypeEnum.OrderCard)
                        {
                            string tittle2 = "Nạp thẻ";
                            string message2 = "Bạn đã nạp thành công " + order.TotalAmount.ToString() + " Point";
                            NotificationModel model2 = new NotificationModel(tittle2, message2, (int)customerId);
                            listNotification.Add(model2);
                            //Check first purchase point && applly first purchase promotion
                            var condition = !(_db.Orders.Where(q => q.CustomerID == order.CustomerID
                            && q.OrderStatus == (int)OrderStatusEnum.Finish && q.FinalAmount > 0
                            && q.OrderType == (int)OrderTypeEnum.OrderCard && q.isSync == true).Any());
                            if (condition) {
                                applyFirstPurchasePromotion(brandId, membershipCard);
                            }
                        }
                        order.isSync = true;
                    }
                    //
                    //if (!orders.Any(q => q.OrderType != (int)OrderTypeEnum.OrderCard)) {
                    //    continue;
                    //}
                    //store custom message for order discount 100%
                    if (memberPoint <= 0)
                    {
                        string tittle0 = "Đơn hàng";
                        string message0 = "Bạn đã đặt đơn hàng thành công";
                        NotificationModel model0 = new NotificationModel(tittle0, message0, (int)customerId);
                        listNotification.Add(model0);
                        continue;
                    }
                    pointAccount.Balance += memberPoint;
                    Console.WriteLine("Add {0} points to membershipCard {1}", memberPoint, membershipCard.Id);
                    //Log to file
                    sw.WriteLine("Add {0} points to membershipCard {1}", memberPoint, membershipCard.Id);
                    //store member point message notification
                    string tittle = "Tích điểm";
                    string message = "Đơn hàng đã tích " + memberPoint.ToString() + " điểm thành công";
                    NotificationModel model = new NotificationModel(tittle, message, (int)customerId);
                    listNotification.Add(model);
                    //update membership type if reach condition
                    updateMembershipCardType((decimal)pointAccount.Balance, membershipCard);
                }
            }
            _db.SaveChanges();
            //send all stored message to customer
            foreach (var item in listNotification)
            {
                sendNotification(item);
            }
            _db.SaveChanges();
            //update new check tiem to CheckTime.txt
            File.WriteAllText(CheckTimePath, newCheckTime.ToString());
            sw.Flush();
        }

        private static void sendNotification(NotificationModel model)
        {
            //Save notification to db
            Notification notification = new Notification()
            {
                Active = true,
                CreateDate = newCheckTime,
                UpdateDate = newCheckTime,
                Title = model.tittle,
                Opening = model.message,
                CustomerId = (int)model.customerid,
                Type = (int)NotificationsTypeEnum.Notify
            };
            _db.Notifications.Add(notification);
            //Create notification request
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(notificationUri);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", appAuthorization);
            //create notification message for android
            object data = new
            {
                to = "/topics/" + model.customerid,
                priority = "high",
                contentavailable = true,
                notification = new
                {
                    sound = "default",
                    badge = "1",
                    body = model.message,
                    tittle = model.tittle
                }
            };
            var json = JsonConvert.SerializeObject(data);
            json = json.ToString().Replace("\\r\\n", "");
            var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            client.PostAsync("", stringContent);
            //var response = await client.PostAsync("", stringContent);
            //var responseString = await response.Content.ReadAsStringAsync();
            //create notification message for iOS
            object data2 = new
            {
                //to = "/topics/foo-bar",
                to = "/topics/" + model.customerid + "_ios",
                priority = "high",
                contentavailable = true,
                notification = new
                {
                    sound = "default",
                    badge = "1",
                    body = model.message,
                    tittle = model.tittle
                },
                data = new
                {
                    message = model.message,
                    tittle = model.tittle
                }
            };
            var json2 = JsonConvert.SerializeObject(data2);
            json2 = json2.ToString().Replace("\\r\\n", "");
            var stringContent2 = new StringContent(json2, UnicodeEncoding.UTF8, "application/json");
            client.PostAsync("", stringContent2);
            //var response2 = await client.PostAsync("", stringContent2);
            //var responseString2 = await response2.Content.ReadAsStringAsync();
        }
        private static void updateMembershipCardType(decimal Balance, MembershipCard membershipCard)
        {
            List<MembershipCardType> membershipCardTypes = _db.MembershipCardTypes.Where(q => q.IsMobile == true).OrderBy(q => q.TypeLevel).ToList();
            //Check membership is mobile or not
            if (!membershipCardTypes.Contains(membershipCard.MembershipCardType))
            {
                return;
            }
            //Update mebership card type
            foreach (var type in membershipCardTypes)
            {
                //Check current membership card type compare to all membership card type
                if (membershipCard.MembershipCardType.TypeLevel >= type.TypeLevel)
                {
                    continue;
                }
                if (Balance >= type.TypePoint)
                {
                    membershipCard.MembershipTypeId = type.Id;
                    membershipCard.MembershipCardType = type;
                    Console.WriteLine("Upgrade membership card {0} to {1}", membershipCard.Id, type.TypeName);
                    //Create promotion voucher when reach new membership type
                    //createPromotionVoucher(membershipCard);
                }
            }
        }

        private static void createPromotionVoucher(MembershipCard membershipCard)
        {
            //get list promotion detail which has regex code
            List<PromotionDetail> listPromotionDetails = _db.PromotionDetails.Where(q => q.RegExCode != null).ToList();
            //concat membershipcard type code and membershipcard code
            string code = membershipCard.MembershipCardType.AppendCode + "_" + membershipCard.MembershipCardCode;
            foreach (var promotionDetail in listPromotionDetails)
            {
                //check promotion detail can apply to this membership card type or not
                bool canApply = Regex.IsMatch(code, promotionDetail.RegExCode);
                if (canApply)
                {
                    //Check exist voucher base on promotion detail and membership card
                    var existVoucher = _db.Vouchers.Where(q => q.PromotionDetailID == promotionDetail.PromotionDetailID && q.MembershipCardId == membershipCard.Id).FirstOrDefault();
                    if (existVoucher != null)
                    {
                        continue;
                    }
                    //Prepare new voucher code
                    string newVoucherCode = generateVoucherCode(brandId);
                    while (true)
                    {
                        var checkVoucher = _db.Vouchers.Where(q => q.VoucherCode == newVoucherCode).FirstOrDefault();
                        if (checkVoucher != null)
                        {
                            newVoucherCode = generateVoucherCode(brandId);
                        }
                        else
                        {
                            break;
                        }
                    }
                    //create new voucher
                    Voucher voucher = new Voucher()
                    {
                        Active = true,
                        IsGetted = true,
                        isUsed = false,
                        MembershipCardId = membershipCard.Id,
                        PromotionDetailID = promotionDetail.PromotionDetailID,
                        PromotionID = _db.Promotions.Where(q => q.PromotionCode == promotionDetail.PromotionCode).FirstOrDefault().PromotionID,
                        Quantity = 1,
                        UsedQuantity = 0,
                        VoucherCode = newVoucherCode
                    };
                    _db.Vouchers.Add(voucher);
                    Console.WriteLine("Created voucher {0}", newVoucherCode);
                    //create notification for new voucher
                    string tittle = "Quà Tặng";
                    string messgae = "Bạn nhận được 1 voucher mới";
                    NotificationModel model = new NotificationModel(tittle,messgae, (int)membershipCard.CustomerId);
                    listNotification.Add(model);
                }
            }
        }

        private static void createPromotionVoucher(MembershipCard membershipCard, int PromotionId, int PromotionDetailiD) {
            //Prepare new voucher code
            string newVoucherCode = generateVoucherCode(brandId);
            while (true)
            {
                var checkVoucher = _db.Vouchers.Where(q => q.VoucherCode == newVoucherCode).FirstOrDefault();
                if (checkVoucher != null)
                {
                    newVoucherCode = generateVoucherCode(brandId);
                }
                else
                {
                    break;
                }
            }
            //create new voucher
            Voucher voucher = new Voucher()
            {
                Active = true,
                IsGetted = true,
                isUsed = false,
                MembershipCardId = membershipCard.Id,
                PromotionDetailID = PromotionDetailiD,
                PromotionID = PromotionId,
                Quantity = 1,
                UsedQuantity = 0,
                VoucherCode = newVoucherCode
            };
            _db.Vouchers.Add(voucher);
            Console.WriteLine("Created voucher {0}", newVoucherCode);
        }

        private static string generateVoucherCode(int brandID)
        {
            var random = new Random();
            switch (brandID)
            {
                case (int)BrandEnum.Passio:
                    string chars = "0123456789";
                    int length = 10;
                    return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                default:
                    return String.Empty;
            }
        }

        private static void applyFirstPurchasePromotion(int brandID,MembershipCard membershipCard)
        {
            var random = new Random();
            switch (brandID)
            {
                case (int)BrandEnum.Passio:
                    //only apply for green member
                    if (membershipCard.MembershipTypeId != (int)MembershipCardTypeEnum.Green) {
                        return;
                    }
                    var promotionId = 117;
                    var promotionDetailId = 215;
                    for (int i = 0; i < 3; i++)
                    {
                        createPromotionVoucher(membershipCard,promotionId,promotionDetailId);
                    }
                    string tittle = "Quà tặng";
                    string message = "Bạn được tặng 3 voucher Latte 50% cho lần nạp thẻ đầu tiên";
                    NotificationModel model3 = new NotificationModel(tittle, message, (int)membershipCard.CustomerId);
                    listNotification.Add(model3);
                    return;
                default:
                    return;
            }
        }

    }
    public class NotificationModel
    {
        public NotificationModel()
        {
        }
        public NotificationModel(string tittle, string message,int customerid)
        {
            this.tittle = tittle;
            this.message = message;
            this.customerid = customerid;
        }
        public string tittle { get; set; }
        public string message { get; set; }
        public int customerid { get; set; }
    }
}
