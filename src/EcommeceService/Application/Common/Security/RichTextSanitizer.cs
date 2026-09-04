using Ganss.Xss;

namespace Application.Common.Security;

public static class RichTextSanitizer
{
    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

    public static string? Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return html;

        return Sanitizer.Sanitize(html);
    }

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedAttributes.Clear();

        foreach (
            string tag in new[]
            {
                "p",
                "br",
                "strong",
                "b",
                "em",
                "i",
                "u",
                "s",
                "h1",
                "h2",
                "h3",
                "ul",
                "ol",
                "li",
                "blockquote",
                "pre",
                "code",
            }
        )
            sanitizer.AllowedTags.Add(tag);

        return sanitizer;
    }
}
