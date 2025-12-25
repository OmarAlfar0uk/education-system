using EduocationSystem.Features.Attendance.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Attendance.Queries
{

    public record GetAttendanceByStudentQuery(int StudentId)
        : IRequest<ServiceResponse<List<AttendanceDto>>>;
}
