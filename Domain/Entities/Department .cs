namespace EduocationSystem.Domain.Entities
{
    public class Department : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        #region Relations
        public ICollection<Student>? Students { get; set; }
        public ICollection<Instructor>? Instructors { get; set; }
        public ICollection<Course>? Courses { get; set; }
        #endregion
    }
}
