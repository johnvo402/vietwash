using System.Security.Claims;
using AuthService.Domain.Entities;
using MediatR;
using Micro.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Micro.Shared.Extensions;

namespace AuthService.Application.Queries;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, ApiResponse<GetMeResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public GetMeQueryHandler(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<ApiResponse<GetMeResponse>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var userAccess = _httpContextAccessor.HttpContext?.GetUserAccessOrDefault();
        if (userAccess == null || string.IsNullOrEmpty(userAccess.UserId))
        {
            return new ApiResponse<GetMeResponse>
            {
                Success = false,
                Message = "User ID not found in claims"
            };
        }

        var user = await _userManager.FindByIdAsync(userAccess.UserId);
        if (user == null)
        {
            return new ApiResponse<GetMeResponse>
            {
                Success = false,
                Message = "User not found"
            };
        }

        return new ApiResponse<GetMeResponse>
        {
            Success = true,
            Value = new GetMeResponse
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName ?? string.Empty,
                Role = userAccess.Role,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
            }
        };
    }
}