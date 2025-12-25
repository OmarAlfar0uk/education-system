using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Attendance.Dtos;
using EduocationSystem.Features.Attendance.Queries;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Attendance.Handlers
{
    public class GetAttendanceByWeekQueryHandler
          : IRequestHandler<GetAttendanceByWeekQuery, ServiceResponse<List<AttendanceDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.Attendance> _attendanceRepo;

        public GetAttendanceByWeekQueryHandler(
            IGenericRepository<Domain.Entities.Attendance> attendanceRepo)
        {
            _attendanceRepo = attendanceRepo;
        }

        public async Task<ServiceResponse<List<AttendanceDto>>> Handle(
            GetAttendanceByWeekQuery request,   // ✅ صح
            CancellationToken cancellationToken)
        {
            var result = await _attendanceRepo.GetAll()
                .Where(a => a.WeekId == request.WeekId) // ✅ حسب الأسبوع
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.Enrollment.StudentId,
                    StudentName =
                        a.Enrollment.Student.User.FirstName + " " +
                        a.Enrollment.Student.User.LastName,
                    WeekId = a.WeekId,
                    WeekNumber = a.Week.WeekNumber,
                    Status = a.Status.ToString()
                })
                .ToListAsync(cancellationToken);

            return ServiceResponse<List<AttendanceDto>>.SuccessResponse(result);
        }
    }

}
