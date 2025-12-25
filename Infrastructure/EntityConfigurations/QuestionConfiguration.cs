using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Enums;

namespace EduocationSystem.Infrastructure.EntityConfigurations
{
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.ToTable("Questions");

            // BaseEntity properties
            builder.HasKey(q => q.Id);
            builder.Property(q => q.Id).ValueGeneratedOnAdd();

            builder.Property(q => q.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()")
                .HasColumnName("CreatedAt");

            builder.Property(q => q.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("UpdatedAt");

            builder.Property(q => q.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("IsDeleted");

            // Specific properties
            builder.Property(q => q.Title)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("Title");

            builder.Property(q => q.Type)
                .IsRequired()
                .HasConversion<string>()  // For enum to string
                .HasMaxLength(50)
                .HasColumnName("Type");

            builder.Property(q => q.ExamId)
                .IsRequired()
                .HasColumnName("ExamId");


            builder.HasOne(q => q.Exam)
           .WithMany(e => e.Questions)
           .HasForeignKey(q => q.ExamId)
           .OnDelete(DeleteBehavior.NoAction); // Or NoAction if needed

            // If you have a navigation property from Question to UserAnswer
            builder.HasMany(q => q.UserAnswers)
                .WithOne(ua => ua.Question)
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.NoAction); // Important: NoAction here too
        }
    }
}