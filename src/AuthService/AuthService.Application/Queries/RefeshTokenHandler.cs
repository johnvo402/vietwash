using AuthService.Application.Commands;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using MediatR;
using Micro.Shared.Extensions;
using Micro.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Queries;

public class RefeshTokenHandler : IRequestHandler<RefeshTokenQuery, ApiResponse<LoginUserResponse>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    public RefeshTokenHandler(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager, ITokenService tokenService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _tokenService = tokenService;
    }
    public async Task<ApiResponse<LoginUserResponse>> Handle(RefeshTokenQuery request, CancellationToken cancellationToken)
    {
        if (!_tokenService.ValidateAccessToken(request.RefreshToken))
        {
            return new ApiResponse<LoginUserResponse>
            {
                Success = false,
                Message = "Unauthorized"
            };
        }
        var userAccess = _httpContextAccessor.HttpContext?.GetUserAccessOrDefault();
        if (userAccess == null || string.IsNullOrEmpty(userAccess.UserId))
        {
            return new ApiResponse<LoginUserResponse>
            {
                Success = false,
                Message = "User ID not found in claims"
            };
        }

        var user = await _userManager.FindByIdAsync(userAccess.UserId);
        if (user == null)
        {
            return new ApiResponse<LoginUserResponse>
            {
                Success = false,
                Message = "User not found" + userAccess.UserId
            };
        }
        var roles = await _userManager.GetRolesAsync(user);


        var (newAccessToken, newExpiresAt) = _tokenService.GenerateAccessToken(user, roles.ToArray());
        var newRefreshToken = await _tokenService.GenerateRefreshToken();
        return new ApiResponse<LoginUserResponse>
        {
            Success = true,
            Value = new LoginUserResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = newExpiresAt
            }
        };
    }
}