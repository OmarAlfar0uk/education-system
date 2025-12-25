using MailKit.Net.Smtp;
using MailKit.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Infrastructure.ApplicationDBContext;
using EduocationSystem.Shared.Helpers;
using EduocationSystem.Shared.Responses;
using System.Text.Json;
using TechZone.Core.Entities;

namespace EduocationSystem.Features.Accounts.Commands
{
    public record SendVerificationEmailCommand(ResendVerificationCodeDto Dto)
        : IRequest<ServiceResponse<bool>>;

    public class SendVerificationEmailCommandHandler
        : IRequestHandler<SendVerificationEmailCommand, ServiceResponse<bool>>
    {
        private readonly EmailSettings _emailSettings;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SendVerificationEmailCommand> _logger;

        public SendVerificationEmailCommandHandler(
            IOptions<EmailSettings> emailSettings,
            ApplicationDbContext context,
            ILogger<SendVerificationEmailCommand> logger)
        {
            _emailSettings = emailSettings.Value;
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResponse<bool>> Handle(SendVerificationEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 🔹 Step 1: Validate the request DTO
                var validationErrors = ValidateRequest(request.Dto);
                if (validationErrors.Any())
                {
                    return ServiceResponse<bool>.ValidationErrorResponse(
                        validationErrors,
                        "Validation failed",
                        "فشل التحقق من صحة البيانات"
                    );
                }

                // 🔹 Step 2: Find the user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Dto.Email, cancellationToken);

                if (user == null)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "User not found",
                        "المستخدم غير موجود",
                        404
                    );
                }

                // 🔹 Step 3: Generate verification code
                var code = request.Dto.ConfirmationCode ?? GenerateVerificationCode();

                // 🔹 Step 4: Save verification code to VerificationCodes table
                var verificationCode = new VerificationCode
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Code = code,
                    Type = TechZone.Core.Entities.VerificationCodeType.EmailVerification,
                    ExpiryDate = DateTime.UtcNow.AddHours(24), // 24 hours expiry
                    IsUsed = false,
                    AttemptCount = 0,
                    MaxAttempts = 3, // Allow 3 attempts
                    CreatedAt = DateTime.UtcNow
                };

                // 🔹 Step 5: Invalidate any previous verification codes for this user and type
                var previousCodes = await _context.VerificationCodes
                    .Where(v => v.UserId == user.Id &&
                               v.Type == TechZone.Core.Entities.VerificationCodeType.EmailVerification &&
                               !v.IsUsed)
                    .ToListAsync(cancellationToken);

                foreach (var prevCode in previousCodes)
                {
                    prevCode.IsUsed = true; // Mark previous codes as used
                }

                await _context.VerificationCodes.AddAsync(verificationCode, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // 🔹 Step 6: Queue and send the email (your existing code)
                var subject = "Confirm Your Email - OnlineExam";
                var body = GetEmailConfirmationTemplate(code);

                await QueueEmailAsync(
                    request.Dto.Email,
                    subject,
                    body,
                    EmailType.Verification,
                    EmailPriority.High,
                    new { Code = code },
                    true,
                    null,
                    "User"
                );

                // 🔹 Step 7: Try sending directly
                var pendingEmail = await _context.emailQueues
                    .Where(e => e.Status == EmailStatus.Pending && e.ToEmail == request.Dto.Email)
                    .OrderByDescending(e => e.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (pendingEmail != null)
                {
                    var sendSuccess = await SendEmailDirectlyAsync(pendingEmail);
                    if (sendSuccess)
                    {
                        await UpdateEmailStatusAsync(pendingEmail.Id, EmailStatus.Sent);
                        _logger.LogInformation("Verification email sent to {Email}", request.Dto.Email);

                        return ServiceResponse<bool>.SuccessResponse(
                            true,
                            "Confirmation email sent successfully",
                            "تم إرسال البريد الإلكتروني لتأكيد الحساب بنجاح"
                        );
                    }

                    await UpdateEmailStatusAsync(pendingEmail.Id, EmailStatus.Failed, "SMTP send failed");
                    return ServiceResponse<bool>.ErrorResponse(
                        "Failed to send confirmation email",
                        "فشل إرسال البريد الإلكتروني لتأكيد الحساب",
                        500
                    );
                }

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Email queued successfully",
                    "تمت إضافة البريد الإلكتروني إلى قائمة الانتظار بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification email to {Email}", request.Dto.Email);
                return ServiceResponse<bool>.InternalServerErrorResponse(
                    "An unexpected error occurred while sending the verification email",
                    "حدث خطأ غير متوقع أثناء إرسال البريد الإلكتروني للتحقق"
                );
            }
        }
        // 🔹 Validation Method
        private Dictionary<string, List<string>> ValidateRequest(ResendVerificationCodeDto dto)
        {
            var errors = new Dictionary<string, List<string>>();

            if (dto == null)
            {
                errors["Request"] = new List<string> { "Request data is required" };
                return errors;
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                errors["Email"] = new List<string> { "Email is required" };
            }
            else if (!IsValidEmail(dto.Email))
            {
                errors["Email"] = new List<string> { "Invalid email format" };
            }

            return errors;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // 🔹 Queue email
        private async Task QueueEmailAsync(
            string toEmail,
            string subject,
            string body,
            EmailType emailType,
            EmailPriority priority = EmailPriority.Normal,
            object? templateData = null,
            bool isHtml = true,
            DateTime? scheduledAt = null,
            string? toName = null)
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

        // 🔹 Send email directly
        private async Task<bool> SendEmailDirectlyAsync(EmailQueue emailItem)
        {
            try
            {
                using var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName ?? "EducationalSystem", _emailSettings.FromEmail));
                emailMessage.To.Add(new MailboxAddress(emailItem.ToName ?? "", emailItem.ToEmail));
                emailMessage.Subject = emailItem.Subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = emailItem.IsHtml ? emailItem.Body : null,
                    TextBody = !emailItem.IsHtml ? emailItem.Body : null
                };
                emailMessage.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                var secureOptions = _emailSettings.Port switch
                {
                    587 => SecureSocketOptions.StartTls,
                    465 => SecureSocketOptions.SslOnConnect,
                    _ => SecureSocketOptions.Auto
                };

                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, secureOptions);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", emailItem.ToEmail);
                return false;
            }
        }

        // 🔹 Update email status
        private async Task UpdateEmailStatusAsync(Guid emailId, EmailStatus status, string? errorMessage = null)
        {
            var email = await _context.emailQueues.FindAsync(emailId);
            if (email == null) return;

            email.Status = status;
            email.ErrorMessage = errorMessage;
            if (status == EmailStatus.Sent)
                email.SentAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // 🔹 HTML Template
        private string GetEmailConfirmationTemplate(string code)
        {
            return $@"
                <html>
                    <body style='font-family: Arial;'>
                        <h2>Email Verification</h2>
                        <p>Your verification code is:</p>
                        <div style='font-size: 28px; font-weight: bold; color: #007bff;'>{code}</div>
                        <p>This code will expire in 24 hours.</p>
                    </body>
                </html>";
        }

        // 🔹 Generate random code
        private string GenerateVerificationCode()
        {
            var random = new Random();
            return string.Concat(Enumerable.Range(0, 6).Select(_ => random.Next(0, 10)));
        }
    }
}
