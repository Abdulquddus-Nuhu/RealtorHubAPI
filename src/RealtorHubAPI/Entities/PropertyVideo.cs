namespace RealtorHubAPI.Entities
{
    public class PropertyVideo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }
    }

}
