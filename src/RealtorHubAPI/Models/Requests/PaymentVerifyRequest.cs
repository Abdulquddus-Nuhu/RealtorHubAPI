namespace RealtorHubAPI.Models.Requests
{
    public record PaymentVerifyRequest
    {
        public string ChargeId { get; set; }
    }
}
