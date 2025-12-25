using MediatR;
using EduocationSystem.Features.Categories.Dtos;
using EduocationSystem.Features.Categories.Queries;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Categories.Endpoints
{
    public static class GetCategoryByIdEndpoint
    {
        public static void MapGetCategoryByIdEndpoint(this WebApplication app)
        {
            app.MapGet("/api/categories/{id}", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetCategoryByIdQuery(id));
                return result;
            })
            //.RequireAuthorization()

            .RequireAuthorization("AdminOrStudent")
            .WithName("GetCategoryById")
            .WithTags("Categories")
            .Produces<ServiceResponse<object>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ServiceResponse<object>>(StatusCodes.Status404NotFound);
        }
    }
}