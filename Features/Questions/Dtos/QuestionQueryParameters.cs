// QuestionQueryParameters.cs
namespace EduocationSystem.Features.Questions.Queries
{
    public class QuestionQueryParameters
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? Search { get; init; }
        public int? ExamId { get; init; }
        public string? Type { get; init; }
        public string? SortBy { get; init; }
    }
}