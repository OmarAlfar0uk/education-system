using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;

namespace EduocationSystem.Domain.Entities
{
    public class UserExamAttempt : BaseEntity
    {
        public string UserId { get; set; }
        public int ExamId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime AttemptDate { get; set; }
        public bool IsHighestScore { get; set; } = false;
        public DateTime? FinishedAt { get; set; }
        public int AttemptNumber { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
        public Exam Exam { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; }

    }
}
