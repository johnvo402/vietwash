using MediatR;
using Micro.Shared.Model;

namespace AuthService.Application.Queries;

public record GetMeQuery() : IRequest<ApiResponse<GetMeResponse>>;
public class GetMeResponse
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? Role { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Avatar { get; set; }

}