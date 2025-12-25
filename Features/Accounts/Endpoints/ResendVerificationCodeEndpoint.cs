using MediatR;
using EduocationSystem.Features.Accounts.Commands;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Accounts.Endpoints
{
    public static class ResendVerificationCodeEndpoint
    {
        public static void MapResendVerificationCodeEndpoint(this WebApplication app)
        {
            app.MapPost("/api/accounts/resend-verification-code", async (ResendVerificationCodeDto dto, IMediator mediator) =>
            {
                var result = await mediator.Send(new SendVerificationEmailCommand(dto));
                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.BadRequest(result);
                }
            })
            .WithName("ResendVerificationCode")
            .WithTags("Accounts")
            .Produces<ServiceResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<bool>>(StatusCodes.Status400BadRequest);
        }
    }
}
