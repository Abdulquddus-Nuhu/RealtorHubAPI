namespace RealtorHubAPI.Models.Requests
{
    public record CreateLandRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
    }
}
