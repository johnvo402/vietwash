using Contracts.Routers;

namespace Presentation.Routes
{
    public static class Router
    {
        public const string Notification = $"{nameof(Notification)}";
        public const string Tags = $"{nameof(Notification)} Endpoint";
        public const string ListNotify =
            $"{nameof(Notification)}/{RouterBase.prefix}{nameof(ListNotify)}";

        //
        public const string CountNotify =
            $"{Notification}/{RouterBase.prefix}{nameof(CountNotify)}";

        public const string ReadOneNotify =
            $"{Notification}/{RouterBase.prefix}{nameof(ReadOneNotify)}/"
            + "{"
            + RouterBase.Id
            + "}";
        public const string ReadAllNotify =
            $"{Notification}/{RouterBase.prefix}{nameof(ReadAllNotify)}";
    }
}
