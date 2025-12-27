using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;

namespace EduocationSystem.Infrastructure.Service
{
    public class EmailBackgroundWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailBackgroundWorker> _logger;

        public EmailBackgroundWorker(IServiceProvider serviceProvider,
            ILogger<EmailBackgroundWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("📧 Email Worker Started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var repo = scope.ServiceProvider.GetRequiredService<IGenericRepository<EmailQueue>>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var emails = repo.GetAll()
                        .Where(e =>
                            e.Status == EmailStatus.Pending &&
                            (e.NextRetryAt == null || e.NextRetryAt <= DateTime.UtcNow) &&
                            (e.ScheduledAt == null || e.ScheduledAt <= DateTime.UtcNow))
                        .OrderByDescending(e => e.Priority)
                        .Take(20)
                        .ToList();

                    foreach (var mail in emails)
                    {
                        try
                        {
                            await emailService.SendEmailAsync(
                                mail.ToEmail,
                                mail.Subject,
                                mail.Body,
                                mail.IsHtml
                            );

                            mail.Status = EmailStatus.Sent;
                            mail.SentAt = DateTime.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Email sending failed");

                            mail.RetryCount++;

                            if (mail.RetryCount >= mail.MaxRetries)
                            {
                                mail.Status = EmailStatus.Failed;
                                mail.ErrorMessage = ex.Message;
                            }
                            else
                            {
                                mail.Status = EmailStatus.Pending;
                                mail.ErrorMessage = ex.Message;
                                mail.NextRetryAt = DateTime.UtcNow.AddMinutes(2 * mail.RetryCount);
                            }
                        }
                    }

                    await uow.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Email worker crashed — recovered and continuing");
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
