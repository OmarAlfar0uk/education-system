using System.Composition;

namespace EduocationSystem.Domain.Entities
{
    public class Week : BaseEntity
    {
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Relations
        public ICollection<Attendance>? AttendanceRecords { get; set; }
        public ICollection<Report>? Reports { get; set; }
    }
}
