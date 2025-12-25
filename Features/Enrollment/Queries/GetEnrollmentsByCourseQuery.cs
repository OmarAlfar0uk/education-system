using EduocationSystem.Features.Enrollment.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Enrollment.Queries
{
    public record GetEnrollmentsByCourseQuery(int CourseId)
        : IRequest<ServiceResponse<List<EnrollmentDto>>>;
}
