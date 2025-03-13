

namespace Application.Common.Auth;

public class AuthorizeModel
{
    public List<string>? Roles { get; set; }

    public List<string>? Permissions { get; set; }
}
public class UserAuth
{
    public Ulid Id { get; set; }

    public string? Role { get; set; } = null;

    public List<string>? Permissions { get; set; } = [];


}
