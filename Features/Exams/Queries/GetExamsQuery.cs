using MediatR;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Queries
{
    public record GetExamsQuery(
        int PageNumber = 1,
        int PageSize = 20,
        int? CategoryId = null
    ) : IRequest<ServiceResponse<PagedResult<UserExamDto>>>;
}