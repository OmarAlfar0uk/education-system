using EduocationSystem.Features.Attendance.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Attendance.Queries
{
    public record GetAttendanceByEnrollmentQuery(int EnrollmentId)
        : IRequest<ServiceResponse<List<AttendanceDto>>>;
}
