using MailKit.Net.Smtp;
using MailKit.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Features.Accounts.Commands;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Infrastructure.ApplicationDBContext;
using EduocationSystem.Shared.Helpers;
using EduocationSystem.Shared.Responses;
using System.Text.Json;

namespace EduocationSystem.Features.Accounts.Orchestrators
{
    public record ResetPasswordOrchestrator(ResetPasswordDto Dto) : IRequest<ServiceResponse<bool>>;
    public class ResetPasswordOrchestratorHandler : IRequestHandler<ResetPasswordOrchestrator, ServiceResponse<bool>>
    {
        private readonly IMediator _mediator;
        private readonly EmailSettings _emailSettings;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ResetPasswordOrchestratorHandler> _logger;


        public ResetPasswordOrchestratorHandler(IMediator mediator, IOptions<EmailSettings> emailSettings, ApplicationDbContext context, ILogger<ResetPasswordOrchestratorHandler> logger)
        {
            _mediator = mediator;
            _emailSettings = emailSettings.Value;
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResponse<bool>> Handle(ResetPasswordOrchestrator request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ResetPasswordCommand(request.Dto), cancellationToken);

            if (result.IsSuccess)
            {
                // send email to user notify them of password change successfully
                var subject = "Password Reset Successful";
                var body = $"<p>Your password has been reset successfully.</p><p>If you did not initiate this change, please contact support immediately.</p>";
                await QueueEmailAsync(request.Dto.Email, subject, body, EmailType.Notification, EmailPriority.High, isHtml: true);
                _logger.LogInformation("Queued password reset notification email to {Email}", request.Dto.Email);
                var queuedEmail = await _context.emailQueues
                   .Where(e => e.Status == EmailStatus.Pending && e.ToEmail == request.Dto.Email)
                   .OrderByDescending(e => e.CreatedAt)
                   .FirstOrDefaultAsync();

                if (queuedEmail != null)
                {
                    var sent = await SendEmailDirectlyAsync(queuedEmail);
                    if (sent)
                        await UpdateEmailStatusAsync(queuedEmail.Id, EmailStatus.Sent);
                    else
                        await UpdateEmailStatusAsync(queuedEmail.Id, EmailStatus.Failed, "SMTP failed");
                }

            }


            return result;
        }
        private async Task QueueEmailAsync(string toEmail, string subject, string body, EmailType emailType, EmailPriority priority = EmailPriority.Normal,
  object? templateData = null, bool isHtml = true, DateTime? scheduledAt = null, string? toName = null)
        {
            var emailQueue = new EmailQueue
            {
                Id = Guid.NewGuid(),
                ToEmail = toEmail,
                Subject = subject,
                ToName = toName ?? string.Empty,
                Body = body,
                IsHtml = isHtml,
                EmailType = emailType,
                Priority = priority,
                ScheduledAt = scheduledAt ?? DateTime.UtcNow,
                TemplateData = templateData != null ? JsonSerializer.Serialize(templateData) : string.Empty,
                CreatedAt = DateTime.UtcNow,
                Status = EmailStatus.Pending,
                ErrorMessage = string.Empty,
                MaxRetries = 3,
                RetryCount = 0
            };

            await _context.emailQueues.AddAsync(emailQueue);
            await _context.SaveChangesAsync();
        }

        private async Task<bool> SendEmailDirectlyAsync(EmailQueue emailItem)
        {
            try
            {
                _logger.LogInformation("Starting email send for {Email}. Server: {Server}, Port: {Port}, SSL: {Ssl}",
                    emailItem.ToEmail, _emailSettings.SmtpServer, _emailSettings.Port, _emailSettings.EnableSsl);

                if (string.IsNullOrEmpty(_emailSettings.SmtpServer) || string.IsNullOrEmpty(_emailSettings.Username))
                {
                    _logger.LogError("Email config incomplete: Server={Server}, Username={Username}",
                        _emailSettings.SmtpServer ?? "NULL", _emailSettings.Username ?? "NULL");
                    return false;
                }

                using var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName ?? "OnlineExam", _emailSettings.FromEmail));
                emailMessage.To.Add(new MailboxAddress(emailItem.ToName ?? "", emailItem.ToEmail));
                emailMessage.Subject = emailItem.Subject;

                var builder = new BodyBuilder();
                if (emailItem.IsHtml)
                {
                    builder.HtmlBody = emailItem.Body;
                }
                else
                {
                    builder.TextBody = emailItem.Body;
                }
                emailMessage.Body = builder.ToMessageBody();

                using var client = new SmtpClient();

                SecureSocketOptions secureOptions;
                if (_emailSettings.Port == 587)
                {
                    secureOptions = SecureSocketOptions.StartTls;
                    _logger.LogInformation("Using StartTls for Port 587");
                }
                else if (_emailSettings.Port == 465)
                {
                    secureOptions = SecureSocketOptions.SslOnConnect;
                    _logger.LogInformation("Using SslOnConnect for Port 465");
                }
                else
                {
                    secureOptions = SecureSocketOptions.Auto;
                    _logger.LogInformation("Using Auto for Port {_Port}", _emailSettings.Port);
                }

                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, secureOptions);
                _logger.LogInformation("Connected to SMTP server");

                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                _logger.LogInformation("SMTP authentication successful");

                await client.SendAsync(emailMessage);
                _logger.LogInformation("Email sent to {Email}", emailItem.ToEmail);

                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP error for {Email}: {Message}", emailItem.ToEmail, ex.Message);
                return false;
            }
        }

        private async Task UpdateEmailStatusAsync(Guid emailId, EmailStatus status, string? errorMessage = null)
        {
            var email = await _context.emailQueues.FindAsync(emailId);
            if (email != null)
            {
                email.Status = status;
                email.ErrorMessage = errorMessage;

                if (status == EmailStatus.Sent)
                {
                    email.SentAt = DateTime.UtcNow;
                }
                else if (status == EmailStatus.Failed)
                {
                    email.RetryCount++;
                    if (email.RetryCount < email.MaxRetries)
                    {
                        email.Status = EmailStatus.Pending;
                        email.NextRetryAt = DateTime.UtcNow.AddMinutes((int)Math.Pow(2, email.RetryCount));
                    }
                    else
                    {
                        email.Status = EmailStatus.Failed;
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

    }
}