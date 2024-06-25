namespace RealtorHubAPI.Entities
{
    public class Message : BaseEntity
    {
        public string SenderEmail { get; set; }
        public string Content { get; set; }
        public string ReceiverEmail { get; set; }
    }
}
