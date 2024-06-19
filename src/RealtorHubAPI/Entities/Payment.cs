using RealtorHubAPI.Entities.Identity;

namespace RealtorHubAPI.Entities
{
    public class Payment : BaseEntity
    {
        public string ChargeId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }

}
