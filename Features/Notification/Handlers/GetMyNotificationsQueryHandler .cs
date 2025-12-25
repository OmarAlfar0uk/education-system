using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Notification.Query;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Notification.Handlers
{
    public class GetMyNotificationsQueryHandler
         : IRequestHandler<GetMyNotificationsQuery, ServiceResponse<List<NotificationDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.Notification> _notificationRepo;
        private readonly ICurrentUserService _currentUser;

        public GetMyNotificationsQueryHandler(
            IGenericRepository<Domain.Entities.Notification> notificationRepo,
            ICurrentUserService currentUser)
        {
            _notificationRepo = notificationRepo;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<List<NotificationDto>>> Handle(
            GetMyNotificationsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            if (string.IsNullOrEmpty(userId))
                return ServiceResponse<List<NotificationDto>>
                    .UnauthorizedResponse("User not logged in", "المستخدم غير مسجل دخول");

            var notifications = await _notificationRepo
                .GetAll()
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Body = n.Body,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return ServiceResponse<List<NotificationDto>>.SuccessResponse(
                notifications,
                "Notifications loaded",
                "تم تحميل الإشعارات");
        }
    }

}
