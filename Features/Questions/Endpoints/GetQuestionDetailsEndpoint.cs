using MediatR;
using EduocationSystem.Features.Questions.Dtos;
using EduocationSystem.Features.Questions.Queries;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Questions.Endpoints
{
    public static class GetQuestionDetailsEndpoint
    {
        public static void MapGetQuestionDetailsEndpoint(this WebApplication app)
        {
            var group = app.MapGroup("/api/questions")
                .WithTags("Questions");

            // GET /api/questions/{id} - Get question details (admin only)
            group.MapGet("/{id}", async (IMediator mediator, int id) =>
            {
                var query = new GetQuestionDetailsQuery(id);
                var result = await mediator.Send(query);

                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("GetQuestionDetails")
            .Produces<ServiceResponse<QuestiondetailsDto>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<QuestiondetailsDto>>(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse<QuestiondetailsDto>>(StatusCodes.Status404NotFound)
            .Produces<ServiceResponse<QuestiondetailsDto>>(StatusCodes.Status500InternalServerError);
        }
    }
}