using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Contracts.Utils;

public static class Generator
{
    private static readonly Dictionary<string, string> _rolePrefixes = new Dictionary<
        string,
        string
    >
    {
        { "ADMIN", "AD" },
        { "MANAGER", "MN" },
        { "STAFF", "ST" },
        { "CUSTOMER", "CUS" },
    };

    private static readonly Random _random = new Random();

    public static string GenerateAccountCode(string role)
    {
        if (!_rolePrefixes.TryGetValue(role.ToUpper(), out var prefix))
            throw new ArgumentException("invalid.");

        // Sinh số random 6 chữ số từ 000000 đến 999999
        int randomNumber = _random.Next(0, 1000000);
        string randomDigits = randomNumber.ToString("D6");

        return $"{prefix}{randomDigits}";
    }

    public static string GenerateCode(string prefix, int numberLength)
    {
        if (numberLength <= 0)
            throw new ArgumentException(
                "Number length must be greater than 0",
                nameof(numberLength)
            );

        int maxValue = (int)Math.Pow(10, numberLength) - 1;
        int minValue = (int)Math.Pow(10, numberLength - 1);

        int number = _random.Next(minValue, maxValue + 1);
        return $"{prefix}{number}".Trim();
    }

    public static string GenerateCode(int numberLength)
    {
        if (numberLength <= 0)
            throw new ArgumentException(
                "Number length must be greater than 0",
                nameof(numberLength)
            );
        if (numberLength > 18)
            throw new ArgumentException("Number length too large for long");

        long minValue = (long)Math.Pow(10, numberLength - 1);
        long maxValue = (long)Math.Pow(10, numberLength) - 1;

        long number = RandomLong(minValue, maxValue);
        return number.ToString();
    }

    private static long RandomLong(long min, long max)
    {
        return (long)(_random.NextDouble() * (max - min + 1)) + min;
    }

    public static string GenerateSlug(string input)
    {
        string slug = input.ToLowerInvariant().Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder();
        foreach (var c in slug)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);

        slug = Regex.Replace(sb.ToString(), @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');

        return slug;
    }

    private const string Characters =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string GenerateRandomString(int length)
    {
        if (length < 0)
            throw new ArgumentException("Length must be non-negative.", nameof(length));

        return new string(
            Enumerable.Repeat(Characters, length).Select(s => s[_random.Next(s.Length)]).ToArray()
        );
    }
}
