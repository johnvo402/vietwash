using Micro.Shared.Domain;
using Micro.Shared.Data; // Importing RoleName

namespace AuthService.Domain.ValueObjects;


public class RoleNameValues : ValueObject
{
    public RoleNameValues(string roleName)
    {
        RoleName = roleName = string.IsNullOrWhiteSpace(roleName) ? "#Customer" : roleName;
    }

    public static RoleNameValues From(string rolename)
    {
        var rolenamevalues = new RoleNameValues(rolename);

        if (!SupportedRoles.Contains(rolenamevalues))
        {
            throw new ArgumentException(rolename);
        }

        return rolenamevalues;
    }

    public static RoleNameValues Admin => new("Admin");

    public static RoleNameValues Manager => new("Manager");

    public static RoleNameValues Customer => new("Customer");

    public static RoleNameValues Staff => new("Staff");


    public string RoleName { get; private set; }

    public static implicit operator string(RoleNameValues rolenamevalues)
    {
        return rolenamevalues.ToString();
    }

    public static explicit operator RoleNameValues(string rolename)
    {
        return From(rolename);
    }

    public override string ToString()
    {
        return RoleName;
    }

    protected static IEnumerable<RoleNameValues> SupportedRoles
    {
        get
        {
            yield return Admin;
            yield return Manager;
            yield return Staff;
            yield return Customer;

        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return RoleName;
    }
}