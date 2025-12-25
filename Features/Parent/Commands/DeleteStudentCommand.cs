using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Parent.Commands
{

    public record DeleteStudentCommand(int StudentId)
        : IRequest<ServiceResponse<bool>>;
}
