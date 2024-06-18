namespace RealtorHubAPI.Models.Requests
{
    public class PaymentInitiateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public int PropertyId { get; set; }
    }
}
