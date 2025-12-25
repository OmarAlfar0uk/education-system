using MediatR;
using Microsoft.EntityFrameworkCore;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Exams.Commands;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Shared.Responses;
using System.Security.Claims;

namespace EduocationSystem.Features.Exams.Handlers
{
    public class SubmitExamCommandHandler : IRequestHandler<SubmitExamCommand, ServiceResponse<ExamResultDto>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<Choice> _choiceRepository;
        private readonly IGenericRepository<UserAnswer> _userAnswerRepository;
        private readonly IGenericRepository<UserSelectedChoice> _userSelectedChoiceRepository;
        private readonly IGenericRepository<UserExamAttempt> _userExamAttemptRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SubmitExamCommandHandler(
            IHttpContextAccessor httpContextAccessor,
            IGenericRepository<Exam> examRepository,
            IGenericRepository<Question> questionRepository,
            IGenericRepository<Choice> choiceRepository,
            IGenericRepository<UserAnswer> userAnswerRepository,
            IGenericRepository<UserSelectedChoice> userSelectedChoiceRepository,
            IGenericRepository<UserExamAttempt> userExamAttemptRepository,
            IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _examRepository = examRepository;
            _questionRepository = questionRepository;
            _choiceRepository = choiceRepository;
            _userAnswerRepository = userAnswerRepository;
            _userSelectedChoiceRepository = userSelectedChoiceRepository;
            _userExamAttemptRepository = userExamAttemptRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse<ExamResultDto>> Handle(SubmitExamCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // ✅ تحقق من المستخدم
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return ServiceResponse<ExamResultDto>.UnauthorizedResponse(
                        "Authentication required",
                        "مطلوب مصادقة"
                    );
                }

                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                // ✅ جلب الامتحان
                var exam = await _examRepository.GetByIdAsync(request.ExamId);
                if (exam == null || exam.IsDeleted || !exam.IsActive)
                {
                    return ServiceResponse<ExamResultDto>.NotFoundResponse(
                        "Exam not found",
                        "الامتحان غير موجود"
                    );
                }

                // ✅ احضار المحاولة الجارية
                var attempt = await _userExamAttemptRepository.GetAll()
                    .Where(a => a.UserId == userId && a.ExamId == request.ExamId && a.FinishedAt == null)
                    .OrderByDescending(a => a.AttemptDate)
                    .FirstOrDefaultAsync();

                if (attempt == null)
                {
                    return ServiceResponse<ExamResultDto>.ErrorResponse(
                        "No active attempt found for this exam",
                        "لم يتم العثور على محاولة نشطة لهذا الامتحان",
                        400
                    );
                }

                // ✅ جلب الأسئلة الخاصة بالامتحان
                var questions = await _questionRepository.GetAll()
                    .Where(q => q.ExamId == request.ExamId && !q.IsDeleted)
                    .Include(q => q.Choices)
                    .ToListAsync();

                int correctAnswers = 0;
                var questionResults = new List<QuestionResultDto>();

                // ✅ معالجة إجابات المستخدم
                foreach (var answer in request.SubmitExamDto.Answers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (question == null) continue;

                    var userAnswer = new UserAnswer
                    {
                        AttemptId = attempt.Id,
                        QuestionId = question.Id
                    };
                    await _userAnswerRepository.AddAsync(userAnswer);
                    await _unitOfWork.SaveChangesAsync();

                    // حفظ اختيارات المستخدم
                    foreach (var choiceId in answer.SelectedOptionIds)
                    {
                        var selectedChoice = new UserSelectedChoice
                        {
                            UserAnswerId = userAnswer.Id,
                            ChoiceId = choiceId
                        };
                        await _userSelectedChoiceRepository.AddAsync(selectedChoice);
                    }
                    await _unitOfWork.SaveChangesAsync();

                    // التحقق من صحة الإجابة
                    var correctChoices = question.Choices
                        .Where(c => c.IsCorrect)
                        .Select(c => c.Id)
                        .OrderBy(id => id)
                        .ToList();

                    var isCorrect = correctChoices.SequenceEqual(answer.SelectedOptionIds.OrderBy(id => id));

                    if (isCorrect)
                        correctAnswers++;

                    questionResults.Add(new QuestionResultDto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Title,
                        IsCorrect = isCorrect,
                        PointsEarned = isCorrect ? 1 : 0, // ✅ كل سؤال بنقطة واحدة
                        CorrectAnswer = string.Join(", ", question.Choices.Where(c => c.IsCorrect).Select(c => c.Text)),
                        UserAnswer = string.Join(", ", question.Choices.Where(c => answer.SelectedOptionIds.Contains(c.Id)).Select(c => c.Text))
                    });
                }

                // ✅ تحديث المحاولة
                attempt.Score = correctAnswers; // ✅ عدد النقاط = عدد الإجابات الصحيحة
                attempt.TotalQuestions = questions.Count;
                attempt.FinishedAt = DateTime.UtcNow;

                _userExamAttemptRepository.Update(attempt);
                await _unitOfWork.SaveChangesAsync();

                // ✅ حساب النسبة المئوية والدرجة
                var percentage = questions.Count > 0 ? ((double)correctAnswers / questions.Count) * 100 : 0;
                var grade = CalculateGrade(percentage);

                // ✅ بناء النتيجة النهائية
                var result = new ExamResultDto
                {
                    ExamId = exam.Id,
                    ExamTitle = exam.Title,
                    TotalQuestions = questions.Count,
                    CorrectAnswers = correctAnswers,
                    Score = correctAnswers, // ✅ كل سؤال = نقطة واحدة
                    TotalPoints = questions.Count,
                    Percentage = percentage,
                    Grade = grade,
                    SubmittedAt = DateTime.UtcNow,
                    QuestionResults = questionResults
                };

                return ServiceResponse<ExamResultDto>.SuccessResponse(
                    result,
                    "Exam submitted successfully",
                    "تم تقديم الامتحان بنجاح"
                );
            }
            catch (Exception ex)
            {
                return ServiceResponse<ExamResultDto>.InternalServerErrorResponse(
                    $"Error while submitting exam: {ex.Message}",
                    "حدث خطأ أثناء تقديم الامتحان"
                );
            }
        }

        private static string CalculateGrade(double percentage)
        {
            if (percentage >= 90) return "A";
            if (percentage >= 80) return "B";
            if (percentage >= 70) return "C";
            if (percentage >= 60) return "D";
            return "F";
        }
    }
}
