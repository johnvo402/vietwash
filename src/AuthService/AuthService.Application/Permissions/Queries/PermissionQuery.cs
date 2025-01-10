using AuthService.Domain.Permissions;
using ErrorOr;
using Micro.Shared.Application.Security.Request;
using Micro.Shared.Model;

namespace AuthService.Application.Permissions.Queries;
public record PermissionQuery(QueryParameters? QueryParameters) : IAuthorizeableRequest<ErrorOr<IEnumerable<Permission>?>>;