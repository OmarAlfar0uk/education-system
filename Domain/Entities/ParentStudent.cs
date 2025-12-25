namespace EduocationSystem.Domain.Entities
{
    public class ParentStudent : BaseEntity
    {
        public int ParentId { get; set; }
        public Parent Parent { get; set; } = null!;

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
    }
}
