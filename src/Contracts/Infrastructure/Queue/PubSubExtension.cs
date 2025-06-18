namespace Infrastructure.Services.DistributedCache;

public static class PubSubExtension
{
    // Delay khởi động subscriber (nếu cần đợi Redis hoặc các service khác khởi tạo)
    public const int InitialSubscribeDelayInSeconds = 2;

    // Độ lệch ngẫu nhiên tối đa để tránh "stampede" (tất cả service subscribe/publish cùng lúc)
    public const double MaximumJitterFactor = 0.2;

    private static readonly Random Random = new();

    /// <summary>
    /// Sinh hệ số ngẫu nhiên giữa [minFactor, maxFactor] để tránh các request cùng lúc.
    /// </summary>
    public static double GenerateJitter(
        double minFactor = 0.0,
        double maxFactor = MaximumJitterFactor
    )
    {
        if (minFactor > maxFactor)
            throw new ArgumentOutOfRangeException(
                nameof(minFactor),
                "minFactor must be <= maxFactor"
            );

        return minFactor + ((maxFactor - minFactor) * Random.NextDouble());
    }

    /// <summary>
    /// Tạo tên channel từ type T, có prefix nếu cần.
    /// </summary>
    public static string BuildChannelName<T>(string prefix = "events")
    {
        return $"{prefix}:{typeof(T).Name}";
    }
}
