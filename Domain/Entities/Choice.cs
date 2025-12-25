using System.ComponentModel.DataAnnotations.Schema;

namespace EduocationSystem.Domain.Entities
{
    public class Choice : BaseEntity
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }

        [ForeignKey(nameof(Question))]
        public int QuestionId { get; set; }

        // Navigation property

        public Question Question { get; set; } = default!;

        public ICollection<UserSelectedChoice> UserSelectedChoices { get; set; } = [];
    }
}
