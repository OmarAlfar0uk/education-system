using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Notification.Commend
{
    public record CreateNotificationCommand(
     string UserId,
     string Title,
     string Body
 ) : IRequest<ServiceResponse<bool>>;

}
