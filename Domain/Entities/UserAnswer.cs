using System.ComponentModel.DataAnnotations.Schema;

namespace EduocationSystem.Domain.Entities
{
    public class UserAnswer : BaseEntity
    {
        [ForeignKey(nameof(UserExamAttempt))]
        public int AttemptId { get; set; }

        [ForeignKey(nameof(Question))]
        public int QuestionId { get; set; }

        // Navigation properties
        public virtual UserExamAttempt Attempt { get; set; }
        public virtual Question Question { get; set; } // Add Question navigation
        public virtual ICollection<UserSelectedChoice> SelectedChoices { get; set; } = [];
    }
}