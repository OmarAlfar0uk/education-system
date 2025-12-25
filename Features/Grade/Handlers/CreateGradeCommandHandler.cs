using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Grade.Command;
using EduocationSystem.Features.Grade.DTOs;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Grade.Handlers
{
    public class CreateGradeCommandHandler : IRequestHandler<CreateGradeCommand, ServiceResponse<GradeDto>>
    {
        private readonly IGenericRepository<Domain.Entities.Grade> _gradeRepo;
        private readonly IGenericRepository<Domain.Entities.Enrollment> _enrollmentRepo;
        private readonly IGenericRepository<Domain.Entities.Notification> _notificationRepo;
        private readonly IUnitOfWork _uow;

        public CreateGradeCommandHandler(
            IGenericRepository<Domain.Entities.Grade> gradeRepo,
            IGenericRepository<Domain.Entities.Enrollment> enrollmentRepo,
            IGenericRepository<Domain.Entities.Notification> notificationRepo,
            IUnitOfWork uow)
        {
            _gradeRepo = gradeRepo;
            _enrollmentRepo = enrollmentRepo;
            _notificationRepo = notificationRepo;
            _uow = uow;
        }

        public async Task<ServiceResponse<GradeDto>> Handle(
            CreateGradeCommand request,
            CancellationToken cancellationToken)
        {
            var enrollment = await _enrollmentRepo.GetAll()
             .Where(e => e.Id == request.Dto.EnrollmentId)
             .Select(e => new
             {
                 StudentUserId = e.Student.UserId,
                 StudentName = e.Student.User.FirstName + " " + e.Student.User.LastName,
                 CourseName = e.Course.Title,

                 ParentUserIds = e.Student.ParentStudents
                     .Select(ps => ps.Parent.UserId)
                     .ToList()
             })
             .FirstOrDefaultAsync(cancellationToken);


            if (enrollment == null)
                return ServiceResponse<GradeDto>.NotFoundResponse(
                    "Enrollment not found",
                    "التسجيل غير موجود");

            var grade = new Domain.Entities.Grade
            {
                EnrollmentId = request.Dto.EnrollmentId,
                AssessmentType = request.Dto.AssessmentType,
                Score = request.Dto.Score
            };

            await _gradeRepo.AddAsync(grade);
            await _uow.SaveChangesAsync();

            // ========== NOTIFICATIONS ========== //

            var notifications = new List<Domain.Entities.Notification>();

            // Student notification
            notifications.Add(new Domain.Entities.Notification
            {
                UserId = enrollment.StudentUserId,
                Title = "New grade added",
                Body = $"You received {request.Dto.Score} in {enrollment.CourseName}",
                IsRead = false
            });

            // Parent notifications
            foreach (var parentId in enrollment.ParentUserIds)
            {
                notifications.Add(new Domain.Entities.Notification
                {
                    UserId = parentId,
                    Title = "Your child's grade has been updated",
                    Body = $"{enrollment.StudentName} received {request.Dto.Score} in {enrollment.CourseName}",
                    IsRead = false
                });
            }

            await _notificationRepo.AddRangeAsync(notifications);
            await _uow.SaveChangesAsync();

            var dto = new GradeDto
            {
                Id = grade.Id,
                EnrollmentId = grade.EnrollmentId,
                AssessmentType = grade.AssessmentType,
                Score = grade.Score
            };

            return ServiceResponse<GradeDto>.SuccessResponse(
                dto,
                "Grade created successfully",
                "تم إضافة الدرجة بنجاح");
        }
    }
}
