using MediatR;
using EduocationSystem.Features.Accounts.Commands;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Features.Accounts.Orchestrators;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Accounts.Endpoints
{
    public static class ConfirmEmailEndpoint
    {
        public static void MapConfirmEmailEndpoint(this WebApplication app)
        {
            app.MapPost("/api/accounts/confirm-email", async (ConfirmEmailWithCodeDto request, IMediator mediator) =>
            {
                var result = await mediator.Send(new EmailConfirmationOrchestrator(request));
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("ConfirmEmail")
            .WithTags("Accounts");
        }
    }
}