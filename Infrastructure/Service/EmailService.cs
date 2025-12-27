using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Shared.Helpers;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace EduocationSystem.Infrastructure.Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            var message = new MailMessage();
            message.From = new MailAddress(_settings.FromEmail, _settings.FromName);
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;

            using var smtp = new SmtpClient(_settings.SmtpServer, _settings.Port);
            smtp.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            smtp.EnableSsl = _settings.EnableSsl;

            await smtp.SendMailAsync(message);
        }
    }

}
