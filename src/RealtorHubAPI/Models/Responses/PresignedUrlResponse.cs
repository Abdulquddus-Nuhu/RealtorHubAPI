namespace RealtorHubAPI.Models.Responses
{
    public record PresignedUrlResponse
    {
        public string PresignedUrl { get; set; }
        public string FileName { get; set; }
        public Guid LandId { get; set; }
    }
}
