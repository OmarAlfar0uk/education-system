using MediatR;
using Microsoft.Extensions.Logging;
using EduocationSystem.Features.Accounts.Commands;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Accounts.Orchestrators
{
    public record OrchestrateRegistrationCommand(RegisterDto RegisterDto) : IRequest<ServiceResponse<bool>>
    {
        public class Handler : IRequestHandler<OrchestrateRegistrationCommand, ServiceResponse<bool>>
        {
            private readonly IMediator _mediator;

            public Handler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public async Task<ServiceResponse<bool>> Handle(OrchestrateRegistrationCommand request, CancellationToken cancellationToken)
            {
                // Step 1: Register user
                var registerResult = await _mediator.Send(new RegisterCommand(request.RegisterDto), cancellationToken);
                if (!registerResult.IsSuccess)
                {
                    // Flatten errors for List overload (explicit call to avoid ambiguity)
                    var errorsList = registerResult.Errors.Any() ? registerResult.Errors : new List<string> { registerResult.Message };
                    return ServiceResponse<bool>.ErrorResponse(
                        errors: errorsList,  // Named param to select List overload
                        message: registerResult.Message,
                        messageAr: registerResult.MessageAr ?? "فشل في التسجيل",
                        statusCode: registerResult.StatusCode
                    );
                }

                var user = registerResult.Data;
                if (user == null)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        errors: new List<string> { "User data not found after registration" },
                        message: "فشل في استرجاع بيانات المستخدم",
                        statusCode: 500
                    );
                }

                // Step 2: Send verification email
                var emailResult = await _mediator.Send(new SendVerificationEmailCommand(new ResendVerificationCodeDto
                {
                   Email = user.Email
                }), cancellationToken);
                if (!emailResult.IsSuccess)
                {
                    // Partial success: User registered, email failed (use SuccessResponse with warning message)
                    return ServiceResponse<bool>.SuccessResponse(
                        data: true,
                        message: $"User registered successfully, but verification email failed: {emailResult.Message}",
                        messageAr: $"تم التسجيل بنجاح، لكن فشل في إرسال بريد التأكيد: {emailResult.MessageAr}"
                    );
                }

                // Full success
                return ServiceResponse<bool>.SuccessResponse(
                    data: true,
                    message: "Registration completed with email verification",
                    messageAr: "تمت عملية التسجيل مع إرسال بريد التأكيد"
                );
            }
        }
    }
}