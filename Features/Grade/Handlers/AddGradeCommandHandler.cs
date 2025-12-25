using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Grade.Command;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Grade.Handlers
{
    public class AddGradeCommandHandler
        : IRequestHandler<AddGradeCommand, ServiceResponse<int>>
    {
        private readonly IGenericRepository<Domain.Entities.Grade> _gradeRepo;
        private readonly IGenericRepository<Domain.Entities.Enrollment> _enrollmentRepo;
        private readonly IUnitOfWork _uow;

        public AddGradeCommandHandler(
            IGenericRepository<Domain.Entities.Grade> gradeRepo,
            IGenericRepository<Domain.Entities.Enrollment> enrollmentRepo,
            IUnitOfWork uow)
        {
            _gradeRepo = gradeRepo;
            _enrollmentRepo = enrollmentRepo;
            _uow = uow;
        }

        public async Task<ServiceResponse<int>> Handle(
            AddGradeCommand request,
            CancellationToken cancellationToken)
        {
            var enrollmentExists = await _enrollmentRepo.GetAll()
                .AnyAsync(e =>
                    e.Id == request.EnrollmentId &&
                    !e.IsDeleted,
                    cancellationToken);

            if (!enrollmentExists)
                return ServiceResponse<int>.NotFoundResponse(
                    "Enrollment not found",
                    "التسجيل غير موجود");

            var grade = new Domain.Entities.Grade
            {
                EnrollmentId = request.EnrollmentId,
                AssessmentType = request.AssessmentType,
                Score = request.Score
            };

            await _gradeRepo.AddAsync(grade);
            await _uow.SaveChangesAsync();

            return ServiceResponse<int>.SuccessResponse(
                grade.Id,
                "Grade added successfully",
                "تم إضافة الدرجة بنجاح");
        }
    }
}
