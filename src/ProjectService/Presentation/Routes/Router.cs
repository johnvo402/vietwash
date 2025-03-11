using Contracts.Routers;

namespace Presentation.Routes
{
    public static class Router
    {

        public static class AuditLogRoute
        {
            public const string AuditLog = $"{RouterBase.prefix}{nameof(AuditLog)}";
            public const string Tags = $"{nameof(AuditLog)} endpoint";
        }

    }
}
