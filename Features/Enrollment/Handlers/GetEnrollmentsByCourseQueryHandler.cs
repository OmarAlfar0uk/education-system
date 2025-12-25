using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Features.Enrollment.Dtos;
using EduocationSystem.Features.Enrollment.Queries;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Enrollment.Handlers
{
    public class GetEnrollmentsByCourseQueryHandler
       : IRequestHandler<GetEnrollmentsByCourseQuery, ServiceResponse<List<EnrollmentDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.Enrollment> _enrollmentRepo;
        private readonly ICurrentUserService _currentUser;

        public GetEnrollmentsByCourseQueryHandler(
            IGenericRepository<Domain.Entities.Enrollment> enrollmentRepo,
            ICurrentUserService currentUser)
        {
            _enrollmentRepo = enrollmentRepo;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<List<EnrollmentDto>>> Handle(
            GetEnrollmentsByCourseQuery request,
            CancellationToken cancellationToken)
        {
            // 🔒 Authorization Guard
            if (!_currentUser.IsInRole("Admin"))
            {
                return ServiceResponse<List<EnrollmentDto>>.ForbiddenResponse(
                    "You are not allowed to view enrollments for this course",
                    "غير مسموح لك بعرض تسجيلات هذا الكورس");
            }

            var enrollments = await _enrollmentRepo.GetAll()
                .Where(e =>
                    e.CourseId == request.CourseId &&
                    !e.IsDeleted)
                .Select(e => new EnrollmentDto
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
                })
                .ToListAsync(cancellationToken);

            return ServiceResponse<List<EnrollmentDto>>
                .SuccessResponse(enrollments);
        }
    }

}
