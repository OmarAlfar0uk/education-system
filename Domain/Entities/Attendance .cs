namespace EduocationSystem.Domain.Entities
{
    public class Attendance : BaseEntity
    {
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; } = null!;

        public int WeekId { get; set; }
        public Week Week { get; set; } = null!;

        public DateTime Date { get; set; }
        public string Status { get; set; } = "Present"; // or Enum
    }
}
