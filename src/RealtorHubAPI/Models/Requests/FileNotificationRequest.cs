namespace RealtorHubAPI.Models.Requests
{
    public record FileNotificationRequest
    {
        //public string FileName { get; set; }
        public string FileUrl { get; set; }
        public Guid LandId { get; set; }
    }
}
