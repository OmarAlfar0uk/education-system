using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Notification.Commend;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Notification.Handlers
{
    public class MarkNotificationAsReadCommandHandler
        : IRequestHandler<MarkNotificationAsReadCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.Notification> _notificationRepo;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public MarkNotificationAsReadCommandHandler(
            IGenericRepository<Domain.Entities.Notification> notificationRepo,
            IUnitOfWork uow,
            ICurrentUserService currentUser)
        {
            _notificationRepo = notificationRepo;
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<bool>> Handle(
            MarkNotificationAsReadCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            var notification = await _notificationRepo
                .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == userId);

            if (notification == null)
                return ServiceResponse<bool>.NotFoundResponse(
                    "Notification not found",
                    "الإشعار غير موجود");

            notification.IsRead = true;
            _notificationRepo.Update(notification);

            await _uow.SaveChangesAsync();

            return ServiceResponse<bool>.SuccessResponse(
                true,
                "Notification marked as read",
                "تم تحديد الإشعار كمقروء");
        }
    }

}
