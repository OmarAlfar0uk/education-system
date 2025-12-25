using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using EduocationSystem.Domain;
using EduocationSystem.Features.Accounts.Commands;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Accounts.Orchestrators
{
    public record EmailConfirmationOrchestrator(ConfirmEmailWithCodeDto Dto) : IRequest<ServiceResponse<bool>>
    {
        public class Handler : IRequestHandler<EmailConfirmationOrchestrator, ServiceResponse<bool>>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IMediator _mediator;
            private readonly ILogger<EmailConfirmationOrchestrator> _logger;

            public Handler(UserManager<ApplicationUser> userManager, IMediator mediator, ILogger<EmailConfirmationOrchestrator> logger)
            {
                _userManager = userManager;
                _mediator = mediator;
                _logger = logger;
            }

            public async Task<ServiceResponse<bool>> Handle(EmailConfirmationOrchestrator request, CancellationToken cancellationToken)
            {
                try
                {
                    var user = await _userManager.FindByEmailAsync(request.Dto.Email);
                    if (user is null)
                    {
                        return ServiceResponse<bool>.NotFoundResponse("User not found");
                    }

                    if (user.EmailConfirmed)
                    {
                        return ServiceResponse<bool>.SuccessResponse(true, "Email is already confirmed");
                    }

                    var userId = user.Id;

                    // Step 1: Verify the code (dispatch VerifyCodeCommand)
                    var verificationResult = await _mediator.Send(new VerifyCodeCommand(userId, request.Dto.Code, VerificationCodeType.EmailVerification), cancellationToken);
                    if (!verificationResult.IsSuccess)
                    {
                        return verificationResult; // Propagate verification errors
                    }

                    // Step 2: Mark email as confirmed
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    

                    // Step 3: Send welcome email (dispatch SendWelcomeEmailCommand)
                    var welcomeEmailResult = await _mediator.Send(new SendWelcomeEmailCommand(user.Email, user.FirstName + " " + user.LastName), cancellationToken);
                    if (!welcomeEmailResult.IsSuccess)
                    {
                        _logger.LogWarning("Failed to send welcome email to {Email}", user.Email);
                        // Continue with partial success (confirmation succeeds, email non-critical)
                    }

                    _logger.LogInformation("Email confirmed for user {UserId}", user.Id);
                    return ServiceResponse<bool>.SuccessResponse(
                        true,
                        "Email confirmed successfully",
                        "تم تأكيد البريد الإلكتروني بنجاح"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error confirming email for user {Email}", request.Dto.Email);
                    return ServiceResponse<bool>.InternalServerErrorResponse("An error occurred while confirming email");
                }
            }
        }
    }
}