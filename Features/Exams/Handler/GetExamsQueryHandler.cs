using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Features.Exams.Queries;
using EduocationSystem.Shared.Responses;
using System.Security.Claims;

namespace EduocationSystem.Features.Exams.Handlers
{
    public class GetUserExamsQueryHandler : IRequestHandler<GetExamsQuery, ServiceResponse<PagedResult<UserExamDto>>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<Exam> _examRepository;

        public GetUserExamsQueryHandler(IGenericRepository<Exam> examRepository, IHttpContextAccessor httpContextAccessor)
        {
            _examRepository = examRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<PagedResult<UserExamDto>>> Handle(GetExamsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authenticated
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return ServiceResponse<PagedResult<UserExamDto>>.UnauthorizedResponse(
                        "Authentication required",
                        "مطلوب مصادقة"
                    );
                }

                var exams = _examRepository.GetAll()
                    .Where(exam => exam.IsActive &&
                           !exam.IsDeleted &&
                           exam.StartDate <= DateTime.UtcNow &&
                           exam.EndDate >= DateTime.UtcNow);

                // Filter by category if provided
                if (request.CategoryId.HasValue)
                {
                    exams = exams.Where(e => e.CategoryId == request.CategoryId.Value);
                }

                // Get total count before pagination
                var totalCount = exams.Count();

                // Apply pagination
                var paginatedExams = exams
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(e => new UserExamDto
                    {
                        Id = e.Id,
                        Title = e.Title,
                        IconUrl = e.IconUrl,
                        CategoryName = e.Category!.Title,
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        Duration = e.Duration,
                        Description = e.Description
                    })
                    .ToList();

                // Create paged result
                var pagedResult = new PagedResult<UserExamDto>(
                    paginatedExams,
                    totalCount,
                    request.PageNumber,
                    request.PageSize
                );

                return ServiceResponse<PagedResult<UserExamDto>>.SuccessResponse(
                    pagedResult,
                    "Exams retrieved successfully",
                    "تم استرجاع الامتحانات بنجاح"
                );
            }
            catch (Exception ex)
            {
                // Log the exception if you have logging configured
                return ServiceResponse<PagedResult<UserExamDto>>.InternalServerErrorResponse(
                    "An error occurred while retrieving exams",
                    "حدث خطأ أثناء استرجاع الامتحانات"
                );
            }
        }
    }
}