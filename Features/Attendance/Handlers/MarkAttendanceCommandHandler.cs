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

        public MarkAttendanceCommandHandler(
            IGenericRepository<Domain.Entities.Attendance> attendanceRepo,
            IUnitOfWork uow)
        {
            _attendanceRepo = attendanceRepo;
            _uow = uow;
        }

        public async Task<ServiceResponse<bool>> Handle(
            MarkAttendanceCommand request,
            CancellationToken cancellationToken)
        {
            var exists = await _attendanceRepo.GetAll()
                .AnyAsync(a =>
                   a.EnrollmentId == request.Dto.EnrollmentId
 &&
                    a.WeekId == request.Dto.WeekId,
                    cancellationToken);

            if (exists)
                return ServiceResponse<bool>.ConflictResponse(
                    "Attendance already marked",
                    "تم تسجيل الحضور مسبقًا");

            await _attendanceRepo.AddAsync(new Domain.Entities.Attendance
            {
                EnrollmentId = request.Dto.EnrollmentId,
                WeekId = request.Dto.WeekId,
                Status = request.Dto.Status
            });

            await _uow.SaveChangesAsync();

            return ServiceResponse<bool>.SuccessResponse(
                true,
                "Attendance marked successfully",
                "تم تسجيل الحضور بنجاح");
        }
    }

}
