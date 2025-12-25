namespace EduocationSystem.Domain.Entities
{
    public class Material : BaseEntity
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string Type { get; set; } = "";

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
    }
}
