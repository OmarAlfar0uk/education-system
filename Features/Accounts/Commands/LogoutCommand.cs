using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using EduocationSystem.Domain;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Accounts.Commands
{
    public record LogoutCommand(string UserId) : IRequest<ServiceResponse<bool>>
    {
        public class Handler : IRequestHandler<LogoutCommand, ServiceResponse<bool>>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly ILogger<Handler> _logger;

            public Handler(UserManager<ApplicationUser> userManager, ILogger<Handler> logger)
            {
                _userManager = userManager;
                _logger = logger;
            }

            public async Task<ServiceResponse<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return ServiceResponse<bool>.NotFoundResponse("User not found");
                }

                // Revoke active refresh token (set RevokedOn to invalidate)
                var activeToken = user.RefreshTokens?.FirstOrDefault(t => t.IsActive);
                if (activeToken != null)
                {
                    activeToken.RevokedOn = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                    _logger.LogInformation("Refresh token revoked for user {UserId}", request.UserId);
                }

                // Optional: Sign out from Identity (for cookie-based, but JWT is stateless—handled by client)
                // await _signInManager.SignOutAsync();

                _logger.LogInformation("User {UserId} logged out successfully", request.UserId);
                return ServiceResponse<bool>.SuccessResponse(true, "Logged out successfully", "تم تسجيل الخروج بنجاح");
            }
        }
    }
}