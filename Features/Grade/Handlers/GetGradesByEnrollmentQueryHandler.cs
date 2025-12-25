using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Grade.DTOs;
using EduocationSystem.Features.Grade.Queries;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Grade.Handlers
{
    public class GetGradesByEnrollmentQueryHandler
         : IRequestHandler<GetGradesByEnrollmentQuery, ServiceResponse<List<GradeDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.Grade> _gradeRepo;

        public GetGradesByEnrollmentQueryHandler(
            IGenericRepository<Domain.Entities.Grade> gradeRepo)
        {
            _gradeRepo = gradeRepo;
        }

        public async Task<ServiceResponse<List<GradeDto>>> Handle(
            GetGradesByEnrollmentQuery request,
            CancellationToken cancellationToken)
        {
            var grades = await _gradeRepo.GetAll()
                .Where(g =>
                    g.EnrollmentId == request.EnrollmentId &&
                    !g.IsDeleted)
                .Select(g => new GradeDto
                {
                    Id = g.Id,
                    EnrollmentId = g.EnrollmentId,
                    AssessmentType = g.AssessmentType,
                    Score = g.Score,
                    CreatedAt = g.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return ServiceResponse<List<GradeDto>>
                .SuccessResponse(grades);
        }
    }
}
