using ErrorOr;
using Micro.Shared.Application.Security.Request;

namespace AuthService.Application.Permissions.Commands.PermissionCreateCommands;
public record PermissionCreateCommand(string Key, string Description) : IAuthorizeableRequest<ErrorOr<string>>;