namespace Infrastructure.Services.PayOs
{
    public class PayOsSetting
    {
        public string PAYOS_CLIENT_ID { get; set; } = default!;
        public string PAYOS_API_KEY { get; set; } = default!;
        public string PAYOS_CHECKSUM_KEY { get; set; } = default!;
    }
}
