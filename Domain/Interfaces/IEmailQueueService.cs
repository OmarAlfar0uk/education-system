using EduocationSystem.Domain.Entities;

namespace EduocationSystem.Domain.Interfaces
{
    public interface IEmailQueueService
    {
        Task QueueAsync(EmailQueue email);
    }

}
