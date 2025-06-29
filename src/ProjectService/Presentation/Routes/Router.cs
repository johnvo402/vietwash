using Contracts.Routers;

namespace Presentation.Routes
{
    public static class Router
    {
        public const string Project = $"{nameof(Project)}";

        public static class BranchRoute
        {
            public const string Tags = $"{nameof(BranchRoute)} endpoint";
            public const string Branches = $"{Project}/{RouterBase.prefix}{nameof(Branches)}";
            public const string GetUpdateDelete =
                $"{Project}/{RouterBase.prefix}{nameof(Branches)}/" + "{" + RouterBase.Id + "}";
        }

        public static class WarehouseRoute
        {
            public const string Tags = $"{nameof(WarehouseRoute)} endpoint";
            public const string Warehouses = $"{Project}/{RouterBase.prefix}{nameof(Warehouses)}";
            public const string GetUpdateDelete =
                $"{Project}/{RouterBase.prefix}{nameof(Warehouses)}/" + "{" + RouterBase.Id + "}";
        }
    }
}
