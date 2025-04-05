using ECommerceMVC.ViewModels;

namespace ECommerceMVC.Services
{
    public interface IVNPayServices
    {
        string CreateRequestUrl(HttpContext context, VNPaymentRequestModel model);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
