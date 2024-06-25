using RealtorHubAPI.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace RealtorHubAPI.Models.Requests
{
    public record CreateLandRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public string Area { get; set; } = string.Empty;
        public PropertyType Type { get; set; }
    }
}
