using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Notification.Query
{
    public record GetMyNotificationsQuery()
    : IRequest<ServiceResponse<List<NotificationDto>>>;

}
