namespace EduocationSystem.Features.Exams.Dtos
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "MultipleChoice", "multi select", etc.
        public string Title { get; set; } = string.Empty;


        public List<ChoiceDto> Choices { get; set; } = new();
    }

    public class ChoiceDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}