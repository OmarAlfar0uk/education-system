using EduocationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduocationSystem.Infrastructure.EntityConfigurations
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.HasOne(r => r.Parent)
                   .WithMany(p => p.Reports)
                   .HasForeignKey(r => r.ParentId);

            builder.HasOne(r => r.Student)
                   .WithMany(s => s.Reports)
                   .HasForeignKey(r => r.StudentId);

            builder.HasOne(r => r.Week)
                   .WithMany(w => w.Reports)
                   .HasForeignKey(r => r.WeekId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
