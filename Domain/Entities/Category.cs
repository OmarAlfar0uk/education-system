using EduocationSystem.Domain.Entities;
using System;

namespace EduocationSystem.Domain
{
    public class Category : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string IconUrl { get; set; } = null!;
        public string? Description { get; set; }

        // navigation property
        public virtual ICollection<Exam>? Exams { get; set; }

        // Each Category can have many Courses
        public ICollection<Course>? Courses { get; set; }

    }
}
