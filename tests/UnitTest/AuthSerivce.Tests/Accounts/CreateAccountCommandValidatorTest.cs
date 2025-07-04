using System.Text.RegularExpressions;
using Application.Features.Accounts.Commands.Create;
using AuthSerivce.Tests;
using AutoFixture;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;

namespace AuthService.Tests.Accounts
{
    public partial class CreateAccountCommandValidatorTests
    {
        private readonly InlineValidator<CreateAccountCommand> validator;
        private readonly Fixture fixture;
        private readonly CreateAccountCommand command;

        public CreateAccountCommandValidatorTests()
        {
            validator = new InlineValidator<CreateAccountCommand>();
            fixture = new Fixture();

            // Setup command with valid defaults
            command = fixture
                .Build<CreateAccountCommand>()
                .With(x => x.DisplayName, "John Doe")
                .With(x => x.PhoneNumber, "+1234567890")
                .With(x => x.Email, "test@example.com")
                .With(x => x.Password, "Password123")
                .With(x => x.Gender, Gender.Male)
                .With(x => x.Status, AccountStatus.Active)
                .With(x => x.Role, "ADMIN")
                .Create();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_WhenDisplayNameNullOrEmpty_ShouldReturnNullFailure(
            string? displayName
        )
        {
            // Arrange
            command.DisplayName = displayName;
            var expectedState = Messager
                .Create<Account>()
                .Property(x => x.DisplayName)
                .Message(MessageType.Null)
                .Negative()
                .Build();
            validator.RuleFor(x => x.DisplayName).NotEmpty().WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result
                .ShouldHaveValidationErrorFor(x => x.DisplayName)
                .WithCustomState(expectedState, new MessageResultComparer())
                .Only();
        }

        [Fact]
        public async Task Validate_WhenDisplayNameExceedsMaxLength_ShouldReturnMaximumLengthFailure()
        {
            // Arrange
            command.DisplayName = new string('A', 257);
            var expectedState = Messager
                .Create<Account>()
                .Property(x => x.DisplayName)
                .Message(MessageType.MaximumLength)
                .Build();

            validator.RuleFor(x => x.DisplayName).MaximumLength(256).WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result
                .ShouldHaveValidationErrorFor(x => x.DisplayName)
                .WithCustomState(expectedState)
                .Only();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_WhenPhoneNumberNullOrEmpty_ShouldReturnNullFailure(
            string? phoneNumber
        )
        {
            // Arrange
            command.PhoneNumber = phoneNumber;
            var expectedState = Messager
                .Create<Account>()
                .Property(x => x.PhoneNumber)
                .Message(MessageType.Null)
                .Negative()
                .Build();

            validator.RuleFor(x => x.PhoneNumber).NotEmpty().WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result
                .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
                .WithCustomState(expectedState)
                .Only();
        }

        [Theory]
        [InlineData("123")]
        [InlineData("1234567890123456")]
        public async Task Validate_WhenPhoneNumberInvalidFormat_ShouldReturnInvalidFailure(
            string phoneNumber
        )
        {
            // Arrange
            command.PhoneNumber = phoneNumber;
            var expectedState = Messager
                .Create<Account>()
                .Property(x => x.PhoneNumber)
                .Message(MessageType.Valid)
                .Negative()
                .Build();

            validator
                .RuleFor(x => x.PhoneNumber)
                .Must(IsValidPhoneNumber)
                .WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result
                .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
                .WithCustomState(expectedState)
                .Only();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_WhenEmailNullOrEmpty_ShouldReturnNullFailure(string? email)
        {
            // Arrange
            command.Email = email;
            var expectedState = Messager
                .Create<Account>()
                .Property(x => x.Email)
                .Message(MessageType.Null)
                .Negative()
                .Build();

            validator.RuleFor(x => x.Email).NotEmpty().WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result
                .ShouldHaveValidationErrorFor(x => x.Email)
                .WithCustomState(expectedState)
                .Only();
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("test@.com")]
        [InlineData("@example.com")]
        public async Task Validate_WhenEmailInvalidFormat_ShouldReturnInvalidFailure(string email)
        {
            // Arrange
            command.Email = email;
            var expectedState = Messager
                .Create<Account>()
                .Property(x => x.Email)
                .Message(MessageType.Valid)
                .Negative()
                .Build();

            validator.RuleFor(x => x.Email).Must(IsValidEmail).WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result
                .ShouldHaveValidationErrorFor(x => x.Email)
                .WithCustomState(expectedState)
                .Only();
        }

        [Fact]
        public async Task Validate_WhenEmailDuplicated_ShouldReturnExistenceFailure()
        {
            // Arrange
            command.Email = "duplicate@example.com";
            var expectedState = Messager
                .Create<Account>()
                .Property(x => x.Email)
                .Message(MessageType.Existence)
                .Build();

            var moqCheck = false;

            validator
                .RuleFor(x => x.Email)
                .MustAsync(async (email, cancellation) => moqCheck)
                .WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result
                .ShouldHaveValidationErrorFor(x => x.Email)
                .WithCustomState(expectedState)
                .Only();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_WhenPasswordNullOrEmpty_ShouldReturnInvalidFailure(
            string? password
        )
        {
            // Arrange
            command.Password = password;
            var expectedState = Messager
                .Create<CreateAccountCommand>(nameof(Account))
                .Property(x => x.Password!)
                .Message(MessageType.Strong)
                .Negative()
                .Build();

            validator.RuleFor(x => x.Password).Must(IsValidPassword).WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result
                .ShouldHaveValidationErrorFor(x => x.Password)
                .WithCustomState(expectedState)
                .Only();
        }

        [Theory]
        [InlineData("weak")]
        [InlineData("nopassword123")]
        [InlineData("NoNumbersHere")]
        public async Task Validate_WhenPasswordInvalidFormat_ShouldReturnInvalidFailure(
            string password
        )
        {
            // Arrange
            command.Password = password;
            var expectedState = Messager
                .Create<CreateAccountCommand>(nameof(Account))
                .Property(x => x.Password!)
                .Message(MessageType.Strong)
                .Negative()
                .Build();

            validator.RuleFor(x => x.Password).Must(IsValidPassword).WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result
                .ShouldHaveValidationErrorFor(x => x.Password)
                .WithCustomState(expectedState)
                .Only();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(999)]
        public async Task Validate_WhenGenderInvalid_ShouldReturnOuttaOptionFailure(int gender)
        {
            // Arrange
            command.Gender = (Gender)gender;
            var expectedState = Messager
                .Create<CreateAccountCommand>(nameof(Account))
                .Property(x => x.Gender!)
                .Message(MessageType.OuttaOption)
                .Build();

            validator.RuleFor(x => x.Gender).IsInEnum().WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result
                .ShouldHaveValidationErrorFor(x => x.Gender)
                .WithCustomState(expectedState)
                .Only();
        }

        [Fact]
        public async Task Validate_WhenStatusNull_ShouldReturnNullFailure()
        {
            // Arrange
            command.Status = 0;
            var expectedState = Messager
                .Create<CreateAccountCommand>(nameof(Account))
                .Property(x => x.Status!)
                .Message(MessageType.Null)
                .Negative()
                .Build();

            validator.RuleFor(x => x.Status).NotEmpty().WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result
                .ShouldHaveValidationErrorFor(x => x.Status)
                .WithCustomState(expectedState)
                .Only();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(999)]
        public async Task Validate_WhenStatusInvalid_ShouldReturnOuttaOptionFailure(int status)
        {
            // Arrange
            command.Status = (AccountStatus)status;
            var expectedState = Messager
                .Create<CreateAccountCommand>(nameof(Account))
                .Property(x => x.Status!)
                .Message(MessageType.OuttaOption)
                .Build();

            validator.RuleFor(x => x.Status).IsInEnum().WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result
                .ShouldHaveValidationErrorFor(x => x.Status)
                .WithCustomState(expectedState)
                .Only();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_WhenRoleNullOrEmpty_ShouldReturnNullFailure(string? role)
        {
            // Arrange
            command.Role = role;
            var expectedState = Messager
                .Create<CreateAccountCommand>(nameof(Account))
                .Property(x => x.Role)
                .Message(MessageType.Null)
                .Negative()
                .Build();

            validator.RuleFor(x => x.Role).NotEmpty().WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Role).WithCustomState(expectedState).Only();
        }

        [Fact]
        public async Task Validate_WhenRoleNotFound_ShouldReturnNotFoundFailure()
        {
            // Arrange
            command.Role = "INVALID_ROLE";
            var expectedState = Messager
                .Create<CreateAccountCommand>(nameof(Account))
                .Property(x => x.Role)
                .Message(MessageType.Found)
                .Negative()
                .Build();

            validator.RuleFor(x => x.Role).Must(IsValidRole).WithState(x => expectedState);

            // Act
            var result = await validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Role).WithCustomState(expectedState).Only();
        }

        private static bool IsValidPhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return false;
            return PhoneValidationRegex().IsMatch(phoneNumber);
        }

        private static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            return EmailValidationRegex().IsMatch(email);
        }

        private static bool IsValidPassword(string? password)
        {
            if (string.IsNullOrEmpty(password))
                return false;
            return PasswordValidationRegex().IsMatch(password);
        }

        private static bool IsValidRole(string? role)
        {
            if (string.IsNullOrEmpty(role))
                return false;
            return new List<string> { "ADMIN", "MANAGER", "STAFF", "CUSTOMER" }.Contains(role);
        }

        [GeneratedRegex(@"^\+?\d{7,15}$")]
        private static partial Regex PhoneValidationRegex();

        [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
        private static partial Regex EmailValidationRegex();

        [GeneratedRegex(@"^((?=\S*?[A-Z])(?=\S*?[a-z])(?=\S*?[0-9]).{8,})\S$")]
        private static partial Regex PasswordValidationRegex();
    }
}
