using Contracts.Routers;

namespace Presentation.Routes
{
    public static class Router
    {
        public const string Project = $"{nameof(Project)}";
        public static class AuditLogRoute
        {
            public const string AuditLog = $"{Project}/{RouterBase.prefix}{nameof(AuditLog)}";
            public const string Tags = $"{nameof(AuditLog)} endpoint";
        }

    }
}
