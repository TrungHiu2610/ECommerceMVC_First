namespace ECommerceMVC.ViewModels
{
    public class VNPaymentRequestModel
    {
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string OrderId { get; set; }
        public string Description { get; internal set; }
        public string? FullName { get; internal set; }
    }
}
