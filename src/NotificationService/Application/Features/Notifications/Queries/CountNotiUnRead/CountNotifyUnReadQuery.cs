using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Notifications.Queries.CountNotiUnRead
{
    public class CountNotifyUnReadQuery : IRequest<Result<CountNotifyUnReadResponse>>;
}
