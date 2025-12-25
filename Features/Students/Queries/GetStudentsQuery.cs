using EduocationSystem.Features.Students.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Students.Queries
{

    public record GetStudentsQuery(PaginationParamsDto<StudentSortBy> Pagination)
        : IRequest<ServiceResponse<PagedResult<StudentDto>>>;

    public enum StudentSortBy
    {
        Name,
        Level
    }
}
