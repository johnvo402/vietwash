namespace Infrastructure.Outbox;

internal static class OutboxDeliveryPolicy
{
    private const int MaxAttempts = 1_000_000;

    public static Guid NewLeaseId() => Guid.NewGuid();

    public static DateTimeOffset LeaseUntil(DateTimeOffset now) => now.AddMinutes(1);

    public static int IncrementAttempts(int attempts) => Math.Min(attempts + 1, MaxAttempts);

    public static DateTimeOffset RetryAt(DateTimeOffset now, int attempts) =>
        now.AddSeconds(Math.Min(3600, Math.Pow(2, Math.Min(12, attempts))));
}
