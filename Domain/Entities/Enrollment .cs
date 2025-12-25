using System.Diagnostics;

namespace EduocationSystem.Domain.Entities
{
    public class Enrollment : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public string Semester { get; set; } = "";
        public string Status { get; set; } = "Active";

        // Relations
        public ICollection<Grade>? Grades { get; set; }
        public ICollection<Attendance>? AttendanceRecords { get; set; }
    }
}
