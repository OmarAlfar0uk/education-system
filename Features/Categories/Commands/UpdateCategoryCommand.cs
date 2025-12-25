using MediatR;
using EduocationSystem.Features.Categories.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Categories.Commands
{
    public record UpdateCategoryCommand(int Id, UpdateCategoryDTo UpdateCategoryDTo) : IRequest<ServiceResponse<int>>;
}