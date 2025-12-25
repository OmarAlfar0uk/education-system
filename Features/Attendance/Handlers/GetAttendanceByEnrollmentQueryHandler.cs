using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Attendance.Dtos;
using EduocationSystem.Features.Attendance.Queries;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Attendance.Handlers
{
    public class GetAttendanceByEnrollmentQueryHandler : IRequestHandler<GetAttendanceByEnrollmentQuery, ServiceResponse<List<AttendanceDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.Attendance> _attendanceRepo;

        public GetAttendanceByEnrollmentQueryHandler(
            IGenericRepository<Domain.Entities.Attendance> attendanceRepo)
        {
            _attendanceRepo = attendanceRepo;
        }

        public async Task<ServiceResponse<List<AttendanceDto>>> Handle(
            GetAttendanceByEnrollmentQuery request,
            CancellationToken cancellationToken)
        {
            var attendance = await _attendanceRepo.GetAll()
                .Where(a =>
                    a.EnrollmentId == request.EnrollmentId &&
                    !a.IsDeleted)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    WeekId = a.WeekId,
                    WeekNumber = a.Week.WeekNumber,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return ServiceResponse<List<AttendanceDto>>
                .SuccessResponse(attendance);
        }
    }
}
