

namespace Micro.Shared.Infrastructure.CurrentUserProvider
{
    public interface ICurrentUser
    {
        string Id { get; }
        string DisplayName { get; }
        string Email { get; }
        IReadOnlyList<string> Permissions { get; }
        IReadOnlyList<string> Roles { get; }
        string OrgId { get; }
    }
}
