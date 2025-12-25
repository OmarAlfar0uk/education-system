using MediatR;
using EduocationSystem.Features.Exams.Commands;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Endpoints
{
    public static class StartExamAttemptEndpoint
    {
        public static void MapStartExamAttemptEndpoint(this WebApplication app)
        {
            app.MapPost("/api/exams/{id}/attempt", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new StartExamAttemptCommand(id));
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .RequireAuthorization("AdminOrStudent")   // ⭐ Admin + Student
            .WithName("StartExamAttempt")
            .WithTags("Exams")
            .Produces<ServiceResponse<List<QuestionDto>>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<List<QuestionDto>>>(StatusCodes.Status400BadRequest)
            .Produces<ServiceResponse<List<QuestionDto>>>(StatusCodes.Status401Unauthorized)
            .Produces<ServiceResponse<List<QuestionDto>>>(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse<List<QuestionDto>>>(StatusCodes.Status404NotFound);
        }
    }

}