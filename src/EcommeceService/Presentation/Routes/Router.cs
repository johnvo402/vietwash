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
		public static class UnitRoute
		{
			public const string Tags = $"{nameof(UnitRoute)} endpoint";
			public const string Units = $"{Ecommerce}/{RouterBase.prefix}{nameof(Units)}";
			public const string GetUpdateDelete = $"{Ecommerce}/{RouterBase.prefix}{nameof(Units)}/" + "{" + RouterBase.Id + "}";
		}

	}
}
