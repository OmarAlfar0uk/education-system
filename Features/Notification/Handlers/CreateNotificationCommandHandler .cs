    using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Notification.Commend;
using EduocationSystem.Features.Notification.Hubs;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace EduocationSystem.Features.Notification.Handlers
{
    public class CreateNotificationCommandHandler
       : IRequestHandler<CreateNotificationCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.Notification> _notificationRepo;
        private readonly IUnitOfWork _uow;


        private readonly IHubContext<NotificationHub> _hub;

        public CreateNotificationCommandHandler(
            IGenericRepository<Domain.Entities.Notification> notificationRepo,
            IUnitOfWork uow,
            IHubContext<NotificationHub> hub)
        {
            _notificationRepo = notificationRepo;
            _uow = uow;
            _hub = hub;
        }



        public CreateNotificationCommandHandler(
            IGenericRepository<Domain.Entities.Notification> notificationRepo,
            IUnitOfWork uow)
        {
            _notificationRepo = notificationRepo;
            _uow = uow;
        }

        public async Task<ServiceResponse<bool>> Handle(
            CreateNotificationCommand request,
            CancellationToken cancellationToken)
        {
            var notification = new Domain.Entities.Notification
            {
                UserId = request.UserId,
                Title = request.Title,
                Body = request.Body,
                IsRead = false
            };

            await _notificationRepo.AddAsync(notification);
            await _uow.SaveChangesAsync();


            await _hub.Clients
            .User(notification.UserId)
            .SendAsync("NewNotification", new
            {
                notification.Id,
                notification.Title,
                notification.Body,
                notification.IsRead,
                notification.CreatedAt
            });

            return ServiceResponse<bool>.SuccessResponse(
                true,
                "Notification created",
                "تم إنشاء الإشعار");
        }
    }

}
