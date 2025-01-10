using Micro.Shared.Domain;

namespace AuthService.Domain.ValueObjects;

public class ActivityTypes : ValueObject
{
    public ActivityTypes(string type)
    {
        Type = type = string.IsNullOrWhiteSpace(type) ? "Login" : type;
    }

    public static ActivityTypes From(string type)
    {
        var activitytypes = new ActivityTypes(type);

        if (!SupportedTypes.Contains(activitytypes))
        {
            throw new ArgumentException(type);
        }

        return activitytypes;
    }

    public static readonly ActivityTypes Login = new("Login");
    public static readonly ActivityTypes Logout = new("Logout");
    public static readonly ActivityTypes PasswordReset = new("PasswordReset");


    public string Type { get; private set; }

    public static implicit operator string(ActivityTypes activitytypes)
    {
        return activitytypes.ToString();
    }

    public static explicit operator ActivityTypes(string type)
    {
        return From(type);
    }

    public override string ToString()
    {
        return Type;
    }

    protected static IEnumerable<ActivityTypes> SupportedTypes
    {
        get
        {
            yield return Login;
            yield return Logout;
            yield return PasswordReset;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
    }
}
