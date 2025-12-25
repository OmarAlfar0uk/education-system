using MediatR;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Categories.Commands
{
    public record DeleteCategoryCommand(int Id) : IRequest<ServiceResponse<bool>>;
}