

namespace Infrastructure.Constants;

public static class Credential
{
    public const string USER_DEFAULT_PASSWORD = "123456";

    public const string ADMIN_ROLE = "ADMIN";

    public const string STAFF_ROLE = "STAFF";
    public const string CUSTOMER = "CUSTOMER";

    public static readonly Dictionary<string, string[]> PermissionGroups =
    new()
    {
        {
            "Users",
            [
                CreatePermission(ActionPermission.create, ObjectPermission.user),
                CreatePermission(ActionPermission.update, ObjectPermission.user),
                CreatePermission(ActionPermission.delete, ObjectPermission.user),
                CreatePermission(ActionPermission.detail, ObjectPermission.user),
                CreatePermission(ActionPermission.list, ObjectPermission.user),
                CreatePermission(ActionPermission.testa, ObjectPermission.user)
            ]
        },
        {
            "Roles",
            [
                CreatePermission(ActionPermission.create, ObjectPermission.role),
                CreatePermission(ActionPermission.update, ObjectPermission.role),
                CreatePermission(ActionPermission.delete, ObjectPermission.role),
                CreatePermission(ActionPermission.detail, ObjectPermission.role),
                CreatePermission(ActionPermission.list, ObjectPermission.role),
                CreatePermission(ActionPermission.testa, ObjectPermission.role)
            ]
        },
        {
            "Orders",
            [
                CreatePermission(ActionPermission.create, ObjectPermission.order),
                CreatePermission(ActionPermission.update, ObjectPermission.order),
                CreatePermission(ActionPermission.delete, ObjectPermission.order),
                CreatePermission(ActionPermission.detail, ObjectPermission.order),
                CreatePermission(ActionPermission.list, ObjectPermission.order),
                CreatePermission(ActionPermission.testa, ObjectPermission.order)
            ]
        },
        {
            "Services",
            [
                CreatePermission(ActionPermission.create, ObjectPermission.service),
                CreatePermission(ActionPermission.update, ObjectPermission.service),
                CreatePermission(ActionPermission.delete, ObjectPermission.service),
                CreatePermission(ActionPermission.detail, ObjectPermission.service),
                CreatePermission(ActionPermission.list, ObjectPermission.service),
                CreatePermission(ActionPermission.testa, ObjectPermission.service)
            ]
        },
        {
            "Tariffs",
            [
                CreatePermission(ActionPermission.create, ObjectPermission.tariff),
                CreatePermission(ActionPermission.update, ObjectPermission.tariff),
                CreatePermission(ActionPermission.delete, ObjectPermission.tariff),
                CreatePermission(ActionPermission.detail, ObjectPermission.tariff),
                CreatePermission(ActionPermission.list, ObjectPermission.tariff),
                CreatePermission(ActionPermission.testa, ObjectPermission.tariff)
            ]
        },
        {
            "Funds",
            [
                CreatePermission(ActionPermission.create, ObjectPermission.fund),
                CreatePermission(ActionPermission.update, ObjectPermission.fund),
                CreatePermission(ActionPermission.delete, ObjectPermission.fund),
                CreatePermission(ActionPermission.detail, ObjectPermission.fund),
                CreatePermission(ActionPermission.list, ObjectPermission.fund),
                CreatePermission(ActionPermission.testa, ObjectPermission.fund)
            ]
        },
        {
            "Reportservices",
            [
                CreatePermission(ActionPermission.create, ObjectPermission.reportservice),
                CreatePermission(ActionPermission.update, ObjectPermission.reportservice),
                CreatePermission(ActionPermission.delete, ObjectPermission.reportservice),
                CreatePermission(ActionPermission.detail, ObjectPermission.reportservice),
                CreatePermission(ActionPermission.list, ObjectPermission.reportservice),
                CreatePermission(ActionPermission.testa, ObjectPermission.reportservice)
            ]
        },
        {
            "Customer",
            [
                CreatePermission(ActionPermission.create, ObjectPermission.customer),
                CreatePermission(ActionPermission.update, ObjectPermission.customer),
                CreatePermission(ActionPermission.delete, ObjectPermission.customer),
                CreatePermission(ActionPermission.detail, ObjectPermission.customer),
                CreatePermission(ActionPermission.list, ObjectPermission.customer),
                CreatePermission(ActionPermission.testa, ObjectPermission.customer)
            ]
        },
        {
            "Dashboard",
            [

                CreatePermission(ActionPermission.list, ObjectPermission.dashboard),

            ]
        }
    };

    public static readonly IReadOnlyCollection<string> ADMIN_CLAIMS =
        PermissionGroups
            .SelectMany(x => x.Value)
            .ToList();

    public static readonly IReadOnlyCollection<string> MANAGER_CLAIMS =
    [

            CreatePermission(ActionPermission.create, ObjectPermission.order),

            CreatePermission(ActionPermission.list, ObjectPermission.order),

            CreatePermission(ActionPermission.detail, ObjectPermission.order),
            CreatePermission(ActionPermission.update, ObjectPermission.order),
         CreatePermission(ActionPermission.create, ObjectPermission.user),
         CreatePermission(ActionPermission.list, ObjectPermission.customer),


    ];

    public const string ADMIN_ROLE_ID = "01J79JQZRWAKCTCQV64VYKMZ56";
    public const string MANAGER_ROLE_ID = "01JB19HK30BGYJBZGNETQY8905";
    public const string CUSTOMER_ROLE_ID = "01JB19HK30BGYJBZGNETQY8908";

    public static class UserIds
    {
        public static readonly long CHLOE_KIM_ID = 1;
        public static readonly long JOHN_DOE_ID = 2;
        public static readonly long ALICE_SMITH_ID = 3;
        public static readonly long BOB_JOHNSON_ID = 4;
        public static readonly long EMILY_BROWN_ID = 5;
        public static readonly long JAMES_WILLIAMS_ID = 6;
        public static readonly long OLIVIA_TAYLOR_ID = 7;
        public static readonly long DANIEL_LEE_ID = 8;
        public static readonly long SHOPHIA_GARCIA_ID = 9;
        public static readonly long MICHAEL_MARTINEZ_ID = 10;
        public static readonly long ISABELLA_HARRIS_ID = 11;
        public static readonly long DAVID_CLARK_ID = 12;
        public static readonly long EMMA_RODRIGUEZ_ID = 13;
        public static readonly long ANDREW_MOORE_ID = 14;
        public static readonly long AVA_JACKSON_ID = 15;
        public static readonly long JOSHUA_WHITE_ID = 16;
        public static readonly long CHARLOTTE_THOMAS_ID = 17;
        public static readonly long ETHAN_KING_ID = 18;
        public static readonly long ABIGAIL_SCOTT_ID = 19;
        public static readonly long LIAM_PEREZ_ID = 20;
    }


    public static string CreatePermission(string action, string obj) => $"{action}:{obj}";
}


