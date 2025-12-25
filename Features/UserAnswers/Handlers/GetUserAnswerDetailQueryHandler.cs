using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.UserAnswers.Dtos;
using EduocationSystem.Features.UserAnswers.Queries;
using EduocationSystem.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduocationSystem.Features.UserAnswers.Handlers
{
    public class GetUserAnswerDetailQueryHandler : IRequestHandler<GetUserAnswerDetailQuery, ServiceResponse<UserAnswerDetailDto>>
    {
        private readonly IGenericRepository<UserExamAttempt> _userExamAttemptRepository;
        private readonly IGenericRepository<UserAnswer> _userAnswerRepository;
        private readonly IGenericRepository<UserSelectedChoice> _userSelectedChoiceRepository;
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IGenericRepository<Choice> _choiceRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetUserAnswerDetailQueryHandler(
            IGenericRepository<UserExamAttempt> userExamAttemptRepository,
            IGenericRepository<UserAnswer> userAnswerRepository,
            IGenericRepository<UserSelectedChoice> userSelectedChoiceRepository,
            IGenericRepository<Exam> examRepository,
            IGenericRepository<Category> categoryRepository,
            IGenericRepository<Choice> choiceRepository,
            IGenericRepository<Question> questionRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _userExamAttemptRepository = userExamAttemptRepository;
            _userAnswerRepository = userAnswerRepository;
            _userSelectedChoiceRepository = userSelectedChoiceRepository;
            _examRepository = examRepository;
            _categoryRepository = categoryRepository;
            _choiceRepository = choiceRepository;
            _questionRepository = questionRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<UserAnswerDetailDto>> Handle(GetUserAnswerDetailQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get current user ID
                var user = _httpContextAccessor.HttpContext?.User;
                var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return ServiceResponse<UserAnswerDetailDto>.UnauthorizedResponse(
                        "User authentication required",
                        "مطلوب مصادقة المستخدم"
                    );
                }

                // Get the user exam attempt
                var userAttempt = await _userExamAttemptRepository.GetAll()
                    .FirstOrDefaultAsync(ua => ua.Id == request.AttemptId &&
                                              ua.UserId == userIdClaim &&
                                              !ua.IsDeleted, cancellationToken);

                if (userAttempt == null)
                {
                    return ServiceResponse<UserAnswerDetailDto>.NotFoundResponse(
                        "Attempt not found or access denied",
                        "المحاولة غير موجودة أو الوصول مرفوض"
                    );
                }

                // Get exam and category details
                var exam = await _examRepository.GetAll()
                    .FirstOrDefaultAsync(e => e.Id == userAttempt.ExamId && !e.IsDeleted, cancellationToken);

                if (exam == null)
                {
                    return ServiceResponse<UserAnswerDetailDto>.NotFoundResponse(
                        "Exam not found",
                        "الامتحان غير موجود"
                    );
                }

                var category = await _categoryRepository.GetAll()
                    .FirstOrDefaultAsync(c => c.Id == exam.CategoryId && !c.IsDeleted, cancellationToken);

                // Get all user answers for this attempt
                var userAnswers = await _userAnswerRepository.GetAll()
                    .Where(ua => ua.AttemptId == request.AttemptId && !ua.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (!userAnswers.Any())
                {
                    // Return basic info without question details
                    var basicResult = new UserAnswerDetailDto
                    {
                        AttemptId = userAttempt.Id,
                        ExamTitle = exam.Title,
                        CategoryName = category?.Title ?? "Unknown Category",
                        AttemptDate = userAttempt.AttemptDate,
                        FinishedAt = userAttempt.FinishedAt,
                        Score = userAttempt.Score,
                        TotalQuestions = userAttempt.TotalQuestions,
                        AttemptNumber = userAttempt.AttemptNumber,
                        IsHighestScore = userAttempt.IsHighestScore,
                        QuestionAnswers = new List<UserQuestionAnswerDto>()
                    };

                    return ServiceResponse<UserAnswerDetailDto>.SuccessResponse(
                        basicResult,
                        "User answer details retrieved successfully",
                        "تم استرجاع تفاصيل الإجابة بنجاح"
                    );
                }

                // Get all selected choices for this attempt
                var userAnswerIds = userAnswers.Select(ua => ua.Id).ToList();
                var selectedChoices = await _userSelectedChoiceRepository.GetAll()
                    .Where(usc => userAnswerIds.Contains(usc.UserAnswerId) && !usc.IsDeleted)
                    .ToListAsync(cancellationToken);

                // Get question details
                var questionIds = userAnswers.Select(ua => ua.QuestionId).Distinct().ToList();
                var questions = await _questionRepository.GetAll()
                    .Where(q => questionIds.Contains(q.Id) && !q.IsDeleted)
                    .Select(q => new { q.Id, q.Title, q.Type })
                    .ToListAsync(cancellationToken);

                // Get choice details for selected choices
                var choiceIds = selectedChoices.Select(usc => usc.ChoiceId).Distinct().ToList();
                var choices = await _choiceRepository.GetAll()
                    .Where(c => choiceIds.Contains(c.Id) && !c.IsDeleted)
                    .Select(c => new { c.Id, c.Text, c.IsCorrect, c.QuestionId })
                    .ToListAsync(cancellationToken);

                // Get all correct choices for the questions - using strongly typed approach
                var correctChoicesData = await _choiceRepository.GetAll()
                    .Where(c => questionIds.Contains(c.QuestionId) && c.IsCorrect && !c.IsDeleted)
                    .Select(c => new { c.Id, c.Text, c.QuestionId })
                    .ToListAsync(cancellationToken);

                // Create lookups
                var questionLookup = questions.ToDictionary(q => q.Id);
                var choiceLookup = choices.ToDictionary(c => c.Id);

                // Create correct choices lookup with strongly typed values
                var correctChoicesByQuestion = correctChoicesData
                    .GroupBy(c => c.QuestionId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(c => new CorrectChoiceDto
                        {
                            ChoiceId = c.Id,
                            ChoiceText = c.Text
                        }).ToList()
                    );

                // Group selected choices by user answer (which corresponds to a question)
                var selectedChoicesByUserAnswer = selectedChoices.GroupBy(usc => usc.UserAnswerId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Build question answers
                var questionAnswers = new List<UserQuestionAnswerDto>();

                foreach (var userAnswer in userAnswers)
                {
                    var question = questionLookup.GetValueOrDefault(userAnswer.QuestionId);
                    var selectedChoicesForAnswer = selectedChoicesByUserAnswer.GetValueOrDefault(userAnswer.Id, new List<UserSelectedChoice>());

                    var selectedChoiceDetails = selectedChoicesForAnswer
                        .Where(usc => choiceLookup.ContainsKey(usc.ChoiceId))
                        .Select(usc =>
                        {
                            var choice = choiceLookup[usc.ChoiceId];
                            return new UserSelectedChoiceDto
                            {
                                ChoiceId = choice.Id,
                                ChoiceText = choice.Text,
                                IsCorrect = choice.IsCorrect
                            };
                        }).ToList();

                    // Get correct choices for this question - using strongly typed approach
                    var correctChoicesForQuestion = correctChoicesByQuestion.ContainsKey(userAnswer.QuestionId)
                        ? correctChoicesByQuestion[userAnswer.QuestionId]
                        : new List<CorrectChoiceDto>();

                    // Calculate if the answer is correct
                    // For multiple choice: exactly one correct selected and it matches
                    // For multiple select: all selected are correct and all correct are selected
                    var isCorrect = false;
                    if (question != null)
                    {
                        var questionType = question.Type.ToString();
                        if (questionType == "MultipleChoice")
                        {
                            isCorrect = selectedChoiceDetails.Count == 1 &&
                                       selectedChoiceDetails[0].IsCorrect;
                        }
                        else if (questionType == "multipleSelect")
                        {
                            isCorrect = selectedChoiceDetails.All(sc => sc.IsCorrect) &&
                                       selectedChoiceDetails.Count == correctChoicesForQuestion.Count;
                        }
                    }

                    questionAnswers.Add(new UserQuestionAnswerDto
                    {
                        QuestionId = userAnswer.QuestionId,
                        QuestionTitle = question?.Title ?? "Unknown Question",
                        QuestionType = question?.Type.ToString() ?? "Unknown",
                        SelectedChoices = selectedChoiceDetails,
                        CorrectChoices = correctChoicesForQuestion,
                        IsCorrect = isCorrect
                    });
                }

                var result = new UserAnswerDetailDto
                {
                    AttemptId = userAttempt.Id,
                    ExamTitle = exam.Title,
                    CategoryName = category?.Title ?? "Unknown Category",
                    AttemptDate = userAttempt.AttemptDate,
                    FinishedAt = userAttempt.FinishedAt,
                    Score = userAttempt.Score,
                    TotalQuestions = userAttempt.TotalQuestions,
                    AttemptNumber = userAttempt.AttemptNumber,
                    IsHighestScore = userAttempt.IsHighestScore,
                    QuestionAnswers = questionAnswers
                };

                return ServiceResponse<UserAnswerDetailDto>.SuccessResponse(
                    result,
                    "User answer details retrieved successfully",
                    "تم استرجاع تفاصيل الإجابة بنجاح"
                );
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"ERROR retrieving user answer details: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");

                return ServiceResponse<UserAnswerDetailDto>.InternalServerErrorResponse(
                    "An error occurred while retrieving user answer details",
                    "حدث خطأ أثناء استرجاع تفاصيل الإجابة"
                );
            }
        }
    }
}