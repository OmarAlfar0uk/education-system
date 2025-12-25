using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Students.Commands;
using EduocationSystem.Features.Students.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EduocationSystem.Features.Students.Handlers
{
    public class CreateStudentCommandHandler
        : IRequestHandler<CreateStudentCommand, ServiceResponse<StudentDto>>
    {
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _uow;

        public CreateStudentCommandHandler(
            IGenericRepository<Student> studentRepo,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork uow)
        {
            _studentRepo = studentRepo;
            _userManager = userManager;
            _uow = uow;
        }

        public async Task<ServiceResponse<StudentDto>> Handle(
            CreateStudentCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Dto.UserId);

            if (user == null)
                return ServiceResponse<StudentDto>.NotFoundResponse(
                    "User not found",
                    "المستخدم غير موجود");

            // Optional: ensure user is not already a student
            var existingStudent = await _studentRepo.FirstOrDefaultAsync(
                s => s.UserId == user.Id);

            if (existingStudent != null)
                return ServiceResponse<StudentDto>.ConflictResponse(
                    "User is already a student",
                    "المستخدم مسجل كطالب بالفعل");

            var student = new Student
            {
                UserId = user.Id,
                DepartmentId = request.Dto.DepartmentId,
                Level = request.Dto.Level
            };

            await _studentRepo.AddAsync(student);
            await _uow.SaveChangesAsync();

            return ServiceResponse<StudentDto>.SuccessResponse(
                new StudentDto
                {
                    Id = student.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email!,
                    Level = student.Level
                },
                "Student created successfully",
                "تم إنشاء الطالب بنجاح");
        }
    }
}
