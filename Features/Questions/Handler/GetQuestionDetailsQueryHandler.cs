using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Questions.Dtos;
using EduocationSystem.Features.Questions.Queries;
using EduocationSystem.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduocationSystem.Features.Questions.Handlers
{
    public class GetQuestionDetailsQueryHandler : IRequestHandler<GetQuestionDetailsQuery, ServiceResponse<QuestiondetailsDto>>
    {
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetQuestionDetailsQueryHandler(
            IGenericRepository<Question> questionRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _questionRepository = questionRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<QuestiondetailsDto>> Handle(GetQuestionDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authenticated and is Admin
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true || !user.IsInRole("Admin"))
                {
                    return ServiceResponse<QuestiondetailsDto>.ForbiddenResponse(
                        "Access forbidden. Admin role required.",
                        "الوصول ممنوع. مطلوب دور المسؤول."
                    );
                }

                var question = await _questionRepository.GetAll()
                    .Include(q => q.Choices.Where(c => !c.IsDeleted))
                    .Include(q => q.Exam)
                    .Where(q => q.Id == request.QuestionId && !q.IsDeleted)
                    .Select(q => new QuestiondetailsDto
                    {
                        Id = q.Id,
                        Title = q.Title,
                        Type = q.Type.ToString(),
                        ExamId = q.ExamId,
                        CreatedAt = q.CreatedAt,
                        UpdatedAt = q.UpdatedAt,
                        Choices = q.Choices.Select(c => new ChoicedetailsDto
                        {
                            Id = c.Id,
                            Text = c.Text,
                            IsCorrect = c.IsCorrect,
                            QuestionId = c.QuestionId,
                            CreatedAt = c.CreatedAt,
                            UpdatedAt = c.UpdatedAt
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (question == null)
                {
                    return ServiceResponse<QuestiondetailsDto>.NotFoundResponse(
                        "Question not found or has been deleted",
                        "السؤال غير موجود أو تم حذفه"
                    );
                }

                return ServiceResponse<QuestiondetailsDto>.SuccessResponse(
                    question,
                    "Question details retrieved successfully",
                    "تم استرجاع تفاصيل السؤال بنجاح"
                );
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"ERROR retrieving question details: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");

                return ServiceResponse<QuestiondetailsDto>.InternalServerErrorResponse(
                    "An error occurred while retrieving question details",
                    "حدث خطأ أثناء استرجاع تفاصيل السؤال"
                );
            }
        }
    }
}