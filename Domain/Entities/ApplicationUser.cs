using EduocationSystem.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Pipelines.Sockets.Unofficial.Arenas;
using System.ComponentModel.DataAnnotations;
using TechZone.Core.Entities;

namespace EduocationSystem.Domain
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public List<RefreshToken>? RefreshTokens { get; set; }
        public virtual ICollection<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();


        public Student? StudentProfile { get; set; }

        public Parent? ParentProfile { get; set; }

        public Instructor? InstructorProfile { get; set; }

        public ICollection<Notification>? Notifications { get; set; }

    }
}
