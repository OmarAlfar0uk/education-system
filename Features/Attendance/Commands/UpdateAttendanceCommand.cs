using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Attendance.Commands
{
    public record UpdateAttendanceCommand(int AttendanceId, string Status)
     : IRequest<ServiceResponse<bool>>;
}
