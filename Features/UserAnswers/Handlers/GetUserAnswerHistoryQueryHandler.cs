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
    public class GetUserAnswerHistoryQueryHandler : IRequestHandler<GetUserAnswerHistoryQuery, ServiceResponse<PagedUserAnswerHistoryDto>>
    {
        private readonly IGenericRepository<UserExamAttempt> _userExamAttemptRepository;
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetUserAnswerHistoryQueryHandler(
            IGenericRepository<UserExamAttempt> userExamAttemptRepository,
            IGenericRepository<Exam> examRepository,
            IGenericRepository<Category> categoryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _userExamAttemptRepository = userExamAttemptRepository;
            _examRepository = examRepository;
            _categoryRepository = categoryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<PagedUserAnswerHistoryDto>> Handle(GetUserAnswerHistoryQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get current user ID
                var user = _httpContextAccessor.HttpContext?.User;
                var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return ServiceResponse<PagedUserAnswerHistoryDto>.UnauthorizedResponse(
                        "User authentication required",
                        "مطلوب مصادقة المستخدم"
                    );
                }

                // Get user exam attempts
                var attemptsQuery = _userExamAttemptRepository.GetAll()
                    .Where(ua => ua.UserId == userIdClaim && !ua.IsDeleted);

                // Apply sorting
                attemptsQuery = request.SortBy?.ToLower() switch
                {
                    "date" => attemptsQuery.OrderBy(ua => ua.AttemptDate),
                    "datedesc" => attemptsQuery.OrderByDescending(ua => ua.AttemptDate),
                    "score" => attemptsQuery.OrderBy(ua => ua.Score),
                    "scoredesc" => attemptsQuery.OrderByDescending(ua => ua.Score),
                    _ => attemptsQuery.OrderByDescending(ua => ua.AttemptDate) // Default: newest first
                };

                var totalCount = await attemptsQuery.CountAsync(cancellationToken);

                var userAttempts = await attemptsQuery
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                if (!userAttempts.Any())
                {
                    var emptyResult = new PagedUserAnswerHistoryDto
                    {
                        Attempts = new List<UserAnswerHistoryDto>(),
                        TotalCount = 0,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                        TotalPages = 0
                    };

                    return ServiceResponse<PagedUserAnswerHistoryDto>.SuccessResponse(
                        emptyResult,
                        "No exam attempts found",
                        "لم يتم العثور على محاولات امتحان"
                    );
                }

                // Get exam IDs and category IDs
                var examIds = userAttempts.Select(ua => ua.ExamId).Distinct().ToList();
                var exams = await _examRepository.GetAll()
                    .Where(e => examIds.Contains(e.Id) && !e.IsDeleted)
                    .Select(e => new { e.Id, e.Title, e.CategoryId })
                    .ToListAsync(cancellationToken);

                var categoryIds = exams.Select(e => e.CategoryId).Distinct().ToList();
                var categories = await _categoryRepository.GetAll()
                    .Where(c => categoryIds.Contains(c.Id) && !c.IsDeleted)
                    .Select(c => new { c.Id, c.Title })
                    .ToListAsync(cancellationToken);

                // Create a lookup for exams and categories
                var examLookup = exams.ToDictionary(e => e.Id);
                var categoryLookup = categories.ToDictionary(c => c.Id);

                // Map to DTOs
                var attempts = userAttempts.Select(ua =>
                {
                    var exam = examLookup.GetValueOrDefault(ua.ExamId);
                    var category = exam != null ? categoryLookup.GetValueOrDefault(exam.CategoryId) : null;

                    return new UserAnswerHistoryDto
                    {
                        AttemptId = ua.Id,
                        ExamTitle = exam?.Title ?? "Unknown Exam",
                        CategoryName = category?.Title ?? "Unknown Category",
                        AttemptDate = ua.AttemptDate,
                        FinishedAt = ua.FinishedAt,
                        Score = ua.Score,
                        TotalQuestions = ua.TotalQuestions,
                        AttemptNumber = ua.AttemptNumber,
                        IsHighestScore = ua.IsHighestScore
                    };
                }).ToList();

                // Apply exam title sorting if requested
                if (request.SortBy?.ToLower() == "exam")
                {
                    attempts = attempts.OrderBy(a => a.ExamTitle).ToList();
                }
                else if (request.SortBy?.ToLower() == "examdesc")
                {
                    attempts = attempts.OrderByDescending(a => a.ExamTitle).ToList();
                }

                var pagedResult = new PagedUserAnswerHistoryDto
                {
                    Attempts = attempts,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
                };

                return ServiceResponse<PagedUserAnswerHistoryDto>.SuccessResponse(
                    pagedResult,
                    "User answer history retrieved successfully",
                    "تم استرجاع تاريخ الإجابات بنجاح"
                );
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"ERROR retrieving user answer history: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");

                return ServiceResponse<PagedUserAnswerHistoryDto>.InternalServerErrorResponse(
                    "An error occurred while retrieving user answer history",
                    "حدث خطأ أثناء استرجاع تاريخ الإجابات"
                );
            }
        }
    }
}