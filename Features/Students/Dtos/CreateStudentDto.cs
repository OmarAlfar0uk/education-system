using System.ComponentModel.DataAnnotations;

namespace EduocationSystem.Features.Students.Dtos
{
    public class CreateStudentDto
    {
        [Required]
        public string UserId { get; set; } = default!;

        [Required]
        public int DepartmentId { get; set; }

        [Range(1, 8)]
        public int Level { get; set; }
    }
}
