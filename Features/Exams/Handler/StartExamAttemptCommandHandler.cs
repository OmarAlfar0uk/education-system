using MediatR;
using Microsoft.EntityFrameworkCore;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Exams.Commands;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Features.Questions.Dtos;
using EduocationSystem.Shared.Responses;
using System.Security.Claims;

namespace EduocationSystem.Features.Exams.Handlers
{
    public class StartExamAttemptCommandHandler : IRequestHandler<StartExamAttemptCommand, ServiceResponse<List<QuestionDto>>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<Choice> _choiceRepository;
        private readonly IGenericRepository<UserExamAttempt> _examAttemptRepository;
        private readonly IUnitOfWork _unitOfWork;

        public StartExamAttemptCommandHandler(
            IGenericRepository<Exam> examRepository,
            IGenericRepository<Question> questionRepository,
            IGenericRepository<Choice> choiceRepository ,
            IHttpContextAccessor httpContextAccessor,
            IGenericRepository<UserExamAttempt> examAttemptRepository,
            IUnitOfWork unitOfWork)
        {
            _examRepository = examRepository;
            _questionRepository = questionRepository;
            _httpContextAccessor = httpContextAccessor;
            _choiceRepository = choiceRepository;
            _examAttemptRepository = examAttemptRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse<List<QuestionDto>>> Handle(StartExamAttemptCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authenticated
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return ServiceResponse<List<QuestionDto>>.UnauthorizedResponse(
                        "Authentication required",
                        "مطلوب مصادقة"
                    );
                }

                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get exam
                var exam = await _examRepository.GetByIdAsync(request.ExamId);
                if (exam == null || exam.IsDeleted || !exam.IsActive)
                {
                    return ServiceResponse<List<QuestionDto>>.NotFoundResponse(
                        "Exam not found",
                        "الامتحان غير موجود"
                    );
                }

                // Check if exam is available
                if (DateTime.UtcNow < exam.StartDate || DateTime.UtcNow > exam.EndDate)
                {
                    return ServiceResponse<List<QuestionDto>>.ErrorResponse(
                        "Exam is not available at this time",
                        "الامتحان غير متاح في هذا الوقت",
                        400
                    );
                }

                // Check if user has already attempted this exam (if you track attempts)
                // var existingAttempt = await _examAttemptRepository.GetUserAttempt(userId, request.ExamId);
                // if (existingAttempt != null && existingAttempt.IsCompleted)
                // {
                //     return ServiceResponse<List<QuestionDto>>.ErrorResponse(
                //         "You have already completed this exam",
                //         "لقد أكملت هذا الامتحان مسبقًا",
                //         400
                //     );
                // }

                // Get questions for the exam
                var questions = _questionRepository.GetAll()
                    .Where(q => q.ExamId == request.ExamId && !q.IsDeleted)
                    .OrderBy(q => q.CreatedAt)
                    .Select(q => new QuestionDto
                    {
                        Id = q.Id,
                        Title = q.Title,
                        Type = q.Type.ToString(),
                        Choices = _choiceRepository.GetAll()
                            .Where(c => c.QuestionId == q.Id && !c.IsDeleted)
                            .Select(c => new ChoiceDto
                            {
                                Id = c.Id,
                                Text = c.Text
                            })
                            .ToList()
                    })
                    .ToList();
                // check if the user take the exam before
                var previousAttempts = await _examAttemptRepository.GetAll().Where(a => a.UserId == userId && a.ExamId == request.ExamId)
                    .ToListAsync();
                //add in the userexamattempt table the start time and the user id and the exam id when the user start the exam
                var ongoingAttempt = previousAttempts.FirstOrDefault(a => a.FinishedAt == null);

                if (ongoingAttempt != null)
                {
                    var endTime = ongoingAttempt.AttemptDate.AddMinutes(exam.Duration);
                    var timeRemaining = endTime - DateTime.UtcNow;

                    if (timeRemaining.TotalMinutes > 0)
                    {
                        return ServiceResponse<List<QuestionDto>>.SuccessResponse(
                            questions,
                            $"You have an ongoing attempt for this exam and your remaining time is {Math.Ceiling(timeRemaining.TotalMinutes)} minutes.",
                            $"لديك محاولة جارية لهذا الامتحان و الوقت المتبقي {Math.Ceiling(timeRemaining.TotalMinutes)} دقيقه"
                        );
                    }
                }
                var attemptNumber = previousAttempts.Count + 1;

                var userExamAttempt = new UserExamAttempt
                {
                    UserId = userId,
                    ExamId = request.ExamId,
                    AttemptDate = DateTime.UtcNow,
                    AttemptNumber = attemptNumber,
                    Score = 0,
                    TotalQuestions = questions.Count,
                    IsHighestScore = false,
                    FinishedAt = null
                };
                try
                {
                    await _examAttemptRepository.AddAsync(userExamAttempt);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log the exception (ex) as needed
                    return ServiceResponse<List<QuestionDto>>.InternalServerErrorResponse(
                        "An error occurred while recording the exam attempt",
                        "حدث خطأ أثناء تسجيل محاولة الامتحان"
                    );
                }

                if (!questions.Any())
                {
                    return ServiceResponse<List<QuestionDto>>.ErrorResponse(
                        "No questions available for this exam",
                        "لا توجد أسئلة متاحة لهذا الامتحان",
                        400
                    );
                }

                // Create exam attempt record (if you're tracking attempts)
                // var attempt = new ExamAttempt
                // {
                //     UserId = userId,
                //     ExamId = request.ExamId,
                //     StartTime = DateTime.UtcNow,
                //     EndTime = DateTime.UtcNow.AddMinutes(exam.Duration)
                // };
                // await _examAttemptRepository.AddAsync(attempt);
                // await _unitOfWork.SaveChangesAsync();



                return ServiceResponse<List<QuestionDto>>.SuccessResponse(
                    questions,
                    "Exam started successfully",
                    "تم بدء الامتحان بنجاح"
                );
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<QuestionDto>>.InternalServerErrorResponse(
                    "An error occurred while starting the exam",
                    "حدث خطأ أثناء بدء الامتحان"
                );
            }
        }
    }
}