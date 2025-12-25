using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Categories.Commands;
using EduocationSystem.Features.Categories.Dtos;
using EduocationSystem.Shared.Responses;
using System.Security.Claims;

namespace EduocationSystem.Features.Categories.Handlers
{
    public class DeleteCategoryCommandHandler
    : IRequestHandler<DeleteCategoryCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public DeleteCategoryCommandHandler(
            IGenericRepository<Category> categoryRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<bool>> Handle(
            DeleteCategoryCommand request,
            CancellationToken cancellationToken)
        {
            // 🔒 Last line of defense
            if (!_currentUser.IsInRole("Admin"))
            {
                return ServiceResponse<bool>.ForbiddenResponse(
                    "Admin access required",
                    "الوصول متاح للمسؤول فقط");
            }

            var category = await _categoryRepository.GetByIdAsync(request.Id);

            if (category == null || category.IsDeleted)
            {
                return ServiceResponse<bool>.NotFoundResponse(
                    "Category not found",
                    "الفئة غير موجودة");
            }

            // Business rule ✔️
            if (category.Exams != null && category.Exams.Any())
            {
                return ServiceResponse<bool>.ConflictResponse(
                    "Category cannot be deleted because it has associated exams",
                    "لا يمكن حذف الفئة لأنها تحتوي على امتحانات مرتبطة");
            }

            _categoryRepository.Delete(category);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResponse<bool>.SuccessResponse(
                true,
                "Category deleted successfully",
                "تم حذف الفئة بنجاح");
        }
    }

}