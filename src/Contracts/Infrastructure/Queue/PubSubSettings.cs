namespace Infrastructure.Services.DistributedCache;

public class PubSubSettings
{
    public string? ChannelPrefix { get; set; }

    public int MaxRetryAttempts { get; set; }
    public int DeadLetterMaxRetryAttempts { get; set; }
    public int MaximumDelayInSec { get; set; } = 90;
    public int DeadLetterBatchSize { get; set; } = 100;
}
