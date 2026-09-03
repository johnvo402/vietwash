using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Application.Common.Auth;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Vouchers.Queries.CheckCode;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Users;
using Domain.Aggregates.Vouchers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Presentation.Endpoints.Vouchers;
using Swashbuckle.AspNetCore.Annotations;

namespace EcommerceService.Tests;

public class CustomerVoucherAuthorizationTests
{
    [Theory]
    [InlineData("CUSTOMER", 501L, 502L)]
    [InlineData("UNKNOWN", 501L, 501L)]
    [InlineData(null, 501L, 501L)]
    [InlineData("CUSTOMER", null, 501)]
    public async Task UnauthorizedActor_IsForbiddenBeforeAnyCustomerOrVoucherQuery(
        string? role,
        long? actorId,
        long targetId
    )
    {
        var unit = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var result = await new CheckCodeHandler(unit.Object, Actor(role, actorId)).Handle(
            new() { VoucherCode = "PRIVATE", CustomerId = targetId },
            default
        );
        Assert.Equal(403, result.Error!.Status);
        unit.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("ADMIN", 7)]
    [InlineData("MANAGER", 7)]
    [InlineData("STAFF", 7)]
    [InlineData("CUSTOMER", 501)]
    public async Task AuthorizedActor_MayCheckAssignedVoucherForEligibleCustomer(
        string role,
        long actorId
    )
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var unit = CustomerUnit("CUSTOMER", true, false, token);
        var voucher = new Voucher
        {
            Id = 20,
            Code = "SAVE",
            Status = ActivationStatus.Active,
            DiscountFixed = true,
            DiscountValue = 25,
            VoucherCustomers = [new() { CustomerId = 501, VoucherId = 20 }],
        };
        var vouchers = new Mock<IAsyncRepository<Voucher>>(MockBehavior.Strict);
        vouchers
            .Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Voucher, bool>>?>()))
            .Returns(
                (Expression<Func<Voucher, bool>> predicate) =>
                    new CashierReliabilityTests.AsyncRows<Voucher>(
                        new[] { voucher }.Where(predicate.Compile())
                    )
            );
        unit.Setup(x => x.Repository<Voucher>(false)).Returns(vouchers.Object);
        var handler = new CheckCodeHandler(unit.Object, Actor(role, actorId));
        var result = await handler.Handle(
            new() { VoucherCode = " SAVE ", CustomerId = 501 },
            token
        );
        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Value!.DiscountValue);
        // Authorization never bypasses the existing voucher assignment/usage predicate.
        voucher.VoucherCustomers.Single().IsUsed = true;
        Assert.Equal(
            404,
            (await handler.Handle(new() { VoucherCode = "SAVE", CustomerId = 501 }, token))
                .Error!
                .Status
        );
    }

    [Theory]
    [InlineData("STAFF", true, false)]
    [InlineData("MANAGER", true, false)]
    [InlineData("ADMIN", true, false)]
    [InlineData("CUSTOMER", false, false)]
    [InlineData("CUSTOMER", true, true)]
    public async Task IneligibleTarget_IsRejectedBeforeVoucherQuery(
        string role,
        bool active,
        bool disabled
    )
    {
        var unit = CustomerUnit(role, active, disabled, default);
        var result = await new CheckCodeHandler(unit.Object, Actor("STAFF", 7)).Handle(
            new() { VoucherCode = "SAVE", CustomerId = 501 },
            default
        );
        Assert.Equal(404, result.Error!.Status);
        unit.Verify(x => x.Repository<Voucher>(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Endpoint_DeclaresAllowedRolesAndReturnsOkForReadOnlyCheck()
    {
        var method = typeof(CheckCodeVoucherEndpoint).GetMethod("HandleAsync")!;
        var authorization = method.GetCustomAttribute<AuthorizeByAttribute>()!;
        var model = JsonSerializer.Deserialize<AuthorizeModel>(
            authorization.Value,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        Assert.Equal(new[] { "ADMIN", "MANAGER", "STAFF", "CUSTOMER" }, model!.Roles);
        Assert.Equal(
            "Check voucher eligibility",
            method.GetCustomAttribute<SwaggerOperationAttribute>()!.Summary
        );
        var query = new CheckCodeQuery { VoucherCode = "SAVE", CustomerId = 501 };
        var sender = new Mock<ISender>(MockBehavior.Strict);
        sender
            .Setup(x => x.Send(query, default))
            .ReturnsAsync(Result<CheckCodeResponse>.Success(new()));
        var response = await new CheckCodeVoucherEndpoint(sender.Object).HandleAsync(query);
        Assert.IsType<OkObjectResult>(response.Result);
    }

    private static ICurrentAccount Actor(string? role, long? id) =>
        Mock.Of<ICurrentAccount>(x =>
            x.Id == id && x.Session == new UserAuth { Id = id ?? 0, Role = role! }
        );

    private static Mock<IUnitOfWork> CustomerUnit(
        string role,
        bool active,
        bool disabled,
        CancellationToken token
    )
    {
        var customer = new User("Target", null, "0901234567", role, "C501")
        {
            Id = 501,
            Status = active ? ActivationStatus.Active : ActivationStatus.Inactive,
            Disabled = disabled,
        };
        var users = new Mock<IAsyncRepository<User>>(MockBehavior.Strict);
        users
            .Setup(x => x.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), token))
            .Returns(
                (Expression<Func<User, bool>> predicate, CancellationToken _) =>
                    Task.FromResult(predicate.Compile()(customer))
            );
        var unit = new Mock<IUnitOfWork>(MockBehavior.Strict);
        unit.Setup(x => x.Repository<User>(false)).Returns(users.Object);
        return unit;
    }
}
