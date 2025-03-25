using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;

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
        public static readonly Ulid CHLOE_KIM_ID = Ulid.Parse("01JD936AXSDNMQ713P5XMVRQDV");
        public static readonly Ulid JOHN_DOE_ID = Ulid.Parse("01JD936AXTYY9KABPPN4PGZP7N");
        public static readonly Ulid ALICE_SMITH_ID = Ulid.Parse("01JD936AXT7ECQRAREV6AAZZPM");
        public static readonly Ulid BOB_JOHNSON_ID = Ulid.Parse("01JD936AXTDYC4SPCNVCRHNS61");
        public static readonly Ulid EMILY_BROWN_ID = Ulid.Parse("01JD936AXT1YQDBJEXHPP9V8DA");
        public static readonly Ulid JAMES_WILLIAMS_ID = Ulid.Parse("01JD936AXT6X0CS4VZB7BK36BP");
        public static readonly Ulid OLIVIA_TAYLOR_ID = Ulid.Parse("01JD936AXVAGPSN007QAQGN00E");
        public static readonly Ulid DANIEL_LEE_ID = Ulid.Parse("01JD936AXVEJZM53ZHK13T6MFF");
        public static readonly Ulid SHOPHIA_GARCIA_ID = Ulid.Parse("01JD936AXVZBVDXQ6MQDZXHCS7");
        public static readonly Ulid MICHAEL_MARTINEZ_ID = Ulid.Parse("01JD936AXV6V9RTKSS4Z5Q773N");
        public static readonly Ulid ISABELLA_HARRIS_ID = Ulid.Parse("01JD936AXVVKTHBPDY8516E9M5");
        public static readonly Ulid DAVID_CLARK_ID = Ulid.Parse("01JD936AXVGEP5E7S0Z3VVM7VD");
        public static readonly Ulid EMMA_RODRIGUEZ_ID = Ulid.Parse("01JD936AXVEC1FGXZYHKA1Q7VG");
        public static readonly Ulid ANDREW_MOORE_ID = Ulid.Parse("01JD936AXVDZGQK7K1KNEB175H");
        public static readonly Ulid AVA_JACKSON_ID = Ulid.Parse("01JD936AXVJ3DJG8B1N17KFXF8");
        public static readonly Ulid JOSHUA_WHITE_ID = Ulid.Parse("01JD936AXWXDDYHNC7DFA9TB0R");
        public static readonly Ulid CHARLOTTE_THOMAS_ID = Ulid.Parse("01JD936AXWNKT0HR51PRNJ52W5");
        public static readonly Ulid ETHAN_KING_ID = Ulid.Parse("01JD936AXWFM847M47AZK1ARGV");
        public static readonly Ulid ABIGAIL_SCOTT_ID = Ulid.Parse("01JD936AXWJ9B8SEJC98P0P01P");
        public static readonly Ulid LIAM_PEREZ_ID = Ulid.Parse("01JD936AXWY0JMVNZW3KXXS5ZK");
    }

    public static string CreatePermission(string action, string obj) => $"{action}:{obj}";
}


