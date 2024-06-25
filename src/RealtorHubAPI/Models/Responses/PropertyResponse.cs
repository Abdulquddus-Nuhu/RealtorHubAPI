using RealtorHubAPI.Entities.Enums;

namespace RealtorHubAPI.Models.Responses
{
    public record PropertyResponse
    {
        public int PropertyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public decimal Price { get; set; }
        public string Area { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public int UserId { get; set; }

        public IEnumerable<PropertyFileResponse> Images { get; set; }
        public IEnumerable<PropertyFileResponse> Videos { get; set; }
    }
}
