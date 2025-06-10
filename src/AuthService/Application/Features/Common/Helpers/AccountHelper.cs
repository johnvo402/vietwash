

namespace Application.Features.Common.Helpers
{
    public static class AccountHelper
    {
        public static string[] GetRolesByRole(string role)
        {
            return role switch
            {
                "ADMIN" => new[] { "STAFF",  "MANAGER" },
                "MANAGER" => new[] { "STAFF" },
                _ => Array.Empty<string>()
            };
        }
    }
}
