using System.Text.RegularExpressions;

namespace Contracts.Utils;

public static class NumberToTextConverter
{
    private static readonly string[] ChuSo =
    {
        "không",
        "một",
        "hai",
        "ba",
        "bốn",
        "năm",
        "sáu",
        "bảy",
        "tám",
        "chín",
    };
    private static readonly string[] DonVi = { "", "nghìn", "triệu", "tỷ" };

    public static string FormatCurrency(decimal number, string? currency = "đ")
    {
        return string.Format("{0:N2}{1}", number, currency);
    }

    public static string ToVietnameseCurrencyText(decimal number)
    {
        if (number == 0)
            return "Không đồng chẵn";

        bool isNegative = number < 0;
        number = Math.Abs(number);

        long soNguyen = (long)Math.Floor(number); // Phần nguyên
        string result = ToText(soNguyen) + " đồng chẵn";

        return (isNegative ? "Âm " : "") + CapitalizeFirst(result.Trim());
    }

    public static string ToText(long number)
    {
        if (number == 0)
            return "không";

        string result = string.Empty;
        int unitIndex = 0;

        while (number > 0)
        {
            int group = (int)(number % 1000);
            if (group != 0)
            {
                string groupText = ReadGroup(group);
                if (!string.IsNullOrEmpty(groupText))
                    result = $"{groupText} {DonVi[unitIndex]} {result}".Trim();
            }
            number /= 1000;
            unitIndex++;
        }

        return Regex.Replace(result.Trim(), @"\s+", " ");
    }

    private static string ReadGroup(int number)
    {
        int tram = number / 100;
        int chuc = (number % 100) / 10;
        int donvi = number % 10;

        string result = "";

        if (tram > 0)
        {
            result += $"{ChuSo[tram]} trăm";
            if (chuc == 0 && donvi > 0)
                result += " linh";
        }

        if (chuc > 1)
        {
            result += $" {ChuSo[chuc]} mươi";
            if (donvi == 1)
                result += " mốt";
            else if (donvi == 5)
                result += " lăm";
            else if (donvi > 0)
                result += $" {ChuSo[donvi]}";
        }
        else if (chuc == 1)
        {
            result += " mười";
            if (donvi == 5)
                result += " lăm";
            else if (donvi > 0)
                result += $" {ChuSo[donvi]}";
        }
        else if (chuc == 0 && donvi > 0)
        {
            result += $" {ChuSo[donvi]}";
        }

        return result.Trim();
    }

    private static string CapitalizeFirst(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        return char.ToUpper(text[0]) + text.Substring(1);
    }
}
