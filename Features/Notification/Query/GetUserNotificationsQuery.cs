using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Notification.Query
{
    public class GetUserNotificationsQuery : IRequest<ServiceResponse<PagedResult<NotificationDto>>>
    {
        public bool OnlyUnread { get; }
        public int Page { get; }
        public int PageSize { get; }

        public GetUserNotificationsQuery(bool onlyUnread, int page, int pageSize)
        {
            OnlyUnread = onlyUnread;
            Page = page;
            PageSize = pageSize;
        }
    }
}
