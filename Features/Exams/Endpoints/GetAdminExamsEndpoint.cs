using MediatR;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Features.Exams.Queries;
using EduocationSystem.Shared.Responses;
using System.Security.Claims;

namespace EduocationSystem.Features.Exams.Endpoints
{
    public static class GetAdminExamsEndpoint
    {
        public static void MapAdminExamEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/admin/exams")
                .WithTags("Exams")
                .RequireAuthorization("AdminOnly");   // ⭐ Admin فقط

            // GET /api/admin/exams
            group.MapGet("/", async (
                IMediator mediator,
                [AsParameters] GetExamsForAdminQuery query) =>
            {
                var result = await mediator.Send(query);
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("GetExamsForAdmin")
            .Produces<ServiceResponse<PagedResult<AdminExamDto>>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<PagedResult<AdminExamDto>>>(StatusCodes.Status401Unauthorized)
            .Produces<ServiceResponse<PagedResult<AdminExamDto>>>(StatusCodes.Status403Forbidden);
        }
    }

}