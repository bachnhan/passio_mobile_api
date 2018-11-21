using HmsService.Models;
using HmsService.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AddVoucherNotification
{
    class Program
    {
        private static HmsEntities _db = new HmsEntities();
        private static List<int> listMobileTypeId = ConstantManager.LIST_MOBILE_TYPE_ID;
        private static string notificationUri = ConstantManager.NOTIFICATION_URI;
        private static string appAuthorization = ConstantManager.APP_AUTHORIZATION;
        static void Main(string[] args)
        {
            var OrderCardFromToDateCustomerIds2 = _db.Orders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish &&
            q.OrderType == (int)OrderTypeEnum.OrderCard && q.CheckInDate <= new DateTime(2018, 11, 4)).Select(q => q.CustomerID).Distinct();
            var OrderCardFromToDateCustomerIds = _db.Orders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish &&
            q.OrderType == (int)OrderTypeEnum.OrderCard && q.CheckInDate >= new DateTime(2018, 11, 4) && q.CheckInDate <= new DateTime(2018, 11, 6)
            ).Select(q => q.CustomerID).Distinct();
            var CustomerIds = _db.Orders.Where(q => q.OrderStatus == (int)OrderStatusEnum.Finish &&
            q.OrderType == (int)OrderTypeEnum.OrderCard && q.CheckInDate >= new DateTime(2018, 11, 4) && q.CheckInDate <= new DateTime(2018, 11, 6)
            && !OrderCardFromToDateCustomerIds2.Contains(q.CustomerID)).Select(q => q.CustomerID).Distinct();
            Console.WriteLine(CustomerIds.Count());
            var cards = _db.MembershipCards.Where(q => listMobileTypeId.Contains((int)q.MembershipTypeId) && CustomerIds.Contains(q.CustomerId) && q.CustomerId != null && q.Vouchers.Where(p => p.PromotionDetailID == 215).Count() < 3).ToList();
            Console.WriteLine(cards.Count());
            var promotionId = 117;
            var promotionDetail = _db.PromotionDetails.Where(q => q.PromotionDetailID == 215).FirstOrDefault();
            var time = DateTime.Now;
            foreach (var card in cards)
            {

                for (int i = 0; i < 3; i++)
                {
                    createVoucherNewBie(card, promotionId, promotionDetail);
                }

                //Save notification to db
                Notification notification = new Notification()
                {
                    Active = true,
                    CreateDate = time,
                    UpdateDate = time,
                    Title = "Quà tặng",
                    Opening = "Bạn được tặng 3 voucher Latte 50% cho lần nạp thẻ đầu tiên",
                    CustomerId = card.CustomerId,
                    Type = (int)NotificationsTypeEnum.Notify
                };
                _db.Notifications.Add(notification);

                _db.SaveChanges();
                //Create notification request
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(notificationUri);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", appAuthorization);
                //create notification message for android
                object data = new
                {
                    to = "/topics/" + card.CustomerId.ToString(),
                    priority = "high",
                    contentavailable = true,
                    notification = new
                    {
                        sound = "default",
                        badge = "1",
                        body = notification.Opening,
                        tittle = notification.Title
                    }
                };
                var json = JsonConvert.SerializeObject(data);
                json = json.ToString().Replace("\\r\\n", "");
                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                client.PostAsync("", stringContent);
                //send message for ios
                object data2 = new
                {
                    //to = "/topics/foo-bar",
                    to = "/topics/" + card.CustomerId.ToString() + "_ios",
                    priority = "high",
                    contentavailable = true,
                    notification = new
                    {
                        sound = "default",
                        badge = "1",
                        body = notification.Opening,
                        tittle = notification.Title
                    },
                    data = new
                    {
                        message = notification.Opening,
                        tittle = notification.Title
                    }
                };
                var json2 = JsonConvert.SerializeObject(data2);
                json2 = json2.ToString().Replace("\\r\\n", "");
                var stringContent2 = new StringContent(json2, UnicodeEncoding.UTF8, "application/json");
                client.PostAsync("", stringContent2);
            }
            Console.WriteLine("Success!");
            Console.ReadLine();
        }

        static void createVoucherNewBie(MembershipCard card, int promotionId, PromotionDetail promotionDetail)
        {
            //Create new voucher
            string seed = DateTime.Now.Minute + "" + DateTime.Now.Second + "" + Math.Abs((int)DateTime.Now.Ticks);

            string voucherCode = seed.Substring(0, 10);

            Voucher voucher = new Voucher();
            voucher.Active = true;
            voucher.isUsed = false;
            voucher.MembershipCardId = card.Id;
            voucher.MembershipCard = card;
            voucher.PromotionID = promotionId;
            voucher.PromotionDetailID = promotionDetail.PromotionDetailID;
            voucher.PromotionDetail = promotionDetail;
            voucher.Quantity = 1;
            voucher.VoucherCode = voucherCode;
            voucher.UsedQuantity = 0;
            voucher.IsGetted = true;

            _db.Vouchers.Add(voucher);

            
        }
    }
}
