using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Micro.Shared.Infrastructure.Security.JwtToken
{
    public class JwtSettings
    {
        public const string Section = "JwtSettings";
        public string Audience { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Secret { get; set; } = null!;
        public int TokenExpirationInMinutes { get; set; }
    }
}
