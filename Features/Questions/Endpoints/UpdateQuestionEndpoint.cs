using MediatR;
using EduocationSystem.Features.Questions.Commands;
using EduocationSystem.Features.Questions.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Questions.Endpoints
{
    public static class UpdateQuestionEndpoint
    {
        public static void MapUpdateQuestionEndpoint(this WebApplication app)
        {                
            // PUT /api/questions/{id} - Update a question and its choices
            app.MapPut("/api/questions/{id}", async (
                IMediator mediator,
                int id,
                UpdateQuestionDto? questionDto) =>
            {
                var command = new UpdateQuestionCommand(id, questionDto);
                var result = await mediator.Send(command);

                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithTags("Questions")
            .WithName("UpdateQuestion")
            .Produces<ServiceResponse<QuestiondataDto>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<QuestiondataDto>>(StatusCodes.Status400BadRequest)
            .Produces<ServiceResponse<QuestiondataDto>>(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse<QuestiondataDto>>(StatusCodes.Status404NotFound)
            .Produces<ServiceResponse<QuestiondataDto>>(StatusCodes.Status500InternalServerError);
        }
    }
}