using Autofac;
using HmsService.Models;
using HmsService.Sdk;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace SmileCardUpdateBarCode
{
    public class ApiSmileCard
    {
        private static readonly string UrlWebApi = System.Configuration.ConfigurationManager.AppSettings["serverApiUri"];

        public static bool SendRequestSmileCard()
        {
            try
            {
                var dateTime = DateTime.Now;
                var startTime = dateTime.GetStartOfDate();
                var endTime = dateTime.GetEndOfDate();
                var orderApi = new OrderApi();
                var rents = orderApi.GetAllOrderByDate(startTime, endTime, 1)//Gán cứng là 1 cho Passio
                    .Where(a => a.OrderType != (int)OrderTypeEnum.DropProduct && a.OrderStatus == (int)OrderStatusEnum.Finish && a.Att1 != null).ToList();

                foreach (var item in rents)
                {
                    if (item.Att2 == null || item.Att2 == StatusBarCode.Failed.ToString())
                    {
                        //Get info Store
                        var storeInfo = StoreInfo.GetStoreInfo((int)item.StoreID);

                        var codeSplit = item.Att1.Split(':');

                        var barCode = codeSplit[1];
                        //Kiểm tra xem có gửi được API?
                        bool check = CreateRequest(barCode, item.FinalAmount, item.CheckInDate, item.InvoiceID, storeInfo);
                        //True => Save Att2 => success
                        if (check)
                        {
                            item.Att2 = StatusBarCode.Success.ToString();
                            orderApi.EditOrder(item);
                        }
                        else
                        {
                            item.Att2 = StatusBarCode.Failed.ToString();
                            orderApi.EditOrder(item);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public static bool CreateRequest(string barCode, double finalAmount, DateTime? checkInDate, string orderCode, SmileCardModel storeInfo)
        {
            try
            {
                string input61 = finalAmount.ToString();
                string string0 = "";
                for (int i = input61.Length; i < 12; i++)
                {

                    string0 += "0";
                    if (i == 11)
                    {
                        input61 = string0 + input61;
                    }
                }

                int lenghtBarcode = barCode.Length;

                barCode = barCode.Remove(lenghtBarcode - 1);


                string xmlTest =
               @"<?xml version=""1.0"" encoding=""utf-8""?>
           <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
            <soap:Body>
            <excuteTerminalQuery xmlns=""http://tempuri.org/"">
             <input>"
                 + "<string>0:7038010000C0008C</string>"
                  //+ "<string>2:6019530094511435</string>"
                  + "<string>2:6019" + barCode + "</string>"
                  + "<string>3:" + storeInfo.ProcessingCode + "</string>"
                  + "<string>4:" + finalAmount.ToString() + "</string>"
                  + "<string>11:225</string>"
                  + "<string>12:" + checkInDate.Value.ToString("hhmmss") + "</string>"
                  + "<string>13:" + checkInDate.Value.ToString("MMdd") + "</string>"
                  + "<string>24:559</string>"
                  + "<string>41:" + storeInfo.TerminalId + "</string>"
                  + "<string>42:" + storeInfo.MerchantId + "</string>"
                  + "<string>57:" + storeInfo.StaffId + "</string>"
                  + "<string>61:" + input61 + " = " + storeInfo.DiscountPercent + "=" + storeInfo.Saving + "#" + "</string>"
                  + "<string>62:" + orderCode + "</string>"
          + "</input>"
       + " </excuteTerminalQuery>"
      + "</soap:Body>"
    + "</soap:Envelope>";

                HttpWebRequest request = CreateWebRequest();
                if (request != null)
                {
                    XmlDocument soapEnvelopeXml = new XmlDocument();
                    soapEnvelopeXml.LoadXml(xmlTest);

                    using (Stream stream = request.GetRequestStream())
                    {
                        soapEnvelopeXml.Save(stream);
                    }
                    using (WebResponse response = request.GetResponse())
                    {
                        using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                        {
                            string soapResult = rd.ReadToEnd();
                            Console.WriteLine(soapResult);
                        }
                    }
                    return true;
                }
                else
                {
                    return false;//Để không bị crash khi gọi API không được 
                }
            }
            catch (Exception ex)
            {
                return false;//Để không bị crash khi gọi API không được
            }

        }

        public static HttpWebRequest CreateWebRequest()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(UrlWebApi);
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

    }
}

