using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Grade.Command;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Grade.Handlers
{
    public class UpdateGradeCommandHandler
         : IRequestHandler<UpdateGradeCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.Grade> _gradeRepo;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public UpdateGradeCommandHandler(
            IGenericRepository<Domain.Entities.Grade> gradeRepo,
            IUnitOfWork uow,
            ICurrentUserService currentUser)
        {
            _gradeRepo = gradeRepo;
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<ServiceResponse<bool>> Handle(
            UpdateGradeCommand request,
            CancellationToken cancellationToken)
        {
            // 🔒 حماية إضافية
            if (!_currentUser.IsInRole("Admin"))
            {
                return ServiceResponse<bool>.ForbiddenResponse(
                    "You are not allowed to update grades",
                    "غير مسموح لك بتعديل الدرجات");
            }

            var grade = await _gradeRepo.GetByIdAsync(request.GradeId);

            if (grade == null || grade.IsDeleted)
                return ServiceResponse<bool>.NotFoundResponse(
                    "Grade not found",
                    "الدرجة غير موجودة");

            grade.AssessmentType = request.AssessmentType;
            grade.Score = request.Score;

            _gradeRepo.Update(grade);
            await _uow.SaveChangesAsync();

            return ServiceResponse<bool>.SuccessResponse(
                true,
                "Grade updated successfully",
                "تم تعديل الدرجة بنجاح");
        }
    }
}
