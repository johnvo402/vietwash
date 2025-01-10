using ErrorOr;
using MediatR;
using Micro.Shared.Application.Security.Request;
using Micro.Shared.Model;

namespace AuthService.Application.Auth.Queries.GetMe;
public record GetMeQuery : IAuthorizeableRequest<ErrorOr<GetMeResponse>>;

public class GetMeResponse
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string[] Role { get; set; } = [];
    public string[] Permission { get; set; } = [];
    public string? PhoneNumber { get; set; }
    public string? Avatar { get; set; }
}