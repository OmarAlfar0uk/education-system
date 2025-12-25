using EduocationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduocationSystem.Infrastructure.EntityConfigurations
{
    public class MaterialConfiguration : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.HasOne(m => m.Course)
                   .WithMany(c => c.Materials)
                   .HasForeignKey(m => m.CourseId);
        }
    }
}
