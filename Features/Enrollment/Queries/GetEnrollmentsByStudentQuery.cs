using EduocationSystem.Features.Enrollment.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Enrollment.Queries
{
    public record GetEnrollmentsByStudentQuery(int StudentId)
         : IRequest<ServiceResponse<List<EnrollmentDto>>>;
}
