using EduocationSystem.Features.Parent.Commands;
using EduocationSystem.Features.Parent.Queries;
using MediatR;

namespace EduocationSystem.Features.Parent.Endpoints
{

    public static class ParentsEndpoints
    {
        public static void MapParentsEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/parents")
                .WithTags("Parents")
                .RequireAuthorization();

            group.MapPost("/", async (
                CreateParentCommand command,
                IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Json(result, statusCode: result.StatusCode);
            });

            group.MapGet("/{id:int}", async (
                int id,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new GetParentByIdQuery(id));
                return Results.Json(result, statusCode: result.StatusCode);
            });

            group.MapGet("/{id:int}/children", async (
                int id,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new GetChildrenByParentQuery(id));
                return Results.Json(result, statusCode: result.StatusCode);
            });

            group.MapDelete("/{id:int}", async (
            int id,
            IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteStudentCommand(id));
                return Results.Json(result, statusCode: result.StatusCode);
            });

         

        }
    }
}