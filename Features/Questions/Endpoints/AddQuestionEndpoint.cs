using MediatR;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Features.Questions.Commands;
using EduocationSystem.Features.Questions.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Questions.Endpoints
{
    public static class AddQuestionEndpoint
    {
        public static void MapAddQuestionEndpoint(this WebApplication app)
        {
            app.MapPost("/api/questions", async (
                IMediator mediator,
                AddQuestionDto? questionDto) =>
            {
                
                var result = await mediator.Send(new AddQuestionCommand(questionDto));

                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithTags("Questions")
            .WithName("AddQuestion")
            .Produces<ServiceResponse<QuestionDto>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<QuestionDto>>(StatusCodes.Status400BadRequest)
            .Produces<ServiceResponse<QuestionDto>>(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse<QuestionDto>>(StatusCodes.Status404NotFound)
            .Produces<ServiceResponse<QuestionDto>>(StatusCodes.Status500InternalServerError);
        }
    }
}