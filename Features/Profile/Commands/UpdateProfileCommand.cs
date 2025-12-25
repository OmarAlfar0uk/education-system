using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using EduocationSystem.Domain;
using EduocationSystem.Features.Profile.Dtos;
using EduocationSystem.Shared.Responses;
using System.Security.Claims;

namespace EduocationSystem.Features.Profile.Commands
{
    public record UpdateProfileCommand(UpdateProfileDto Dto) : IRequest<ServiceResponse<bool>>;

    public class Handler : IRequestHandler<UpdateProfileCommand, ServiceResponse<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<Handler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Handler(
            UserManager<ApplicationUser> userManager,
            ILogger<Handler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<bool>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get user ID from token
                var userId = GetUserIdFromToken();
                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResponse<bool>.UnauthorizedResponse("Invalid token", "رمز الدخول غير صالح");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResponse<bool>.NotFoundResponse("User not found", "المستخدم غير موجود");
                }

                // Update only profile fields (no password update)
                user.FirstName = request.Dto.FirstName;
                user.LastName = request.Dto.LastName;
                user.Phone = request.Dto.Phone;
                user.ImageUrl = request.Dto.Image;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = updateResult.Errors.Select(e => e.Description).ToList();
                    return ServiceResponse<bool>.ErrorResponse(
                        errors,
                        "Profile update failed",
                        "فشل تحديث الملف الشخصي",
                        400
                    );
                }

                _logger.LogInformation("Profile updated for user {UserId}", userId);
                return ServiceResponse<bool>.SuccessResponse(true, "Profile updated successfully", "تم تحديث الملف الشخصي بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return ServiceResponse<bool>.InternalServerErrorResponse("An error occurred while updating profile", "حدث خطأ أثناء تحديث الملف الشخصي");
            }
        }

        private string GetUserIdFromToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                return httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            return null;
        }
    }
}