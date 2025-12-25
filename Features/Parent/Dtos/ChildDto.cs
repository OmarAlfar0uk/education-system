namespace EduocationSystem.Features.Parent.Dtos
{
    public class ChildDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Level { get; set; }
    }
}
