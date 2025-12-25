using MailKit.Net.Smtp;
using MailKit.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Infrastructure.ApplicationDBContext;
using EduocationSystem.Shared.Helpers;
using EduocationSystem.Shared.Responses;
using System.Text.Json;

namespace EduocationSystem.Features.Accounts.Commands
{
    public record SendResetEmailCommand(string Email, string ResetCode) : IRequest<ServiceResponse<bool>>
    {
        public class Handler : IRequestHandler<SendResetEmailCommand, ServiceResponse<bool>>
        {
            private readonly EmailSettings _emailSettings;
            private readonly ApplicationDbContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(IOptions<EmailSettings> emailSettings, ApplicationDbContext context, ILogger<Handler> logger)
            {
                _emailSettings = emailSettings.Value;
                _context = context;
                _logger = logger;
            }

            public async Task<ServiceResponse<bool>> Handle(SendResetEmailCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var subject = "Reset Your Password - EducationalSystem";
                    var body = GetPasswordResetTemplate(request.ResetCode);

                    // Queue the email
                    await QueueEmailAsync(request.Email, subject, body, EmailType.PasswordReset, EmailPriority.High, new { Code = request.ResetCode }, true, null, "User");

                    // Send directly
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
                            _logger.LogInformation("Reset email sent to {Email}", request.Email);
                            return ServiceResponse<bool>.SuccessResponse(true, "Reset email sent");
                        }
                        else
                        {
                            await UpdateEmailStatusAsync(pendingEmail.Id, EmailStatus.Failed, "SMTP send failed");
                            return ServiceResponse<bool>.InternalServerErrorResponse("Failed to send reset email");
                        }
                    }

                    return ServiceResponse<bool>.SuccessResponse(true, "Email queued successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending reset email to {Email}", request.Email);
                    return ServiceResponse<bool>.InternalServerErrorResponse("Failed to send reset email");
                }
            }

            // QueueEmailAsync, SendEmailDirectlyAsync, UpdateEmailStatusAsync (same as your SendVerificationEmailCommand)

            private string GetPasswordResetTemplate(string resetCode)
            {
                return $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <title>Password Reset</title>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .code-box {{ background-color: #fff5f5; border: 2px solid #dc3545; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; color: #dc3545; border-radius: 10px; margin: 20px 0; letter-spacing: 3px; }}
                            .footer {{ margin-top: 30px; font-size: 12px; color: #666; }}
                            .warning {{ background-color: #f8d7da; border: 1px solid #f5c6cb; padding: 10px; border-radius: 5px; margin: 15px 0; color: #721c24; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h2>Educational System - Password Reset Request</h2>
                            <p>We received a request to reset your password for your Educational System account.</p>
                            <p>Use the following code to reset your password:</p>
                            
                            <div class='code-box'>
                                {resetCode}
                            </div>
                            
                            <div class='warning'>
                                <strong>Security Notice:</strong> This code will expire in 15 minutes for your security.
                            </div>
                            
                            <p>Enter this code in the password reset form to continue.</p>
                            <p>If you didn't request a password reset, please ignore this email and your password will remain unchanged.</p>
                            
                            <div class='footer'>
                                <p>This is an automated email. Please do not reply.</p>
                                <p>&copy; 2025 Educational System. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>";
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
                    emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName ?? "EducationalSystem", _emailSettings.FromEmail));
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
}