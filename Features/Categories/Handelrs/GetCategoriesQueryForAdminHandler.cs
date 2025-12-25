using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Categories.Dtos;
using EduocationSystem.Features.Categories.Queries;
using EduocationSystem.Shared.Responses;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace EduocationSystem.Features.Categories.Handlers
{
    public class GetCategoriesQueryForAdminHandler : IRequestHandler<GetCategoriesQueryForAdmin, ServiceResponse<PagedResult<AdminCategoryDto>>>
    {
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetCategoriesQueryForAdminHandler(
            IGenericRepository<Category> categoryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _categoryRepository = categoryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<PagedResult<AdminCategoryDto>>> Handle(GetCategoriesQueryForAdmin request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authenticated
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return ServiceResponse<PagedResult<AdminCategoryDto>>.UnauthorizedResponse(
                        "Authentication required",
                        "مطلوب مصادقة"
                    );
                }
                // get all the roles of the user
                var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                // Check if user is in Admin role
                if (!user.IsInRole("Admin"))
                {
                    return ServiceResponse<PagedResult<AdminCategoryDto>>.ForbiddenResponse(
                        "Access forbidden. Admin role required.",
                        "الوصول ممنوع. مطلوب دور المسؤول."
                    );
                }

                var query = _categoryRepository.GetAll();

                // Apply search filter
                if (!string.IsNullOrEmpty(request.Search))
                {
                    query = query.Where(c => c.Title.Contains(request.Search));
                }

                // Apply sorting
                query = request.SortBy?.ToLower() switch
                {
                    "name" => query.OrderBy(c => c.Title),
                    "namedesc" => query.OrderByDescending(c => c.Title),
                    "creationdate" => query.OrderBy(c => c.CreatedAt),
                    "creationdatedesc" => query.OrderByDescending(c => c.CreatedAt),
                    _ => query.OrderBy(c => c.Id) // Default sorting
                };

                // Get total count before pagination
                var totalCount = query.Count();

                // Apply pagination
                var paginatedCategories = query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(c => new AdminCategoryDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        IconUrl = c.IconUrl,
                        CreationDate = c.CreatedAt
                    })
                    .ToList();

                // Create paged result
                var pagedResult = new PagedResult<AdminCategoryDto>(
                    paginatedCategories,
                    totalCount,
                    request.PageNumber,
                    request.PageSize
                );

                return ServiceResponse<PagedResult<AdminCategoryDto>>.SuccessResponse(
                    pagedResult,
                    "Categories retrieved successfully",
                    "تم استرجاع الفئات بنجاح"
                );
            }
            catch (Exception ex)
            {
                // Log the exception if you have logging configured
                return ServiceResponse<PagedResult<AdminCategoryDto>>.InternalServerErrorResponse(
                    "An error occurred while retrieving categories",
                    "حدث خطأ أثناء استرجاع الفئات"
                );
            }
        }
    }
}