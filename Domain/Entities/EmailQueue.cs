namespace EduocationSystem.Domain.Entities
{
    public enum EmailStatus { Pending, Sent, Failed }
    public enum EmailType { Verification, PasswordReset, Notification, Welcome }
    public enum EmailPriority { High, Normal, Low }

    public class EmailQueue
    {
        public Guid Id { get; set; }
        public string ToEmail { get; set; } = null!;
        public string? ToName { get; set; }
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool IsHtml { get; set; } = true;
        public EmailType EmailType { get; set; }
        public EmailPriority Priority { get; set; } = EmailPriority.Normal;
        public DateTime? ScheduledAt { get; set; }
        public string? TemplateData { get; set; } // JSON-serialized data
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public EmailStatus Status { get; set; } = EmailStatus.Pending;
        public string? ErrorMessage { get; set; }
        public int MaxRetries { get; set; } = 3;
        public int RetryCount { get; set; } = 0;
        public DateTime? NextRetryAt { get; set; }
        public DateTime? SentAt { get; set; }
    }
}