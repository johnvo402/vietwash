using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace Contracts.Utils;

public static class Generator
{
    private static readonly Dictionary<string, string> _rolePrefixes = new Dictionary<string, string>
    {
        { "ADMIN", "AD" },
        { "MANAGER", "MN" },
        { "STAFF", "ST" },
        { "CUSTOMER", "CUS" },
    };

    private static readonly Random _random = new Random();

    public static string GenerateAccountCode(string code)
    {
        if (!_rolePrefixes.TryGetValue(code.ToUpper(), out var prefix))
            throw new ArgumentException("invalid.");

        // Sinh số random 6 chữ số từ 000000 đến 999999
        int randomNumber = _random.Next(0, 1000000);
        string randomDigits = randomNumber.ToString("D6");

        return $"{prefix}{randomDigits}";
    }

    public static string GenerateCode(string prefix, int numberLength)
    {
        if (numberLength <= 0)
            throw new ArgumentException("Number length must be greater than 0", nameof(numberLength));

        int maxValue = (int)Math.Pow(10, numberLength) - 1;
        int minValue = (int)Math.Pow(10, numberLength - 1);

        int number = _random.Next(minValue, maxValue + 1);
        return ($"{prefix}{number}").Trim();
    }

    public static string GenerateSlug(string input)
    {
        string slug = input.ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder();
        foreach (var c in slug)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);

        slug = Regex.Replace(sb.ToString(), @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');

        return slug;
    }
}
