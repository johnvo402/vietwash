using System.Data.Common;
using System.Linq.Expressions;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Users.Commands.Create;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;

namespace EcommerceService.Tests;

public class UserSynchronizationTests
{
    [Fact]
    public async Task SameCreateEventTwice_SucceedsAndPersistsOneUserWithAuthId()
    {
        var harness = new UserSyncHarness();
        CreateAccountCommand command = CreateCommand(100);

        PubSubResponse<CreateAccountCommand> first = await harness.Handle(command);
        PubSubResponse<CreateAccountCommand> duplicate = await harness.Handle(command);

        Assert.True(first.IsSuccess);
        Assert.True(duplicate.IsSuccess);
        User user = Assert.Single(harness.Users);
        Assert.Equal(100, user.Id);
        Assert.Equal(1, harness.CommitCount);
    }

    [Fact]
    public async Task DuplicateDetectedByPrimaryKeyRace_IsAcknowledgedAsSuccess()
    {
        var harness = new UserSyncHarness();
        CreateAccountCommand command = CreateCommand(100);
        Assert.True((await harness.Handle(command)).IsSuccess);
        harness.BypassExistingUserCheck = true;

        PubSubResponse<CreateAccountCommand> duplicate = await harness.Handle(command);

        Assert.True(duplicate.IsSuccess);
        Assert.Single(harness.Users);
        Assert.Equal(1, harness.RollbackCount);
    }

    [Fact]
    public async Task UnrelatedDatabaseViolation_RemainsTransientFailure()
    {
        var harness = new UserSyncHarness { SaveFailureConstraint = "ix_user_email" };

        PubSubResponse<CreateAccountCommand> result = await harness.Handle(CreateCommand(100));

        Assert.False(result.IsSuccess);
        Assert.Equal(PubSubErrorType.Transient, result.ErrorType);
        Assert.Empty(harness.Users);
        Assert.Equal(1, harness.RollbackCount);
    }

    [Theory]
    [InlineData("pk_user", "user", PostgresErrorCodes.UniqueViolation, true)]
    [InlineData("ix_user_email", "user", PostgresErrorCodes.UniqueViolation, false)]
    [InlineData("pk_user", "branch_user", PostgresErrorCodes.UniqueViolation, false)]
    [InlineData("pk_user", "user", PostgresErrorCodes.ForeignKeyViolation, false)]
    public void DuplicateClassifier_RequiresExactUserPrimaryKey(
        string constraint,
        string table,
        string sqlState,
        bool expected
    )
    {
        DbUpdateException exception = DuplicateException(constraint, table, sqlState);

        Assert.Equal(expected, CreateUserHandler.IsDuplicateUserPrimaryKey(exception));
    }

    [Fact]
    public void DuplicateClassifier_DoesNotSwallowGenericDatabaseFailure() =>
        Assert.False(
            CreateUserHandler.IsDuplicateUserPrimaryKey(
                new DbUpdateException("Database unavailable")
            )
        );

    [Fact]
    public void DuplicateConstraint_MatchesTheActualUserModel()
    {
        var options = new DbContextOptionsBuilder<Infrastructure.Data.TheDbContext>()
            .UseNpgsql("Host=localhost;Database=model_validation")
            .Options;
        using var context = new Infrastructure.Data.TheDbContext(options);
        var user = context.Model.FindEntityType(typeof(User))!;

        Assert.Equal("user", user.GetTableName());
        Assert.Equal(CreateUserHandler.UserPrimaryKeyConstraint, user.FindPrimaryKey()!.GetName());
    }

    private static CreateAccountCommand CreateCommand(long id) =>
        new()
        {
            PayloadId = Guid.NewGuid(),
            Payload = new CreateAccountEvent
            {
                Id = id,
                PublicId = Ulid.NewUlid(),
                DisplayName = "Customer",
                PhoneNumber = "0900000000",
                Role = "CUSTOMER",
                Code = $"CUS-{id}",
                Status = ActivationStatus.Active,
            },
        };

    private static DbUpdateException DuplicateException(
        string constraint,
        string table,
        string sqlState = PostgresErrorCodes.UniqueViolation
    ) =>
        new(
            "Simulated database failure.",
            new PostgresException(
                "duplicate key value violates unique constraint",
                "ERROR",
                "ERROR",
                sqlState,
                "",
                "",
                0,
                0,
                "",
                "",
                "public",
                table,
                "id",
                "bigint",
                constraint,
                "",
                "",
                ""
            )
        );

    private sealed class UserSyncHarness
    {
        private readonly Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        private readonly Mock<IAsyncRepository<User>> users = new(MockBehavior.Strict);
        private readonly Mock<IMediaUpdateService> media = new(MockBehavior.Strict);
        private User? stagedUser;

        public List<User> Users { get; } = [];
        public bool BypassExistingUserCheck { get; set; }
        public string? SaveFailureConstraint { get; set; }
        public int CommitCount { get; private set; }
        public int RollbackCount { get; private set; }

        public UserSyncHarness()
        {
            unitOfWork.Setup(unit => unit.Repository<User>(false)).Returns(users.Object);
            users
                .Setup(repository =>
                    repository.AnyAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(
                    (Expression<Func<User, bool>> criteria, CancellationToken _) =>
                        Task.FromResult(!BypassExistingUserCheck && Users.Any(criteria.Compile()))
                );
            users
                .Setup(repository =>
                    repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())
                )
                .Returns(
                    (User user, CancellationToken _) =>
                    {
                        stagedUser = user;
                        return Task.FromResult(user);
                    }
                );
            unitOfWork
                .Setup(unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<DbTransaction>().Object);
            unitOfWork
                .Setup(unit => unit.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken _) =>
                    {
                        if (SaveFailureConstraint is not null)
                            throw DuplicateException(SaveFailureConstraint, "user");
                        if (stagedUser is not null && Users.Any(user => user.Id == stagedUser.Id))
                            throw DuplicateException(
                                CreateUserHandler.UserPrimaryKeyConstraint,
                                "user"
                            );
                        return Task.CompletedTask;
                    }
                );
            unitOfWork
                .Setup(unit => unit.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken _) =>
                    {
                        if (stagedUser is not null)
                            Users.Add(stagedUser);
                        stagedUser = null;
                        CommitCount++;
                        return Task.CompletedTask;
                    }
                );
            unitOfWork
                .Setup(unit => unit.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken _) =>
                    {
                        stagedUser = null;
                        RollbackCount++;
                        return Task.CompletedTask;
                    }
                );
            media
                .Setup(service => service.DeleteMediaAsync(It.IsAny<string?>()))
                .Returns(Task.CompletedTask);
        }

        public async Task<PubSubResponse<CreateAccountCommand>> Handle(CreateAccountCommand command)
        {
            var handler = new CreateUserHandler(unitOfWork.Object, media.Object);
            return await handler.Handle(command, CancellationToken.None);
        }
    }
}
