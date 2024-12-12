using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ITokenService = AuthService.Application.Interfaces.ITokenService;
using Micro.Shared.Model;


namespace AuthService.Application.Commands
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ApiResponse<LoginUserResponse>>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;

        public LoginUserCommandHandler(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public async Task<ApiResponse<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            // Validate user exists
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new ApiResponse<LoginUserResponse> { Success = false, Message = "Invalid Email" };
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            await _signInManager.SignInAsync(user, false);
            if (!result.Succeeded)
            {
                return new ApiResponse<LoginUserResponse> { Success = false, Message = "Invalid Password" };
            }

            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user, roles.ToArray());
            var refreshToken = await _tokenService.GenerateRefreshToken();

            return new ApiResponse<LoginUserResponse>
            {
                Success = true,
                Value = new LoginUserResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt
                }
            };
        }
    }
}
