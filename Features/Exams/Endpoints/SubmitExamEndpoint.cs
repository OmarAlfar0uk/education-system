using MediatR;
using EduocationSystem.Features.Exams.Commands;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Endpoints
{
    public static class SubmitExamEndpoint
    {
        public static void MapSubmitExamEndpoint(this WebApplication app)
        {
            app.MapPost("/api/exams/{id}/submit", async (
                int id,
                SubmitExamDto submitExamDto,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new SubmitExamCommand(id, submitExamDto));
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .RequireAuthorization("AdminOrStudent")   // ⭐ Admin + Student
            .WithName("SubmitExam")
            .WithTags("Exams")
            .Produces<ServiceResponse<ExamResultDto>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<ExamResultDto>>(StatusCodes.Status400BadRequest)
            .Produces<ServiceResponse<ExamResultDto>>(StatusCodes.Status401Unauthorized)
            .Produces<ServiceResponse<ExamResultDto>>(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse<ExamResultDto>>(StatusCodes.Status404NotFound);
        }
    }

}