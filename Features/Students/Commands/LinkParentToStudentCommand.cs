using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Students.Commands
{

    public record LinkParentToStudentCommand(int ParentId, int StudentId)
        : IRequest<ServiceResponse<bool>>;
}
