namespace EduocationSystem.Domain.Entities
{
    public class Report : BaseEntity
    {
        public int ParentId { get; set; }
        public Parent Parent { get; set; } = null!;

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int? WeekId { get; set; }
        public Week? Week { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Summary { get; set; } = "";
    }
}
