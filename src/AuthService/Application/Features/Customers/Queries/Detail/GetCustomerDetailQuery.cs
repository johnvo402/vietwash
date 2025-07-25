using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Customers.Queries.Detail;

public record GetCustomerDetailQuery(long AccountId) : IRequest<Result<GetCustomerDetailResponse>>;
