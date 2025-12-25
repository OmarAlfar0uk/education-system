using MediatR;
using EduocationSystem.Features.Categories.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Categories.Queries
{
    public record GetUserCategoriesQuery(int PageNumber = 1, int PageSize = 20)
        : IRequest<ServiceResponse<PagedResult<UserCategoryDto>>>;
}