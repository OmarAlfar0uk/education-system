namespace EduocationSystem.Features.Exams.Dtos
{
    public class CreateExamDto
    {
        public string Title { get; set; } = null!;
        public string IconUrl { get; set; } = null!;
        public int CategoryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public string? Description { get; set; }
    }
}
