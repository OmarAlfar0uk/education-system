using System.Composition;

namespace EduocationSystem.Domain.Entities
{
    public class Parent : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public string Relation { get; set; } = string.Empty;

        // Relations
        public ICollection<ParentStudent>? Children { get; set; }
        public ICollection<Report>? Reports { get; set; }
    }
}
