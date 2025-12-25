namespace EduocationSystem.Features.Enrollment.Dtos
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string Semester { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
