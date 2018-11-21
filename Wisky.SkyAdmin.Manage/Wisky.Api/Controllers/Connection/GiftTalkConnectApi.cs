using HmsService.Models;
using HmsService.Sdk;
using Newtonsoft.Json;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace Wisky.Api.Connection
{
    public class GiftTalkConnectApi : IConnectAPI
    {
        int paymentPartnerCode = 22;
        public ResultObj ThirdPartyCardDetail(string barCode, int brandID, int storeID)
        {
            ResultObj result = new ResultObj();
            var api = new PaymentPartnerApi();
            var token = api.Get().Select(q => q.Att1).FirstOrDefault();
            var url = api.Get().Select(q => q.Att2).FirstOrDefault();
            var Mapppingapi = new SystemPartnerMappingApi();
            var config = Mapppingapi.Get().Where(q => q.BrandID == brandID && q.StoreID == storeID
                                && q.PartnerID == paymentPartnerCode).Select(r => new
                                {
                                    merchant = r.Att1,
                                    merchant_id = r.Att2,
                                    money_type = r.Att3,
                                    succesStatusCode = r.Att4
                                }).FirstOrDefault();
            var merchant = config.merchant;
            var money_type = config.money_type;
            var succesStatusCode = Int32.Parse(config.succesStatusCode);
            FormUrlEncodedContent stringContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("money_type", money_type ),
                new KeyValuePair<string, string>("account", barCode),
                new KeyValuePair<string, string>("token", token),
                new KeyValuePair<string, string>("merchant", merchant)
                });
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = stringContent;
            try
            {
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var reponseData = response.Content.ReadAsAsync<JsonRes>().Result;
                    if (reponseData.Status == succesStatusCode)
                    {
                        result.Enum = (int)PaymentTypeEnum.GiftTalk;
                        result.success = true;
                        result.message = "Thẻ GiftTalk có thể sử dụng.";
                        result.result = reponseData.Data;
                        return result;
                    }
                    result.success = false;
                    result.message = "Không phải thẻ GiftTalk.";
                    return result;
                }
                result.success = false;
                result.message = "Không thể kết nối.";
                return result;
            }
            catch (Exception e)
            {
                result.success = false;
                result.message = "Không thể kết nối.";
                return result;
            }
        }
        public ResultObj Payment(string barCode, int brandID, int storeID,decimal amount,string detail)
        {
            ResultObj result = new ResultObj();
            DetailObj detailObj = JsonConvert.DeserializeObject<DetailObj>(detail);
            var api = new PaymentPartnerApi();
            var token = api.Get().Select(q => q.Att1).FirstOrDefault();
            var url = api.Get().Select(q => q.Att3).FirstOrDefault();
            var Mapppingapi = new SystemPartnerMappingApi();
            var config = Mapppingapi.Get().Where(q => q.BrandID == brandID && q.StoreID == storeID
                                && q.PartnerID == paymentPartnerCode).Select(r => new
                                {
                                    merchant = r.Att1,
                                    merchant_id = r.Att2,
                                    money_type = r.Att3,
                                    succesStatusCode = r.Att4
                                }).FirstOrDefault();
            var merchant = config.merchant;
            var merchant_id = config.merchant_id;
            var money_type = config.money_type;
            var succesStatusCode = Int32.Parse(config.succesStatusCode);
            FormUrlEncodedContent stringContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("money_type", money_type),
                new KeyValuePair<string, string>("account", barCode),
                new KeyValuePair<string, string>("token", token),
                new KeyValuePair<string, string>("merchant", merchant),
                new KeyValuePair<string, string>("merchant_id", merchant_id),
                new KeyValuePair<string, string>("amount", amount.ToString()),
                new KeyValuePair<string, string>("store_id", storeID.ToString()),
                new KeyValuePair<string, string>("table_number", detailObj.table_number.ToString()),
                new KeyValuePair<string, string>("bill_id", detailObj.bill_id.ToString()),
                new KeyValuePair<string, string>("customer", detailObj.Customer),
                new KeyValuePair<string, string>("check_number", ""),
                new KeyValuePair<string, string>("description", "Khách hàng thanh toán thông qua POS")
                });
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = stringContent;
            try
            {
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var reponseData = response.Content.ReadAsAsync<JsonRes>().Result;
                    if (reponseData.Status == succesStatusCode)
                    {
                        result.Enum = (int)PaymentTypeEnum.GiftTalk;
                        result.success = true;
                        result.message = "Giao dịch thành công.";
                        return result;
                    }
                    result.Enum = (int)PaymentTypeEnum.GiftTalk;
                    result.success = false;
                    result.message = "Giao dịch thất bại.";
                    return result;
                }
                result.Enum = (int)PaymentTypeEnum.GiftTalk;
                result.success = false;
                result.message = "Không thể kết nối.";
                return result;
            }
            catch (Exception e)
            {
                result.Enum = (int)PaymentTypeEnum.GiftTalk;
                result.success = false;
                result.message = "Không thể kết nối.";
                return result;
            }
        }
    }

    class JsonRes
    {
        [JsonProperty(PropertyName = "data")]
        public GiftCardDetail Data { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "Status")]
        public int Status { get; set; }
    }
    public class GiftCardDetail
    {
        public string account { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public decimal balance { get; set; }
    }
    public class ResultObj
    {
        public Boolean success { get; set; }
        public string message { get; set; }
        public int Enum { get; set; }
        public GiftCardDetail result { get; set; }
    }
    public class DetailObj
    {
        public string table_number { get; set; }
        public string bill_id { get; set; }
        public string Customer { get; set; }
    }
}