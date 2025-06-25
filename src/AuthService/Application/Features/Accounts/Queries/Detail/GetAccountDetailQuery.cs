using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Accounts.Queries.Detail;

public record GetAccountDetailQuery(long AccountId) : IRequest<Result<GetAccountDetailResponse>>;
