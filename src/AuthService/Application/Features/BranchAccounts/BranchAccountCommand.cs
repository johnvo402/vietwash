using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Features.BranchAccounts
{
    public class BranchAccountCommand
        : PubSubBasePayload<BranchCreateEvent>,
            IRequest<PubSubResponse<BranchAccountCommand>>;

    public class BranchCreateEvent
    {
        public long BranchId { get; set; }
        public string Name { get; set; }
    }
}
