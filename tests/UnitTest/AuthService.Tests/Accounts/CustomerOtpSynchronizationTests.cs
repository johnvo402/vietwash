using System.Data.Common;
using System.Text.Json;
using Application.Common.Auth;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Accounts.Commands.VerifyOtpLoginCustomer;
using Contracts.Application.Common.Interfaces.Services.Token;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Events;
using Domain.Otp;
using Infrastructure.Constants;
using Moq;
using Shared.Kernel.Common.Specs.Interfaces;

namespace AuthService.Tests.Accounts;

public class CustomerOtpSynchronizationTests
{
    [Fact]
    public async Task InvalidOtp_DoesNotVerifySynchronizeOrCreateLoginSideEffects()
    {
        Account account = CustomerAccount();
        var harness = new OtpHarness(account, otpIsValid: false);

        var result = await harness.Handle();

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.Verified);
        Assert.False(account.Verified);
        Assert.Empty(account.UncommittedEvents);
        harness.UnitOfWork.VerifyNoOtherCalls();
        harness.TokenFactory.VerifyNoOtherCalls();
        harness.SecurityService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task NewCustomer_ValidOtp_VerifiesPersistsAndEmitsSynchronizationEvent()
    {
        var harness = new OtpHarness(existingAccount: null, otpIsValid: true);

        var result = await harness.Handle();

        Account account = Assert.IsType<Account>(harness.PersistedAccount);
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Verified);
        Assert.True(result.Value.IsNew);
        Assert.True(account.Verified);
        Assert.Equal(ROLE.CUSTOMER, account.Role);
        Assert.IsType<AccountCreateEvent>(Assert.Single(account.UncommittedEvents));
        Assert.Equal(account.Id.ToString(), harness.SessionUserId);
        UserAuth session = JsonSerializer.Deserialize<UserAuth>(
            harness.SessionJson!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        )!;
        Assert.Equal(account.Id, session.Id);
        Assert.Equal(ROLE.CUSTOMER, session.Role);
        Assert.Empty(session.Branches!);
        harness.Accounts.Verify(
            repository => repository.AddAsync(account, It.IsAny<CancellationToken>()),
            Times.Once
        );
        harness.UnitOfWork.Verify(
            unit => unit.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        harness.UnitOfWork.Verify(
            unit => unit.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task OldUnverifiedCustomer_ValidOtp_SelfHealsAndEmitsSynchronizationEvent()
    {
        Account account = CustomerAccount();
        var harness = new OtpHarness(account, otpIsValid: true);

        var result = await harness.Handle();

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Verified);
        Assert.False(result.Value.IsNew);
        Assert.True(account.Verified);
        Assert.IsType<AccountCreateEvent>(Assert.Single(account.UncommittedEvents));
        harness.Accounts.Verify(repository => repository.UpdateAsync(account), Times.Once);
    }

    [Fact]
    public async Task AlreadyVerifiedCustomer_ValidOtp_LogsInWithoutDuplicateSynchronization()
    {
        Account account = CustomerAccount();
        account.VerifiedCustomer();
        var harness = new OtpHarness(account, otpIsValid: true);

        var result = await harness.Handle();

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Verified);
        Assert.False(result.Value.IsNew);
        Assert.True(account.Verified);
        Assert.Empty(account.UncommittedEvents);
        harness.Accounts.Verify(
            repository => repository.UpdateAsync(It.IsAny<Account>()),
            Times.Never
        );
        harness.Accounts.Verify(
            repository => repository.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SaveFailure_RollsBackAndDoesNotCreateASuccessfulLoginSession()
    {
        var harness = new OtpHarness(existingAccount: null, otpIsValid: true) { FailSave = true };

        await Assert.ThrowsAsync<InvalidOperationException>(() => harness.Handle());

        harness.UnitOfWork.Verify(
            unit => unit.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
        harness.UnitOfWork.Verify(
            unit => unit.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        harness.SecurityService.VerifyNoOtherCalls();
    }

    private static Account CustomerAccount() =>
        new("Customer", null, null, "0900000000", ROLE.CUSTOMER, "CUS-1");

    private sealed class OtpHarness
    {
        private readonly Mock<ICurrentAccount> currentAccount = new(MockBehavior.Strict);
        private readonly Mock<ISmsOtpClient> smsClient = new(MockBehavior.Strict);
        private readonly Mock<IDynamicSpecificationRepository<Account>> accountLookup = new(
            MockBehavior.Strict
        );
        private readonly Mock<IAsyncRepository<AccountToken>> accountTokens = new(
            MockBehavior.Strict
        );

        public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Strict);
        public Mock<IAsyncRepository<Account>> Accounts { get; } = new(MockBehavior.Strict);
        public Mock<ITokenFactory> TokenFactory { get; } = new(MockBehavior.Strict);
        public Mock<ITokenSecurityService> SecurityService { get; } = new(MockBehavior.Strict);
        public Account? PersistedAccount { get; private set; }
        public bool FailSave { get; set; }
        public string? SessionUserId { get; private set; }
        public string? SessionJson { get; private set; }

        public OtpHarness(Account? existingAccount, bool otpIsValid)
        {
            currentAccount.SetupGet(account => account.ClientIp).Returns("127.0.0.1");
            smsClient
                .Setup(client =>
                    client.VerifyAsync(It.IsAny<VerifyPinRequest>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(otpIsValid);

            if (!otpIsValid)
                return;

            accountLookup
                .Setup(repository =>
                    repository.FindByConditionAsync(
                        It.IsAny<ISpecification<Account>>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(existingAccount);
            UnitOfWork
                .Setup(unit => unit.DynamicReadOnlyRepository<Account>(false))
                .Returns(accountLookup.Object);
            UnitOfWork.Setup(unit => unit.Repository<Account>(false)).Returns(Accounts.Object);
            UnitOfWork
                .Setup(unit => unit.Repository<AccountToken>(false))
                .Returns(accountTokens.Object);
            UnitOfWork
                .Setup(unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<DbTransaction>().Object);
            UnitOfWork
                .Setup(unit => unit.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken _) =>
                    {
                        if (FailSave)
                            throw new InvalidOperationException("Simulated save failure");
                        return Task.CompletedTask;
                    }
                );
            UnitOfWork
                .Setup(unit => unit.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            UnitOfWork
                .Setup(unit => unit.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Accounts
                .Setup(repository =>
                    repository.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>())
                )
                .Returns(
                    (Account account, CancellationToken _) =>
                    {
                        PersistedAccount = account;
                        return Task.FromResult(account);
                    }
                );
            Accounts
                .Setup(repository => repository.UpdateAsync(It.IsAny<Account>()))
                .Returns(
                    (Account account) =>
                    {
                        PersistedAccount = account;
                        return Task.CompletedTask;
                    }
                );
            accountTokens
                .Setup(repository =>
                    repository.AddAsync(It.IsAny<AccountToken>(), It.IsAny<CancellationToken>())
                )
                .Returns((AccountToken token, CancellationToken _) => Task.FromResult(token));

            DateTime accessExpiry = DateTime.UtcNow.AddMinutes(30);
            TokenFactory.SetupGet(factory => factory.AccesstokenExpiredTime).Returns(accessExpiry);
            TokenFactory
                .Setup(factory =>
                    factory.CreateToken(
                        It.IsAny<IEnumerable<KeyValuePair<string, object>>>(),
                        It.IsAny<DateTime>()
                    )
                )
                .Returns("signed-token");
            SecurityService
                .Setup(service =>
                    service.AddSessionUserAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<TimeSpan>()
                    )
                )
                .Returns(
                    (string userId, string sessionJson, TimeSpan _) =>
                    {
                        SessionUserId = userId;
                        SessionJson = sessionJson;
                        return Task.CompletedTask;
                    }
                );
        }

        public async Task<Contracts.ApiWrapper.Result<VerifyOtpResponse>> Handle()
        {
            var handler = new VerifyOtpHandler(
                currentAccount.Object,
                smsClient.Object,
                UnitOfWork.Object,
                TokenFactory.Object,
                SecurityService.Object
            );
            return await handler.Handle(
                new VerifyOtpCommand { PhoneNumber = "0900000000", Otp = "123456" },
                CancellationToken.None
            );
        }
    }
}
