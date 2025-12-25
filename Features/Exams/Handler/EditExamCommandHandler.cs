using MediatR;
using Microsoft.EntityFrameworkCore;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Exams.Commands;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Handlers
{
    public class EditExamCommandHandler : IRequestHandler<EditExamCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EditExamCommandHandler(
            IGenericRepository<Exam> examRepository,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _examRepository = examRepository;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<bool>> Handle(EditExamCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;

                // 🔒 Authentication check
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return ServiceResponse<bool>.UnauthorizedResponse(
                        "Authentication required",
                        "مطلوب مصادقة"
                    );
                }

                // 🧑‍💼 Role check
                if (!user.IsInRole("Admin"))
                {
                    return ServiceResponse<bool>.ForbiddenResponse(
                        "Access forbidden. Admin role required.",
                        "الوصول ممنوع. مطلوب دور المسؤول."
                    );
                }

                var dto = request.EditExamDto;
                if (dto == null || dto.ExamId <= 0)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Invalid request data",
                        "بيانات الطلب غير صالحة",
                        400
                    );
                }

                // 🔍 Find the exam
                var exam = await _examRepository
                    .GetAll()
                    .FirstOrDefaultAsync(e => e.Id == dto.ExamId && !EF.Property<bool>(e, "IsDeleted"), cancellationToken);

                if (exam == null)
                {
                    return ServiceResponse<bool>.NotFoundResponse(
                        "Exam not found",
                        "الامتحان غير موجود"
                    );
                }

                // 🧾 Check if at least one field is changed
                bool isModified = false;

                if (!string.IsNullOrWhiteSpace(dto.Title) && dto.Title != exam.Title)
                {
                    exam.Title = dto.Title;
                    isModified = true;
                }

                if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description != exam.Description)
                {
                    exam.Description = dto.Description;
                    isModified = true;
                }

                if (dto.CategoryId > 0 && dto.CategoryId != exam.CategoryId)
                {
                    exam.CategoryId = dto.CategoryId;
                    isModified = true;
                }

                if (dto.StartDate != default && dto.StartDate != exam.StartDate)
                {
                    exam.StartDate = dto.StartDate;
                    isModified = true;
                }

                if (dto.EndDate != default && dto.EndDate != exam.EndDate)
                {
                    if (dto.EndDate <= exam.StartDate)
                    {
                        return ServiceResponse<bool>.ErrorResponse(
                            "End date must be after start date",
                            "يجب أن يكون تاريخ الانتهاء بعد تاريخ البدء",
                            400
                        );
                    }
                    exam.EndDate = dto.EndDate;
                    isModified = true;
                }

                if (dto.Duration > 0 && dto.Duration != exam.Duration)
                {
                    exam.Duration = dto.Duration;
                    isModified = true;
                }

                if (!string.IsNullOrWhiteSpace(dto.IconUrl) && dto.IconUrl != exam.IconUrl)
                {
                    exam.IconUrl = dto.IconUrl;
                    isModified = true;
                }

                // ❌ No changes were made
                if (!isModified)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "At least one field must be changed",
                        "يجب تعديل حقل واحد على الأقل",
                        400
                    );
                }

                // ✅ Save changes
                exam.UpdatedAt = DateTime.UtcNow;
                _examRepository.Update(exam);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Exam updated successfully",
                    "تم تحديث الامتحان بنجاح"
                );
            }
            catch (Exception)
            {
                return ServiceResponse<bool>.InternalServerErrorResponse(
                    "An error occurred while updating the exam",
                    "حدث خطأ أثناء تحديث الامتحان"
                );
            }
        }
    }
}
