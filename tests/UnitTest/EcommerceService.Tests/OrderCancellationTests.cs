using Application.Feature.Orders.Command.UpdateStatus;
using Application.Feature.Orders.Queries.GetLinkPayment;
using Domain.Aggregates.Orders.Enums;
using Moq;
using Net.payOS.Errors;
using Net.payOS.Types;
using Serilog;

namespace EcommerceService.Tests;

public class OrderCancellationTests
{
    [Theory]
    [InlineData("PENDING", ProcessedOrderPaymentState.Pending)]
    [InlineData("PROCESSING", ProcessedOrderPaymentState.Processing)]
    [InlineData("PAID", ProcessedOrderPaymentState.Paid)]
    [InlineData("CANCELLED", ProcessedOrderPaymentState.Cancelled)]
    [InlineData("unexpected", ProcessedOrderPaymentState.Unknown)]
    [InlineData(null, ProcessedOrderPaymentState.Unknown)]
    public void PayOsState_IsHandledExplicitly(
        string? providerState,
        ProcessedOrderPaymentState expected
    ) => Assert.Equal(expected, ProcessedOrderPaymentCancellation.GetState(providerState));

    [Fact]
    public async Task ProcessedCancellation_NoPaymentLinkCanProceed()
    {
        Mock<IOrderPaymentLinkClient> client = new(MockBehavior.Strict);
        client
            .Setup(x => x.GetPaymentLinkInformationAsync(10))
            .ThrowsAsync(new PayOSError(PayOsErrorPolicy.PaymentLinkNotFoundCode, "Not found"));

        ProcessedOrderPaymentCancellationResult result =
            await ProcessedOrderPaymentCancellation.EnsureSafeAsync(
                client.Object,
                10,
                "Customer request",
                Mock.Of<ILogger>()
            );

        Assert.True(result.IsSafe);
        Assert.Equal(ProcessedOrderPaymentState.NotFound, result.State);
        client.Verify(
            x => x.CancelPaymentLinkAsync(It.IsAny<long>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Theory]
    [InlineData("401")]
    [InlineData("20")]
    [InlineData("01")]
    public async Task ProcessedCancellation_OtherProviderErrorsFailClosed(string code)
    {
        Mock<IOrderPaymentLinkClient> client = new(MockBehavior.Strict);
        client
            .Setup(x => x.GetPaymentLinkInformationAsync(10))
            .ThrowsAsync(new PayOSError(code, "Provider failure"));

        ProcessedOrderPaymentCancellationResult result =
            await ProcessedOrderPaymentCancellation.EnsureSafeAsync(
                client.Object,
                10,
                "Customer request",
                Mock.Of<ILogger>()
            );

        Assert.False(result.IsSafe);
        client.Verify(
            x => x.CancelPaymentLinkAsync(It.IsAny<long>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ProcessedCancellation_PendingLinkMustBeRemotelyCancelled()
    {
        Mock<IOrderPaymentLinkClient> client = new(MockBehavior.Strict);
        client.Setup(x => x.GetPaymentLinkInformationAsync(10)).ReturnsAsync(Link("PENDING"));
        client
            .Setup(x => x.CancelPaymentLinkAsync(10, "Customer request"))
            .ReturnsAsync(Link("CANCELLED"));

        ProcessedOrderPaymentCancellationResult result =
            await ProcessedOrderPaymentCancellation.EnsureSafeAsync(
                client.Object,
                10,
                "Customer request",
                Mock.Of<ILogger>()
            );

        Assert.True(result.IsSafe);
        Assert.Equal(ProcessedOrderPaymentState.Cancelled, result.State);
        client.Verify(x => x.CancelPaymentLinkAsync(10, "Customer request"), Times.Once);
    }

    [Fact]
    public async Task ProcessedCancellation_AlreadyCancelledLinkIsIdempotentlySafe()
    {
        Mock<IOrderPaymentLinkClient> client = new(MockBehavior.Strict);
        client.Setup(x => x.GetPaymentLinkInformationAsync(10)).ReturnsAsync(Link("CANCELLED"));

        ProcessedOrderPaymentCancellationResult result =
            await ProcessedOrderPaymentCancellation.EnsureSafeAsync(
                client.Object,
                10,
                "Customer request",
                Mock.Of<ILogger>()
            );

        Assert.True(result.IsSafe);
        client.Verify(
            x => x.CancelPaymentLinkAsync(It.IsAny<long>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Theory]
    [InlineData("PAID", ProcessedOrderPaymentState.Paid)]
    [InlineData("PROCESSING", ProcessedOrderPaymentState.Processing)]
    [InlineData("UNKNOWN_STATE", ProcessedOrderPaymentState.Unknown)]
    public async Task ProcessedCancellation_UnsafeProviderStatesRejectLocalCancellation(
        string providerState,
        ProcessedOrderPaymentState expectedState
    )
    {
        Mock<IOrderPaymentLinkClient> client = new(MockBehavior.Strict);
        client.Setup(x => x.GetPaymentLinkInformationAsync(10)).ReturnsAsync(Link(providerState));

        ProcessedOrderPaymentCancellationResult result =
            await ProcessedOrderPaymentCancellation.EnsureSafeAsync(
                client.Object,
                10,
                "Customer request",
                Mock.Of<ILogger>()
            );

        Assert.False(result.IsSafe);
        Assert.Equal(expectedState, result.State);
        client.Verify(
            x => x.CancelPaymentLinkAsync(It.IsAny<long>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task AmbiguousCancelFailure_RechecksOnceAndAcceptsRemoteCancellation()
    {
        Mock<IOrderPaymentLinkClient> client = new(MockBehavior.Strict);
        client
            .SetupSequence(x => x.GetPaymentLinkInformationAsync(10))
            .ReturnsAsync(Link("PENDING"))
            .ReturnsAsync(Link("CANCELLED"));
        client
            .Setup(x => x.CancelPaymentLinkAsync(10, "Customer request"))
            .ThrowsAsync(new HttpRequestException("Timeout"));

        ProcessedOrderPaymentCancellationResult result =
            await ProcessedOrderPaymentCancellation.EnsureSafeAsync(
                client.Object,
                10,
                "Customer request",
                Mock.Of<ILogger>()
            );

        Assert.True(result.IsSafe);
        client.Verify(x => x.GetPaymentLinkInformationAsync(10), Times.Exactly(2));
        client.Verify(x => x.CancelPaymentLinkAsync(10, "Customer request"), Times.Once);
    }

    [Fact]
    public async Task AmbiguousCancelFailure_NonCancelledRecheckFailsClosedWithoutLooping()
    {
        Mock<IOrderPaymentLinkClient> client = new(MockBehavior.Strict);
        client
            .SetupSequence(x => x.GetPaymentLinkInformationAsync(10))
            .ReturnsAsync(Link("PENDING"))
            .ReturnsAsync(Link("PENDING"));
        client
            .Setup(x => x.CancelPaymentLinkAsync(10, "Customer request"))
            .ThrowsAsync(new HttpRequestException("Timeout"));

        ProcessedOrderPaymentCancellationResult result =
            await ProcessedOrderPaymentCancellation.EnsureSafeAsync(
                client.Object,
                10,
                "Customer request",
                Mock.Of<ILogger>()
            );

        Assert.False(result.IsSafe);
        client.Verify(x => x.GetPaymentLinkInformationAsync(10), Times.Exactly(2));
        client.Verify(x => x.CancelPaymentLinkAsync(10, "Customer request"), Times.Once);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, true, false)]
    [InlineData(OrderStatus.InProgress, true, false)]
    [InlineData(OrderStatus.Processed, true, true)]
    public void CancellationResourcePlan_ReleasesVoucherButOnlyProcessedUsesPayOs(
        OrderStatus previousStatus,
        bool releaseVoucher,
        bool requiresPayOs
    )
    {
        OrderCancellationResourcePlan plan = OrderCancellationResourcePolicy.Create(
            previousStatus,
            OrderStatus.Cancelled,
            customerId: 7,
            voucherId: 8
        );

        Assert.True(plan.IsCancellation);
        Assert.Equal(releaseVoucher, plan.ShouldReleaseVoucher);
        Assert.Equal(requiresPayOs, plan.RequiresPayOsCoordination);
        Assert.True(plan.MaterialsRemainConsumed);
    }

    [Fact]
    public void CancellationResourcePlan_NoVoucherOrderStillCancelsNormally()
    {
        OrderCancellationResourcePlan plan = OrderCancellationResourcePolicy.Create(
            OrderStatus.Pending,
            OrderStatus.Cancelled,
            customerId: 7,
            voucherId: null
        );

        Assert.True(plan.IsCancellation);
        Assert.False(plan.ShouldReleaseVoucher);
        Assert.False(plan.RequiresPayOsCoordination);
    }

    private static PaymentLinkInformation Link(string status) =>
        new(
            id: "payment-link",
            orderCode: 10,
            amount: 100,
            amountPaid: status == "PAID" ? 100 : 0,
            amountRemaining: status == "PAID" ? 0 : 100,
            status: status,
            createdAt: "2026-09-01T00:00:00Z",
            transactions: [],
            canceledAt: status == "CANCELLED" ? "2026-09-01T01:00:00Z" : null,
            cancellationReason: status == "CANCELLED" ? "Customer request" : null
        );
}
