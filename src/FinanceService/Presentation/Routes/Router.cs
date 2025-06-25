using Contracts.Routers;

namespace Presentation.Routes
{
    public static class Router
    {
        public const string Finance = "Finance";

        public static class FundRoute
        {
            public const string Tags = $"{nameof(FundRoute)} endpoint";
            public const string Funds = $"{Finance}/{RouterBase.prefix}{nameof(Funds)}";
            public const string GetUpdateDelete =
                $"{Finance}/{RouterBase.prefix}{nameof(Funds)}/" + "{" + RouterBase.Id + "}";
        }
        public static class FundBehaviorRoute
        {
            public const string Tags = $"{nameof(FundBehaviorRoute)} endpoint";
            public const string FundBehaviors = $"{Finance}/{RouterBase.prefix}{nameof(FundBehaviors)}";
            public const string GetUpdateDelete =
                $"{Finance}/{RouterBase.prefix}{nameof(FundBehaviors)}/" + "{" + RouterBase.Id + "}";
        }
    }
}
