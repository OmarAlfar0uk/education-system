namespace EduocationSystem.Features.Grade.DTOs
{
    public class GradeDto
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public string AssessmentType { get; set; } = "";
        public decimal Score { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
