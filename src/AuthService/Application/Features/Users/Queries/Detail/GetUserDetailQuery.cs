using Mediator;

namespace Application.Features.Users.Queries.Detail;

public record GetUserDetailQuery(long UserId) : IRequest<GetUserDetailResponse>;
