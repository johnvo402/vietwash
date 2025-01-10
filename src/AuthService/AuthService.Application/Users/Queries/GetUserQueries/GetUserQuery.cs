using AuthService.Domain.Users.Entity;
using ErrorOr;
using MediatR;
using Micro.Shared.Application.Security.Request;
using Micro.Shared.Model;

namespace AuthService.Application.Users.Queries.GetUserQueries;

public record GetUserQuery(QueryParameters? request) : IAuthorizeableRequest<ErrorOr<ApiResponseQuery<User>>>;