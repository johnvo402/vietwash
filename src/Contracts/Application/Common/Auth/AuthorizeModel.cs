

namespace Application.Common.Auth;

public class AuthorizeModel
{
    public List<string>? Roles { get; set; }
}
public class UserAuth
{
    public long Id { get; set; }

    public string? Role { get; set; } = null;


}
