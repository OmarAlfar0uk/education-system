using MediatR;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Queries
{
    public record GetExamsForAdminQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? Search = null,
        int? CategoryId = null,
        bool? IsActive = null,
        string? SortBy = null
    ) : IRequest<ServiceResponse<PagedResult<AdminExamDto>>>;
}