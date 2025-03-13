using Contracts.Routers;

namespace Presentation.Routes
{
    public static class Router
    {
        public const string Ecommerce = "Ecommerce";

        public static class ServiceRoute
        {
            public const string Tags = $"{nameof(ServiceRoute)} endpoint";
            public const string Services = $"{Ecommerce}/{RouterBase.prefix}{nameof(Services)}";
        }

    }
}
