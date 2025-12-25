namespace EduocationSystem.Features.UserAnswers.Dtos
{
    public class UserAnswerHistoryDto
    {
        public int AttemptId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime AttemptDate { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public int AttemptNumber { get; set; }
        public bool IsHighestScore { get; set; }
        public bool IsCompleted => FinishedAt.HasValue;
        public decimal ScorePercentage => TotalQuestions > 0 ? (decimal)Score / TotalQuestions * 100 : 0;
    }

    public class PagedUserAnswerHistoryDto
    {
        public List<UserAnswerHistoryDto> Attempts { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}