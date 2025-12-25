using MediatR;
using EduocationSystem.Features.Questions.Commands;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Questions.Endpoints
{
    public static class DeleteQuestionEndpoint
    {
        public static void MapDeleteQuestionEndpoint(this WebApplication app)
        {
            var group = app.MapGroup("/api/questions")
                .WithTags("Questions");

            // DELETE /api/questions/{id} - Delete a question and its choices (soft delete)
            group.MapDelete("/{id}", async (IMediator mediator, int id) =>
            {
                var command = new DeleteQuestionCommand(id);
                var result = await mediator.Send(command);

                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("DeleteQuestion")
            .Produces<ServiceResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<bool>>(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse<bool>>(StatusCodes.Status404NotFound)
            .Produces<ServiceResponse<bool>>(StatusCodes.Status500InternalServerError);
        }
    }
}