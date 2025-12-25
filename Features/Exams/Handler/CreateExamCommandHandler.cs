using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Exams.Commands;
using EduocationSystem.Shared.Responses;
using System.Security.Claims;

namespace EduocationSystem.Features.Exams.Handlers
{
    public class CreateExamCommandHandler : IRequestHandler<CreateExamCommand, ServiceResponse<int>>
    {
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateExamCommandHandler(
            IGenericRepository<Exam> examRepository,
            IGenericRepository<Category> categoryRepository,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _examRepository = examRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<int>> Handle(CreateExamCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;

                // 🔒 Check authentication
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return ServiceResponse<int>.UnauthorizedResponse(
                        "Authentication required",
                        "مطلوب مصادقة"
                    );
                }

                // 🧑‍💼 Check role
                if (!user.IsInRole("Admin"))
                {
                    return ServiceResponse<int>.ForbiddenResponse(
                        "Access forbidden. Admin role required.",
                        "الوصول ممنوع. مطلوب دور المسؤول."
                    );
                }

                // 🧾 Validate input DTO
                if (request?.CreateExamDto == null)
                {
                    return ServiceResponse<int>.ErrorResponse(
                        "Invalid request data",
                        "بيانات الطلب غير صالحة",
                        400
                    );
                }

                // ✅ Required field validations
                if (string.IsNullOrWhiteSpace(request.CreateExamDto.Title))
                {
                    return ServiceResponse<int>.ErrorResponse(
                        "Title is required",
                        "العنوان مطلوب",
                        400
                    );
                }

                if (request.CreateExamDto.CategoryId <= 0)
                {
                    return ServiceResponse<int>.ErrorResponse(
                        "CategoryId is required and must be valid",
                        "يجب إدخال رقم فئة صالح",
                        400
                    );
                }

                if (request.CreateExamDto.StartDate == default)
                {
                    return ServiceResponse<int>.ErrorResponse(
                        "Start date is required",
                        "تاريخ البداية مطلوب",
                        400
                    );
                }

                if (request.CreateExamDto.EndDate == default)
                {
                    return ServiceResponse<int>.ErrorResponse(
                        "End date is required",
                        "تاريخ النهاية مطلوب",
                        400
                    );
                }

                if (request.CreateExamDto.EndDate <= request.CreateExamDto.StartDate)
                {
                    return ServiceResponse<int>.ErrorResponse(
                        "End date must be after start date",
                        "تاريخ النهاية يجب أن يكون بعد تاريخ البداية",
                        400
                    );
                }

                if (request.CreateExamDto.Duration <= 0)
                {
                    return ServiceResponse<int>.ErrorResponse(
                        "Duration must be greater than 0 minutes",
                        "المدة يجب أن تكون أكبر من صفر",
                        400
                    );
                }

                // 🔍 Check if category exists
                var categoryExists = await _categoryRepository.GetByIdAsync(request.CreateExamDto.CategoryId);
                if (categoryExists == null)
                {
                    return ServiceResponse<int>.ErrorResponse(
                        "Category not found",
                        "الفئة غير موجودة",
                        404
                    );
                }

                // 🆕 Create exam
                var exam = new Exam
                {
                    Title = request.CreateExamDto.Title.Trim(),
                    IconUrl = request.CreateExamDto.IconUrl,
                    CategoryId = request.CreateExamDto.CategoryId,
                    StartDate = request.CreateExamDto.StartDate,
                    EndDate = request.CreateExamDto.EndDate,
                    Duration = request.CreateExamDto.Duration,
                    Description = request.CreateExamDto.Description?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                await _examRepository.AddAsync(exam);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResponse<int>.SuccessResponse(
                    exam.Id,
                    "Exam created successfully",
                    "تم إنشاء الامتحان بنجاح"
                );
            }
            catch (Exception ex)
            {
                // (يمكن إضافة تسجيل Log هنا)
                return ServiceResponse<int>.InternalServerErrorResponse(
                    "An error occurred while creating the exam",
                    "حدث خطأ أثناء إنشاء الامتحان"
                );
            }
        }
    }
}
