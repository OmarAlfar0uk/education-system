using EduocationSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduocationSystem.Features.Questions.Dtos
{
    public class UpdateQuestionDto
    {
        [StringLength(500, ErrorMessage = "Question title cannot exceed 500 characters")]
        public string? Title { get; set; }

        public QuestionType? Type { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Exam ID must be a positive number")]
        public int? ExamId { get; set; }

        public List<UpdateChoiceDto>? Choices { get; set; }
    }

    public class UpdateChoiceDto
    {
        public int? Id { get; set; } // null for new choices, existing ID for updates

        [StringLength(500, ErrorMessage = "Choice text cannot exceed 500 characters")]
        public string? Text { get; set; }

        public bool? IsCorrect { get; set; }
    }
}