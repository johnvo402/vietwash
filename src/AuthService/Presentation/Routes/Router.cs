using Contracts.Routers;
using Domain.Aggregates.Accounts;

namespace Presentation.Routes
{
    public static class Router
    {
        public const string Auth = $"{nameof(Auth)}";

        public static class AccountRoute
        {
            public const string Accounts = $"{Auth}/{RouterBase.prefix}{nameof(Accounts)}";
            public const string GetUpdateDelete =
                $"{Auth}/{RouterBase.prefix}{nameof(Accounts)}/" + "{" + RouterBase.Id + "}";
            public const string GetRouteName = $"{Accounts}DetailEndpoint";

            public const string Profile =
                $"{Auth}/{RouterBase.prefix}{nameof(Accounts)}/{nameof(Profile)}";

            public const string ChangePassword =
                $"{Auth}/{RouterBase.prefix}{nameof(Accounts)}/{nameof(ChangePassword)}";
            public const string RequestResetPassowrd =
                $"{Auth}/{RouterBase.prefix}{nameof(Accounts)}/{nameof(RequestResetPassowrd)}";
            public const string ResetPassowrd =
                $"{Auth}/{RouterBase.prefix}{nameof(Accounts)}/{nameof(ResetPassowrd)}";

            public const string Login =
                $"{Auth}/{RouterBase.prefix}{nameof(Accounts)}/{nameof(Login)}";
            public const string RefreshToken =
                $"{Auth}/{RouterBase.prefix}{nameof(Accounts)}/{nameof(RefreshToken)}";
            public const string Logout =
                $"{Auth}/{RouterBase.prefix}{nameof(Accounts)}/{nameof(Logout)}";
            public const string CustomerLogin =
                $"{Auth}/{RouterBase.prefix}{nameof(Accounts)}/{nameof(CustomerLogin)}";

            public const string RequestOtp =
                $"{Auth}/{RouterBase.prefix}{nameof(Accounts)}/{nameof(RequestOtp)}";
            public const string CustomerLoginVerify =
                $"{Auth}/{RouterBase.prefix}{nameof(Accounts)}/{nameof(CustomerLoginVerify)}";
            public const string Tags = $"{nameof(Accounts)} endpoint";
        }

        public static class CustomerRoute
        {
            public const string Customers = $"{Auth}/{RouterBase.prefix}{nameof(Customers)}";
            public const string GetUpdateDelete =
                $"{Auth}/{RouterBase.prefix}{nameof(Customers)}/" + "{" + RouterBase.Id + "}";
            public const string GetRouteName = $"{Customers}DetailEndpoint";
            public const string GetList = $"{Auth}/{RouterBase.prefix}{nameof(Customers)}";
            public const string Tags = $"{nameof(Customers)} endpoint";
        }

        public static class MediaRoute
        {
            public const string Media = $"{Auth}/{RouterBase.prefix}{nameof(Media)}";
            public const string Tags = $"{nameof(Media)} endpoint";
        }
    }
}
