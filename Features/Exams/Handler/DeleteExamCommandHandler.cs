using MediatR;
using Microsoft.EntityFrameworkCore;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Exams.Commands;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Handlers
{
    public class DeleteExamCommandHandler : IRequestHandler<DeleteExamCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteExamCommandHandler(
            IGenericRepository<Exam> examRepository,
            IGenericRepository<Question> questionRepository,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _examRepository = examRepository;
            _questionRepository = questionRepository;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<bool>> Handle(DeleteExamCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;

                // 🔒 Authentication check
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return ServiceResponse<bool>.UnauthorizedResponse(
                        "Authentication required",
                        "يجب تسجيل الدخول أولاً."
                    );
                }

                // 🧑‍💼 Role check
                if (!user.IsInRole("Admin"))
                {
                    return ServiceResponse<bool>.ForbiddenResponse(
                        "Access forbidden. Admin role required.",
                        "الوصول مرفوض. هذه العملية تتطلب صلاحيات المسؤول."
                    );
                }

                // 🧾 Validate request
                if (request?.DeleteExamDto == null || request.DeleteExamDto.ExamId <= 0)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Invalid exam ID.",
                        "رقم الامتحان غير صالح.",
                        400
                    );
                }

                // 🔍 Find the exam
                var exam = await _examRepository
                    .GetAll()
                    .FirstOrDefaultAsync(e => e.Id == request.DeleteExamDto.ExamId, cancellationToken);

                if (exam == null)
                {
                    return ServiceResponse<bool>.NotFoundResponse(
                        "Exam not found.",
                        "الامتحان غير موجود."
                    );
                }

                // 📦 Check if already soft deleted
                if (exam.IsDeleted)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "The exam is already in the trash.",
                        "الامتحان موجود بالفعل في سلة المحذوفات.",
                        400
                    );
                }

                // 🧩 Get related questions
                var questions = await _questionRepository
                    .GetAll()
                    .Where(q => q.ExamId == exam.Id)
                    .ToListAsync(cancellationToken);

                // 🗑️ Soft delete all related questions
                foreach (var question in questions)
                {
                    question.IsDeleted = true;
                    _questionRepository.Update(question);
                }

                // 🗑️ Soft delete the exam
                exam.IsDeleted = true;
                exam.DeletedAt = DateTime.UtcNow; // useful if you’re tracking deletion time
                _examRepository.Update(exam);

                await _unitOfWork.SaveChangesAsync();

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Exam moved to trash successfully.",
                    "تم نقل الامتحان إلى سلة المحذوفات بنجاح."
                );
            }
            catch (Exception)
            {
                return ServiceResponse<bool>.InternalServerErrorResponse(
                    "An unexpected error occurred while deleting the exam.",
                    "حدث خطأ غير متوقع أثناء محاولة حذف الامتحان."
                );
            }
        }
    }
}
