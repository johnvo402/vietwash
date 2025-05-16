namespace Contracts.Utils;

public class Generator
{
    private readonly Dictionary<string, string> _rolePrefixes = new Dictionary<string, string>
    {
        { "ADMIN", "AD" },
        { "MANAGER", "MN" },
        { "STAFF", "ST" },
        { "CUSTOMER", "CUS" },
    };

    private readonly Random _random = new Random();

    public string GenerateAccountCode(string role)
    {
        if (!_rolePrefixes.TryGetValue(role.ToUpper(), out var prefix))
            throw new ArgumentException("Role invalid.");

        // Sinh số random 6 chữ số từ 000000 đến 999999
        int randomNumber = _random.Next(0, 1000000);
        string randomDigits = randomNumber.ToString("D6");

        return $"{prefix}{randomDigits}";
    }
}
