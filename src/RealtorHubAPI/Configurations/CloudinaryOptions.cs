namespace RealtorHubAPI.Configurations
{
    public class CloudinaryOptions
    {
        public string CloudName { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }

    public class MinioOptions
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public bool UseSSL { get; set; }
    }

    public class CoinbaseOptions
    {
        public string ApiKey { get; set; }
        public string WebhookSecret { get; set; }
    }

}
