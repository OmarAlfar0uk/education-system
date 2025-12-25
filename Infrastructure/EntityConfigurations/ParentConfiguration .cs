using EduocationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduocationSystem.Infrastructure.EntityConfigurations
{
    public class ParentConfiguration : IEntityTypeConfiguration<Parent>
    {
        public void Configure(EntityTypeBuilder<Parent> builder)
        {
            builder.HasOne(p => p.User)
        .WithOne(u => u.ParentProfile)
        .HasForeignKey<Parent>(p => p.UserId)
        .OnDelete(DeleteBehavior.Restrict); 

        }
    }
}
