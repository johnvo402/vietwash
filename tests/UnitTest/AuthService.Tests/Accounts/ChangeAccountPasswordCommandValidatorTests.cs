using Application.Features.Accounts.Commands.ChangePassword;
using FluentValidation.TestHelper;
using Xunit.Abstractions;

namespace AuthService.Tests.Accounts;

public class ChangeAccountPasswordCommandValidatorTests
{
    private readonly ChangeAccountPasswordCommandValidator validator;
    private readonly ITestOutputHelper _output;

    public ChangeAccountPasswordCommandValidatorTests(ITestOutputHelper output)
    {
        validator = new ChangeAccountPasswordCommandValidator();
        _output = output;
    }

    private ChangeAccountPasswordCommand ValidCommand() =>
        new()
        {
            OldPassword = "Test123@",
            NewPassword = "Test456@A"
        };

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void OldPassword_Empty_ShouldFail(string? oldPassword)
    {
        var command = ValidCommand();
        command.OldPassword = oldPassword;

        var result = validator.TestValidate(command);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NewPassword_Empty_ShouldFail(string? newPassword)
    {
        var command = ValidCommand();
        command.NewPassword = newPassword;

        var result = validator.TestValidate(command);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("test456@")]     // No uppercase
    [InlineData("Test")]         // Too short, no number
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // Too long
    [InlineData("Tesnt}ewp")]    // Contains special char `}`
    [InlineData("Tes>ne@123")]   // Contains disallowed char `>`
    public void NewPassword_Invalid_ShouldFail(string newPassword)
    {
        var command = ValidCommand();
        command.NewPassword = newPassword;

        var result = validator.TestValidate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void NewPassword_Valid_ShouldPass()
    {
        var command = ValidCommand();
        var result = validator.TestValidate(command);
        _output.WriteLine("Kết quả test: " + result.ToString());
        Assert.True(result.IsValid);
    }
}
