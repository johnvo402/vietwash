using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Funds.Enums;
using Mediator;

namespace Application.Features.Funds.Events
{
    public class UpdateStatusOrderEvent
        : PubSubBasePayload<CreateFundEventPayload>,
            IRequest<PubSubResponse<UpdateStatusOrderEvent>>;

    public class CreateFundEventPayload
    {
        public string TypeId { get; set; } = default!;
        public long BehaviorId { get; set; } = default!;
        public long ReferenceId { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public Dictionary<string, object>? Metadata { get; set; }
        public long BranchId { get; set; } = default!;
        public long? ObjectId { get; set; }

        public decimal Point { get; set; }
    }
}
