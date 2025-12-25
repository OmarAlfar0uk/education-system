using EduocationSystem.Features.Parent.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Parent.Queries
{
    public record GetChildrenByParentQuery(int ParentId)
    : IRequest<ServiceResponse<List<ChildDto>>>;
}
