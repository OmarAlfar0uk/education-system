using EduocationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduocationSystem.Infrastructure.EntityConfigurations
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.ToTable("Attendance");

            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.Enrollment)
                   .WithMany(e => e.AttendanceRecords)
                   .HasForeignKey(a => a.EnrollmentId)
                   .OnDelete(DeleteBehavior.Restrict);  

            builder.HasOne(a => a.Week)
                   .WithMany(w => w.AttendanceRecords)
                   .HasForeignKey(a => a.WeekId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
