using MediatR;
using Microsoft.AspNetCore.Mvc;
using EduocationSystem.Features.Exams.Commands;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Endpoints
{
    public static class EditExamEndpoint
    {
        public static void MapEditExamEndpoint(this WebApplication app)
        {
            app.MapPut("/api/exams", async (
                [FromBody] EditExamDto dto,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new EditExamCommand(dto));
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithTags("Exams")
            .WithSummary("Edit an existing exam")
            .RequireAuthorization("AdminOnly")
            .Produces<ServiceResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces<ServiceResponse<bool>>(StatusCodes.Status401Unauthorized)
            .Produces<ServiceResponse<bool>>(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse<bool>>(StatusCodes.Status404NotFound);
        }
    }
}
