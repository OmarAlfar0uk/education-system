namespace EduocationSystem.Domain.Entities
{
    public class Instructor : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public string Title { get; set; } = "";

        // Relations
        public ICollection<Course>? Courses { get; set; }
    }
}
