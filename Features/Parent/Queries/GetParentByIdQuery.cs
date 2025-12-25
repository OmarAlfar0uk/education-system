using EduocationSystem.Features.Parent.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Parent.Queries
{

    public record GetParentByIdQuery(int Id)
        : IRequest<ServiceResponse<ParentDto>>;
}
