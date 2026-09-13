using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Events;
using Shared.Kernel.Common;
using Shared.Kernel.Common.Events;

namespace EcommerceService.Tests;

public class DomainEventQueueTests
{
    [Fact]
    public void AggregateRoot_RaisesAndDequeuesEventsInOrder()
    {
        var aggregate = new TestAggregate();
        aggregate.Raise(1);
        aggregate.Raise(2);

        Assert.Equal(
            [1, 2],
            aggregate.UncommittedEvents.Cast<TestEvent>().Select(x => x.Sequence)
        );
        Assert.True(aggregate.TryDequeueUncommittedEvent(out IDomainEvent? first));
        Assert.Equal(1, Assert.IsType<TestEvent>(first).Sequence);
        Assert.Equal(
            2,
            Assert.IsType<TestEvent>(Assert.Single(aggregate.DequeueUncommittedEvents())).Sequence
        );
        Assert.Empty(aggregate.UncommittedEvents);
    }

    [Fact]
    public void AggregateRoot_NoLongerRequiresAnEventFilterOverride()
    {
        Assert.Null(typeof(AggregateRoot).GetMethod("TryApplyDomainEvent"));
        Assert.Empty(new TestAggregate().UncommittedEvents);
    }

    [Fact]
    public void OrderEvents_AreImmutableSnapshotsInLifecycleOrder()
    {
        var item = new OrderItem
        {
            ServiceName = "Wash",
            UnitRelationName = "Kg",
            Quantity = 2,
            UnitPrice = 50,
        };
        var order = new Order(
            2,
            7,
            "OD-SNAPSHOT",
            100,
            100,
            OrderStatus.InProgress,
            orderItems: [item]
        );

        _ = order.TransitionTo(OrderStatus.Processed);
        UpdateStatusOrderEvent processed = Assert.IsType<UpdateStatusOrderEvent>(
            Assert.Single(order.UncommittedEvents)
        );
        _ = order.TransitionTo(OrderStatus.Completed, PaymentMethod.Cash);
        item.ServiceName = "Changed after event";

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(OrderStatus.Processed, processed.Status);
        Assert.Equal("OD-SNAPSHOT", processed.OrderCode);
        Assert.Collection(
            order.UncommittedEvents,
            first => Assert.IsType<UpdateStatusOrderEvent>(first),
            second => Assert.IsType<UpdateStatusOrderEvent>(second),
            third => Assert.IsType<EInvoiceEvent>(third),
            fourth => Assert.IsType<CreateFundEvent>(fourth)
        );
        EInvoiceEvent invoice = order.UncommittedEvents.OfType<EInvoiceEvent>().Single();
        Assert.Equal("Wash", Assert.Single(invoice.Items).ServiceName);
    }

    private sealed record TestEvent(int Sequence) : IDomainEvent;

    private sealed class TestAggregate : AggregateRoot
    {
        public void Raise(int sequence) => RaiseDomainEvent(new TestEvent(sequence));
    }
}
