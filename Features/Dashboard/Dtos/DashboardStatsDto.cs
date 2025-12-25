namespace EduocationSystem.Features.Dashboard.Dtos
{
    public class DashboardStatsDto
    {
        public int TotalExams { get; set; }
        public int ActiveExams { get; set; }
        public int InactiveExams { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public int TotalExamAttempts { get; set; }
        public decimal AverageScore { get; set; }
    }
}