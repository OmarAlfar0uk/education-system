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

            group.MapGet("/", async (
            int page,
            int pageSize,
            bool onlyUnread,
            IMediator mediator) =>
                    {
                        var result = await mediator.Send(
                            new GetUserNotificationsQuery(onlyUnread, page, pageSize)
                        );

                        return Results.Json(result, statusCode: result.StatusCode);
                    })
             .WithName("GetUserNotifications");


            group.MapPut("/mark-all-read", async (
            IMediator mediator) =>
                    {
                        var result = await mediator.Send(new MarkAllNotificationsAsReadCommand());
                        return Results.Json(result, statusCode: result.StatusCode);
                    })
          .WithName("MarkAllNotificationsAsRead");



            group.MapDelete("/{id:int}", async (
            int id,
            IMediator mediator) =>
                    {
                        var result = await mediator.Send(new DeleteNotificationCommand(id));
                        return Results.Json(result, statusCode: result.StatusCode);
                    })
        .WithName("DeleteNotification");


            group.MapDelete("/clear/all", async (
              IMediator mediator) =>
            {
                var result = await mediator.Send(new ClearAllNotificationsCommand());
                return Results.Json(result, statusCode: result.StatusCode);
            });

        }
    }
}

