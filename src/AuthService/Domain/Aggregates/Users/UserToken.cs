
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Users;

public class UserToken : BaseEntity
{
    public string? RefreshToken { get; set; }

    public string? ClientIp { get; set; }

    public string? UserAgent { get; set; }

    public string? FamilyId { get; set; }

    public bool IsBlocked { get; set; }

    public long UserId { get; set; }

    public User? User { get; set; }

    public DateTimeOffset ExpiredTime { get; set; }
}
