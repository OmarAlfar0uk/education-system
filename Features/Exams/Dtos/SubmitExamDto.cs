namespace EduocationSystem.Features.Exams.Dtos
{
    public class SubmitExamDto
    {
        public List<AnswerSubmissionDto> Answers { get; set; } = new();
    }

    public class AnswerSubmissionDto
    {
        public int QuestionId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new(); // For multiple choice

    }
}