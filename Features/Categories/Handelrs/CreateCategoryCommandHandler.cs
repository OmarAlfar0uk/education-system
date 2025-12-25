using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Categories.Commands;
using EduocationSystem.Features.Categories.Dtos;
using EduocationSystem.Shared.Responses;
using System.Security.Claims;
using System.Text;

namespace EduocationSystem.Features.Categories.Handlers
{
    public class CreateCategoryCommandHandler
     : IRequestHandler<CreateCategoryCommand, ServiceResponse<int>>
    {
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CreateCategoryCommandHandler(
            IGenericRepository<Category> categoryRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<int>> Handle(
            CreateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            // 🔒 Last line of defense
            if (!_currentUser.IsInRole("Admin"))
            {
                return ServiceResponse<int>.ForbiddenResponse(
                    "Admin access required",
                    "الوصول متاح للمسؤول فقط");
            }

            if (request?.CreateCategoryDTo == null)
            {
                return ServiceResponse<int>.ErrorResponse(
                    "Invalid request data",
                    "بيانات الطلب غير صالحة",
                    400);
            }

            if (string.IsNullOrWhiteSpace(request.CreateCategoryDTo.Title))
            {
                return ServiceResponse<int>.ErrorResponse(
                    "Title is required",
                    "العنوان مطلوب",
                    400);
            }

            if (request.CreateCategoryDTo.Icon == null ||
                request.CreateCategoryDTo.Icon.Length == 0)
            {
                return ServiceResponse<int>.ErrorResponse(
                    "Icon file is required",
                    "ملف الأيقونة مطلوب",
                    400);
            }

            if (request.CreateCategoryDTo.Icon.Length > 5 * 1024 * 1024)
            {
                return ServiceResponse<int>.ErrorResponse(
                    "Icon file size must be less than 5MB",
                    "يجب أن يكون حجم ملف الأيقونة أقل من 5 ميجابايت",
                    400);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg" };
            var fileExtension = Path.GetExtension(request.CreateCategoryDTo.Icon.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return ServiceResponse<int>.ErrorResponse(
                    "Invalid image format",
                    "صيغة الصورة غير مدعومة",
                    400);
            }

            var exists = await _categoryRepository
                .FirstOrDefaultAsync(c => c.Title == request.CreateCategoryDTo.Title);

            if (exists != null)
            {
                return ServiceResponse<int>.ConflictResponse(
                    "Category title must be unique",
                    "يجب أن يكون عنوان الفئة فريدًا");
            }

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{request.CreateCategoryDTo.Icon.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.CreateCategoryDTo.Icon.CopyToAsync(stream, cancellationToken);
            }

            var category = new Category
            {
                Title = request.CreateCategoryDTo.Title,
                IconUrl = "/uploads/" + fileName,
                CreatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResponse<int>.SuccessResponse(
                category.Id,
                "Category created successfully",
                "تم إنشاء الفئة بنجاح");
        }
    }

}