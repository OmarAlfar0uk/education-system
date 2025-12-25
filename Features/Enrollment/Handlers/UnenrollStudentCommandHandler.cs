using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Enrollment.Commands;

using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Enrollments.Handlers
{
    public class UnenrollStudentCommandHandler
     : IRequestHandler<UnenrollStudentCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.Enrollment> _enrollmentRepo;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public UnenrollStudentCommandHandler(
            IGenericRepository<Domain.Entities.Enrollment> enrollmentRepo,
            IUnitOfWork uow,
            ICurrentUserService currentUser)
        {
            _enrollmentRepo = enrollmentRepo;
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<bool>> Handle(
            UnenrollStudentCommand request,
            CancellationToken cancellationToken)
        {
            // 🔒 Authorization Guard
            if (!_currentUser.IsInRole("Admin"))
            {
                return ServiceResponse<bool>.ForbiddenResponse(
                    "Only admins can unenroll students",
                    "فقط المشرف يمكنه إلغاء تسجيل الطلاب");
            }

            var enrollment = await _enrollmentRepo.GetByIdAsync(request.EnrollmentId);

            if (enrollment == null || enrollment.IsDeleted)
                return ServiceResponse<bool>.NotFoundResponse(
                    "Enrollment not found",
                    "التسجيل غير موجود");

            enrollment.Status = "Dropped";
            _enrollmentRepo.Delete(enrollment);

            await _uow.SaveChangesAsync();

            return ServiceResponse<bool>.SuccessResponse(
                true,
                "Student unenrolled successfully",
                "تم إلغاء تسجيل الطالب بنجاح");
        }
    }

}
