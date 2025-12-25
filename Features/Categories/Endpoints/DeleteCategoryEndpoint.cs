using MediatR;
using EduocationSystem.Features.Categories.Commands;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Categories.Endpoints
{
    public static class DeleteCategoryEndpoint
    {
        public static void MapDeleteCategoryEndpoint(this WebApplication app)
        {
            app.MapDelete("/api/categories/{id}", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteCategoryCommand(id));
                return result;
            })
            .RequireAuthorization("AdminOnly")
            .WithName("DeleteCategory")
            .WithTags("Categories")
            .Produces<ServiceResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<bool>>(StatusCodes.Status404NotFound);
        }
    }
}