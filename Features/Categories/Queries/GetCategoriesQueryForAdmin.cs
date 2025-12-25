using MediatR;
using EduocationSystem.Features.Categories.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Categories.Queries
{
    public record GetCategoriesQueryForAdmin(
        int PageNumber = 1,
        int PageSize = 10,
        string? Search = null,
        string? SortBy = null
    ) : IRequest<ServiceResponse<PagedResult<AdminCategoryDto>>>;
}