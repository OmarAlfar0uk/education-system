using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Enums;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Questions.Dtos;
using EduocationSystem.Features.Questions.Queries;
using EduocationSystem.Shared.Responses;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Questions.Handlers
{
    public class GetQuestionsForAdminQueryHandler : IRequestHandler<GetQuestionsForAdminQuery, ServiceResponse<PagedResult<AdminQuestionDto>>>
    {
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<Choice> _choiceRepository;
        private readonly IGenericRepository<Exam> _examRepository;

        public GetQuestionsForAdminQueryHandler(
            IGenericRepository<Question> questionRepository,
            IHttpContextAccessor httpContextAccessor,
            IGenericRepository<Choice> choiceRepository,
            IGenericRepository<Exam> examRepository)
        {
            _questionRepository = questionRepository;
            _httpContextAccessor = httpContextAccessor;
            _choiceRepository = choiceRepository;
            _examRepository = examRepository;
        }

        public async Task<ServiceResponse<PagedResult<AdminQuestionDto>>> Handle(GetQuestionsForAdminQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authenticated
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return ServiceResponse<PagedResult<AdminQuestionDto>>.UnauthorizedResponse(
                        "Authentication required",
                        "مطلوب مصادقة"
                    );
                }

                // Check if user is in Admin role
                if (!user.IsInRole("Admin"))
                {
                    return ServiceResponse<PagedResult<AdminQuestionDto>>.ForbiddenResponse(
                        "Access forbidden. Admin role required.",
                        "الوصول ممنوع. مطلوب دور المسؤول."
                    );
                }

                // Get exam titles for the join
                var exams = await _examRepository.GetAll()
                    .Select(e => new { e.Id, e.Title })
                    .ToDictionaryAsync(e => e.Id, e => e.Title, cancellationToken);

                // Base query for questions
                var questionQuery = _questionRepository.GetAll();

                // Apply search filter
                if (!string.IsNullOrEmpty(request.Parameters.Search))
                {
                    questionQuery = questionQuery.Where(q => q.Title.Contains(request.Parameters.Search));
                }

                // Apply exam filter
                if (request.Parameters.ExamId.HasValue)
                {
                    questionQuery = questionQuery.Where(q => q.ExamId == request.Parameters.ExamId.Value);
                }

                // Apply type filter
                if (!string.IsNullOrEmpty(request.Parameters.Type))
                {
                    if (Enum.TryParse<QuestionType>(request.Parameters.Type, true, out var questionType))
                    {
                        questionQuery = questionQuery.Where(q => q.Type == questionType);
                    }
                    else if (int.TryParse(request.Parameters.Type, out var typeId) &&
                             Enum.IsDefined(typeof(QuestionType), typeId))
                    {
                        questionQuery = questionQuery.Where(q => (int)q.Type == typeId);
                    }
                }

                // Apply sorting
                questionQuery = request.Parameters.SortBy?.ToLower() switch
                {
                    "title" => questionQuery.OrderBy(q => q.Title),
                    "titledesc" => questionQuery.OrderByDescending(q => q.Title),
                    "type" => questionQuery.OrderBy(q => q.Type),
                    "typedesc" => questionQuery.OrderByDescending(q => q.Type),
                    "creationdate" => questionQuery.OrderBy(q => q.CreatedAt),
                    "creationdatedesc" => questionQuery.OrderByDescending(q => q.CreatedAt),
                    _ => questionQuery.OrderBy(q => q.Id)
                };

                var totalCount = await questionQuery.CountAsync(cancellationToken);

                // Get paginated questions with exam title from dictionary
                var questions = await questionQuery
                    .Skip((request.Parameters.PageNumber - 1) * request.Parameters.PageSize)
                    .Take(request.Parameters.PageSize)
                    .Select(q => new AdminQuestionDto
                    {
                        Id = q.Id,
                        Title = q.Title,
                        Type = q.Type.ToString(),
                        ExamId = q.ExamId,
                        ExamTitle = exams.ContainsKey(q.ExamId) ? exams[q.ExamId] : "Unknown Exam",
                        CreatedAt = q.CreatedAt,
                        UpdatedAt = q.UpdatedAt,
                        IsDeleted = q.IsDeleted,
                        Choices = new List<AdminChoiceDto>()
                    })
                    .ToListAsync(cancellationToken);

                // If we have questions, get their choices
                if (questions.Any())
                {
                    var questionIds = questions.Select(q => q.Id).ToList();

                    // Get all choices for these questions
                    var choices = await _choiceRepository.GetAll()
                        .Where(c => questionIds.Contains(c.QuestionId))
                        .Select(c => new AdminChoiceDto
                        {
                            Id = c.Id,
                            Text = c.Text,
                            IsCorrect = c.IsCorrect,
                            QuestionId = c.QuestionId,
                            CreatedAt = c.CreatedAt,
                            UpdatedAt = c.UpdatedAt,
                            IsDeleted = c.IsDeleted
                        })
                        .ToListAsync(cancellationToken);

                    // Group choices by question ID for efficient lookup
                    var choicesByQuestionId = choices.GroupBy(c => c.QuestionId)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    // Assign choices to each question
                    foreach (var question in questions)
                    {
                        if (choicesByQuestionId.TryGetValue(question.Id, out var questionChoices))
                        {
                            question.Choices = questionChoices;
                        }
                    }
                }

                var pagedResult = new PagedResult<AdminQuestionDto>(questions, totalCount, request.Parameters.PageNumber, request.Parameters.PageSize);

                return ServiceResponse<PagedResult<AdminQuestionDto>>.SuccessResponse(
                    pagedResult,
                    "Questions retrieved successfully",
                    "تم استرجاع الأسئلة بنجاح"
                );
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResult<AdminQuestionDto>>.InternalServerErrorResponse(
                    "An error occurred while retrieving questions",
                    "حدث خطأ أثناء استرجاع الأسئلة"
                );
            }
        }
    }
}