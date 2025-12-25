namespace EduocationSystem.Features.Dashboard.Dtos
{
    public class MostActiveExamsDto
    {
        public List<ExamActivityDto> Exams { get; set; } = new();
    }

    public class ExamActivityDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int AttemptCount { get; set; }
        public int TotalParticipants { get; set; }
        public decimal AverageScore { get; set; }
        public bool IsActive { get; set; }
    }
}