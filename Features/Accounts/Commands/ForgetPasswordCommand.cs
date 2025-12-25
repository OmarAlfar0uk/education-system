using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using EduocationSystem.Domain;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Shared.Responses;
using TechZone.Core.Entities;

namespace EduocationSystem.Features.Accounts.Commands
{
    public record ForgotPasswordCommand(ForgotPasswordDto Dto) : IRequest<ServiceResponse<bool>>
    {
        public class Handler : IRequestHandler<ForgotPasswordCommand, ServiceResponse<bool>>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IMediator _mediator;
            private readonly ILogger<Handler> _logger;

            public Handler(UserManager<ApplicationUser> userManager, IMediator mediator, ILogger<Handler> logger)
            {
                _userManager = userManager;
                _mediator = mediator;
                _logger = logger;
            }

            public async Task<ServiceResponse<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByEmailAsync(request.Dto.Email);
                if (user == null)
                {
                    // Don't reveal existence for security
                    return ServiceResponse<bool>.SuccessResponse(true, "If the email is registered, a reset code has been sent", "إذا كان البريد مسجلاً، تم إرسال كود إعادة التعيين");
                }

                // Generate reset code
                var resetCode = GenerateResetCode();

                // Save code to user
                user.VerificationCodes.Add(new VerificationCode
                {
                    Code = resetCode,
                    Type = TechZone.Core.Entities.VerificationCodeType.PasswordReset,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(15) // Code valid for 15 minutes
                });

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    _logger.LogError("Failed to save reset code for {Email}", request.Dto.Email);
                    return ServiceResponse<bool>.InternalServerErrorResponse("Failed to initiate reset");
                }

                // Dispatch send reset email
                var emailResult = await _mediator.Send(new SendResetEmailCommand(request.Dto.Email, resetCode), cancellationToken);
                if (!emailResult.IsSuccess)
                {
                    _logger.LogWarning("Failed to send reset email for {Email}", request.Dto.Email);
                    return ServiceResponse<bool>.InternalServerErrorResponse("Failed to send reset email");
                }

                _logger.LogInformation("Password reset initiated for {Email}", request.Dto.Email);
                return ServiceResponse<bool>.SuccessResponse(true, "Reset code sent to email", "تم إرسال كود إعادة التعيين إلى البريد");
            }

            private string GenerateResetCode()
            {
                var random = new Random();
                return random.Next(100000, 999999).ToString();  // 6-digit code
            }
        }
    }
}