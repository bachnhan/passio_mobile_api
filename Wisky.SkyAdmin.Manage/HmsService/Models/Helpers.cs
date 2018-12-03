using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models
{
    public static class ConstantManager
    {
        public const int STT_SUCCESS = 0;
        public const int STT_FAIL = 1;
        public const int STT_MISSING_PARAM = 2;
        public const int STT_UNAUTHORIZED = 3;
        public const int STATUS_SUCCESS = 200;
        public const int NUMBER_BLOG_POST = 20;
        public const string MES_LOGIN_SUCCESS = "Login successfully";
        public const string MES_LOGIN_FAIL = "Login fail";
        public const string MES_INVALID_LOGIN_ATTEMP = "Invalid login attempt.";
        public const string MES_WRONG_USERNAME_OR_PASSWORD = "Sai tên đăng nhập hoặc mật khẩu";
        public const string MES_NO_STORE__ASSIGNED_USERNAME = "Tài khoản của bạn chưa thuộc bất kỳ cửa hàng nào. Xin vui lòng liên hệ Quản trị viên để được giúp đỡ";
        public const string MES_NOTIFICATION_SUCCESS = "return notification success";
        public const string MES_NOTIFICATION_FAIL = "return notification fail";
        public const string MES_EDITACOUNTPHONE_FAIL = "Kết nối thất bại";
        public const string MES_EDITACOUNTPHONE_CUSTOMER_FAIL = "Could not find customer";
        public const string MES_CONNECTFB_VALID = "Mã định dạng sai";
        public const string MES_CONNECTFB_ISTHERE = "Không thể cập nhật. \nTài khoản đã được đăng ký!";
        public const string MES_CONNECTACCOUTNKIT_ISTHERE = "Không thể cập nhật. \nSố điện đã được đăng ký!";

        public const string MES_EDITACOUNTPHONE_SUCCESS = "Kết nối thành công";
        public const string MES_UPDATECUSTOMER_FAIL = " update account phone fail";
        public const string MES_UPDATE_SUCCESS = "Update successfully";
        public const string MES_CONNECTFB_FAIL = "Kết nối thất bại";
        public const string MES_UPDATE_FAIL = "Update fail";
        public const string MES_MISSING_PARAM = "Missing parameter";
        public const string MES_UNVALID_TOKEN = "Token is unvalid or expired";
        //check role khi xem store
        public const string MES_STORE_UNAUTHENTICATED = "You do not have permission to see report of this store";
        // check role khi nhan token
        public const string MES_ROLE_UNAUTHENTICATED = "You do not have permission";
        public const string MES_LOAD_REPORT_SUCCESS = "Load report success";
        public const string MES_FAIL = "Return fail";
        public const string MES_UPDATE_DELIVERYINFO = "Existed least 1 default delivery info";
        public const string MES_FULL_DELIVERY = "Full deliveryInfo";
        public const string MES_SUCCESS = "Return success";
        public const string ROLE_ADMIN = "Administrator";
        public const string ROLE_MANAGER = "StoreManager";
        
        public const string MES_PROMOTION_FAIL = "Voucher không hợp lệ";
        public const string MES_PROMOTION_DATE_FAIL = "Không áp dụng với khung giờ hiện tại";
        public const string MES_PROMOTION_SUCCESS = "Chúc mừng! Mã giảm giá hợp lệ";
        public const string MES_STORE_FAIL = "Don't have store";
        public const string MES_STORE_SUCCESS = "Success";
        public const string MES_PAYMENT_CASH = "Thanh toán bằng tiền mặt";
        public const string MES_PAYMENT_MOMO = "Thanh toán qua MomMo";
        public const string MES_PAYMENT_MEMBER_POINT = "Thanh toán bằng thẻ thành viên";
        public const string MES_CHECK_CUSTOMERID_FAIL = "Không tìm thấy khách hàng";
        public const string MES_CHECK_CUSTOMERDEVICE_FAIL = "Không thể cập nhật.";
        public const string MES_CHECK_CUSTOMERDEVICE_SUCCESS = "Cập nhật thành công token device.";
        public const string MES_CREATE_ORDER_FAIL = "Tạo đơn hàng không thành công";
        public const string MES_UPDATE_ACCOUNT_FAIL = "Tài khoản không đủ";

        public const string MES_CREATE_ORDER_SUCCESS = "Tạo đơn hàng thành công";
        public const string MES_CREATE_PAYMENT_MOMO_SUCCESS = "Thanh toán với momo thành công";
        public const string MES_CREATE_PAYMENT_MOMO_FAIL = "Thanh toán với momo thất bại";
        public const string MES_BLOCK_ORDER_BY_MEMBER_PAYMENT = "Thanh toán này tạm thời khóa";
        public const string PRIVATE_KEY = "WiskySRKey";
        public const string PREFIX_MOBILE = "MB";
        public const int MAX_RECORD = 25;
        public const string DELIVERY = "DELIVERY";
        public const int MAX_DELIVERYINFO = 3;
        public const string PRODUCT_CATEGORY_EVENT = "EVENT";
        public const string PRODUCT_CATEGORY_OTHER = "KHÁC";
        public const string PRODUCT_CATEGORY_ITALI = "ITALI PANINI";
        public const string PRODUCT_CATEGORY_FRESH_JUICE = "FRESH JUICE";
        public const string PRODUCT_CATEGORY_CAKE = "FRANK CAKE";
        public const string PRODUCT_CATEGORY_COMBO = "COMBO";
        public const string PRODUCT_CATEGORY_SPARKLING = "SPARKLING";
        public const string PRODUCT_CATEGORY_PASSIO_COFFEE = "PASSIO COFFEE";
        public const string PRODUCT_CATEGORY_FRESH_AND_EASY = "FRESH & EASY";
        public const string IMAGE_SERVER_PATH = "D:\\PublicIIS\\MobileAPI\\ServerImage";
        public const string IMAGE_SERVER_URL = "http://115.165.166.32:30779/ServerImage";
        public const string IMAGE_FORMAT_EXTENSION = ".Png";
        public const string MESS_ORDER_NEW = "Đang chờ confirm";
        public const string FORMART_DATETIME = "yyyy-MM-ddTHH:mm";
        public const string FORMART_DATETIME_2 = "dd/MM/yyyy";
        public const string FORMART_DATETIME_3 = "MM-dd-yyyy";
        public const string PARTNERCODEMOMO = "MOMOIQA420180417";
        public const int CALLCENTER_ID = 3;
        public const double DELIVERY_FREE = 50000;
        public const double DISCOUNT_VOUCHER = 100000;
        public const string MES_CHECK_VOUCHER_RULE_1 = "Áp dụng cho đơn hàng trên ";
        public const string MES_CHECK_VOUCHER_RULE_2 = "Áp dụng cho đơn hàng khi mua ";
        public const int PROMOTION_RULE_1 = 1;//rule min order
        public const int PROMOTION_RULE_2 = 2;//rule discount each product
        public const double MEMBER_POINT_AMOUNT = 10000;
        public const string NOTIFICATION_URI = "http://fcm.googleapis.com/fcm/send";
        public static string APP_AUTHORIZATION = "key=AIzaSyD6dQmAfR1Zl6vwhe34poCnviuyPGhEcak";
        public const string MES_CHECK_PERMISSION_FAIL = "Người dùng chưa được cấp quyền để xem báo cáo này";
        public static List<int> LIST_MOBILE_TYPE_ID = new List<int>{ (int)MembershipCardTypeEnum.Green, (int)MembershipCardTypeEnum.Silver, (int)MembershipCardTypeEnum.Gold, (int)MembershipCardTypeEnum.Platinum };

        //Travel Api mobile error message
        public const string MES_CONFIRM_PASSWORD_NOT_MATCH = "Confirm password is not matched";
        public const string MES_USER_EXISTED = "User existed";
        public const string MES_EMAIL_EXISTED = "User existed";
    }
}
