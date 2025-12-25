using MediatR;
using EduocationSystem.Features.Accounts.Commands;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Features.Accounts.Orchestrators;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Accounts.Endpoints
{
    public static class ForgotPasswordEndpoint
    {
        public static void MapForgotPasswordEndpoint(this WebApplication app)
        {
            app.MapPost("/api/accounts/forgot-password", async (ForgotPasswordDto request, IMediator mediator) =>
            {
                var result = await mediator.Send(new OrchestrateForgetPasswordCommand(request));
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("ForgotPassword")
            .WithTags("Accounts");
        }
    }
}