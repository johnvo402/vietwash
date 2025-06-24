using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Accounts.Queries.Profiles;

public class GetAccountProfileQuery : IRequest<Result<GetAccountProfileResponse>>;
