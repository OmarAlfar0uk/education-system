using System.ComponentModel.DataAnnotations;

namespace EduocationSystem.Features.Attendance.Dtos
{
    public class MarkAttendanceDto
    {
        public int EnrollmentId { get; set; }

        [Required]
        public int WeekId { get; set; }

        [Required]
        public string Status { get; set; } = "Present";
    }
}
