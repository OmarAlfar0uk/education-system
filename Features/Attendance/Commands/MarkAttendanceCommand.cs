using EduocationSystem.Features.Attendance.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Attendance.Commands
{

    public record MarkAttendanceCommand(MarkAttendanceDto Dto)
        : IRequest<ServiceResponse<bool>>;
}
