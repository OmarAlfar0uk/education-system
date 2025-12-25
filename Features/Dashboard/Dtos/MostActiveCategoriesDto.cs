namespace EduocationSystem.Features.Dashboard.Dtos
{
    public class MostActiveCategoriesDto
    {
        public List<CategoryActivityDto> Categories { get; set; } = new();
    }

    public class CategoryActivityDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ExamCount { get; set; }
        public int TotalAttempts { get; set; }
        public int TotalParticipants { get; set; }
        public decimal AverageScore { get; set; }
    }
}