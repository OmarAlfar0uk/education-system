using EduocationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduocationSystem.Infrastructure.EntityConfigurations
{
    public class GradeConfiguration : IEntityTypeConfiguration<Grade>
    {
        public void Configure(EntityTypeBuilder<Grade> builder)
        {
            builder.HasOne(g => g.Enrollment)
                   .WithMany(e => e.Grades)
                   .HasForeignKey(g => g.EnrollmentId);
        }
    }
}
