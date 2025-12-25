using EduocationSystem.Features.Parent.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Parent.Commands
{

    public record CreateParentCommand(string UserId)
        : IRequest<ServiceResponse<ParentDto>>;
}
