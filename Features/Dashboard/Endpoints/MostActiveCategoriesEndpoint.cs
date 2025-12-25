using MediatR;
using EduocationSystem.Features.Dashboard.Dtos;
using EduocationSystem.Features.Dashboard.Queries;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Dashboard.Endpoints
{
    public static class MostActiveCategoriesEndpoint
    {
        public static void MapMostActiveCategoriesEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/admin/dashboard")
                .WithTags("Admin Dashboard")
                    .RequireAuthorization("AdminOnly");

            // GET /api/admin/dashboard/most-active-categories - Data for graph of most active categories
            group.MapGet("/most-active-categories", async (IMediator mediator) =>
            {
                var query = new GetMostActiveCategoriesQuery();
                var result = await mediator.Send(query);
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("GetMostActiveCategories")
            .Produces<ServiceResponse<MostActiveCategoriesDto>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<MostActiveCategoriesDto>>(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse<MostActiveCategoriesDto>>(StatusCodes.Status500InternalServerError);
        }
    }
}