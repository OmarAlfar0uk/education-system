namespace EduocationSystem.Domain.Entities
{
    public class ActivityLog : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public string Action { get; set; } = "";
        public string? Metadata { get; set; }
    }
}
