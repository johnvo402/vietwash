using System.Linq.Expressions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Accounts.Commands.Create;
using AuthService.Tests;
using AutoFixture;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using CSharpFunctionalExtensions;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using FluentValidation.TestHelper;
using Moq;

namespace AuthService.Tests.Accounts;

public class CreateAccountCommandValidatorTests
{
    private readonly CreateAccountCommandValidator validator;
    private readonly Fixture fixture;

    public CreateAccountCommandValidatorTests()
    {
        // Mock dependencies
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var accessorServiceMock = new Mock<IActionAccessorService>();

        // Khởi tạo validator có DI
        fixture = new Fixture();
    }
    private CreateAccountCommandValidator GetValidator(bool phoneExists = false)
    {
        var repoMock = new Mock<IAsyncRepository<Account>>();
        repoMock
            .Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Account, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(phoneExists);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock
            .Setup(x => x.Repository<Account>(It.IsAny<bool>()))
            .Returns(repoMock.Object);

        var accessorMock = new Mock<IActionAccessorService>();
        accessorMock.Setup(x => x.Id).Returns("1");

        return new CreateAccountCommandValidator(uowMock.Object, accessorMock.Object);
    }

    private CreateAccountCommand ValidCommand() =>
        fixture.Build<CreateAccountCommand>()
            .With(x => x.DisplayName, "John Doe")
            .With(x => x.PhoneNumber, "+1234567890")
            .With(x => x.Email, "test@example.com")
            .With(x => x.Password, "Password123")
            .With(x => x.Gender, Gender.Male)
            .With(x => x.Status, AccountStatus.Active)
            .With(x => x.Role, "ADMIN")
            .Create();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task DisplayName_NullOrEmpty_ShouldFail(string? displayName)
    {
        var command = ValidCommand();
        command.DisplayName = displayName;
        var validator = GetValidator();

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }

    [Fact]
    public async Task DisplayName_TooLong_ShouldFail()
    {
        var command = ValidCommand();
        command.DisplayName = new string('X', 257);
        var validator = GetValidator();

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }
    [Fact]
    public async Task DisplayName_Valid_ShouldPass()
    {
        var validator = GetValidator();
        var command = ValidCommand();
        command.DisplayName = "Valid Name";

        var result = await validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.DisplayName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task PhoneNumber_NullOrEmpty_ShouldFail(string? phoneNumber)
    {
        var validator = GetValidator();
        var command = ValidCommand();
        command.PhoneNumber = phoneNumber;

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234567890123456")]
    public async Task PhoneNumber_InvalidFormat_ShouldFail(string phone)
    {
        var validator = GetValidator();
        var command = ValidCommand();
        command.PhoneNumber = phone;

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }
    [Fact]
    public async Task PhoneNumber_ValidFormat_ShouldPass()
    {
        var validator = GetValidator();
        var command = ValidCommand();
        command.PhoneNumber = "+1234567890";

        var result = await validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }
    [Fact]
    public async Task PhoneNumber_ExistingPhone_ShouldFail()
    {
        var validator = GetValidator(true);
        // Arrange
        var command = ValidCommand();
        command.PhoneNumber = "+1234567890";

        var accountRepoMock = new Mock<IAsyncRepository<Account>>();
        accountRepoMock
            .Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // giả lập: số đã tồn tại

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.Repository<Account>(It.IsAny<bool>()))
            .Returns(accountRepoMock.Object); // gán mock repo vào UoW

        var accessorMock = new Mock<IActionAccessorService>();
        accessorMock.Setup(x => x.Id).Returns("1");

        var validatorWithMock = new CreateAccountCommandValidator(unitOfWorkMock.Object, accessorMock.Object);

        // Act
        var result = await validatorWithMock.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Email_NullOrEmpty_ShouldFail(string? email)
    {
        var validator = GetValidator();
        var command = ValidCommand();
        command.Email = email;

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("test@.com")]
    public async Task Email_InvalidFormat_ShouldFail(string email)
    {
        var validator = GetValidator();
        var command = ValidCommand();
        command.Email = email;

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    // [Theory]
    // [InlineData(null)]
    // [InlineData("")]
    // public async Task Password_WeakOrEmpty_ShouldFail(string? password)
    // {
    //     var command = ValidCommand();
    //     command.Password = password;

    //     var result = await validator.TestValidateAsync(command);
    //     result.ShouldHaveValidationErrorFor(x => x.Password);
    // }

    [Theory]
    [InlineData("nopass123")]
    [InlineData("NOLOWER123")]
    [InlineData("NoNumber")]
    public async Task Password_InvalidFormat_ShouldFail(string password)
    {
        var validator = GetValidator();
        var command = ValidCommand();
        command.Password = password;

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public async Task Gender_InvalidEnum_ShouldFail(int genderValue)
    {
        var validator = GetValidator();
        var command = ValidCommand();
        command.Gender = (Gender)genderValue;

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Gender);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public async Task Status_InvalidEnum_ShouldFail(int statusValue)
    {
        var validator = GetValidator();
        var command = ValidCommand();
        command.Status = (AccountStatus)statusValue;

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Role_NullOrEmpty_ShouldFail(string? role)
    {
        var validator = GetValidator();
        var command = ValidCommand();
        command.Role = role;

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public async Task Role_InvalidRole_ShouldFail()
    {
        var validator = GetValidator();
        var command = ValidCommand();
        command.Role = "INVALID_ROLE";

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }
}
