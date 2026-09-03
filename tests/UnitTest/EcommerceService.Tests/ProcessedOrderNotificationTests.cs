using System.Linq.Expressions;
using Application.Common.HandleEventDomains.Orders;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Jobs;
using Contracts.Application.Common.Interfaces.Services.Notifications;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.Users;
using Grpc.Core;
using Moq;
using Notification_Grpc;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace EcommerceService.Tests;

public class ProcessedOrderNotificationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeliveryResult_CompletesNormally_AndLogsOnlyUndelivered(bool delivered)
    {
        using var fixture = new Fixture();
        using var cancellation = new CancellationTokenSource();
        fixture.Notification.Setup(x => x.SendNotifyAsync(It.IsAny<SendNotificationRequest>(), cancellation.Token))
            .ReturnsAsync(delivered);

        await fixture.Handler.Handle(new UpdateStatusOrderEvent { Order = fixture.Order }, cancellation.Token);

        fixture.Notification.Verify(x => x.SendNotifyAsync(It.Is<SendNotificationRequest>(r =>
            r.TemplateId == "laundry_processed" && r.Parameters["order_code"] == "OD-1001"
            && r.Parameters["branch_name"] == "Branch Two" && r.Data["order_id"] == "1001"
            && r.UserIds.Count == 1 && r.UserIds[0] == "501"), cancellation.Token), Times.Once);
        fixture.Branches.Verify(x => x.FindByConditionAsync(
            It.IsAny<Expression<Func<BranchUser, bool>>>(),
            It.IsAny<Expression<Func<BranchUser, OnlyId>>>(), cancellation.Token), Times.Once);
        if (delivered)
            Assert.Empty(fixture.Logs.Events);
        else
            AssertFailureLog(fixture, LogEventLevel.Warning, "Processed-order notification was not delivered");
    }

    [Fact]
    public async Task NotificationGrpcUnavailable_IsLoggedAndNeverRethrown()
    {
        using var fixture = new Fixture();
        var failure = new RpcException(new Status(StatusCode.Unavailable, "Notification service is down"));
        fixture.Notification.Setup(x => x.SendNotifyAsync(It.IsAny<SendNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(failure);

        await fixture.Handler.Handle(new UpdateStatusOrderEvent { Order = fixture.Order }, CancellationToken.None);

        var log = AssertFailureLog(fixture, LogEventLevel.Error, "Failed to send processed-order notification");
        Assert.Same(failure, log.Exception);
    }

    [Fact]
    public async Task BranchLookupFailure_IsAlsoBestEffort_AndDoesNotSend()
    {
        using var fixture = new Fixture();
        var failure = new InvalidOperationException("Branch projection is unavailable");
        fixture.Branches.Setup(x => x.FindByConditionAsync(
            It.IsAny<Expression<Func<BranchUser, bool>>>(),
            It.IsAny<Expression<Func<BranchUser, OnlyId>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(failure);

        await fixture.Handler.Handle(new UpdateStatusOrderEvent { Order = fixture.Order }, CancellationToken.None);

        Assert.Same(failure, AssertFailureLog(fixture, LogEventLevel.Error, "Failed to send processed-order notification").Exception);
        fixture.Notification.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MissingBranchName_UsesBranchIdFallback()
    {
        using var fixture = new Fixture();
        fixture.Branches.Setup(x => x.FindByConditionAsync(
            It.IsAny<Expression<Func<BranchUser, bool>>>(),
            It.IsAny<Expression<Func<BranchUser, OnlyId>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OnlyId?)null);
        fixture.Notification.Setup(x => x.SendNotifyAsync(It.IsAny<SendNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await fixture.Handler.Handle(new UpdateStatusOrderEvent { Order = fixture.Order }, CancellationToken.None);

        fixture.Notification.Verify(x => x.SendNotifyAsync(
            It.Is<SendNotificationRequest>(r => r.Parameters["branch_name"] == "#2"), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Empty(fixture.Logs.Events);
    }

    [Fact]
    public async Task OptionalDeliveryCancellation_DoesNotEscapeHandler()
    {
        using var fixture = new Fixture();
        using var cancellation = new CancellationTokenSource();
        fixture.Notification.Setup(x => x.SendNotifyAsync(It.IsAny<SendNotificationRequest>(), cancellation.Token))
            .ThrowsAsync(new OperationCanceledException(cancellation.Token));

        await fixture.Handler.Handle(new UpdateStatusOrderEvent { Order = fixture.Order }, cancellation.Token);

        AssertFailureLog(fixture, LogEventLevel.Error, "Failed to send processed-order notification");
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.InProgress)]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task OtherStatuses_DoNotQueryOrSend(OrderStatus status)
    {
        using var fixture = new Fixture();
        fixture.Order.Status = status;
        await fixture.Handler.Handle(new UpdateStatusOrderEvent { Order = fixture.Order }, CancellationToken.None);
        fixture.UnitOfWork.VerifyNoOtherCalls();
        fixture.Notification.VerifyNoOtherCalls();
        Assert.Empty(fixture.Logs.Events);
    }

    [Fact]
    public async Task NoCustomer_DoesNotPrepareOrSendNotification()
    {
        using var fixture = new Fixture();
        fixture.Order.CustomerId = null;
        await fixture.Handler.Handle(new UpdateStatusOrderEvent { Order = fixture.Order }, CancellationToken.None);
        fixture.UnitOfWork.VerifyNoOtherCalls();
        fixture.Notification.VerifyNoOtherCalls();
    }

    private static LogEvent AssertFailureLog(Fixture fixture, LogEventLevel level, string message)
    {
        var log = Assert.Single(fixture.Logs.Events);
        Assert.Equal(level, log.Level);
        Assert.StartsWith(message, log.MessageTemplate.Text);
        Assert.Equal(1001L, Assert.IsType<ScalarValue>(log.Properties["OrderId"]).Value);
        Assert.Equal("OD-1001", Assert.IsType<ScalarValue>(log.Properties["OrderCode"]).Value);
        Assert.Equal(2L, Assert.IsType<ScalarValue>(log.Properties["BranchId"]).Value);
        Assert.Equal(501L, Assert.IsType<ScalarValue>(log.Properties["CustomerId"]).Value);
        Assert.Equal(OrderStatus.Processed, Assert.IsType<ScalarValue>(log.Properties["Status"]).Value);
        return log;
    }

    private sealed class Fixture : IDisposable
    {
        public Order Order { get; } = new(2, 7, "OD-1001", 100, 110, OrderStatus.Processed, customerId: 501) { Id = 1001 };
        public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Strict);
        public Mock<IAsyncRepository<BranchUser>> Branches { get; } = new(MockBehavior.Strict);
        public Mock<INotificationGrpc> Notification { get; } = new(MockBehavior.Strict);
        public NotificationLogSink Logs { get; } = new();
        private readonly Logger logger;
        public UpdateStatusOrderEventHandler Handler { get; }

        public Fixture()
        {
            logger = new LoggerConfiguration().WriteTo.Sink(Logs).CreateLogger();
            UnitOfWork.Setup(x => x.Repository<BranchUser>(false)).Returns(Branches.Object);
            Branches.Setup(x => x.FindByConditionAsync(
                It.IsAny<Expression<Func<BranchUser, bool>>>(),
                It.IsAny<Expression<Func<BranchUser, OnlyId>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OnlyId { Name = "Branch Two" });
            Handler = new UpdateStatusOrderEventHandler(UnitOfWork.Object, Notification.Object, logger);
        }

        public void Dispose() => logger.Dispose();
    }
}

internal sealed class NotificationLogSink : ILogEventSink
{
    public List<LogEvent> Events { get; } = [];
    public void Emit(LogEvent logEvent) => Events.Add(logEvent);
}
