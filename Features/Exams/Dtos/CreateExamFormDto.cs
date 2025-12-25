namespace EduocationSystem.Features.Exams.Dtos
{
    public class CreateExamFormDto
    {
            // Title: 3–20 letters/spaces – validated by FluentValidation on the Command
            public string Title { get; set; } = default!;
            public IFormFile? Icon { get; set; }                 // required at controller level
            public int? CategoryId { get; set; }                 // nullable to show “select category” error
            public DateTime StartDate { get; set; }              // local
            public DateTime EndDate { get; set; }                // local
            public int Duration { get; set; }                    // minutes (20..180)
            public string? Description { get; set; }
        }
    }
