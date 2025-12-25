using MediatR;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Features.Exams.Queries;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Endpoints
{
    public static class GetUserExamsEndpoint
    {
        public static void MapUserExamEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/exams")
                .WithTags("Exams")
                .RequireAuthorization("AdminOrStudent");   // ⭐ Admin + Student

            // GET /api/exams - List exams (with optional filters)
            group.MapGet("/", async (
                IMediator mediator,
                [AsParameters] GetExamsQuery query) =>
            {
                var result = await mediator.Send(query);
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("GetExams")
            .Produces<ServiceResponse<PagedResult<UserExamDto>>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<PagedResult<UserExamDto>>>(StatusCodes.Status401Unauthorized)
            .Produces<ServiceResponse<PagedResult<UserExamDto>>>(StatusCodes.Status403Forbidden);
        }
    }

}