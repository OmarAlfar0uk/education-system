using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Dashboard.Dtos;
using EduocationSystem.Features.Dashboard.Queries;
using EduocationSystem.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace EduocationSystem.Features.Dashboard.Handlers
{
    public class GetDashboardStatsQueryHandler
       : IRequestHandler<GetDashboardStatsQuery, ServiceResponse<DashboardStatsDto>>
    {
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IGenericRepository<UserExamAttempt> _userExamAttemptRepository;
        private readonly IGenericRepository<ApplicationUser> _userRepo;
        private readonly ICurrentUserService _currentUser;

        public GetDashboardStatsQueryHandler(
            IGenericRepository<Exam> examRepository,
            IGenericRepository<Question> questionRepository,
            IGenericRepository<Category> categoryRepository,
            IGenericRepository<UserExamAttempt> userExamAttemptRepository,
            IGenericRepository<ApplicationUser> userRepo,
            ICurrentUserService currentUser)
        {
            _examRepository = examRepository;
            _questionRepository = questionRepository;
            _categoryRepository = categoryRepository;
            _userExamAttemptRepository = userExamAttemptRepository;
            _userRepo = userRepo;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<DashboardStatsDto>> Handle(
            GetDashboardStatsQuery request,
            CancellationToken cancellationToken)
        {
            // 🔒 Last line of defense
            if (!_currentUser.IsInRole("Admin"))
            {
                return ServiceResponse<DashboardStatsDto>.ForbiddenResponse(
                    "Admin access required",
                    "الوصول متاح للمسؤول فقط");
            }

            var totalExams = await _examRepository
                .GetAll().CountAsync(e => !e.IsDeleted, cancellationToken);

            var activeExams = await _examRepository
                .GetAll().CountAsync(e => e.IsActive && !e.IsDeleted, cancellationToken);

            var inactiveExams = await _examRepository
                .GetAll().CountAsync(e => !e.IsActive && !e.IsDeleted, cancellationToken);

            var totalQuestions = await _questionRepository
                .GetAll().CountAsync(q => !q.IsDeleted, cancellationToken);

            var totalCategories = await _categoryRepository
                .GetAll().CountAsync(c => !c.IsDeleted, cancellationToken);

            var totalUsers = await _userRepo
                .GetAll().CountAsync(u => u.EmailConfirmed, cancellationToken);

            var totalExamAttempts = await _userExamAttemptRepository
                .GetAll().CountAsync(a => !a.IsDeleted, cancellationToken);

            decimal averageScore = 0;
            var completedAttempts = _userExamAttemptRepository.GetAll()
                .Where(a => !a.IsDeleted && a.FinishedAt.HasValue && a.TotalQuestions > 0);

            if (await completedAttempts.AnyAsync(cancellationToken))
            {
                averageScore = await completedAttempts
                    .AverageAsync(a => (decimal)a.Score / a.TotalQuestions * 100, cancellationToken);
            }

            var stats = new DashboardStatsDto
            {
                TotalExams = totalExams,
                ActiveExams = activeExams,
                InactiveExams = inactiveExams,
                TotalQuestions = totalQuestions,
                TotalCategories = totalCategories,
                TotalUsers = totalUsers,
                TotalExamAttempts = totalExamAttempts,
                AverageScore = Math.Round(averageScore, 2)
            };

            return ServiceResponse<DashboardStatsDto>.SuccessResponse(
                stats,
                "Dashboard stats retrieved successfully",
                "تم استرجاع إحصائيات لوحة التحكم بنجاح");
        }
    }

}