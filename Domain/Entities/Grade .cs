namespace EduocationSystem.Domain.Entities
{
    public class Grade : BaseEntity
    {
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; } = null!;

        public string AssessmentType { get; set; } = "";
        public decimal Score { get; set; }
    }
}
