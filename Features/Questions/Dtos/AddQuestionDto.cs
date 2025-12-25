using EduocationSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduocationSystem.Features.Questions.Dtos
{
    public class AddQuestionDto
    {
        [Required(ErrorMessage = "Question title is required")]
        [StringLength(500, ErrorMessage = "Question title cannot exceed 500 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Question type is required")]
        public QuestionType Type { get; set; }

        [Required(ErrorMessage = "Exam ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Exam ID must be a positive number")]
        public int ExamId { get; set; }

        [Required(ErrorMessage = "At least one choice is required")]
        [MinLength(2, ErrorMessage = "Question must have at least 2 choices")]
        [MaxLength(10, ErrorMessage = "Question cannot have more than 10 choices")]
        public List<AddChoiceDto> Choices { get; set; } = new();
    }

    public class AddChoiceDto
    {
        [Required(ErrorMessage = "Choice text is required")]
        [StringLength(500, ErrorMessage = "Choice text cannot exceed 500 characters")]
        public string Text { get; set; } = string.Empty;

        [Required(ErrorMessage = "IsCorrect flag is required")]
        public bool IsCorrect { get; set; }
    }
}