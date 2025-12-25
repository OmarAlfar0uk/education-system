using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Notification.Commend
{
    public record MarkNotificationAsReadCommand(int NotificationId)
        : IRequest<ServiceResponse<bool>>;

}
