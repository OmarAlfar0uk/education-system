using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Notification.Commend
{
    public record DeleteNotificationCommand(int Id)
        : IRequest<ServiceResponse<bool>>;
}
