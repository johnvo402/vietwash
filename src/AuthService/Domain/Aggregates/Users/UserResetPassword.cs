using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Users;

public class UserResetPassword : BaseEntity
{
    public string Token { get; set; } = string.Empty;

    public DateTimeOffset Expiry { get; set; }

    public long UserId { get; set; }

    public User? User { get; set; }
}
