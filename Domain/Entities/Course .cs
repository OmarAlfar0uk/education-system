namespace EduocationSystem.Domain.Entities
{
    public class Course : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int Credits { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public int InstructorId { get; set; }
        public Instructor Instructor { get; set; } = null!;

        // Relations
        public ICollection<Material>? Materials { get; set; }
        public ICollection<Enrollment>? Enrollments { get; set; }
        public ICollection<Week>? Weeks { get; set; }
    }
}
