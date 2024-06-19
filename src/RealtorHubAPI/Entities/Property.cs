using RealtorHubAPI.Entities.Identity;
using System.Text.Json.Serialization;

namespace RealtorHubAPI.Entities
{
    public class Property : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int UserId { get; set; }
        public User User { get; set; }

        [JsonIgnore]
        public ICollection<PropertyImage> Images { get; set; }

        [JsonIgnore]
        public ICollection<PropertyVideo> Videos { get; set; }
    }

}
