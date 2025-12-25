using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Categories.Dtos;
using EduocationSystem.Features.Categories.Queries;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Categories.Handlers
{
    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, ServiceResponse<CategoryDetailsDto>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<Category> _categoryRepository;

        public GetCategoryByIdQueryHandler(IGenericRepository<Category> categoryRepository ,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _categoryRepository = categoryRepository;
        }

        public async Task<ServiceResponse<CategoryDetailsDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            // Check if user is authenticated
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return ServiceResponse<CategoryDetailsDto>.UnauthorizedResponse(
                    "Authentication required",
                    "مطلوب مصادقة"
                );
            }
            try
            {
                var category = await _categoryRepository.GetByIdAsync(request.Id);
                if (category == null || category.IsDeleted)
                {
                    return ServiceResponse<CategoryDetailsDto>.NotFoundResponse(
                        "Category not found",
                        "الفئة غير موجودة"
                    );
                }

                var categoryDto = new CategoryDetailsDto
                {
                    Id = category.Id,
                    Title = category.Title,
                    IconUrl = category.IconUrl,
                    CreationDate = category.CreatedAt
                };

                return ServiceResponse<CategoryDetailsDto>.SuccessResponse(
                    categoryDto,
                    "Category retrieved successfully",
                    "تم استرجاع الفئة بنجاح"
                );
            }
            catch (Exception ex)
            {
                return ServiceResponse<CategoryDetailsDto>.InternalServerErrorResponse(
                    "An error occurred while retrieving category",
                    "حدث خطأ أثناء استرجاع الفئة"
                );
            }
        }
    }
}