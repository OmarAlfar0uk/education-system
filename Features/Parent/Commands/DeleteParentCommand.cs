using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Parent.Commands
{

    public record DeleteParentCommand(int ParentId)
        : IRequest<ServiceResponse<bool>>;
}
