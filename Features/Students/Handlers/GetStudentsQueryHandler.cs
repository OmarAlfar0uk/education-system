using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Students.Dtos;
using EduocationSystem.Features.Students.Queries;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Students.Handlers
{ 
public class GetStudentsQueryHandler
    : IRequestHandler<GetStudentsQuery, ServiceResponse<PagedResult<StudentDto>>>
{
    private readonly IGenericRepository<Student> _studentRepo;

    public GetStudentsQueryHandler(IGenericRepository<Student> studentRepo)
    {
        _studentRepo = studentRepo;
    }

    public async Task<ServiceResponse<PagedResult<StudentDto>>> Handle(
        GetStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var p = request.Pagination;

        var baseQuery = _studentRepo.GetAll()
            .Where(s => !EF.Property<bool>(s, "IsDeleted"));

        // 🔍 Search
        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            baseQuery = baseQuery.Where(s =>
                s.User.FirstName.Contains(p.Search) ||
                s.User.LastName.Contains(p.Search) ||
                s.User.Email!.Contains(p.Search));
        }

        // ↕ Sorting
        baseQuery = p.SortBy switch
        {
            StudentSortBy.Level =>
                p.SortDirection == SortDirection.Asc
                    ? baseQuery.OrderBy(s => s.Level)
                    : baseQuery.OrderByDescending(s => s.Level),

            _ =>
                p.SortDirection == SortDirection.Asc
                    ? baseQuery.OrderBy(s => s.User.FirstName)
                    : baseQuery.OrderByDescending(s => s.User.FirstName)
        };

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var students = await baseQuery
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .Select(s => new StudentDto
            {
                Id = s.Id,
                FullName = s.User.FirstName + " " + s.User.LastName,
                Email = s.User.Email!,
                Department = s.Department.Name,
                Level = s.Level
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<StudentDto>(
            students,
            totalCount,
            p.Page,
            p.PageSize
        );

        return ServiceResponse<PagedResult<StudentDto>>
            .SuccessResponse(result);
    }
}

}
