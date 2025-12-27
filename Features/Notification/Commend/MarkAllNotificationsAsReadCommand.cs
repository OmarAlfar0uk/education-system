using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Notification.Commend
{
    public class MarkAllNotificationsAsReadCommand : IRequest<ServiceResponse<bool>>
    {
    }
}
