using System.Globalization;
using System.Text;

namespace Utilities;
public static class Generator
{

    /// <summary>
    /// Generate keywords from the input phrase by creating original and non-diacritic versions.
    /// </summary>
    /// <param name="input">The input phrase (e.g., "bánh mì").</param>
    /// <returns>A list of keywords.</returns>
    public static string GenerateKeywords(List<string> input)
    {
        if (input.Count == 0)
            return "";



        var keywords = new StringBuilder();
        // Add individual words and their non-diacritic versions
        foreach (var word in input)
        {
            keywords.Append(word); // Original word
            keywords.Append(" ");
            keywords.Append(RemoveDiacritics(word));
            keywords.Append(" ");

        }

        return keywords.ToString().ToLower().Trim();
    }

    /// <summary>
    /// Removes diacritics (accents) from the given string.
    /// </summary>
    /// <param name="text">The input string (e.g., "bánh mì").</param>
    /// <returns>The string without diacritics (e.g., "banh mi").</returns>
    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}


