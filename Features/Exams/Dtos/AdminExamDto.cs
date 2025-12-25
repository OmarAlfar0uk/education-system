namespace EduocationSystem.Features.Exams.Dtos
{
    public class AdminExamDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationDate { get; set; }
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }
    }
}