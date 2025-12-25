using MediatR;
using EduocationSystem.Features.UserAnswers.Dtos;
using EduocationSystem.Features.UserAnswers.Queries;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.UserAnswers.Endpoints
{
    public static class GetDetailedUserAnswer
    {
        public static void MapGetDetailedUserAnswerEndpoint(this WebApplication app)
        {
            var group = app.MapGroup("/api/my-answers")
                .WithTags("User Answers");
            // GET /api/my-answers/{attemptId} - Get detailed result for a specific attempt
            group.MapGet("/{attemptId}", async (IMediator mediator, int attemptId) =>
            {
                var query = new GetUserAnswerDetailQuery(attemptId);
                var result = await mediator.Send(query);
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("GetUserAnswerDetail")
            .Produces<ServiceResponse<UserAnswerDetailDto>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<UserAnswerDetailDto>>(StatusCodes.Status401Unauthorized)
            .Produces<ServiceResponse<UserAnswerDetailDto>>(StatusCodes.Status404NotFound)
            .Produces<ServiceResponse<UserAnswerDetailDto>>(StatusCodes.Status500InternalServerError);
        }
    }
}
