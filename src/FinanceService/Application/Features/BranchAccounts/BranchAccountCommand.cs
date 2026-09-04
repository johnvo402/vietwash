using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Features.BranchUsers
{
    public class BranchUserCommand
        : PubSubBasePayload<BranchCreateEvent>,
            IRequest<PubSubResponse<BranchUserCommand>>;

    public class BranchCreateEvent
    {
        public long BranchId { get; set; }
        public string Name { get; set; } = default!;
    }
}
