using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EduocationSystem.Domain;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Infrastructure.ApplicationDBContext;
using EduocationSystem.Shared.Responses;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EduocationSystem.Features.Accounts.Commands
{
    public record VerifyCodeCommand(string UserId, string Code, VerificationCodeType CodeType)
        : IRequest<ServiceResponse<bool>>
    {
        public class VerifyCodeCommandHandler
            : IRequestHandler<VerifyCodeCommand, ServiceResponse<bool>>
        {
            private readonly ApplicationDbContext _context;
            private readonly ILogger<VerifyCodeCommandHandler> _logger;

            public VerifyCodeCommandHandler(ApplicationDbContext context, ILogger<VerifyCodeCommandHandler> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<ServiceResponse<bool>> Handle(VerifyCodeCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var verificationCode = await _context.VerificationCodes
                        .FirstOrDefaultAsync(v =>
                            v.UserId == request.UserId &&
                            v.Code == request.Code ,
                            cancellationToken);

                    // 1️⃣ Not found
                    if (verificationCode is null)
                    {
                        return ServiceResponse<bool>.ErrorResponse(
                            "Invalid verification code.",
                            "الكود غير صحيح",
                            400
                        );
                    }

                    // 2️⃣ Check if expired
                    if (DateTime.UtcNow > verificationCode.ExpiryDate)
                    {
                        return ServiceResponse<bool>.ErrorResponse(
                              "Code has expired." ,
                            "انتهت صلاحية الكود",
                            400
                        );
                    }

                    // 3️⃣ Check if already used
                    if (verificationCode.IsUsed)
                    {
                        return ServiceResponse<bool>.ErrorResponse(
                            "Code has already been used.",
                            "تم استخدام الكود بالفعل",
                            400
                        );
                    }

                    // 4️⃣ Check attempts
                    if (verificationCode.AttemptCount >= verificationCode.MaxAttempts)
                    {
                        return ServiceResponse<bool>.ErrorResponse(
                            "Maximum attempts reached." ,
                            "تم تجاوز الحد الأقصى للمحاولات",
                            400
                        );
                    }

                    // 5️⃣ Increment attempt count
                    verificationCode.AttemptCount++;

                    // ✅ Mark as used (success)
                    verificationCode.IsUsed = true;

                    // Save changes
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Verification code validated for user {UserId}", request.UserId);

                    return ServiceResponse<bool>.SuccessResponse(
                        true,
                        "Code verified successfully",
                        "تم التحقق من الكود بنجاح"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error verifying code for user {UserId}", request.UserId);
                    return ServiceResponse<bool>.InternalServerErrorResponse("An error occurred while verifying the code.");
                }
            }
        }
    }
}
