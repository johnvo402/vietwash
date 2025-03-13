using Contracts.Routers;

namespace Presentation.Routes
{
    public static class Router
    {

        public static class Ecommerce
        {
            public const string Tags = $"{nameof(Ecommerce)} endpoint";
            public const string Base = "api/ecommerce";
        }

        public static class ServiceRoute
        {
            public const string Tags = $"{nameof(ServiceRoute)} endpoint";
            public const string Services = $"{RouterBase.prefix}{nameof(Services)}";
        }

    }
}
