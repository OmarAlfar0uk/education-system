using System.ComponentModel.DataAnnotations.Schema;

namespace EduocationSystem.Domain.Entities
{
    public class UserSelectedChoice : BaseEntity
    {
        [ForeignKey(nameof(UserAnswer))]
        public int UserAnswerId { get; set; }
        [ForeignKey(nameof(Choice))]
        public int ChoiceId { get; set; }

        // Navigation properties
        public UserAnswer UserAnswer { get; set; } = default!;
        public Choice Choice { get; set; } = default!;
    }
}
