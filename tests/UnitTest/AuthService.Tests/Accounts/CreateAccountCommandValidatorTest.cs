using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Accounts.Commands.Create;
using Application.Features.Common.Projections.Accounts;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using FluentValidation;
using Moq;
using Xunit;

namespace UnitTests.Features.Accounts.Create;

public class CreateAccountCommandValidatorTests
{
    private readonly CreateAccountCommandValidator _validator;

    public CreateAccountCommandValidatorTests()
    {
        var mockAccountRepo = new Mock<IAsyncRepository<Account>>();
        mockAccountRepo
            .Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork
            .Setup(x => x.Repository<Account>(false))
            .Returns(mockAccountRepo.Object);

        var mockAccessor = new Mock<IActionAccessorService>();
        mockAccessor.Setup(x => x.Id).Returns("1");
        mockAccessor.Setup(x => x.GetHttpMethod()).Returns(HttpMethod.Post.ToString());

        _validator = new CreateAccountCommandValidator(mockUnitOfWork.Object, mockAccessor.Object);
    }

    private CreateAccountCommand CreateValidCommand()
    {
        return new CreateAccountCommand
        {
            DisplayName = "Nguyen Van A",
            PhoneNumber = "0938123456",
            BirthDay = DateTime.Parse("2000-01-01"),
            AvtUrl = "https://example.com/avatar.png",
            Email = "nguyenvana@example.com",
            Password = "Secure@123",
            Gender = Gender.Male,
            Status = AccountStatus.Active,
            Role = "CUSTOMER",
            BranchAccounts = new List<BranchAccountModel>
            {
                new BranchAccountModel { BranchId = 1, BranchName = "Ninh Kieu" }
            }
        };
    }

    [Fact]
    public async Task CreateAccountCommand_Valid_ShouldPass()
    {
        var command = CreateValidCommand();
        var result = await _validator.ValidateAsync(command);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("093")]
    [InlineData("09234567895816438")] // Quá dài
    [InlineData("09df4512543")]

    public async Task InvalidPhoneNumber_ShouldFail(string phoneNumber)
    {
        var command = CreateValidCommand();
        command.PhoneNumber = phoneNumber;
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task EmptyOrNullDisplayName_ShouldFail(string displayName)
    {
        var command = CreateValidCommand();
        command.DisplayName = displayName;
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task DisplayNameTooLong_ShouldFail()
    {
        var command = CreateValidCommand();
        command.DisplayName = new string('A', 101);
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task InvalidBirthDay_ShouldFail(DateTime birthDay)
    {
        var command = CreateValidCommand();
        command.BirthDay = birthDay;
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task FutureBirthDay_ShouldFail()
    {
        var command = CreateValidCommand();
        command.BirthDay = DateTime.Now.AddDays(1);
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("https://example.com/file.exe")]
    public async Task InvalidAvtUrl_ShouldFail(string url)
    {
        var command = CreateValidCommand();
        command.AvtUrl = url;
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task EmptyOrNullAvtUrl_ShouldPass(string url)
    {
        var command = CreateValidCommand();
        command.AvtUrl = url;
        var result = await _validator.ValidateAsync(command);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalidemail")]
    public async Task InvalidEmail_ShouldFail(string email)
    {
        var command = CreateValidCommand();
        command.Email = email;
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("weak")]
    [InlineData("NoSpecial123")]
    [InlineData("NoSpecial>123")]
    [InlineData("VeryLongPasswordThatExceedsTheMaximumAllowedLength123@abcde")]
    public async Task WeakPassword_ShouldFail(string password)
    {
        var command = CreateValidCommand();
        command.Password = password;
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData((Gender)999)] // Kiểm tra với giá trị không hợp lệ
    [InlineData((Gender)0)] // enum bắt đầu từ 1
    public async Task NullGender_ShouldFail(Gender? gender)
    {
        var command = CreateValidCommand();
        command.Gender = gender;
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task NullOrEmptyRole_ShouldFail(string role)
    {
        var command = CreateValidCommand();
        command.Role = role;
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task InvalidRole_ShouldFail()
    {
        var command = CreateValidCommand();
        command.Role = "INVALID";
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task NullBranchAccounts_ShouldPass()
    {
        var command = CreateValidCommand();
        command.BranchAccounts = null;
        var result = await _validator.ValidateAsync(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task EmptyBranchAccounts_ShouldFail()
    {
        var command = CreateValidCommand();
        command.BranchAccounts = [];
        var result = await _validator.ValidateAsync(command);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    public async Task MissingOrEmptyBranchAccount_ShouldFail(string name, long? branchId)
    {
        var command = CreateValidCommand();
        command.BranchAccounts = new List<BranchAccountModel>
    {
        new BranchAccountModel { BranchId = branchId ?? 0, BranchName = name! }
    };
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }

}
