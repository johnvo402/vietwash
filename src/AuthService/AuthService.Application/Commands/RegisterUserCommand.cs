using System.Text.Json.Serialization;
using MediatR;
using Micro.Shared.Model;
using Micro.Shared.Data;
namespace AuthService.Application.Commands;

public record RegisterUserCommand(
    string Password,
    string? Email = null,
    string? DisplayName = null,
    string? PhoneNumber = null,
    string? Role = RoleName.DefaultRole
) : IRequest<ApiResponse<string>>;
