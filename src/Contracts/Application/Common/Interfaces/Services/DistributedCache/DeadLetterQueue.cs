using Shared.Kernel.Common;

namespace Application.Common.Interfaces.Services.DistributedCache;

public class DeadLetter : BaseEntity
{
    public Guid RequestId { get; set; }
    public object? ErrorDetail { get; set; }
    public int RetryCount { get; set; }
}
