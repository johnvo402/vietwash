using Contracts.Routers;
using Domain.Aggregates.Services;
using Domain.Aggregates.Tariffs;

namespace Presentation.Routes
{
    public static class Router
    {
        public const string Ecommerce = "Ecommerce";

        public static class ServiceRoute
        {
            public const string Tags = $"{nameof(ServiceRoute)} endpoint";
            public const string Services = $"{Ecommerce}/{RouterBase.prefix}{nameof(Services)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Services)}/" + "{" + RouterBase.Id + "}";
            public const string GetDetail =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Services)}/detail/"
                + "{"
                + RouterBase.Id
                + "}";
        }

        public static class TariffRoute
        {
            public const string Tags = $"{nameof(TariffRoute)} endpoint";
            public const string Tariffs = $"{Ecommerce}/{RouterBase.prefix}{nameof(Tariffs)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Tariffs)}/" + "{" + RouterBase.Id + "}";
        }

        public static class UnitRoute
        {
            public const string Tags = $"{nameof(UnitRoute)} endpoint";
            public const string Units = $"{Ecommerce}/{RouterBase.prefix}{nameof(Units)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Units)}/" + "{" + RouterBase.Id + "}";
        }

        public static class CategoryRoute
        {
            public const string Tags = $"{nameof(CategoryRoute)} endpoint";
            public const string Categories = $"{Ecommerce}/{RouterBase.prefix}{nameof(Categories)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Categories)}/" + "{" + RouterBase.Id + "}";
        }
		public static class OrderRoute
		{
			public const string Tags = $"{nameof(OrderRoute)} endpoint";
			public const string Orders = $"{Ecommerce}/{RouterBase.prefix}{nameof(Orders)}";
			public const string GetUpdateDelete = $"{Ecommerce}/{RouterBase.prefix}{nameof(Orders)}/" + "{" + RouterBase.Id + "}";
			public const string UpdateStatus = $"{Ecommerce}/{RouterBase.prefix}{nameof(Orders)}/{nameof(UpdateStatus)}" + "{" + RouterBase.Id + "}";
		}
		public static class FundRoute
		{
			public const string Tags = $"{nameof(FundRoute)} endpoint";
			public const string Funds = $"{Ecommerce}/{RouterBase.prefix}{nameof(Funds)}";
			public const string GetUpdateDelete = $"{Ecommerce}/{RouterBase.prefix}{nameof(Funds)}/" + "{" + RouterBase.Id + "}";
		}
		
	}
}
