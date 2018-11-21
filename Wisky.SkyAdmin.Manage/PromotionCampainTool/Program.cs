using HmsService.Models;
using HmsService.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SendNotificationTool
{
    class Program
    {
        private static HmsEntities _db = new HmsEntities();
        private static List<int> listMobileTypeId = ConstantManager.LIST_MOBILE_TYPE_ID;
        private static string notificationUri = ConstantManager.NOTIFICATION_URI;
        private static string appAuthorization = ConstantManager.APP_AUTHORIZATION;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool ReadConsoleW(IntPtr hConsoleInput, [Out] byte[]
           lpBuffer, uint nNumberOfCharsToRead, out uint lpNumberOfCharsRead,
           IntPtr lpReserved);

        public static IntPtr GetWin32InputHandle()
        {
            const int STD_INPUT_HANDLE = -10;
            IntPtr inHandle = GetStdHandle(STD_INPUT_HANDLE);
            return inHandle;
        }

        public static string ReadLine()
        {
            const int bufferSize = 1024;
            var buffer = new byte[bufferSize];

            uint charsRead = 0;

            ReadConsoleW(GetWin32InputHandle(), buffer, bufferSize, out charsRead, (IntPtr)0);
            // -2 to remove ending \n\r
            int nc = ((int)charsRead - 2) * 2;
            var b = new byte[nc];
            for (var i = 0; i < nc; i++)
                b[i] = buffer[i];

            var utf8enc = Encoding.UTF8;
            var unicodeenc = Encoding.Unicode;
            return utf8enc.GetString(Encoding.Convert(unicodeenc, utf8enc, b));
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Nhập tiêu đề cho thông báo: ");
            var tittle = ReadLine();
            Console.WriteLine("Nhập nội dung thông báo (liên tục không xuống dòng, ngắn gọn xúc tích): ");
            var message = ReadLine();
            Console.WriteLine("Xin bạn xem lại thông báo, (nhập Y để tiếp tục)");
            Console.WriteLine(tittle);
            Console.WriteLine(message);
            var check = Console.ReadLine().Trim().ToUpper();
            if (!check.Equals("Y"))
            {
                return;
            }
            Console.WriteLine("Thông báo này sẽ được gửi tới tất cả khách hàng sử dụng ứng dụng Passio, bạn có chắc chắn muốn gửi (nhập Y để gửi): ");
            var check2 = Console.ReadLine().Trim().ToUpper();
            if (!check2.Equals("Y")) {
                return;
            }
            //Check permission
            if (!(_db.AspNetUsers.Where(q => q.UserName == "NotiPermission").Any())) {
                Console.WriteLine("You dont have permission to send notification.");
                return;
            }

            //Save notification to db
            var time = DateTime.Now;
            Notification notification = new Notification()
            {
                Active = true,
                CreateDate = time,
                UpdateDate = time,
                Title = tittle,
                Opening = message,
                CustomerId = null,
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
                to = "/topics/PASSIO",
                //to = "/topics/18706",
                priority = "high",
                contentavailable = true,
                notification = new
                {
                    sound = "default",
                    badge = "1",
                    body = message,
                    tittle = tittle
                }
            };
            var json = JsonConvert.SerializeObject(data);
            json = json.ToString().Replace("\\r\\n", "");
            var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            var result = client.PostAsync("", stringContent).Result;
            //send message for ios
            object data2 = new
            {
                //to = "/topics/foo-bar",
                to = "/topics/ios",
                //to = "/topics/18706_ios",
                priority = "high",
                contentavailable = true,
                notification = new
                {
                    sound = "default",
                    badge = "1",
                    body = message,
                    tittle = tittle
                },
                data = new
                {
                    message = message,
                    tittle = tittle
                }
            };
            var json2 = JsonConvert.SerializeObject(data2);
            json2 = json2.ToString().Replace("\\r\\n", "");
            var stringContent2 = new StringContent(json2, UnicodeEncoding.UTF8, "application/json");
            client.PostAsync("", stringContent2);
            Console.ReadLine();
        }
    }
}
