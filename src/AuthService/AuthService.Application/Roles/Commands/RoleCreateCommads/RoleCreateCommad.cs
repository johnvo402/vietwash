using ErrorOr;
using Micro.Shared.Application.Security.Request;

namespace AuthService.Application.Roles.Commands.RoleCreateCommads;
public record RoleCreateCommand(string Name) : IAuthorizeableRequest<ErrorOr<string>>;
