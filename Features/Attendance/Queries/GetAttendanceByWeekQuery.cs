using EduocationSystem.Features.Attendance.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Attendance.Queries
{

    public record GetAttendanceByWeekQuery(int WeekId)
        : IRequest<ServiceResponse<List<AttendanceDto>>>;
}
