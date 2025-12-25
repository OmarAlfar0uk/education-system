using EduocationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduocationSystem.Infrastructure.EntityConfigurations
{
    public class ParentStudentConfiguration : IEntityTypeConfiguration<ParentStudent>
    {
        public void Configure(EntityTypeBuilder<ParentStudent> builder)
        {
            builder.ToTable("ParentStudents");

            builder.HasKey(ps => new { ps.ParentId, ps.StudentId });

            builder.HasOne(ps => ps.Parent)
                   .WithMany(p => p.Children)
                   .HasForeignKey(ps => ps.ParentId)
                   .OnDelete(DeleteBehavior.Restrict);  // ⛔ FIXED

            builder.HasOne(ps => ps.Student)
                   .WithMany(s => s.ParentLinks)
                   .HasForeignKey(ps => ps.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
