using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Students.Commands;
using EduocationSystem.Features.Students.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Students.Handlers
{
    public class UpdateStudentCommandHandler
      : IRequestHandler<UpdateStudentCommand, ServiceResponse<StudentDto>>
    {
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly IUnitOfWork _uow;

        public UpdateStudentCommandHandler(
            IGenericRepository<Student> studentRepo,
            IUnitOfWork uow)
        {
            _studentRepo = studentRepo;
            _uow = uow;
        }

        public async Task<ServiceResponse<StudentDto>> Handle(
            UpdateStudentCommand request,
            CancellationToken cancellationToken)
        {
            var student = await _studentRepo.GetAll()
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);

            if (student == null)
                return ServiceResponse<StudentDto>.NotFoundResponse(
                    "Student not found",
                    "الطالب غير موجود"
                );

            student.DepartmentId = request.Dto.DepartmentId;
            student.Level = request.Dto.Level;

            _studentRepo.Update(student);
            await _uow.SaveChangesAsync();

            var dto = await _studentRepo.GetAll()
                .Where(s => s.Id == student.Id)
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    FullName = s.User.FirstName + " " + s.User.LastName,
                    Email = s.User.Email!,
                    Department = s.Department.Name,
                    Level = s.Level
                })
                .FirstAsync(cancellationToken);

            return ServiceResponse<StudentDto>.SuccessResponse(
                dto,
                "Student updated successfully",
                "تم تحديث بيانات الطالب"
            );
        }
    }

}
