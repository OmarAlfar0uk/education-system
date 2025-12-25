using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Enrollment.Commands
{
    public record UnenrollStudentCommand(int EnrollmentId)
        : IRequest<ServiceResponse<bool>>;
}
