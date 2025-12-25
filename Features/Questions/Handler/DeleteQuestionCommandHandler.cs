using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Questions.Commands;
using EduocationSystem.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduocationSystem.Features.Questions.Handlers
{
    public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<Choice> _choiceRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteQuestionCommandHandler(
            IGenericRepository<Question> questionRepository,
            IGenericRepository<Choice> choiceRepository,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork)
        {
            _questionRepository = questionRepository;
            _choiceRepository = choiceRepository;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse<bool>> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
        {
            // Check if user is authenticated and is Admin
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true || !user.IsInRole("Admin"))
            {
                return ServiceResponse<bool>.ForbiddenResponse(
                    "Access forbidden. Admin role required.",
                    "الوصول ممنوع. مطلوب دور المسؤول."
                );
            }

            try
            {
                // Get the existing question with choices
                var existingQuestion = await _questionRepository.GetAll()
                    .Include(q => q.Choices.Where(c => !c.IsDeleted))
                    .FirstOrDefaultAsync(q => q.Id == request.QuestionId && !q.IsDeleted, cancellationToken);

                if (existingQuestion == null)
                {
                    return ServiceResponse<bool>.NotFoundResponse(
                        "Question not found or has been deleted",
                        "السؤال غير موجود أو تم حذفه"
                    );
                }

                // Soft delete all choices associated with the question
                foreach (var choice in existingQuestion.Choices)
                {
                    _choiceRepository.Delete(choice);
                }

                // Soft delete the question
                _questionRepository.Delete(existingQuestion);

                await _unitOfWork.SaveChangesAsync();

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Question and associated choices deleted successfully",
                    "تم حذف السؤال والاختيارات المرتبطة به بنجاح"
                );
            }
            catch (Exception ex)
            {
                // Log the actual error
                Console.WriteLine($"ERROR deleting question: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                }

                return ServiceResponse<bool>.InternalServerErrorResponse(
                    $"An error occurred while deleting the question: {ex.Message}",
                    $"حدث خطأ أثناء حذف السؤال: {ex.Message}"
                );
            }
        }
    }
}