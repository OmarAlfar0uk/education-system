using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Dashboard.Dtos;
using EduocationSystem.Features.Dashboard.Queries;
using EduocationSystem.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduocationSystem.Features.Dashboard.Handlers
{
    public class GetMostActiveCategoriesQueryHandler
      : IRequestHandler<GetMostActiveCategoriesQuery, ServiceResponse<MostActiveCategoriesDto>>
    {
        private readonly IGenericRepository<UserExamAttempt> _userExamAttemptRepository;
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly ICurrentUserService _currentUser;

        public GetMostActiveCategoriesQueryHandler(
            IGenericRepository<UserExamAttempt> userExamAttemptRepository,
            IGenericRepository<Exam> examRepository,
            IGenericRepository<Category> categoryRepository,
            ICurrentUserService currentUser)
        {
            _userExamAttemptRepository = userExamAttemptRepository;
            _examRepository = examRepository;
            _categoryRepository = categoryRepository;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<MostActiveCategoriesDto>> Handle(
            GetMostActiveCategoriesQuery request,
            CancellationToken cancellationToken)
        {
            // 🔒 Last line of defense
            if (!_currentUser.IsInRole("Admin"))
            {
                return ServiceResponse<MostActiveCategoriesDto>.ForbiddenResponse(
                    "Admin access required",
                    "الوصول متاح للمسؤول فقط");
            }

            // Exams
            var exams = await _examRepository.GetAll()
                .Where(e => !e.IsDeleted)
                .Select(e => new { e.Id, e.CategoryId })
                .ToListAsync(cancellationToken);

            // Attempts
            var attempts = await _userExamAttemptRepository.GetAll()
                .Where(a => !a.IsDeleted)
                .Select(a => new
                {
                    a.ExamId,
                    a.UserId,
                    a.Score,
                    a.TotalQuestions,
                    a.FinishedAt
                })
                .ToListAsync(cancellationToken);

            // Categories
            var categories = await _categoryRepository.GetAll()
                .Where(c => !c.IsDeleted)
                .Select(c => new { c.Id, c.Title })
                .ToListAsync(cancellationToken);

            var examsByCategory = exams
                .GroupBy(e => e.CategoryId)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Id).ToList());

            var categoryActivities = categories
                .Select(category =>
                {
                    var categoryExamIds = examsByCategory.GetValueOrDefault(category.Id, new List<int>());
                    var categoryAttempts = attempts
                        .Where(a => categoryExamIds.Contains(a.ExamId))
                        .ToList();

                    var completedAttempts = categoryAttempts
                        .Where(a => a.FinishedAt.HasValue && a.TotalQuestions > 0)
                        .ToList();

                    var averageScore = completedAttempts.Any()
                        ? completedAttempts.Average(a => (decimal)a.Score / a.TotalQuestions * 100)
                        : 0;

                    return new CategoryActivityDto
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Title,
                        ExamCount = categoryExamIds.Count,
                        TotalAttempts = categoryAttempts.Count,
                        TotalParticipants = categoryAttempts
                            .Select(a => a.UserId)
                            .Distinct()
                            .Count(),
                        AverageScore = Math.Round(averageScore, 2)
                    };
                })
                .OrderByDescending(c => c.TotalAttempts)
                .Take(10)
                .ToList();

            var result = new MostActiveCategoriesDto
            {
                Categories = categoryActivities
            };

            return ServiceResponse<MostActiveCategoriesDto>.SuccessResponse(
                result,
                "Most active categories data retrieved successfully",
                "تم استرجاع بيانات أكثر التصنيفات نشاطًا بنجاح");
        }

    }
}