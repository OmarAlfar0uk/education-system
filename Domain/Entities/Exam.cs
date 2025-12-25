using EduocationSystem.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduocationSystem.Domain
{
    public class Exam : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string IconUrl { get; set; } = null!;
        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }

        // navigation property
        public virtual Category? Category { get; set; }
        public virtual ICollection<Question> Questions { get; set; } = [];

    }
}
