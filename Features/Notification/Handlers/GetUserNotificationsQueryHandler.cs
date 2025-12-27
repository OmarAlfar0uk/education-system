using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Notification.Query;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Notification.Handlers
{
    public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, ServiceResponse<PagedResult<NotificationDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.Notification> _notificationRepo;
        private readonly ICurrentUserService _currentUser;

        public GetUserNotificationsQueryHandler(
            IGenericRepository<Domain.Entities.Notification> notificationRepo,
            ICurrentUserService currentUser)
        {
            _notificationRepo = notificationRepo;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<PagedResult<NotificationDto>>> Handle(
            GetUserNotificationsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _notificationRepo.GetAll()
                .Where(n => n.UserId == _currentUser.UserId && !n.IsDeleted);

            if (request.OnlyUnread)
                query = query.Where(n => !n.IsRead);

            query = query.OrderByDescending(n => n.CreatedAt);

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Body = n.Body,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<NotificationDto>(
                items,
                total,
                request.Page,
                request.PageSize
            );

            return ServiceResponse<PagedResult<NotificationDto>>
                .SuccessResponse(result, "Notifications loaded", "تم تحميل الإشعارات");
        }
    }
}
