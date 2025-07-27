using Application.Features.Accounts.Commands.CustomerLogin;
using Contracts.Common.Messages;
using FluentValidation.TestHelper;
using Xunit;

namespace UnitTests.Features.Accounts.Commands;

public class CustomerLoginValidatorTests
{
    private readonly CustomerLoginValidator _validator = new();

    [Theory]
    [InlineData("0939006144")]
    public void Validate_WithValidPhoneNumber_ShouldPass(string phoneNumber)
    {
        var command = new CustomerLoginCommand { PhoneNumber = phoneNumber };
        var result = _validator.TestValidate(command);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenPhoneNumberIsNullOrEmpty_ShouldFail(string phoneNumber)
    {
        var command = new CustomerLoginCommand { PhoneNumber = phoneNumber };
        var result = _validator.TestValidate(command);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("032345678948671698")]
    [InlineData("+844")]
    [InlineData("++84901234567")]
    [InlineData("0978acgb6548213")]
    [InlineData("08abc12345")]
    public void Validate_WhenPhoneNumberTooLongOrShort_ShouldFail(string phoneNumber)
    {
        var command = new CustomerLoginCommand { PhoneNumber = phoneNumber };
        var result = _validator.TestValidate(command);
        Assert.False(result.IsValid);
    }

}
