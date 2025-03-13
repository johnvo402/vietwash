using Contracts.Routers;

namespace Presentation.Routes
{
    public static class Router
    {
        public const string Auth = $"{nameof(Auth)}";
        public static class UserRoute
        {
            public const string Users = $"{Auth}/{RouterBase.prefix}{nameof(Users)}";
            public const string GetUpdateDelete = $"{Auth}/{RouterBase.prefix}{nameof(Users)}/" + "{" + RouterBase.Id + "}";
            public const string GetRouteName = $"{Users}DetailEndpoint";

            public const string Profile = $"{Auth}/{RouterBase.prefix}{nameof(Users)}/{nameof(Profile)}";

            public const string ChangePassword = $"{Auth}/{RouterBase.prefix}{nameof(Users)}/{nameof(ChangePassword)}";
            public const string RequestResetPassowrd =
                $"{Auth}/{RouterBase.prefix}{nameof(Users)}/{nameof(RequestResetPassowrd)}";
            public const string ResetPassowrd = $"{Auth}/{RouterBase.prefix}{nameof(Users)}/{nameof(ResetPassowrd)}";

            public const string Login = $"{Auth}/{RouterBase.prefix}{nameof(Users)}/{nameof(Login)}";
            public const string RefreshToken = $"{Auth}/{RouterBase.prefix}{nameof(Users)}/{nameof(RefreshToken)}";
            public const string Logout = $"{Auth}/{RouterBase.prefix}{nameof(Users)}/{nameof(Logout)}";

            public const string Tags = $"{nameof(Users)} endpoint";
        }

        public static class RoleRoute
        {
            public const string Roles = $"{Auth}/{RouterBase.prefix}{nameof(Roles)}";

            public const string GetUpdateDelete = $"{Auth}/{RouterBase.prefix}{nameof(Roles)}/" + "{" + RouterBase.Id + "}";

            public const string GetRouteName = $"{Roles}DetailEndpoint";

            public const string Tags = $"{nameof(Roles)} endpoint";
        }

        public static class PermissionRoute
        {
            public const string Permissions = $"{Auth}/{RouterBase.prefix}{nameof(Permissions)}";

            public const string Tags = $"{nameof(Permissions)} endpoint";
        }

        public static class RegionRoute
        {
            public const string Provinces = $"{Auth}/{RouterBase.prefix}{nameof(Provinces)}";
            public const string Districts = $"{Auth}/{RouterBase.prefix}{nameof(Districts)}";
            public const string Communes = $"{Auth}/{RouterBase.prefix}{nameof(Communes)}";
            public const string Tags = $"{nameof(RegionRoute)} endpoint";
        }
    }
}
