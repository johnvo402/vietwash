namespace Infrastructure.Services.PayOs
{
    public class PayOsSetting
    {
        public string? ClientId { get; set; }
        public string? ApiKey { get; set; }
        public string? ChecksumKey { get; set; }

        public bool IsEnabled { get; set; } = false;
    }
}
