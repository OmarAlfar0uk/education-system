using MediatR;
using Microsoft.AspNetCore.Mvc;
using EduocationSystem.Features.Exams.Commands;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Endpoints
{
    public static class DeleteExamEndpoint
    {
        public static void MapDeleteExamEndpoint(this WebApplication app)
        {
            app.MapDelete("api/exams/", async (
                [FromBody] DeleteExamDto dto,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteExamCommand(dto));
                return Results.Ok(result);
            })

                .RequireAuthorization("AdminOnly")
            .WithTags("Exams")
            .WithSummary("Soft delete an exam and its related questions")
            .Produces<ServiceResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<bool>>(StatusCodes.Status404NotFound);
        }
    }
}
