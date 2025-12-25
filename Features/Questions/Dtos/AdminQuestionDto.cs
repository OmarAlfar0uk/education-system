// AdminQuestionDto.cs
namespace EduocationSystem.Features.Questions.Dtos
{
    public class AdminQuestionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public List<AdminChoiceDto> Choices { get; set; } = new();
    }
}