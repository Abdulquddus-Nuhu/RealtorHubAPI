using RealtorHubAPI.Entities.Enums;
using RealtorHubAPI.Entities.Identity;
using System.ComponentModel.DataAnnotations;

namespace RealtorHubAPI.Entities
{
    public class ActivityLog : BaseEntity
    {
        public int? UserId { get; set; }
        public User? User { get; set; }

        [StringLength(50, MinimumLength = 1)]
        public string? UserEmail { get; set; }
        public ActivityType ActivityType { get; set; }
        public string? Details { get; set; } = string.Empty;
        public string? Data { get; set; }
    }

}
