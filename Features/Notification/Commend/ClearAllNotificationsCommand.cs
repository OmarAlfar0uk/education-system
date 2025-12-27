using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Notification.Commend
{
    public record ClearAllNotificationsCommand
         : IRequest<ServiceResponse<bool>>;
}
