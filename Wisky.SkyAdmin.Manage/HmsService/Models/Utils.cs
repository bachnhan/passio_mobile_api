using AutoMapper;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Repositories;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Data;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using COMExcel = Microsoft.Office.Interop.Excel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.Configuration;

namespace HmsService.Models
{
    public static class Utils
    {
        private static readonly string UrlWebApi = System.Configuration.ConfigurationManager.AppSettings["urlWebApi.Delivery"];
        public const string SysAdminRole = "SysAdmin";
        public const string AdminRole = "Administrator";

        public const string AdminAuthorizeRoles = "Administrator,SysAdmin";
        public const string SysAdminAuthorizeRoles = "SystemAdmin";
        

        public static bool HasRequiredAttribute(this PropertyInfo property)
        {
            return property.IsDefined(typeof(RequiredAttribute), true);
        }
        public static string ResolveServerUrl(string serverUrl, bool forceHttps)
        {
            if (serverUrl.IndexOf("://") > -1)
                return serverUrl;

            string newUrl = serverUrl;
            Uri originalUri = System.Web.HttpContext.Current.Request.Url;
            newUrl = (forceHttps ? "https" : originalUri.Scheme) +
                "://" + originalUri.Authority + newUrl;
            return newUrl;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lon1"></param>
        /// <param name="lat2"></param>
        /// <param name="lon2"></param>
        /// <param name="unit">'M' is statute miles (default)
        /// 'K' is kilometers
        /// 'N' is nautical miles 
        /// </param>
        /// <returns></returns>
        public static double distance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515;
            if (unit == 'K')
            {
                dist = dist * 1.609344;
            }
            else if (unit == 'N')
            {
                dist = dist * 0.8684;
            }
            return (dist);
        }

        public static bool IsDigitsOnly(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return false;
            }
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        public static double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
        public static MvcHtmlString RenderHtmlAttributes(KeyValuePair<string, string>[] values)
        {
            if (values == null)
            {
                return null;
            }

            var result = new StringBuilder();

            foreach (var value in values)
            {
                result.AppendFormat("{0}=\"{1}\"", value.Key, value.Value);
            }

            return new MvcHtmlString(result.ToString());
        }

        public static MvcHtmlString RenderHtmlAttributes(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            var result = new StringBuilder();

            foreach (var property in properties)
            {
                result.AppendFormat("{0}=\"{1}\"", property.Name, property.GetValue(obj));
            }

            return new MvcHtmlString(result.ToString());
        }

        public static void SetMessage(this Controller controller, string message)
        {
            controller.ViewData["Message"] = message;
        }

        public static string ToErrorsString(this DbEntityValidationException ex)
        {
            return string.Join(Environment.NewLine, ex.EntityValidationErrors.SelectMany(q => q.ValidationErrors.Select(p => p.ErrorMessage)));
        }

        public static DateTime GetEndOfDate(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, 23, 59, 59);
        }

