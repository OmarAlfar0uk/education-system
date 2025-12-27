using EduocationSystem.Domain.Entities;

namespace EduocationSystem.Domain.Interfaces
{
    public interface INotificationService
    {
        Task QueueAsync(EmailQueue email, CancellationToken cancellationToken = default);

      
        Task<IReadOnlyList<EmailQueue>> GetPendingAsync(
            int take = 50,
            CancellationToken cancellationToken = default);

        Task MarkAsSentAsync(Guid id, CancellationToken cancellationToken = default);

        Task MarkAsFailedAsync(
            Guid id,
            string error,
            CancellationToken cancellationToken = default);
    }
}
