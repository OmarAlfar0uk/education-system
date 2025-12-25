using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Attendance.Commands;
using EduocationSystem.Shared.Responses;
using EduocationSystem.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Attendance.Handlers
{
    public class MarkAttendanceCommandHandler
     : IRequestHandler<MarkAttendanceCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.Attendance> _attendanceRepo;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<Domain.Entities.Notification> _notificationRepo;

        public MarkAttendanceCommandHandler(    
            IGenericRepository<Domain.Entities.Attendance> attendanceRepo,
             IGenericRepository<Domain.Entities.Notification> notificationRepo,
            IUnitOfWork uow)
        {
            _attendanceRepo = attendanceRepo;
            _notificationRepo = notificationRepo;
            _uow = uow;
        }

        public async Task<ServiceResponse<bool>> Handle(
      MarkAttendanceCommand request,
      CancellationToken cancellationToken)
        {
            var exists = await _attendanceRepo.GetAll()
                .AnyAsync(a =>
                    a.EnrollmentId == request.Dto.EnrollmentId &&
                    a.WeekId == request.Dto.WeekId,
                    cancellationToken);

            if (exists)
                return ServiceResponse<bool>.ConflictResponse(
                    "Attendance already marked",
                    "تم تسجيل الحضور مسبقًا");

            var attendance = new Domain.Entities.Attendance
            {
                EnrollmentId = request.Dto.EnrollmentId,
                WeekId = request.Dto.WeekId,
                Status = request.Dto.Status
            };

            await _attendanceRepo.AddAsync(attendance);

            var student = await _attendanceRepo.GetAll()
                .Where(a => a.EnrollmentId == request.Dto.EnrollmentId)
                .Select(a => new
                {
                    a.Enrollment.Student.UserId,
                    a.Week.WeekNumber
                })
                .FirstAsync(cancellationToken);

            await _notificationRepo.AddAsync(new Domain.Entities.Notification
            {
                UserId = student.UserId,
                Title = "Attendance update",
                Body = $"Your attendance for week {student.WeekNumber} was marked as {request.Dto.Status}",
                IsRead = false
            });

            await _uow.SaveChangesAsync();

            return ServiceResponse<bool>.SuccessResponse(
                true,
                "Attendance marked successfully",
                "تم تسجيل الحضور بنجاح");
        }

    }

}
