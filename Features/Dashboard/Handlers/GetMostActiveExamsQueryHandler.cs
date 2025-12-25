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
    public class GetMostActiveExamsQueryHandler
    : IRequestHandler<GetMostActiveExamsQuery, ServiceResponse<MostActiveExamsDto>>
    {
        private readonly IGenericRepository<UserExamAttempt> _userExamAttemptRepository;
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly ICurrentUserService _currentUser;

        public GetMostActiveExamsQueryHandler(
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

        public async Task<ServiceResponse<MostActiveExamsDto>> Handle(
            GetMostActiveExamsQuery request,
            CancellationToken cancellationToken)
        {
            // 🔒 Last line of defense
            if (!_currentUser.IsInRole("Admin"))
            {
                return ServiceResponse<MostActiveExamsDto>.ForbiddenResponse(
                    "Admin access required",
                    "الوصول متاح للمسؤول فقط");
            }

            // Group attempts by exam
            var examAttempts = await _userExamAttemptRepository.GetAll()
                .Where(a => !a.IsDeleted)
                .GroupBy(a => a.ExamId)
                .Select(g => new
                {
                    ExamId = g.Key,
                    AttemptCount = g.Count(),
                    TotalParticipants = g.Select(a => a.UserId).Distinct().Count(),
                    AverageScore = g
                        .Where(a => a.FinishedAt.HasValue && a.TotalQuestions > 0)
                        .Select(a => (decimal)a.Score / a.TotalQuestions * 100)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .OrderByDescending(x => x.AttemptCount)
                .Take(10)
                .ToListAsync(cancellationToken);

            if (!examAttempts.Any())
            {
                return ServiceResponse<MostActiveExamsDto>.SuccessResponse(
                    new MostActiveExamsDto { Exams = new List<ExamActivityDto>() },
                    "No exam activity data found",
                    "لم يتم العثور على بيانات نشاط الامتحانات");
            }

            // Exams
            var examIds = examAttempts.Select(e => e.ExamId).ToList();
            var exams = await _examRepository.GetAll()
                .Where(e => examIds.Contains(e.Id) && !e.IsDeleted)
                .Select(e => new { e.Id, e.Title, e.CategoryId, e.IsActive })
                .ToListAsync(cancellationToken);

            // Categories
            var categoryIds = exams.Select(e => e.CategoryId).Distinct().ToList();
            var categories = await _categoryRepository.GetAll()
                .Where(c => categoryIds.Contains(c.Id) && !c.IsDeleted)
                .Select(c => new { c.Id, c.Title })
                .ToListAsync(cancellationToken);

            var examLookup = exams.ToDictionary(e => e.Id);
            var categoryLookup = categories.ToDictionary(c => c.Id);

            var examActivities = examAttempts.Select(ea =>
            {
                var exam = examLookup.GetValueOrDefault(ea.ExamId);
                var category = exam != null
                    ? categoryLookup.GetValueOrDefault(exam.CategoryId)
                    : null;

                return new ExamActivityDto
                {
                    ExamId = ea.ExamId,
                    ExamTitle = exam?.Title ?? "Unknown Exam",
                    CategoryName = category?.Title ?? "Unknown Category",
                    AttemptCount = ea.AttemptCount,
                    TotalParticipants = ea.TotalParticipants,
                    AverageScore = Math.Round(ea.AverageScore, 2),
                    IsActive = exam?.IsActive ?? false
                };
            }).ToList();

            var result = new MostActiveExamsDto
            {
                Exams = examActivities
            };

            return ServiceResponse<MostActiveExamsDto>.SuccessResponse(
                result,
                "Most active exams data retrieved successfully",
                "تم استرجاع بيانات أكثر الامتحانات نشاطًا بنجاح");
        }
    }

}