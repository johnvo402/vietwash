using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Funds.Enums;
using Mediator;

namespace Application.Features.Funds.Events
{
    public class UpdateStatusOrderEvent
        : PubSubBasePayload<UpdateStatusOrderEventPayload>,
            IRequest<PubSubResponse<UpdateStatusOrderEvent>>;

    public class UpdateStatusOrderEventPayload
    {
        public string TypeId { get; set; } = default!;
        public long BehaviorId { get; set; } = default!;
        public long OrderId { get; set; } = default!;
        public Ulid PublicId { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public string? Code { get; set; }
        public long BranchId { get; set; } = default!;
        public long CustomerId { get; set; } = default!;
    }
}
