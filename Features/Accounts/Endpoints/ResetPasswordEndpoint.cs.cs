using MediatR;
using EduocationSystem.Features.Accounts.Commands;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Features.Accounts.Orchestrators;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Accounts.Endpoints
{
    public static class ResetPasswordEndpoint
    {
        public static void MapResetPasswordEndpoint(this WebApplication app)
        {
            app.MapPost("/api/accounts/reset-password", async (ResetPasswordDto request, IMediator mediator) =>
            {
                var result = await mediator.Send(new ResetPasswordOrchestrator(request));
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("ResetPassword")
            .WithTags("Accounts");
        }
    }
}