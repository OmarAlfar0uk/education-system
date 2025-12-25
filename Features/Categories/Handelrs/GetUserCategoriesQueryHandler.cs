using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Categories.Dtos;
using EduocationSystem.Features.Categories.Queries;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Categories.Handlers
{
    public class GetUserCategoriesQueryHandler : IRequestHandler<GetUserCategoriesQuery, ServiceResponse<PagedResult<UserCategoryDto>>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<Category> _categoryRepository;

        public GetUserCategoriesQueryHandler(IGenericRepository<Category> categoryRepository, IHttpContextAccessor httpContextAccessor)
        {
            _categoryRepository = categoryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<PagedResult<UserCategoryDto>>> Handle(GetUserCategoriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authenticated
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return ServiceResponse<PagedResult<UserCategoryDto>>.UnauthorizedResponse(
                        "Authentication required",
                        "مطلوب مصادقة"
                    );
                }
                var categories = _categoryRepository.GetAll().Where(category => category.IsDeleted==false);

                // Get total count before pagination
                var totalCount = categories.Count();

                // Apply pagination
                var paginatedCategories = categories
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(c => new UserCategoryDto
                    {
                        Title = c.Title,
                        IconUrl = c.IconUrl
                    })
                    .ToList();

                // Create paged result
                var pagedResult = new PagedResult<UserCategoryDto>(
                    paginatedCategories,
                    totalCount,
                    request.PageNumber,
                    request.PageSize
                );

                return ServiceResponse<PagedResult<UserCategoryDto>>.SuccessResponse(
                    pagedResult,
                    "Categories retrieved successfully",
                    "تم استرجاع الفئات بنجاح"
                );
            }
            catch (Exception ex)
            {
                // Log the exception if you have logging configured
                return ServiceResponse<PagedResult<UserCategoryDto>>.InternalServerErrorResponse(
                    "An error occurred while retrieving categories",
                    "حدث خطأ أثناء استرجاع الفئات"
                );
            }
        }
    }
}