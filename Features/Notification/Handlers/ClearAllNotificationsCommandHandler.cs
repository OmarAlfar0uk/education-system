using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Notification.Commend;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Notification.Handlers
{
    public class ClearAllNotificationsCommandHandler : IRequestHandler<ClearAllNotificationsCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.Notification> _notificationRepo;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public ClearAllNotificationsCommandHandler(
            IGenericRepository<Domain.Entities.Notification> notificationRepo,
            IUnitOfWork uow,
            ICurrentUserService currentUser)
        {
            _notificationRepo = notificationRepo;
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<bool>> Handle(
            ClearAllNotificationsCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            var notifications = await _notificationRepo
                .GetAll()
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .ToListAsync(cancellationToken);

            if (!notifications.Any())
                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "No notifications found",
                    "لا توجد إشعارات");

            foreach (var n in notifications)
                _notificationRepo.Delete(n);

            await _uow.SaveChangesAsync();

            return ServiceResponse<bool>.SuccessResponse(
                true,
                "All notifications cleared",
                "تم مسح كل الإشعارات");
        }
    }
}
