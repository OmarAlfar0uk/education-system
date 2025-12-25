using MediatR;
using EduocationSystem.Features.Dashboard.Dtos;
using EduocationSystem.Features.Dashboard.Queries;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Dashboard.Endpoints
{
    public static class DashboardStatsEndpoint
    {
        public static void MapDashboardStatsEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/admin/dashboard")
                .WithTags("Admin Dashboard")
                     .RequireAuthorization("AdminOnly");

            // GET /api/admin/dashboard/stats - Gets counts of active/inactive exams and other stats
            group.MapGet("/stats", async (IMediator mediator) =>
            {
                var query = new GetDashboardStatsQuery();
                var result = await mediator.Send(query);
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("GetDashboardStats")
            .Produces<ServiceResponse<DashboardStatsDto>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<DashboardStatsDto>>(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse<DashboardStatsDto>>(StatusCodes.Status500InternalServerError);
        }
    }
}