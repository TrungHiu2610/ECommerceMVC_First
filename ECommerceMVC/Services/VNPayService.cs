using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using System.Configuration;

namespace ECommerceMVC.Services
{
    public class VNPayService : IVNPayServices
    {
        private readonly IConfiguration _config;

        public VNPayService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateRequestUrl(HttpContext context, VNPaymentRequestModel model)
        {
            // tạo các variables cho response
            var tick = DateTime.Now.Ticks.ToString();
            VnPayLibrary vnpay = new VnPayLibrary();
            // cac variables da khai bao trong appsettings
            vnpay.AddRequestData("vnp_Version", _config["VNPay:vnp_Version"]);
            vnpay.AddRequestData("vnp_Command", _config["VNPay:vnp_Command"]);
            vnpay.AddRequestData("vnp_TmnCode", _config["VNPay:vnp_TmnCode"]);
            vnpay.AddRequestData("vnp_CurrCode", _config["VNPay:vnp_CurrCode"]);
            vnpay.AddRequestData("vnp_Locale", _config["VNPay:vnp_Locale"]);
            vnpay.AddRequestData("vnp_ReturnUrl", _config["VNPay:vnp_ReturnUrl"]);
            //--------------------------------
            vnpay.AddRequestData("vnp_Amount", model.Amount.ToString());
            vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang: {model.OrderId}"); 
            vnpay.AddRequestData("vnp_OrderType", "Other");// default value, mai mot modify sau
            vnpay.AddRequestData("vnp_TxnRef", "DH" + tick);

            string paymentUrl = vnpay.CreateRequestUrl(_config["VNPay:vnp_Url"], _config["VNPay:vnp_HashSecret"]);
            return paymentUrl;
        }

        public VnPaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            VnPayLibrary vnpay = new VnPayLibrary();
            
            // lay ra response hop le
            foreach(var(key,value) in collections)
            {
                if(!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.GetResponseData(key);
                }
            }

            var vnp_orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
            var vnp_TransactionId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            string vnp_SecureHash = collections["vnp_SecureHash"].ToString();

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _config["VNPay:vnp_SecureHash"]);
            if(!checkSignature)
            {
                return new VnPaymentResponseModel()
                {
                    Success = false
                };
            }
            return new VnPaymentResponseModel()
            {
                Success = true,
                OrderId = vnp_orderId.ToString(),
                TransactionId = vnp_TransactionId.ToString(),
                VnPayResponseCode = vnp_ResponseCode,
                OrderDescription = vnp_OrderInfo,
                PaymentMethod = "VnPay",
                Token = vnp_SecureHash
            };
        }
    }
}
