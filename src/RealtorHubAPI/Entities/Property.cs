using RealtorHubAPI.Entities.Enums;
using RealtorHubAPI.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RealtorHubAPI.Entities
{
    public class Property : BaseEntity
    {
        [StringLength(100)]
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public bool IsAvailable { get; set; } = true;
        public decimal Price { get; set; }

        [StringLength(100)]
        public string Area { get; set; } = string.Empty;

        public PropertyType Type { get; set; } = PropertyType.Land;
        public int UserId { get; set; }
        public User User { get; set; }

        [JsonIgnore]
        public ICollection<PropertyImage> Images { get; set; }

        [JsonIgnore]
        public ICollection<PropertyVideo> Videos { get; set; }
    }

}
