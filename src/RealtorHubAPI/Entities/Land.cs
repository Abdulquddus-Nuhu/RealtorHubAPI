using RealtorHubAPI.Entities.Identity;

namespace RealtorHubAPI.Entities
{
    public class Land : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public bool IsAvailable { get; set; } = true;
        //public int RealtorId { get; set; }
        //public Realtor Realtor { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public ICollection<LandImage> Images { get; set; }
        public ICollection<LandVideo> Videos { get; set; }
    }

}
