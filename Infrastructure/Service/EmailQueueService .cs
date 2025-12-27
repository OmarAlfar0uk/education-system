using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;

namespace EduocationSystem.Infrastructure.Service
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly IGenericRepository<EmailQueue> _emailRepo;
        private readonly IUnitOfWork _uow;

        public EmailQueueService(
            IGenericRepository<EmailQueue> emailRepo,
            IUnitOfWork uow)
        {
            _emailRepo = emailRepo;
            _uow = uow;
        }

        public async Task QueueAsync(EmailQueue email)
        {
            email.Status = EmailStatus.Pending;
            email.CreatedAt = DateTime.UtcNow;

            await _emailRepo.AddAsync(email);
            await _uow.SaveChangesAsync();
        }
    }

}
