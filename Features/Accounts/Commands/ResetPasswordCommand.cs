using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EduocationSystem.Domain;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Infrastructure.ApplicationDBContext;
using EduocationSystem.Shared.Responses;
using TechZone.Core.Entities;

namespace EduocationSystem.Features.Accounts.Commands
{
    public record ResetPasswordCommand(ResetPasswordDto Dto) : IRequest<ServiceResponse<bool>>
    {
        public class Handler : IRequestHandler<ResetPasswordCommand, ServiceResponse<bool>>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly ILogger<Handler> _logger;
            private readonly ApplicationDbContext _context;

            public Handler(UserManager<ApplicationUser> userManager, ILogger<Handler> logger, ApplicationDbContext context)
            {
                _userManager = userManager;
                _logger = logger;
                _context = context;
            }

            public async Task<ServiceResponse<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByEmailAsync(request.Dto.Email);
                if (user == null)
                    return ServiceResponse<bool>.NotFoundResponse("User not found");

                // ✅ Validate code
                var verificationCode = await _context.VerificationCodes
                    .FirstOrDefaultAsync(vc =>
                        vc.UserId == user.Id &&
                        vc.Code == request.Dto.Code &&
                        vc.Type == TechZone.Core.Entities.VerificationCodeType.PasswordReset,
                        cancellationToken);

                if (verificationCode == null)
                    return ServiceResponse<bool>.ErrorResponse("Invalid code", "كود التحقق غير صحيح", 400);

                if (verificationCode.IsUsed)
                    return ServiceResponse<bool>.ErrorResponse("Code already used", "تم استخدام هذا الكود من قبل", 400);

                if (verificationCode.ExpiryDate < DateTime.UtcNow)
                    return ServiceResponse<bool>.ErrorResponse("Code expired", "انتهت صلاحية الكود", 400);

                // ✅ Validate passwords match
                if (request.Dto.NewPassword != request.Dto.ConfirmPassword)
                    return ServiceResponse<bool>.ErrorResponse("Passwords do not match", "كلمات المرور غير متطابقة", 400);

                // ✅ Reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, request.Dto.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ServiceResponse<bool>.ValidationErrorResponse(new Dictionary<string, List<string>> { { "Password", errors } }, "Reset failed");
                }

                // ✅ Mark verification code as used
                verificationCode.IsUsed = true;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Password reset for {Email}", request.Dto.Email);
                return ServiceResponse<bool>.SuccessResponse(true, "Password reset successfully", "تم إعادة تعيين كلمة المرور بنجاح");
            }
        }
    }
}
