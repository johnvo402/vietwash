using AuthService.Domain.Roles;
using ErrorOr;
using Micro.Shared.Application.Security.Request;
using Micro.Shared.Model;

namespace AuthService.Application.Roles.Queries;
public record RoleQuery(QueryParameters? QueryParameters) : IAuthorizeableRequest<ErrorOr<List<Role>>>;