namespace RealtorHubAPI.Models.Requests
{
    public record FileNotificationRequest
    {
        //public string FileName { get; set; }
        public string FileUrl { get; set; }
        public int LandId { get; set; }
    }
}
