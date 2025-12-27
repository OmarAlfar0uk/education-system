using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;

namespace EduocationSystem.Infrastructure.Service
{
    public class NotificationService 
    {
        private readonly IGenericRepository<Notification> _notificationRepo;
        private readonly IEmailQueueService _emailQueue;
        private readonly IUnitOfWork _uow;

        public NotificationService(
            IGenericRepository<Notification> notificationRepo,
            IEmailQueueService emailQueue,
            IUnitOfWork uow)
        {
            _notificationRepo = notificationRepo;
            _emailQueue = emailQueue;
            _uow = uow;
        }

        public async Task NotifyAsync(string userId, string title, string body, bool sendEmail = false, string? email = null)
        {
            await _notificationRepo.AddAsync(new Notification
            {
                UserId = userId,
                Title = title,
                Body = body
            });

            await _uow.SaveChangesAsync();

            if (!sendEmail || email == null)
                return;

            var template = EmailTemplateHelper.LoadTemplate("Notification.html");

            template = EmailTemplateHelper.ReplaceTokens(template, new()
        {
            {"title", title},
            {"body", body}
        });

            await _emailQueue.QueueAsync(new EmailQueue
            {
                ToEmail = email,
                Subject = title,
                Body = template,
                EmailType = EmailType.Notification,
                Priority = EmailPriority.Normal
            });
        }
    }

}
