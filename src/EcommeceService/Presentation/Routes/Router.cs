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

        public static class CategoryRoute
        {
            public const string Tags = $"{nameof(CategoryRoute)} endpoint";
            public const string Categories = $"{Ecommerce}/{RouterBase.prefix}{nameof(Categories)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Categories)}/" + "{" + RouterBase.Id + "}";
        }
    }
}
