using EduocationSystem.Features.Notification.Commend;
using EduocationSystem.Features.Notification.Query;
using MediatR;

namespace EduocationSystem.Features.Notification
{

    public static class NotificationsEndpoints
    {
        public static void MapNotificationsEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/notifications")
                .WithTags("Notifications")
                .RequireAuthorization();

            group.MapGet("/me", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new GetMyNotificationsQuery());
                return Results.Json(result, statusCode: result.StatusCode);
            });

            group.MapPost("/", async (
                CreateNotificationCommand command,
                IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Json(result, statusCode: result.StatusCode);
            });

            group.MapPut("/{id:int}/read", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(
                    new MarkNotificationAsReadCommand(id));

                return Results.Json(result, statusCode: result.StatusCode);
            });
        }
    }
}

