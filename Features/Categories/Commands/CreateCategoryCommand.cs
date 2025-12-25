using MediatR;
using EduocationSystem.Features.Categories.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Categories.Commands
{
    public record CreateCategoryCommand(createCategoryDTo CreateCategoryDTo) : IRequest<ServiceResponse<int>>;
}