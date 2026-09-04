using Application.Common.Security;

namespace EcommerceService.Tests;

public sealed class RichTextSanitizerTests
{
    [Theory]
    [InlineData("<img src=x onerror=alert(1)><p>Safe</p>", "<p>Safe</p>")]
    [InlineData("<script>alert(1)</script><strong>Bold</strong>", "<strong>Bold</strong>")]
    [InlineData("<p onclick=alert(1)>Text</p>", "<p>Text</p>")]
    public void Sanitize_RemovesExecutableMarkup(string input, string expected) =>
        Assert.Equal(expected, RichTextSanitizer.Sanitize(input));

    [Fact]
    public void Sanitize_PreservesEditorFormatting()
    {
        const string input = "<h2>Title</h2><blockquote><p><u>Body</u></p></blockquote><ul><li>One</li></ul>";

        Assert.Equal(input, RichTextSanitizer.Sanitize(input));
    }
}
