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

        public static class TariffRoute
        {
            public const string Tags = $"{nameof(TariffRoute)} endpoint";
            public const string Tariffs = $"{Ecommerce}/{RouterBase.prefix}{nameof(Tariffs)}";
            public const string GetUpdateDelete = $"{Ecommerce}/{RouterBase.prefix}{nameof(Tariffs)}/" + "{" + RouterBase.Id + "}";
        }

    }
}
