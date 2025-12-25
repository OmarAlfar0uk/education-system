using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduocationSystem.Domain.Entities;

namespace EduocationSystem.Infrastructure.EntityConfigurations
{
    public class UserExamAttemptConfiguration : IEntityTypeConfiguration<UserExamAttempt>
    {
        public void Configure(EntityTypeBuilder<UserExamAttempt> builder)
        {
            builder.ToTable("UserExamAttempts");

            // BaseEntity properties
            builder.HasKey(ua => ua.Id);
            builder.Property(ua => ua.Id).ValueGeneratedOnAdd();

            builder.Property(ua => ua.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()")
                .HasColumnName("CreatedAt");

            builder.Property(ua => ua.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("UpdatedAt");

            builder.Property(ua => ua.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("IsDeleted");

            // Specific properties
            builder.Property(ua => ua.UserId)
                .IsRequired()
                .HasMaxLength(450)
                .HasColumnName("UserId");

            builder.Property(ua => ua.ExamId)
                .IsRequired()
                .HasColumnName("ExamId");

            builder.Property(ua => ua.Score)
                .IsRequired()
                .HasColumnName("Score");

            builder.Property(ua => ua.TotalQuestions)
                .IsRequired()
                .HasColumnName("TotalQuestions");

            builder.Property(ua => ua.AttemptDate)
                .IsRequired()
                .HasColumnType("datetime")
                .HasColumnName("AttemptDate");

            builder.Property(ua => ua.IsHighestScore)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("IsHighestScore");

            builder.HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ua => ua.Exam)
                .WithMany()
                .HasForeignKey(ua => ua.ExamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Navigation to UserAnswers
            builder.HasMany(ua => ua.UserAnswers)
                .WithOne(ua => ua.Attempt)
                .HasForeignKey(ua => ua.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}