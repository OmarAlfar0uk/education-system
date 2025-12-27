using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Notification.Commend;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Notification.Handlers
{
    public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.Notification> _notificationRepo;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public DeleteNotificationCommandHandler(
            IGenericRepository<Domain.Entities.Notification> notificationRepo,
            IUnitOfWork uow,
            ICurrentUserService currentUser)
        {
            _notificationRepo = notificationRepo;
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<bool>> Handle(
            DeleteNotificationCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            var notification = await _notificationRepo
                .FirstOrDefaultAsync(n =>
                    n.Id == request.Id &&
                    n.UserId == userId &&
                    !n.IsDeleted);

            if (notification == null)
                return ServiceResponse<bool>.NotFoundResponse(
                    "Notification not found",
                    "الإشعار غير موجود");

            _notificationRepo.Delete(notification);

            await _uow.SaveChangesAsync();

            return ServiceResponse<bool>.SuccessResponse(
                true,
                "Notification deleted",
                "تم حذف الإشعار");
        }
    }
}
