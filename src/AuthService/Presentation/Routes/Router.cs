using Contracts.Routers;

namespace Presentation.Routes
{
    public static class Router
    {
        public static class UserRoute
        {
            public const string Users = $"{RouterBase.prefix}{nameof(Users)}";
            public const string GetUpdateDelete = $"{RouterBase.prefix}{nameof(Users)}/" + "{" + RouterBase.Id + "}";
            public const string GetRouteName = $"{Users}DetailEndpoint";

            public const string Profile = $"{RouterBase.prefix}{nameof(Users)}/{nameof(Profile)}";

            public const string ChangePassword = $"{RouterBase.prefix}{nameof(Users)}/{nameof(ChangePassword)}";
            public const string RequestResetPassowrd =
                $"{RouterBase.prefix}{nameof(Users)}/{nameof(RequestResetPassowrd)}";
            public const string ResetPassowrd = $"{RouterBase.prefix}{nameof(Users)}/{nameof(ResetPassowrd)}";

            public const string Login = $"{RouterBase.prefix}{nameof(Users)}/{nameof(Login)}";
            public const string RefreshToken = $"{RouterBase.prefix}{nameof(Users)}/{nameof(RefreshToken)}";
            public const string Logout = $"{RouterBase.prefix}{nameof(Users)}/{nameof(Logout)}";

            public const string Tags = $"{nameof(Users)} endpoint";
        }

        public static class RoleRoute
        {
            public const string Roles = $"{RouterBase.prefix}{nameof(Roles)}";

            public const string GetUpdateDelete = $"{RouterBase.prefix}{nameof(Roles)}/" + "{" + RouterBase.Id + "}";

            public const string GetRouteName = $"{Roles}DetailEndpoint";

            public const string Tags = $"{nameof(Roles)} endpoint";
        }

        public static class PermissionRoute
        {
            public const string Permissions = $"{RouterBase.prefix}{nameof(Permissions)}";

            public const string Tags = $"{nameof(Permissions)} endpoint";
        }

        public static class RegionRoute
        {
            public const string Provinces = $"{RouterBase.prefix}{nameof(Provinces)}";
            public const string Districts = $"{RouterBase.prefix}{nameof(Districts)}";
            public const string Communes = $"{RouterBase.prefix}{nameof(Communes)}";
            public const string Tags = $"{nameof(RegionRoute)} endpoint";
        }
    }
}
