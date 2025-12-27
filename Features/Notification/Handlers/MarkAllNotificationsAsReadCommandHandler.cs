using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Notification.Commend;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Notification.Handlers
{
    public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.Notification> _notificationRepo;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public MarkAllNotificationsAsReadCommandHandler(
            IGenericRepository<Domain.Entities.Notification> notificationRepo,
            IUnitOfWork uow,
            ICurrentUserService currentUser)
        {
            _notificationRepo = notificationRepo;
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<bool>> Handle(
            MarkAllNotificationsAsReadCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            var notifications = await _notificationRepo.GetAll()
                .Where(n => n.UserId == userId && !n.IsDeleted && !n.IsRead)
                .ToListAsync(cancellationToken);

            if (!notifications.Any())
            {
                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "No unread notifications",
                    "لا توجد إشعارات غير مقروءة");
            }

            foreach (var n in notifications)
                n.IsRead = true;

            await _uow.SaveChangesAsync();

            return ServiceResponse<bool>.SuccessResponse(
                true,
                "All notifications marked as read",
                "تم تحديد جميع الإشعارات كمقروءة");
        }
    }
}