        public static DateTime GetStartOfDate(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0);
        }
        public static DateTime GetStartOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1, 0, 0, 0);
        }
        public static DateTime GetEndOfMonth(this DateTime value)
        {
            DateTime firstDayOfTheMonth = new DateTime(value.Year, value.Month, 1);
            return firstDayOfTheMonth.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
        }


        public static TDest ToExactType<TSource, TDest>(this TSource source)
            where TDest : class, new()
        {
            var result = new TDest();
            DependencyUtils.Resolve<IMapper>().Map(source, result);

            return result;
        }

        public static async Task<string> CreateOrUpdateLanguageValue<TEntity>(this IBaseService<TEntity> service, string keyName, string value, int languageId, int storeId)
            where TEntity : class, IEntity
        {
            Expression<Func<LanguageKey, bool>> languageKeyExpression = q => q.Name == keyName && q.StoreId == storeId;

            var languageKeyRepo = DependencyUtils.Resolve<ILanguageKeyRepository>();
            var languageValueRepo = DependencyUtils.Resolve<ILanguageValueRepository>();

            var languageKey = await languageKeyRepo.FirstOrDefaultActiveAsync(languageKeyExpression);

            // Create if there is no language key exist yet
            if (languageKey == null)
            {
                // Generate keyname if needed
                if (string.IsNullOrEmpty(keyName))
                {
                    do
                    {
                        keyName = Guid.NewGuid().ToString();
                    } while ((await languageKeyRepo.FirstOrDefaultActiveAsync(languageKeyExpression)) != null);
                }

                languageKey = new LanguageKey()
                {
                    Name = keyName,
                    StoreId = storeId,
                    Active = true,
                };
                languageKeyRepo.Add(languageKey);
            }

            // Create/update value
            var languageValue = await languageValueRepo.FirstOrDefaultActiveAsync(q => q.LanguageKey.Name == keyName && q.LanguageId == languageId);
            if (languageValue == null)
            {
                languageValue = new LanguageValue()
                {
                    LanguageId = languageId,
                    LanguageKey = languageKey,
                    Active = true,
                };
                languageValueRepo.Add(languageValue);
            }
            languageValue.Value = value;

            await service.SaveAsync();
            return keyName;
        }

        /// <summary>
        /// Using this method to get verify if a string contains another string (case and accent insensitive)
        /// </summary>
        /// <returns></returns>
        public static bool CustomContains(string source, string toCheck)
        {
            if (string.IsNullOrWhiteSpace(source))
                return false;
            CompareInfo ci = new CultureInfo("en-US").CompareInfo;
            CompareOptions co = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
            return ci.IndexOf(source, toCheck, co) != -1;
        }

        /// <summary>
        /// using this method to get DateTime.Now
        /// </summary>
        /// <returns></returns>
        public static DateTime GetCurrentDateTime()
        {

            #region Get DateTime.Now
            //Get time UTC 
            DateTime utcNow = DateTime.UtcNow;
            //Parse UTC to time SE Asia
            DateTime datetimeNow = TimeZoneInfo.ConvertTime(utcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            #endregion

            return datetimeNow;
        }

        /// <summary>
        /// using this to convert string to dd/mm/yyyy
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string datetime)
        {
            try
            {
                return DateTime.ParseExact(datetime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new Exception("Chuỗi ngày tháng không đúng định dạng");
            }
        }
        public static DateTime ToHourTime(this string datetime)
        {
            try
            {
                return DateTime.ParseExact(datetime, "HH:mm", CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new Exception("Chuỗi giờ không đúng định dạng");
            }
        }
        public static DateTime ToDateTime2(this string datetime)
        {
            try
            {
                return DateTime.ParseExact(datetime, "MM//dd//yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new Exception("Chuỗi ngày tháng không đúng định dạng");
            }
        }

        public static DateTime ToDateTimeHour(this string datetime)
        {
            try
            {
                return DateTime.ParseExact(datetime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

            }
            catch
            {
                throw new Exception("Chuỗi ngày tháng không đúng định dạng");
            }
        }
        public static DateTime ToDateTimeHourSeconds(this string datetime)
        {
            try
            {
                return DateTime.ParseExact(datetime, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            }
            catch
            {
                throw new Exception("Chuỗi ngày tháng không đúng định dạng");
            }
        }

        public static DateTime ToDateTimeHour2(this string datetime)
        {
            try
            {
                return DateTime.ParseExact(datetime, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);


            }
            catch
            {
                throw new Exception("Chuỗi ngày tháng không đúng định dạng");



            }
        }

        public static string ToMoney(double money)
        {
            return string.Format("{0:n0}", money);
        }



        public static string GetEnumDescription(Enum en)
        {
            Type type = en.GetType();

            MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return en.ToString();
        }

        public static bool ExportToExcel(List<string> headers, IEnumerable<object> _list, string fileName)
        {
            // Khởi động chtr Excel
            COMExcel.Application exApp = new COMExcel.Application();

            // Thêm file temp xls
            COMExcel.Workbook exBook = exApp.Workbooks.Add(
                      COMExcel.XlWBATemplate.xlWBATWorksheet);

            // Lấy sheet 1.
            COMExcel.Worksheet exSheet = (COMExcel.Worksheet)exBook.Worksheets[1];
            COMExcel.Range r = (COMExcel.Range)exSheet.Cells[1, 1];

            // header.Add("#;1;2;r");
            // i represents for column
            int maxRow = 2;
            var col = 1;

            #region Add header
            for (int i = 0; i < headers.Count; i++)
            {
                var header = headers[i];
                string[] items = header.Split(';');

                var value = items[0];
                var row = Int32.Parse(items[1]);
                var range = Int32.Parse(items[2]);

                if (maxRow < row + 1)
                {
                    maxRow = row + 1;
                }

                r = (COMExcel.Range)exSheet.Cells[row, col];

                if (range < 2)
                {
                    r.Value2 = items[0];

                }
                else
                {
                    var type = items[3];

                    //merge column
                    if (type.Equals("c"))
                    {
                        var mergedCell = (COMExcel.Range)exSheet.Range[r, exSheet.Cells[row, col + range - 1]].Merge();
                        r.Value2 = value;
                        col--;
                    }
                    // merge row
                    else
                    {
                        var mergedCell = (COMExcel.Range)exSheet.Range[r, exSheet.Cells[range, col]].Merge();
                        r.Value2 = value;
                    }
                }

                col++;
            }
            #endregion

            //#region Add value to table
            var list = _list.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var type = item.GetType();
                PropertyInfo[] properties;

                properties = type.GetProperties();
                r = (COMExcel.Range)exSheet.Cells[maxRow + i, 1];
                r.Value2 = i + 1;
                for (int j = 0; j < properties.Length; j++)
                {
                    var property = properties[j];
                    r = (COMExcel.Range)exSheet.Cells[maxRow + i, j + 2];
                    r.Value2 = property.GetValue(item, null);
                }
            }
            //#endregion

            #region fit all column
            COMExcel.Range usedrange = exSheet.UsedRange; // detect all col were used (column whic has value)
            //usedrange.Column.autofit();
            usedrange.Columns.AutoFit();
            #endregion

            #region save file to local disk
            var issuccess = true;
            try
            {
                exBook.SaveAs(fileName, COMExcel.XlFileFormat.xlWorkbookNormal,
                             null, null, false, false,
                            COMExcel.XlSaveAsAccessMode.xlExclusive,
                            false, false, false, false, false);


                //folderBrowserDialog1.ShowDialog();
                //System.Windows
                //if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
                //    && saveFileDialog1.FileName.Length > 0)
                //{
                //    System.Windows.Input.
                //    richTextBox1.SaveFile(saveFileDialog1.FileName,
                //        RichTextBoxStreamType.PlainText);
                //}
            }
            catch (Exception e)
            {
                //message = e.message.tostring();
                issuccess = false;
            }
            finally
            {
                exApp.Quit();
            }

            return issuccess;
        }
        #endregion

        public static int GetBrandId()
        {
            if (HttpContext.Current.Session["BrandId"] == null)
            {
                var _db = new HmsEntities();

                var Username = HttpContext.Current.User.Identity.Name;
                AspNetUser user = _db.AspNetUsers.FirstOrDefault(u => u.UserName.Equals(Username));

                //var Brand = _publisherUserService.GetPublisherUserByUserId(User.Id).FirstOrDefault().Brand;
                if (user != null)
                {
                    var currentUser = _db.AspNetUsers.FirstOrDefault(p => p.Id == user.Id);
                    if (currentUser != null)
                    {
                        HttpContext.Current.Session["BrandId"] = currentUser.BrandId;

                    }
                }

            }
            return HttpContext.Current.Session["BrandId"] == null ? -1 : (int)HttpContext.Current.Session["BrandId"];
        }

        public static string getBrandName(int brandId)
        {
            var brandApi = new BrandApi();
            var brandName = brandApi.Get(brandId).BrandName;
            return brandName;
        }

        public static Store GetStore()
        {
            #region Code cũ
            //var _db = new HmsEntities();

            //var Username = HttpContext.Current.User.Identity.Name;
            //AspNetUser user = _db.AspNetUsers.FirstOrDefault(u => u.UserName.Equals(Username));
            //var result = new Store();
            ////var Brand = _publisherUserService.GetPublisherUserByUserId(User.Id).FirstOrDefault().Brand;
            //if (user != null)
            //{
            //    var storeUser = _db.StoreUsers.FirstOrDefault(q => q.Username == user.UserName && q.Store.isAvailable.Value);
            //    if (storeUser != null)
            //    {
            //        result = storeUser.Store;
            //    }
            //} 
            #endregion

            var storeUserApi = new StoreUserApi();
            var storeApi = new StoreApi();
            var aspNetUserApi = new AspNetUserApi();

            var Username = HttpContext.Current.User.Identity.Name;
            var user = aspNetUserApi.GetUserByUsername(Username);
            var storeUsers = storeUserApi.GetStoresFromUser(Username);

            StoreViewModel firstStore = new StoreViewModel();
            foreach (var item in storeUsers)
            {
                var store = storeApi.Get(item.StoreId);
                if ((store.Type == 5 || store.Type == 6 || store.Type == 7) && store.BrandId == user.BrandId)
                {
                    firstStore = store;
                    break;
                }
            }

            return firstStore.ToEntity();
        }

        public static Store GetStore(int storeId)
        {
            var storeApi = new StoreApi();
            var store = storeApi.Get(storeId).ToEntity();
            return store;
        }
        public static IEnumerable<int> GetStoreId(int brandId)
        {
            if (HttpContext.Current.Session["StoreId"] == null)
            {
                var _db = new HmsEntities();
                //get store
                var api = new StoreApi();

                var store = api.GetActiveStoreByBrandId(brandId);

                //var Username = HttpContext.Current.User.Identity.Name;
                //AspNetUser user = _db.AspNetUsers.FirstOrDefault(u => u.UserName.Equals(Username));

                //var Brand = _publisherUserService.GetPublisherUserByUserId(User.Id).FirstOrDefault().Brand;
                if (store != null)
                {
                    HttpContext.Current.Session["StoreId"] = store.Select(q => q.ID).ToList();
                }
                else
                {
                    HttpContext.Current.Session["StoreId"] = null;
                }
            }
            return HttpContext.Current.Session["StoreId"] == null ? null : (IEnumerable<int>)HttpContext.Current.Session["StoreId"];
        }

        public static string DisplayName(this Enum value)
        {
            try
            {
                Type enumType = value.GetType();
                var enumValue = Enum.GetName(enumType, value);
                MemberInfo member = enumType.GetMember(enumValue)[0];

                var attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);
                var outString = ((DisplayAttribute)attrs[0]).Name;

                if (((DisplayAttribute)attrs[0]).ResourceType != null)
                {
                    outString = ((DisplayAttribute)attrs[0]).GetName();
                }

                return outString;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public static async Task RequestNotiMessage(NotifyMessage msg)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlWebApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsync("PostMessageApi/PostNotiMessage", new StringContent(
                    new JavaScriptSerializer().Serialize(msg), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    Uri gizmoUrl = response.Headers.Location;
                }
            }
        }

        public static async Task PostNotiMessageToStores(IEnumerable<StoreViewModel> stores, int messageType)
        {
            string message = "";
            switch (messageType)
            {
                case (int)NotifyMessageType.AccountChange:
                    message = "Danh sách tài khoản vừa được cập nhật";
                    break;
                case (int)NotifyMessageType.CategoryChange:
                    message = "Danh sách loại sản phẩm vừa được cập nhật";
                    break;
                case (int)NotifyMessageType.ProductChange:
                    message = "Danh sách sản phẩm vừa được cập nhật";
                    break;
                case (int)NotifyMessageType.PromotionChange:
                    message = "Danh sách khuyến mãi vừa được cập nhật";
                    break;
                case (int)NotifyMessageType.NoThing:
                default:
                    message = "Nothing";
                    return;
            }
            foreach (var store in stores)
            {
                var msg = new NotifyMessage
                {
                    StoreId = store.ID,
                    StoreName = store.Name,
                    NotifyType = messageType,
                    Content = message
                };
                await RequestNotiMessage(msg);
            }
        }

        public static async Task RequestOrderWebApi(NotifyOrder msg)
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri(UrlWebApi); // UrlWebApi bị NULL <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //HTTP POST
                //HttpResponseMessage response = await client.PostAsJsonAsync("OrderApi/NotifyOrderToPosJson", msg);
                //if (response.IsSuccessStatusCode)
                //{
                //    Uri gizmoUrl = response.Headers.Location;
                //}

                HttpResponseMessage response = await client.PostAsync("OrderApi/NotifyOrderToPosJson", new StringContent(
                    new JavaScriptSerializer().Serialize(msg), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    Uri gizmoUrl = response.Headers.Location;
                }
            }
        }
        public static string GenerateInvoiceCode()
        {
            DateTime dt = new DateTime(2016, 1, 1);
            TimeSpan ts = DateTime.Now - dt;

            string code = ShortCodes.LongToShortCode((long)ts.TotalMilliseconds / 10);//1/10s
            return code;
        }

        //TO DO: Convert datetime to millisoconds
        //public static string ConvertDatetime ()
        //{
        //    DateTime[] dates;
        //    var minDates = dates.Min();
        //    var msDates = dates.Select(dates => (dates - minDates).TotalMilliseconds).ToArray();
        //    long dateticks = DateTime.Now.Ticks;
        //    long datemilliseconds = dateticks/100000;
        //    return datemilliseconds.ToString();
        //}
        //TO DO: Convert DateTime to Millisecond
        public static double ConvertDatetimes(DateTime start, DateTime end)
        {
            double milliseconds = (end - start).TotalMilliseconds;
            return milliseconds;
        }

        public static bool SendSMS(string phone, string message, int brandId)
        {
            try
            {
                var brandApi = new BrandApi();
                var brand = brandApi.Get(brandId);
                if (brand != null)
                {
                    if (!String.IsNullOrEmpty(brand.ApiSMSKey))
                    {
                        var APIKey = brand.ApiSMSKey;
                        var SecretKey = brand.SecurityApiSMSKey;
                        int SmsType = brand.SMSType.Value;//Loại tin nhắn 1: Brandname Quảng cáo, 2: Brandname CSKH :6 đầu số cố định 8755    
                        var BrandName = brand.BrandNameSMS;
                        var url = "";
                        if (SmsType == 2 || SmsType == 1)
                        {
                            url = "http://api.esms.vn/MainService.svc/xml/SendMultipleSMSBrandname/";
                        }
                        else
                        {
                            url = "http://api.esms.vn/MainService.svc/xml/SendMultipleMessage_V2/";
                        }

                        // declare ascii encoding
                        UTF8Encoding encoding = new UTF8Encoding();

                        string strResult = string.Empty;

                        string customers = "";

                        string[] lstPhone = phone.Split(',');

                        for (int i = 0; i < lstPhone.Count(); i++)
                        {
                            customers = customers + @"<CUSTOMER>"
                                            + "<PHONE>" + lstPhone[i] + "</PHONE>"
                                            + "</CUSTOMER>";
                        }
                        string SampleXml = "";
                        if (SmsType == 2 || SmsType == 1)
                        {
                            SampleXml = @"<RQST>"
                                          + "<APIKEY>" + APIKey + "</APIKEY>"
                                          + "<SECRETKEY>" + SecretKey + "</SECRETKEY>"
                                          + "<BRANDNAME>" + BrandName + "</BRANDNAME>"
                                          + "<SMSTYPE>" + SmsType + "</SMSTYPE>"//SMSTYPE 1: Brandname Quảng cáo, 2: Brandname CSKH
                                          + "<CONTENT>" + message + "</CONTENT>"
                                          + "<CONTACTS>" + customers + "</CONTACTS>"
                                          + "</RQST>";
                        }
                        else
                        {
                            SampleXml = @"<RQST>"
                                               + "<APIKEY>" + APIKey + "</APIKEY>"
                                               + "<SECRETKEY>" + SecretKey + "</SECRETKEY>"
                                               + "<ISFLASH>0</ISFLASH>"
                                               + "<SMSTYPE>" + SmsType + "</SMSTYPE>"//SMSTYPE 1: Brandname Quảng cáo, 2: Brandname CSKH
                                               + "<CONTENT>" + message + "</CONTENT>"
                                               + "<CONTACTS>" + customers + "</CONTACTS>"
                                               + "</RQST>";

                        }

                        if (SampleXml != "")
                        {
                            string postData = SampleXml;

                            // convert xmlstring to byte using ascii encoding
                            byte[] data = encoding.GetBytes(postData);

                            // declare httpwebrequet wrt url defined above
                            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
                            // set method as post
                            webrequest.Method = "POST";
                            webrequest.Timeout = 500000;
                            // set content type
                            webrequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                            // set content length
                            webrequest.ContentLength = data.Length;
                            // get stream data out of webrequest object
                            Stream newStream = webrequest.GetRequestStream();
                            newStream.Write(data, 0, data.Length);
                            newStream.Close();
                            // declare & read response from service
                            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();

                            // set utf8 encoding
                            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
                            // read response stream from response object
                            StreamReader loResponseStream =
                                new StreamReader(webresponse.GetResponseStream(), enc);
                            // read string from stream data
                            strResult = loResponseStream.ReadToEnd();
                            // close the stream object
                            loResponseStream.Close();
                            // close the response object
                            webresponse.Close();
                            // below steps remove unwanted data from response string
                            strResult = strResult.Replace("</string>", "");
                            if (webresponse.StatusCode == HttpStatusCode.OK || webresponse.StatusDescription.Equals("OK"))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }


        public static class ShortCodes
        {
            private static Random rand = new Random();

            // You may change the "shortcode_Keyspace" variable to contain as many or as few characters as you
            // please.  The more characters that aer included in the "shortcode_Keyspace" constant, the shorter
            // the codes you can produce for a given long.
            const string shortcode_Keyspace = "0123456789abcdefghijklmnopqrstuvwxyz";

            // Arbitrary constant for the maximum length of ShortCodes generated by the application.
            const int shortcode_maxLen = 12;


            public static string LongToShortCode(long number)
            {
                int ks_len = shortcode_Keyspace.Length;
                string sc_result = "";
                long num_to_encode = number;
                long i = 0;
                do
                {
                    i++;
                    sc_result = shortcode_Keyspace[(int)(num_to_encode % ks_len)] + sc_result;
                    num_to_encode = ((num_to_encode - (num_to_encode % ks_len)) / ks_len);
                }
                while (num_to_encode != 0);
                return sc_result;
            }

         

            public static long ShortCodeToLong(string shortcode)
            {
                int ks_len = shortcode_Keyspace.Length;
                long sc_result = 0;
                int sc_length = shortcode.Length;
                string code_to_decode = shortcode;
                for (int i = 0; i < code_to_decode.Length; i++)
                {
                    sc_length--;
                    char code_char = code_to_decode[i];
                    sc_result += shortcode_Keyspace.IndexOf(code_char) * (long)(Math.Pow((double)ks_len, (double)sc_length));
                }
                return sc_result;
            }
        }
    }
}
