using EduocationSystem.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechZone.Core.Entities
{
    public enum VerificationCodeType
    {
        EmailVerification,
        PhoneVerification,
        PasswordReset,
        OtherVerification
    }

    public enum DestinationStatus
    {
        Email,
        Phone,
        Both,
        None
    }

    public class VerificationCode
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [Required, MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        public VerificationCodeType Type { get; set; } = VerificationCodeType.EmailVerification;
        public DestinationStatus Destination { get; set; } = DestinationStatus.None;

        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddMinutes(60);

        [Range(0, 10)]
        public int AttemptCount { get; set; } = 0;

        [Range(1, 10)]
        public int MaxAttempts { get; set; } = 3;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;
    }


}
