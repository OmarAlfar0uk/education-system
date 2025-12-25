using MediatR;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Features.Exams.Queries;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Endpoints
{
    public static class GetExamByIDEndpoint
    {
        public static void MapGetExamByIDEndpoint(this WebApplication app)
        {
            var group = app.MapGroup("/api/exams")
                .WithTags("Exams")
                .RequireAuthorization("AdminOrStudent");   // ⭐ Admin + Student

            group.MapGet("/{id}", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetExamDetailsQuery(id));
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .WithName("GetExamDetails")
            .Produces<ServiceResponse<UserExamDetailsDto>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<UserExamDetailsDto>>(StatusCodes.Status401Unauthorized)
            .Produces<ServiceResponse<UserExamDetailsDto>>(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse<UserExamDetailsDto>>(StatusCodes.Status404NotFound);
        }
    }

}
