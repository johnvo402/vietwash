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
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Orders)}/" + "{" + RouterBase.Id + "}";
            public const string UpdateStatus =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Orders)}/{nameof(UpdateStatus)}"
                + "{"
                + RouterBase.Id
                + "}";
        }

        public static class SaleResultRoute
        {
            public const string Tags = $"{nameof(SaleResultRoute)} endpoint";

            public const string DashboardCard =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(DashboardCard)}";
            public const string RevenueStatistic =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(RevenueStatistic)}";
            public const string TopService = $"{Ecommerce}/{RouterBase.prefix}{nameof(TopService)}";
            public const string NetRevenueBranch =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(NetRevenueBranch)}";
        }

        public static class UserRoute
        {
            public const string Users = $"{Ecommerce}/{RouterBase.prefix}{nameof(Users)}";
            public const string GetRouteName = $"{Users}DetailEndpoint";
            public const string Tags = $"{nameof(Users)} endpoint";
        }


        public static class ReportRoute
        {
            public const string Tags = $"{nameof(ReportRoute)} endpoint";
            public const string ReportServiceOrder =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(ReportServiceOrder)}";
            public const string Order = $"{Ecommerce}/{RouterBase.prefix}{nameof(ReportRoute)}/{nameof(Order)}";
        }

        public static class SupplierRoute
        {
            public const string Tags = $"{nameof(SupplierRoute)} endpoint";
            public const string Suppliers = $"{Ecommerce}/{RouterBase.prefix}{nameof(Suppliers)}";
            public const string GetUpdateDelete =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Suppliers)}/" + "{" + RouterBase.Id + "}";
            public const string GetDetail =
                $"{Ecommerce}/{RouterBase.prefix}{nameof(Suppliers)}/detail/"
                + "{"
                + RouterBase.Id
                + "}";
        }
    }
}
