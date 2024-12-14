namespace Micro.Shared.Model;

public class UserAccess
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IList<string> Role { get; set; } = [];
    public List<string> Permissions { get; set; } = new();
}