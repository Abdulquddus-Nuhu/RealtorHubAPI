using RealtorHubAPI.Entities.Identity;

namespace RealtorHubAPI.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public string ChargeId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public Guid LandId { get; set; }
        public Land Land { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }

}
