namespace EduocationSystem.Features.UserAnswers.Dtos
{
    public class UserAnswerDetailDto
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
        public List<UserQuestionAnswerDto> QuestionAnswers { get; set; } = new();
    }

    public class UserQuestionAnswerDto
    {
        public int QuestionId { get; set; }
        public string QuestionTitle { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public List<UserSelectedChoiceDto> SelectedChoices { get; set; } = new();
        public List<CorrectChoiceDto> CorrectChoices { get; set; } = new();
    }

    public class UserSelectedChoiceDto
    {
        public int ChoiceId { get; set; }
        public string ChoiceText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class CorrectChoiceDto
    {
        public int ChoiceId { get; set; }
        public string ChoiceText { get; set; } = string.Empty;
    }
}