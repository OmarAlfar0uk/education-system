namespace EduocationSystem.Features.Exams.Dtos
{
    public class EditExamDto
    {
        public int ExamId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public string? IconUrl { get; set; }
    }
}
