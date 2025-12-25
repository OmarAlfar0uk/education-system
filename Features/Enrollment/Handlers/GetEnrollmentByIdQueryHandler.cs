using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Enrollment.Dtos;
using EduocationSystem.Features.Enrollment.Queries;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Enrollment.Handlers
{
    public class GetEnrollmentByIdQueryHandler
    : IRequestHandler<GetEnrollmentByIdQuery, ServiceResponse<EnrollmentDto>>
    {
        private readonly IGenericRepository<Domain.Entities.Enrollment> _enrollmentRepo;
        private readonly ICurrentUserService _currentUser;

        public GetEnrollmentByIdQueryHandler(
            IGenericRepository<Domain.Entities.Enrollment> enrollmentRepo,
            ICurrentUserService currentUser)
        {
            _enrollmentRepo = enrollmentRepo;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<EnrollmentDto>> Handle(
            GetEnrollmentByIdQuery request,
            CancellationToken cancellationToken)
        {
            var enrollment = await _enrollmentRepo.GetAll()
                .Where(e => e.Id == request.Id && !e.IsDeleted)
                .Select(e => new
                {
                    Enrollment = e,
                    Dto = new EnrollmentDto
                    {
                        Id = e.Id,
                        StudentId = e.StudentId,
                        StudentName =
                            e.Student.User.FirstName + " " +
                            e.Student.User.LastName,
                        CourseId = e.CourseId,
                        CourseName = e.Course.Title,
                        Semester = e.Semester,
                        Status = e.Status,
                        CreatedAt = e.CreatedAt
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (enrollment == null)
                return ServiceResponse<EnrollmentDto>.NotFoundResponse(
                    "Enrollment not found",
                    "التسجيل غير موجود");

            // 🔒 Authorization Guard
            if (!_currentUser.IsInRole("Admin"))
            {
                // Student يشوف نفسه بس
                if (_currentUser.IsInRole("Student") &&
                    enrollment.Enrollment.Student.UserId != _currentUser.UserId)
                {
                    return ServiceResponse<EnrollmentDto>.ForbiddenResponse(
                        "You are not allowed to view this enrollment",
                        "غير مسموح لك بعرض هذا التسجيل");
                }
            }

            return ServiceResponse<EnrollmentDto>.SuccessResponse(enrollment.Dto);
        }
    }

}