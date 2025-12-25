using MediatR;
using EduocationSystem.Features.Dashboard.Dtos;
using EduocationSystem.Features.Dashboard.Queries;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Dashboard.Endpoints
{
    public static class MostActiveExamsEndpoint
    {
        public static void MapMostActiveExamsEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/admin/dashboard")
                .WithTags("Admin Dashboard")
                    .RequireAuthorization("AdminOnly");

            // GET /api/admin/dashboard/most-active-exams - Data for graph of most active exams
            group.MapGet("/most-active-exams", async (IMediator mediator) =>
            {
                var query = new GetMostActiveExamsQuery();
                var result = await mediator.Send(query);
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("GetMostActiveExams")
            .Produces<ServiceResponse<MostActiveExamsDto>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<MostActiveExamsDto>>(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse<MostActiveExamsDto>>(StatusCodes.Status500InternalServerError);
        }
    }
}