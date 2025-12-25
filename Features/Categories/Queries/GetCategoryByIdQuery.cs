using MediatR;
using EduocationSystem.Features.Categories.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Categories.Queries
{
    public record GetCategoryByIdQuery(int Id) : IRequest<ServiceResponse<CategoryDetailsDto>>;
}