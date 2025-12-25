using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Infrastructure.ApplicationDBContext;
using EduocationSystem.Shared.Helpers;
using EduocationSystem.Shared.Responses;
using System.Text.Json;

namespace EduocationSystem.Features.Accounts.Commands
{
    public record SendWelcomeEmailCommand(string Email, string FullName) : IRequest<ServiceResponse<bool>>
    {
        public class Handler : IRequestHandler<SendWelcomeEmailCommand, ServiceResponse<bool>>
        {
            private readonly EmailSettings _emailSettings;
            private readonly ApplicationDbContext _context;
            private readonly ILogger<SendWelcomeEmailCommand> _logger;

            public Handler(IOptions<EmailSettings> emailSettings, ApplicationDbContext context, ILogger<SendWelcomeEmailCommand> logger)
            {
                _emailSettings = emailSettings.Value;
                _context = context;
                _logger = logger;
            }

            public async Task<ServiceResponse<bool>> Handle(SendWelcomeEmailCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var subject = "Welcome to OnlineExam!";
                    var body = GetWelcomeTemplate(request.FullName);

                    // Queue the email
                    await QueueEmailAsync(request.Email, subject, body, EmailType.Welcome, EmailPriority.Normal, new { UserName = request.FullName }, true, null, request.FullName);

                    // Send directly for immediate processing
                    var pendingEmail = await _context.emailQueues
                        .Where(e => e.Status == EmailStatus.Pending && e.ToEmail == request.Email)
                        .OrderByDescending(e => e.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (pendingEmail != null)
                    {
                        var sendSuccess = await SendEmailDirectlyAsync(pendingEmail);
                        if (sendSuccess)
                        {
                            await UpdateEmailStatusAsync(pendingEmail.Id, EmailStatus.Sent);
                            _logger.LogInformation("Welcome email sent to {Email}", request.Email);
                            return ServiceResponse<bool>.SuccessResponse(true, "Welcome email sent", "تم إرسال بريد الترحيب");
                        }
                        else
                        {
                            await UpdateEmailStatusAsync(pendingEmail.Id, EmailStatus.Failed, "SMTP send failed");
                            return ServiceResponse<bool>.InternalServerErrorResponse("Failed to send welcome email", "فشل في إرسال بريد الترحيب");
                        }
                    }

                    return ServiceResponse<bool>.SuccessResponse(true, "Welcome email queued successfully", "تم جدولة بريد الترحيب");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending welcome email to {Email}", request.Email);
                    return ServiceResponse<bool>.InternalServerErrorResponse("Failed to send welcome email", "فشل في إرسال بريد الترحيب");
                }
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

            private string GetWelcomeTemplate(string fullName)
            {
                return $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <title>Welcome to EducationalSystem</title>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                            .content {{ background-color: #f8f9fa; padding: 20px; border-radius: 0 0 5px 5px; }}
                            .footer {{ margin-top: 30px; font-size: 12px; color: #666; text-align: center; }}
                            .features {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>Welcome to EducationalSystem!</h1>
                            </div>
                            <div class='content'>
                                <h2>Hello {fullName}!</h2>
                                <p>Thank you for joining OnlineExam. Start testing your knowledge today!</p>
                                <div class='features'>
                                    <h3>You can now:</h3>
                                    <ul>
                                        <li>Browse categories and exams</li>
                                        <li>Take timed quizzes</li>
                                        <li>View your scores and answers</li>
                                        <li>Edit your profile anytime</li>
                                    </ul>
                                </div>
                                <p>Happy learning!</p>
                            </div>
                            <div class='footer'>
                                <p>&copy; 2025 OnlineExam. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>";
            }
        }
    }
}