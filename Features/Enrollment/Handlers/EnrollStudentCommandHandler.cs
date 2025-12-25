using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Features.Enrollment.Commands;

namespace EduocationSystem.Features.Enrollment.Handlers
{
    public class EnrollStudentCommandHandler
     : IRequestHandler<EnrollStudentCommand, ServiceResponse<int>>
    {
        private readonly IGenericRepository<Domain.Entities.Enrollment> _enrollmentRepo;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public EnrollStudentCommandHandler(
            IGenericRepository<Domain.Entities.Enrollment> enrollmentRepo,
            IUnitOfWork uow,
            ICurrentUserService currentUser)
        {
            _enrollmentRepo = enrollmentRepo;
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<int>> Handle(
            EnrollStudentCommand request,
            CancellationToken cancellationToken)
        {
            // 🔒 Authorization Guard
            if (_currentUser.IsInRole("Student"))
            {
                if (_currentUser.UserId != request.StudentId.ToString())
                {
                    return ServiceResponse<int>.ForbiddenResponse(
                        "You can only enroll yourself",
                        "لا يمكنك تسجيل طالب آخر");
                }
            }

            // Parent ممنوع
            if (_currentUser.IsInRole("Parent"))
            {
                return ServiceResponse<int>.ForbiddenResponse(
                    "Parents cannot enroll students",
                    "ولي الأمر لا يمكنه تسجيل الطلاب");
            }

            var exists = await _enrollmentRepo.GetAll()
                .AnyAsync(e =>
                    e.StudentId == request.StudentId &&
                    e.CourseId == request.CourseId &&
                    e.Status == "Active" &&
                    !e.IsDeleted,
                    cancellationToken);

            if (exists)
                return ServiceResponse<int>.ConflictResponse(
                    "Student already enrolled in this course",
                    "الطالب مسجل بالفعل في هذا الكورس");

            var enrollment = new Domain.Entities.Enrollment
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                Semester = request.Semester,
                Status = "Active"
            };

            await _enrollmentRepo.AddAsync(enrollment);
            await _uow.SaveChangesAsync();

            return ServiceResponse<int>.SuccessResponse(
                enrollment.Id,
                "Student enrolled successfully",
                "تم تسجيل الطالب بنجاح");
        }
    }

}