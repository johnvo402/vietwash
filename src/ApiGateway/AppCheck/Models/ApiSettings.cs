namespace ApiGateway.AppCheck.Models
{
    public class ApiSettings
    {
        public WebSettings? Web { get; set; }
        public MobileSettings? Mobile { get; set; }
    }

    public class WebSettings
    {
        public string? ApiKey { get; set; }
        public string? Platform { get; set; }
        public List<string>? Origin { get; set; }
    }

    public class MobileSettings
    {
        public string? ApiKey { get; set; }
        public string? Platform { get; set; }
    }
}
