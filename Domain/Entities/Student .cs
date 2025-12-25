using System.Composition;

namespace EduocationSystem.Domain.Entities
{
    public class Student : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public string EnrollmentNumber { get; set; } = null!;
        public int Level { get; set; }

        // Relations
        public ICollection<Enrollment>? Enrollments { get; set; }
        public ICollection<ActivityLog>? ActivityLogs { get; set; }
        public ICollection<Report>? Reports { get; set; }
        public ICollection<ParentStudent>? ParentLinks { get; set; }
        public ICollection<ParentStudent> ParentStudents { get; set; }

    }
}
