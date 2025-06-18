namespace Contracts.Dtos.Responses;

public class PubSubResponse<T>
{
    public bool IsSuccess { get; set; } = true;

    public Guid? PayloadId { get; set; }

    public T? ResponseData { get; set; }

    public int RetryCount { get; set; }

    public DateTimeOffset? LastAttemptTime { get; set; }

    public object? Error { get; set; }

    public PubSubErrorType? ErrorType { get; set; }
}

public enum PubSubErrorType
{
     /// <summary>
    /// 3-party service error
    /// </summary>
    Transient  = 1,

    /// <summary>
    /// server error itself
    /// </summary>
    Persistent = 2
}
