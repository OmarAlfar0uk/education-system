using EduocationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduocationSystem.Infrastructure.EntityConfigurations
{
    public class WeekConfiguration : IEntityTypeConfiguration<Week>
    {
        public void Configure(EntityTypeBuilder<Week> builder)
        {
            builder.ToTable("Weeks");

            builder.HasKey(w => w.Id);

            builder.HasOne(w => w.Course)
                   .WithMany(c => c.Weeks)
                   .HasForeignKey(w => w.CourseId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
