using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Micro.Shared.Infrastructure.CurrentUserProvider
{
    public class CurrentUserProvider : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Id => GetSingleClaimValue("id");

        public string DisplayName => GetSingleClaimValue(JwtRegisteredClaimNames.Name);
        public string Email => GetSingleClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

        public IReadOnlyList<string> Permissions => GetClaimValues("permissions");
        public IReadOnlyList<string> Roles => GetClaimValues(ClaimTypes.Role);
        public string OrgId => GetSingleClaimValue("org_id");

        private List<string> GetClaimValues(string claimType) =>
            _httpContextAccessor.HttpContext?.User.Claims
                .Where(claim => claim.Type == claimType)
                .Select(claim => claim.Value)
                .ToList() ?? new List<string>();

        private string GetSingleClaimValue(string claimType) =>
            _httpContextAccessor.HttpContext?.User.Claims
                .SingleOrDefault(claim => claim.Type == claimType)?.Value ?? string.Empty;
    }
}
