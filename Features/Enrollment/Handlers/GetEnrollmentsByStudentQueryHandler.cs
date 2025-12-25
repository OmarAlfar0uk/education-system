using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Enrollment.Dtos;
using EduocationSystem.Features.Enrollment.Queries;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Enrollment.Handlers
{
    public class GetEnrollmentsByStudentQueryHandler
       : IRequestHandler<GetEnrollmentsByStudentQuery, ServiceResponse<List<EnrollmentDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.Enrollment> _enrollmentRepo;
        private readonly ICurrentUserService _currentUser;

        public GetEnrollmentsByStudentQueryHandler(
            IGenericRepository<Domain.Entities.Enrollment> enrollmentRepo,
            ICurrentUserService currentUser)
        {
            _enrollmentRepo = enrollmentRepo;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<List<EnrollmentDto>>> Handle(
            GetEnrollmentsByStudentQuery request,
            CancellationToken cancellationToken)
        {
            // 🔒 Authorization Guard
            if (_currentUser.IsInRole("Student"))
            {
                if (_currentUser.UserId != request.StudentId.ToString())
                {
                    return ServiceResponse<List<EnrollmentDto>>.ForbiddenResponse(
                        "You are not allowed to view enrollments of another student",
                        "غير مسموح لك بعرض تسجيلات طالب آخر");
                }
            }

            // Parent ممنوع من هنا
            if (_currentUser.IsInRole("Parent"))
            {
                return ServiceResponse<List<EnrollmentDto>>.ForbiddenResponse(
                    "Parents cannot access this endpoint",
                    "ولي الأمر لا يمكنه الوصول إلى هذه البيانات");
            }

            var enrollments = await _enrollmentRepo.GetAll()
                .Where(e =>
                    e.StudentId == request.StudentId &&
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
