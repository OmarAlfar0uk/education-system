using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Enrollment.Commands
{
    public record EnrollStudentCommand(
     int StudentId,
        int CourseId,
        string Semester
  ) : IRequest<ServiceResponse<int>>;
}
